// Gone through at v1.3
using System;
using System.Linq;
using MDunGen.Commons;
using MDunGen.Resources;

namespace MDunGen.Sections;

public class HallwaySection : SectionBase
{
	MAPDIRECTION left;
	MAPDIRECTION right;

	MapCoordinate BottomLeft => coord + left + left + MapCoordinate.Down;

	public HallwaySection(SectionBuildArguments args, bool debug = false) : base(args, debug)
	{
		PathResource pathRes = args.sectionDefinition as PathResource;
	}
	public override void Build(Action<BuildLogEventArgument> log)
	{
		log.Invoke(new()
		{
			source = "RoomSection::Build()",
			message = $"Building Hallway section{(debug == true ? " with debug flag" : "")}",
			sectionIndex = sectionIndex,
			levelIndex = levelIndex,
			mapLocations = [coord]
		});

		left = DungeonUtils.TwistLeft(orientation);
		right = DungeonUtils.TwistRight(orientation);

		// Shift start coord one down for this section type
		SetMinMaxCoord(coord + MapCoordinate.Down + MapCoordinate.Down); 

		BuildHallwayStart();
		MapCoordinate stepLocation = BottomLeft + orientation;
		for (int i = 0; i < Depth - 2; i++)
		{
			AddHallwayStep(stepLocation);
			stepLocation = stepLocation + orientation;
		}
		BuildHallwayEnd(stepLocation);
		foreach (MapPiece mapPiece in pieces)
		{
			mapPiece.AddSection(sectionIndex);
			map.SavePiece(mapPiece);
		}
		SealSection(0, -1, 0);

		if (debug)
		{
			Godot.GD.Print($"HallwaySection:: Orientation[{pieces.First().Orientation}] Depth[{Depth}] Width[{Width}]");
		}
	}
	private void BuildHallwayStart()
	{
		ClaimSlice(BottomLeft);
		
		MapPiece start = map.GetPiece(BottomLeft);
		start.State = MAPPIECESTATE.PENDING;
		start.AddExtra(new KeyData() { key = PIECEKEYS.ARCH, dir = orientation, variantID = 2 });
		start.keyFloor = new KeyData() { key = PIECEKEYS.F, dir = orientation, variantID = 3 };
		pieces.Add(start);
		start.Save();
	}
	private void BuildHallwayEnd(MapCoordinate endCoord)
	{
		ClaimSlice(endCoord);
		MapPiece end = map.GetPiece(endCoord + right + right + right);
		end.AddExtra(new KeyData() { key = PIECEKEYS.ARCH, dir = DungeonUtils.Flip(orientation), variantID = 2 });
		end.State = MAPPIECESTATE.PENDING;
		end.keyFloor = new KeyData() { key = PIECEKEYS.F, dir = DungeonUtils.Flip(orientation), variantID = 3 };
		pieces.Add(end);
		end.Save();
	}
	private void AddHallwayStep(MapCoordinate stepCoord)
	{
		ClaimSlice(stepCoord);

		MapPiece stepLeft = map.GetPiece(stepCoord);
		stepLeft.State = MAPPIECESTATE.PENDING;
		stepLeft.AddExtra(new KeyData() { key = PIECEKEYS.ARCH, dir = orientation, variantID = 2 });
		stepLeft.keyFloor = new KeyData() { key = PIECEKEYS.F, dir = orientation, variantID = 4 };
		pieces.Add(stepLeft);
		stepLeft.Save();
		
		MapPiece stepRight = map.GetPiece(stepCoord + right + right + right);
		stepRight.State = MAPPIECESTATE.PENDING;
		stepRight.keyFloor = new KeyData() { key = PIECEKEYS.F, dir = DungeonUtils.Flip(orientation), variantID = 4 };
		pieces.Add(stepRight);
		stepRight.Save();
	}


	/// <summary>
	/// Claim a slice in room space relative to left side, bottom
	/// </summary>
	/// <param name="stepCoord"></param>
	private void ClaimSlice(MapCoordinate startCoord)
	{
		// do it by row
		for (int i = 0; i < 3; i++)
		{
			ClaimPiece(startCoord);
			ClaimPiece(startCoord + right);
			ClaimPiece(startCoord + right + right);
			ClaimPiece(startCoord + right + right + right);
			startCoord += MapCoordinate.Up;
		}
	}
	private void ClaimPiece(MapCoordinate pieceCoord)
	{
		MapPiece mp = map.GetPiece(pieceCoord);
		mp.State = MAPPIECESTATE.PENDING;
		mp.Orientation = orientation;
		pieces.Add(mp);
		mp.Save();
	}
}// EOF CLASS