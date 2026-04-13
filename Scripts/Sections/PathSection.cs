// Gone through at v1.3
using Godot;
using MDunGen.Commons;
using MDunGen.Resources;
using System;
using System.Collections.Generic;

namespace MDunGen.Sections;

public class PathSection : SectionBase
{
	override public int TileCount => AllPieces().Count;
	override public List<MapPiece> Pieces => AllPieces();
	private Line[] lines;
	/// <summary>
	/// Left side is always the master line and the first entry in lines Array
	/// </summary>
	private Line LeftSide => lines[0];
	private Line RightSide => lines[lines.Length - 1];
	private bool isFinished = false;
	internal bool IsValid = false;
	internal int Floor => LeftSide.Floor;

	// Corridor things
	private int corMaxTotal = 60;
	private int corMaxStraight = 5;
	private int corMinStraight = 2;
	private bool nextTurnIsRight = false;

	public PathSection(SectionBuildArguments args, bool debug) : base(args, debug)
	{
		PathResource pathRes = args.sectionDefinition as PathResource;
		corMaxTotal = pathRes.corMaxTotal;
		corMaxStraight = pathRes.corMaxStraight;
		corMinStraight = pathRes.corMinStraight;
	}

	public override void Build(Action<BuildLogEventArgument> log)
	{
		log.Invoke(new() { source = "RoomSection::Build()", message = "Building Path section", sectionIndex = sectionIndex, levelIndex = levelIndex, mapLocations = [coord] });

		MapPiece step = map.GetPiece(coord);
		MAPDIRECTION startDir = step.Orientation;
		if (map.Sections.Count > 0)
		{
			//BuildStartConnection(step, DungeonUtils.Flip(step.Orientation));
		}
		//VerifyWidth(step);
		SetupLines(step);
		int breaker = corMaxTotal * 2 + 5;
		int turnTimer = RollTurn();

		while (!isFinished)
		{
			turnTimer--;
			// Either add a step or make a turn
			if (turnTimer < 1)
			{
				if (nextTurnIsRight) { TurnRight(); } else { TurnLeft(); }
				turnTimer = RollTurn();
			}
			else
			{
				AddStep(corMaxTotal);
			}
			if (PathBlocked()) { TrimLines(); isFinished = true; }
			breaker--;
			if (breaker < 0)
			{
				isFinished = true;
				GD.PrintErr($"PathSection", "Build", "Add step loop hit BREAKER!");
			}
			if (LeftSide.Count >= corMaxTotal) { isFinished = true; }
		}

		// To short so we fail it
		if (TileCount < 1)
		{
			IsValid = false;
			return;
		}

		foreach (MapPiece piece in Pieces)
		{
			//if (sectionIndex != piece.SectionIndex)
			//{
			//	//GD.Print($"PathSection::Build() Foreign Piece! coord[{piece.Coord}]");
			//	//map.MovePieceOwnershipToSection(piece, sectionIndex);
			//}
		}
		MakeSureStartPieceIsFirstPieceAndFixTheOrientation(startDir);

		SealSection();
		BuildEndCap();
		if (sectionDefinition.arches) { FitSmallArches(); }
		IsValid = true;
		SetMinMaxCoord();
	}

	private void MakeSureStartPieceIsFirstPieceAndFixTheOrientation(MAPDIRECTION dir)
	{
		if (lines[0].First.Coord != coord)
		{
			// Clear out the start piece from all lines
			for (int i = 0; i < lines.Length; i++)
			{
				lines[i].Remove(coord);
			}
		}
		lines[0].InsertAsFirst(map.GetExistingPiece(coord));
		lines[0].First.Orientation = dir;
	}

	private List<MapPiece> AllPieces()
	{
		List<MapPiece> a = new List<MapPiece>();
		for (int i = 0; i < lines.Length; i++)
		{
			a.AddRange(lines[i].Steps);
		}
		return a;
	}

	private void TrimLines()
	{
		int length = LeftSide.Count;
		for (int i = 0; i < lines.Length; i++)
		{
			length = Math.Min(length, lines[i].Count);
		}
		for (int i = 0; i < lines.Length; i++)
		{
			lines[i].TrimToLength(length);
		}
	}

	internal MapPiece GetRandomAlongPathNoCorner(out MAPDIRECTION dir)
	{
		MapPiece result = null;
		dir = MAPDIRECTION.ANY;
		// Check for corner and go recursive 10x before failing
		int breaker = 10;
		while (breaker > 0)
		{
			breaker--;
			result = GetRandomAlongPath(out dir);

			if (result is not null && !result.IsCorner(dir)) { return result; }

			if (breaker < 1) { break; }
		}
		return null;
	}
	public override void SealSection(int wallVariant = 0, int floorVariant = 0, int ceilingVariant = 0)
	{
		base.SealSection(1);
	}
	/// <summary>
	/// remember to check state before using returned piece
	/// </summary>
	/// <param name="dir"></param>
	/// <returns></returns>
	internal MapPiece GetRandomAlongPath(out MAPDIRECTION dir)
	{
		if (sectionDefinition.sizeWidthMax < 2)
		{
			return LeftSide.GetRandomAlongPath(out dir);
		}
		// Pick from wider paths
		int max = LeftSide.Count + RightSide.Count;
		int pick = rng.Next(max);
		if (pick < LeftSide.Count)
		{
			return LeftSide.GetRandomAlongPath(out dir, true, false);

		}
		else
		{
			return RightSide.GetRandomAlongPath(out dir, false, true);
		}
	}
	[Obsolete("NEeds rewrite and fixing")]
	private void BuildStartConnection(MapPiece startPiece, MAPDIRECTION dir)
	{
		MapPiece startPieceConnection = startPiece.Neighbour(dir, false);
		ISection otherSection;
		if (startPieceConnection.MainSection > -1 && startPieceConnection.MainSection < map.Sections.Count)
		{
			otherSection = map.Sections[startPieceConnection.MainSection];
			int conn1 = AddConnection(
				dir,
				map.Sections[startPieceConnection.MainSection],
				startPiece.Coord,
				startPieceConnection.Coord,
				true
				);
			int conn2 = otherSection.AddConnection(DungeonUtils.Flip(dir), this, startPieceConnection.Coord, startPiece.Coord, true);
			map.Connections[conn1].connectedToConnectionID = conn2;
			map.Connections[conn2].connectedToConnectionID = conn1;
		}
	}
	private void BuildEndCap()
	{
		MapPiece endPieceConnection = LeftSide.Last.Neighbour(LeftSide.Last.Orientation, true);

		if (sectionDefinition.sizeWidthMax < 2)
		{
			CapLineEndsWithWalls();
			if (endPieceConnection.State == MAPPIECESTATE.PENDING && endPieceConnection.MainSection >= 0 && !endPieceConnection.IsPartOfSection(sectionIndex))
			{
				AddConnection(
					LeftSide.Last.Orientation,
					map.Sections[endPieceConnection.MainSection],
					LeftSide.Last.Coord,
					endPieceConnection.Coord,
					true
					);
			}
			return;
		}
		MapPiece endPieceConnection2 = RightSide.Last.Neighbour(RightSide.Last.Orientation, true);

		if (endPieceConnection.State == MAPPIECESTATE.PENDING && endPieceConnection2.State == MAPPIECESTATE.PENDING)
		{
			// Avoid messing with start connection if path is only 1 tile long
			if (LeftSide.Count > 1)
			{

				LeftSide.Last.AssignWall(new KeyData() { key = PIECEKEYS.NONE, dir = LeftSide.Last.Orientation }, true);

				if (!RightSide.Last.isEmpty)
				{
					RightSide.Last.AssignWall(new KeyData() { key = PIECEKEYS.NONE, dir = DungeonUtils.Flip(RightSide.Last.Orientation) }, true);
				}
			}

			LeftSide.Last.Neighbour(LeftSide.Last.Orientation, true).AssignWall(new KeyData() { key = PIECEKEYS.NONE, dir = DungeonUtils.Flip(LeftSide.Last.Orientation) }, true);
			RightSide.Last.Neighbour(LeftSide.Last.Orientation, true).AssignWall(new KeyData() { key = PIECEKEYS.NONE, dir = DungeonUtils.Flip(RightSide.Last.Orientation) }, true);
		}
		else
		{
			CapLineEndsWithWalls();
			if (!endPieceConnection.isEmpty && endPieceConnection2.isEmpty && endPieceConnection.MainSection >= 0 && !endPieceConnection.IsPartOfSection(sectionIndex))
			{
				// Add a single door to connect corridors on left side
				AddConnection(LeftSide.Last.Orientation, map.Sections[endPieceConnection.MainSection], LeftSide.Last.Coord, endPieceConnection.Coord, true);
			}
			else if (endPieceConnection.isEmpty && !endPieceConnection2.isEmpty && endPieceConnection2.MainSection >= 0 && !endPieceConnection2.IsPartOfSection(sectionIndex))
			{
				// Add a single door to connect corridors on right side
				AddConnection(RightSide.Last.Orientation, map.Sections[endPieceConnection.MainSection], RightSide.Last.Coord, endPieceConnection.Coord, true);
			}
		}
	}
	private void CapLineEndsWithWalls()
	{
		for (int i = 0; i < lines.Length; i++)
		{
			MAPDIRECTION dir = lines[i].Last.Orientation;
			MapPiece next = lines[i].Last.Neighbour(dir, true);
			if (next.isEmpty && next.MainSection < 0)
			{
				lines[i].Last.AssignWall(new KeyData() { key = PIECEKEYS.W, dir = lines[i].Last.Orientation }, true);
			}
			if (next.HasWall(DungeonUtils.Flip(dir)))
			{
				lines[i].Last.AssignWall(new KeyData() { key = PIECEKEYS.W, dir = lines[i].Last.Orientation }, true);
			}
		}
	}
	/// <summary>
	/// This rolls if it should turn left or right and returns in how many steps the turn should happen
	/// defaults to corMinStraight value
	/// </summary>
	/// <returns></returns>
	private int RollTurn()
	{
		nextTurnIsRight = true;
		if (rng.Next(100) < 50)
		{
			nextTurnIsRight = false;
		}
		if (corMinStraight < corMaxStraight)
		{
			return rng.Next(corMinStraight, corMaxStraight);
		}
		return corMinStraight;
	}
	private void TurnRight()
	{
		MAPDIRECTION newDir = DungeonUtils.TwistRight(lines[0].Last.Orientation);
		MapPiece[] turners = RightSide.GetTurners(sizeX, newDir);
		if (TurnNotBlocked(turners))
		{
			for (int i = 0; i < turners.Length; i++)
			{
				turners[i].AddSection(sectionIndex);
				turners[i].Orientation = newDir;
				turners[i].State = MAPPIECESTATE.PENDING;
				lines[i].AddStep(turners[i], true);
				turners[i].Save();
			}
		}
	}
	private void TurnLeft()
	{
		MAPDIRECTION newDir = DungeonUtils.TwistLeft(lines[0].Last.Orientation);
		MapPiece[] turners = LeftSide.GetTurners(sizeX, newDir, true);
		if (TurnNotBlocked(turners))
		{
			for (int i = 0; i < turners.Length; i++)
			{
				turners[i].AddSection(sectionIndex);
				turners[i].Orientation = newDir;
				turners[i].State = MAPPIECESTATE.PENDING;
				lines[i].AddStep(turners[i], true);
				turners[i].Save();
			}
		}
	}
	private bool TurnNotBlocked(MapPiece[] turners)
	{
		for (int i = 0; i < turners.Length; i++)
		{
			if (turners[i].State != MAPPIECESTATE.UNUSED)
			{
				return false;
			}
		}
		return true;
	}

	private bool PathBlocked()
	{
		for (int i = 0; i < lines.Length; i++)
		{
			if (lines[i].Blocked) { return true; }
		}
		return false;
	}

	private void AddStep(int maxSteps)
	{
		for (int i = 0; i < lines.Length; i++)
		{
			lines[i].Walk(maxSteps, i == 0);
		}
	}

	private void SetupLines(MapPiece step)
	{
		lines = new Line[sizeX];
		step.AddSection(sectionIndex);
		step.State = MAPPIECESTATE.PENDING;
		lines[0] = new Line(map, this, step, rng);
		if (lines.Length > 1)
		{
			MAPDIRECTION expandTo = DungeonUtils.TwistRight(step.Orientation);
			for (int i = 1; i < lines.Length; i++)
			{
				lines[i] = new Line(map, this, lines[i - 1].Last.Neighbour(expandTo, true), rng);
				lines[i].Last.Orientation = step.Orientation;
				lines[i].Last.State = MAPPIECESTATE.PENDING;
			}
		}
	}
	private void VerifyWidth(MapPiece step)
	{
		int cleared = 0;
		MAPDIRECTION dir = DungeonUtils.TwistRight(step.Orientation);
		for (int i = 0; i < sizeX; i++)
		{
			if (step.State != MAPPIECESTATE.UNUSED)
			{
				sizeX = cleared;
				return;
			}
			cleared++;
			step = step.Neighbour(dir, true);
		}
		sizeX = cleared;
	}
}// EOF CLASS