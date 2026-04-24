// Gone through at v1.3
using Godot;
using MDunGen.Commons;
using MDunGen.Resources;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MDunGen.Sections;

public class SectionBase : ISection
{
	#region fields
	/// <summary>
	/// The deterministic random number generator for this section
	/// </summary>
	private protected readonly PRNGMarsenneTwister rng;
	/// <summary>
	/// Set in constructor. Usually only used when using section debug mode.
	/// </summary>
	private protected readonly bool debug;
	public PRNGMarsenneTwister RNG => rng;

	/// <summary>
	/// The parent map data this section belongs to
	/// </summary>
	private protected MapData map;
	/// <summary>
	/// Assigned section index on creation
	/// </summary>
	private protected readonly int sectionIndex;
	/// <summary>
	/// Level the section was grown on
	/// </summary>
	private protected readonly int levelIndex;
	/// <summary>
	/// The section name. Might not be unique in the Dungeon
	/// </summary>
	private protected string sectionName = string.Empty;

	private protected List<int> connections;
	public List<int> Connections => connections;
	public int ConnectionCount => connections.Count;

	public MAPDIRECTION Orientation => orientation;


	private protected List<MapPiece> pieces;


	private protected MapCoordinate coord;
	private protected int MinY => minY;
	private protected int MaxY => maxY;
	public MapCoordinate Coord => coord;

	#endregion

	#region Needs some work

	ATTACHHEIGHT growHeight = ATTACHHEIGHT.BOTTOM;
	ATTACHHEIGHT attachHeight = ATTACHHEIGHT.BOTTOM;
	public ATTACHHEIGHT GrowHeight => growHeight;
	public ATTACHHEIGHT AttachHeight => attachHeight;

	private protected SectionResource sectionDefinition;

	internal MAPDIRECTION orientation;

	public ROOMCONNECTIONRESPONCE defaultConnectionResponses;

	ROOMCONNECTIONRESPONCE ISection.defaultConnectionResponses { get => defaultConnectionResponses; }

	public bool BridgeAllowed => defaultConnectionResponses.HasFlag(ROOMCONNECTIONRESPONCE.BRIDGE);
	public bool DoorAllowed => defaultConnectionResponses.HasFlag(ROOMCONNECTIONRESPONCE.DOOR);
	public bool PlaceArches => sectionDefinition.arches;

	private protected int sizeZ;
	private protected int sizeX;
	private protected int sizeY;

	private protected int minX = 0;
	private protected int maxX = 0;
	private protected int minY = 0;
	private protected int maxY = 0;
	private protected int minZ = 0;
	private protected int maxZ = 0;

	private float waterLevel = -1.0f;
	private Material waterMaterial;
	private float waterDepth = 1.0f;

	#endregion

	#region Properties
	public MapCoordinate MaxCoord => new MapCoordinate(maxX, MaxY, maxZ);
	public MapCoordinate MinCoord => new MapCoordinate(minX, MinY, minZ);


	/// <summary>
	/// Section space width. As the section size perpendicular to the orientation.
	/// </summary>
	public int Width => orientation == MAPDIRECTION.NORTH || orientation == MAPDIRECTION.SOUTH ? sizeX : sizeZ;
	/// <summary>
	/// Section space depth. As the section size in the orientation.
	/// </summary>
	public int Depth => orientation == MAPDIRECTION.NORTH || orientation == MAPDIRECTION.SOUTH ? sizeZ : sizeX;

	public int SectionIndex => sectionIndex;
	public int LevelIndex => levelIndex;
	public string SectionName => sectionName;
	public virtual int TileCount => Pieces.Count;
	public virtual List<MapPiece> Pieces => pieces;
	public float WaterLevel => waterLevel;
	public Material WaterMaterial => waterMaterial;
	public float WaterDepth => waterDepth;

	//public int PropCount => Props.Count;

	#endregion



	public Node3D sectionContainer;

	public Node3D SectionContainer { get => sectionContainer; set => sectionContainer = value; }

	public SectionBase(SectionBuildArguments args, bool debug)
	{
		if (args.sectionDefinition is null) { GD.PushError("Section definition was NULL"); return; }

		this.debug = debug;

		pieces = new List<MapPiece>();
		connections = new List<int>();
		rng = new PRNGMarsenneTwister(args.Seed);

		map = args.map;

		sectionDefinition = args.sectionDefinition;
		levelIndex = args.levelIndex;
		sectionIndex = args.sectionID;

		growHeight = args.sectionDefinition.GrowHeight;
		attachHeight = args.sectionDefinition.AttachHeight;

		waterMaterial = args.sectionDefinition.waterMaterial;
		waterDepth = args.sectionDefinition.waterDepth;
		waterLevel = args.sectionDefinition.waterLevel;

		sectionName = sectionDefinition.sectionName;
		coord = args.piece.Coord;
		defaultConnectionResponses = sectionDefinition.defaultResponses;

		orientation = args.piece.Orientation;
		if (orientation == MAPDIRECTION.ANY) { orientation = (MAPDIRECTION)rng.Next(1, 5); args.piece.Orientation = orientation; }

		sectionDefinition.VerifyValues();
		sizeX = rng.Next(sectionDefinition.sizeWidthMin, sectionDefinition.sizeWidthMax + 1);
		sizeZ = rng.Next(sectionDefinition.sizeDepthMin, sectionDefinition.sizeDepthMax + 1);
		sizeY = rng.Next(sectionDefinition.nbFloorsMin, sectionDefinition.nbFloorsMax + 1);

		ResolveWidthDepth();
		SetMinMaxCoord(coord);
	}

	public virtual void Build(Action<BuildLogEventArgument> log)
	{
		log.Invoke(new() { severity = BUILDLOGSEVERITY.WARNING, source = "SectionBase::Build()", message = "Building a base section!", levelIndex = levelIndex, sectionIndex = sectionIndex });
	}

	public virtual bool AddOpening(MapCoordinate coord, MAPDIRECTION dir, bool wide, bool overrideLocked, Action<BuildLogEventArgument> log)
	{
		MapPiece piece = Pieces.Find(p => p.Coord == coord);
		if (piece is null)
		{
			log.Invoke(new()
			{
				source = "SectionBase::AddOpening()",
				severity = BUILDLOGSEVERITY.ERROR,
				message = $"no piece on coord({coord}) part of room[{sectionIndex}]",
				mapLocations = [coord]
			});
			return false;
		}
		if (wide)
		{
			piece.AssignWall(new KeyData() { key = PIECEKEYS.WDW, dir = dir }, overrideLocked);
			MapPiece nb = piece.Neighbour(DungeonUtils.TwistRight(dir), true);
			nb.AssignWall(new KeyData() { key = PIECEKEYS.OCCUPIED, dir = dir }, overrideLocked);
		}
		else
		{
			piece.AssignWall(new KeyData() { key = PIECEKEYS.WD, dir = dir }, overrideLocked);
		}
		map.SavePiece(piece);
		return true;
	}

	/// <summary>
	/// Puts wall,floor and ceiling keys against other sections
	/// Pass -1 to skip the category
	/// </summary>
	/// <exception cref="NotImplementedException"></exception>
	public virtual void SealSection(int wallVariant = 0, int floorVariant = 0, int ceilingVariant = 0)
	{
		List<MapPiece> sectionPieces = Pieces;
		foreach (MapPiece piece in sectionPieces)
		{
			// Floor if no section piece below it
			if (!sectionPieces.Exists(p => p.Coord == piece.Coord + MAPDIRECTION.DOWN) && floorVariant >= 0)
			{
				piece.keyFloor = new KeyData() { key = PIECEKEYS.F, dir = orientation, variantID = floorVariant };
			}
			// Ceiling if no section piece above it
			if (!sectionPieces.Exists(p => p.Coord == piece.Coord + MAPDIRECTION.UP) && ceilingVariant >= 0)
			{
				piece.keyCeiling = new KeyData() { key = PIECEKEYS.C, dir = orientation, variantID = ceilingVariant };
			}

			// Walls
			if (wallVariant >= 0)
			{
				for (int i = 1; i < 5; i++)
				{
					// Wall if no piece in that direction
					if (!sectionPieces.Exists(p => p.Coord == piece.Coord + (MAPDIRECTION)i))
					{
						MapPiece nb = map.GetExistingPiece(piece.Coord + (MAPDIRECTION)i);
						// Wall if the piece in that direction has a wall towards this piece
						if (nb is null || nb.isEmpty || nb.HasWall(DungeonUtils.Flip((MAPDIRECTION)i)))
						{
							piece.AssignWall(new KeyData() { key = PIECEKEYS.W, dir = (MAPDIRECTION)i, variantID = wallVariant }, true);
						}
					}
				}
			}
		}
	}
	/// <summary>
	/// Gets and returns the pieces that have walls on the given floor<br/>
	/// Note that floor is the internal section relative floor
	/// </summary>
	/// <param name="floor"></param>
	/// <param name="includeCorners"></param>
	/// <returns></returns>
	public List<MapPiece> GetWallPieces(int floor, bool includeCorners = false)
	{
		// Confirmed
		List<MapPiece> candidates = Pieces.FindAll(p => p.sectionFloor == floor && p.HasNorthWall);

		candidates.AddRange(Pieces.FindAll(p => p.sectionFloor == floor && p.HasEastWall && !p.HasNorthWall));
		candidates.AddRange(Pieces.FindAll(p => p.sectionFloor == floor && p.HasSouthWall && !p.HasNorthWall && !p.HasEastWall));
		candidates.AddRange(Pieces.FindAll(p => p.sectionFloor == floor && p.HasWestWall && !p.HasNorthWall && !p.HasEastWall && !p.HasSouthWall));
		if (!includeCorners)
		{
			int count = 0;
			count += candidates.RemoveAll(p => p.HasNorthWall && p.HasEastWall);
			count += candidates.RemoveAll(p => p.HasEastWall && p.HasSouthWall);
			count += candidates.RemoveAll(p => p.HasSouthWall && p.HasWestWall);
			count += candidates.RemoveAll(p => p.HasWestWall && p.HasNorthWall);
		}
		return candidates;
	}
	public List<MapPiece> GetOutsideWalls(bool includeCorners = false)
	{
		List<MapPiece> candidates = Pieces.FindAll(p => p.HasNorthWall);
		candidates.AddRange(Pieces.FindAll(p => p.HasEastWall && !p.HasNorthWall));
		candidates.AddRange(Pieces.FindAll(p => p.HasSouthWall && !p.HasNorthWall && !p.HasEastWall));
		candidates.AddRange(Pieces.FindAll(p => p.HasWestWall && !p.HasNorthWall && !p.HasEastWall && !p.HasSouthWall));
		if (!includeCorners)
		{
			int count = 0;
			count += candidates.RemoveAll(p => p.HasNorthWall && p.HasEastWall);
			count += candidates.RemoveAll(p => p.HasEastWall && p.HasSouthWall);
			count += candidates.RemoveAll(p => p.HasSouthWall && p.HasWestWall);
			count += candidates.RemoveAll(p => p.HasWestWall && p.HasNorthWall);
		}
		return candidates;
	}
	public List<MapPiece> GetOutsideWallsOnFloor(int floor, bool includeCorners = false)
	{
		List<MapPiece> candidates = GetOutsideWalls(includeCorners);
		candidates.RemoveAll(p => p.Coord.y != floor);
		return candidates;
	}


	public bool GetOuterWallFreeNeighbour(out MapPiece neighbour, out MAPDIRECTION dir, bool includeCorners = false)
	{
		int aHeight = ResolveAttachHeight();


		// Confirmed
		List<MapPiece> candidates = GetOutsideWallsOnFloor(aHeight, includeCorners);

		dir = MAPDIRECTION.ANY;
		int breaker = 20;
		while (breaker > 0 && candidates.Count > 0)
		{
			int idx = RNG.Next(candidates.Count);
			neighbour = candidates[idx].Neighbour(candidates[idx].OutsideWallDirection(), true);
			if (neighbour.isEmpty)
			{
				// Found empty valid neighbor
				dir = candidates[idx].OutsideWallDirection();
				return true;
			}
			breaker--;
		}
		neighbour = null;
		return false;
	}

	private int ResolveAttachHeight()
	{
		switch (AttachHeight)
		{
			case ATTACHHEIGHT.RANDOM:
				return RNG.Next(sizeY);
			case ATTACHHEIGHT.BOTTOM:
				return minY;
			case ATTACHHEIGHT.CENTER:
				return minY + Mathf.FloorToInt((maxY - minY) * 0.5f);
			case ATTACHHEIGHT.TOP:
				return maxY;
		}
		return Coord.y;
	}
	private void AdjustSectionHeight(int y)
	{
		switch (GrowHeight)
		{
			case ATTACHHEIGHT.RANDOM:
				int offset = RNG.Next(sizeY);
				minY = y - offset;
				maxY = y + sizeY - 1 - offset;
				return;
			case ATTACHHEIGHT.CENTER:
				minY = y - Mathf.FloorToInt(sizeY * 0.5f);
				maxY = y + Mathf.FloorToInt(sizeY * 0.5f);
				return;
			case ATTACHHEIGHT.TOP:
				minY = y - sizeY + 1;
				maxY = y;
				return;
		}
		minY = y;
		maxY = y + sizeY - 1;
	}

	/// <summary>
	/// Set min max based on arguments
	/// </summary>
	/// <param name="growLocation"></param>
	private protected void SetMinMaxCoord(MapCoordinate growLocation)
	{
		switch (orientation)
		{
			case MAPDIRECTION.NORTH:
				minX = growLocation.x - Mathf.FloorToInt(sizeX * 0.5f);
				maxX = minX + sizeX - 1;
				maxZ = growLocation.z;
				minZ = maxZ - sizeZ + 1;
				break;
			case MAPDIRECTION.SOUTH:
				maxX = growLocation.x + Mathf.FloorToInt(sizeX * 0.5f);
				minX = maxX - sizeX + 1;
				minZ = growLocation.z;
				maxZ = minZ + sizeZ - 1;
				break;
			case MAPDIRECTION.EAST:
				minX = growLocation.x;
				maxX = minX + sizeX - 1;
				minZ = growLocation.z - Mathf.FloorToInt(sizeZ * 0.5f);
				maxZ = minZ + sizeZ - 1;
				break;
			case MAPDIRECTION.WEST:
				maxX = growLocation.x;
				minX = maxX - sizeX + 1;
				maxZ = growLocation.z + Mathf.FloorToInt(sizeZ * 0.5f);
				minZ = maxZ - sizeZ + 1;
				break;
		}

		/*if (sizeX % 2 == 0 && orientation == MAPDIRECTION.SOUTH)
		{
			minX -= 1;
			maxX -= 1;
		}

		if (sizeX % 2 == 0 && orientation == MAPDIRECTION.WEST)
		{
			minZ -= 1;
			maxZ -= 1;
		}*/
		AdjustSectionHeight(growLocation.y);
	}



	/// <summary>
	/// Finds the furthest pieces to describe a square that the section covers<br/>
	/// Make sure to do this after build to get correct values
	/// </summary>
	private protected void SetMinMaxCoord()
	{
		MapPiece startPiece = Pieces.First();

		minX = startPiece.Coord.x;
		maxX = startPiece.Coord.x;
		minZ = startPiece.Coord.z;
		maxZ = startPiece.Coord.z;
		minY = startPiece.Coord.y;
		maxY = startPiece.Coord.y;

		foreach (MapPiece piece in Pieces)
		{
			if (piece.Coord.x < minX) { minX = piece.Coord.x; }
			if (piece.Coord.x > maxX) { maxX = piece.Coord.x; }

			if (piece.Coord.z < minZ) { minZ = piece.Coord.z; }
			if (piece.Coord.z > maxZ) { maxZ = piece.Coord.z; }

			if (piece.Coord.y < minY) { minY = piece.Coord.y; }
			if (piece.Coord.y > maxY) { maxY = piece.Coord.y; }
		}
	}

	/// <summary>
	/// Todo rewrite this so it finds the furthest corner pieces and calculates which ones is closest to the center of them all
	/// Also add parameter so it can be done per section floor
	/// </summary>
	/// <returns></returns>
	private protected MapPiece GetCenterPiece()
	{
		MapPiece centerOfStartLine = GetCenterOfRow(Pieces[0], orientation);
		MapPiece centerOfRoom = GetCenterOfRow(centerOfStartLine, DungeonUtils.TwistLeft(orientation));
		return centerOfRoom;
	}
	private protected MapPiece GetCenterOfRow(MapPiece piece, MAPDIRECTION dir)
	{
		List<MapPiece> negP = new List<MapPiece>();
		List<MapPiece> posP = new List<MapPiece>() { piece };
		int breaker = 100;
		while (Pieces.Exists(p => p.Coord == piece.Coord + dir))
		{
			piece = Pieces.Find(p => p.Coord == piece.Coord + dir);
			posP.Add(piece);
			breaker--; if (breaker < 1)
			{
				break;
			}
		}

		breaker = 100;
		dir = DungeonUtils.Flip(dir);
		piece = posP.First();
		while (Pieces.Exists(p => p.Coord == piece.Coord + dir))
		{
			piece = Pieces.Find(p => p.Coord == piece.Coord + dir);
			negP.Add(piece);
			breaker--; if (breaker < 1)
			{
				GD.Print($"RoomBase::GetCenterOfRow() negP iteration exceeded allowed part of eternity");
				break;
			}
		}


		if (negP.Count > 0) { negP.Reverse(); }

		negP.AddRange(posP);

		if (negP.Count > 1)
		{
			return negP[Mathf.FloorToInt(negP.Count * 0.5)];
		}
		return posP[0];
	}


	/*public virtual void AddProp(SectionProp pData)
	{
		Props.Add(pData);
	}

	public virtual bool AddPropOnRandomTile(KeyData keyData, out MapPiece pick)
	{
		throw new NotImplementedException();
	}
	*/

	public virtual MapPiece GetRandomPiece()
	{
		return Pieces[rng.Next(0, Pieces.Count)];
	}

	public virtual MapPiece GetRandomFloor()
	{
		if (!Pieces.Exists(p => p.hasFloor))
		{
			GD.PushError($"SectionBase::GetRandomFloor() Section[{sectionIndex}] has no Floors!");
			return null;
		}
		MapPiece pick = Pieces[rng.Next(0, Pieces.Count)];
		while (!pick.hasFloor) { pick = Pieces[rng.Next(0, Pieces.Count)]; }
		return pick;
	}



	public virtual bool IsInside(Vector3 worldPosition)
	{
		MapCoordinate coord = DungeonUtils.GlobalSnapCoordinate((Vector3I)worldPosition);
		if (Pieces.Exists(p => p.Coord == coord))
		{
			if (Pieces.Find(p => p.Coord == coord).isEmpty)
			{
				GD.PushError($"SectionBase::IsInside() Empty piece inside section!");
			}
			return true;
		}
		return false;
	}


	/// <summary>
	/// Make depth and width consistent relative to section orientation
	/// </summary>
	private void ResolveWidthDepth()
	{
		if (orientation != MAPDIRECTION.NORTH && orientation != MAPDIRECTION.SOUTH)
		{
			int d = sizeZ;
			sizeZ = sizeX;
			sizeX = d;
		}
	}

	public virtual MapPiece RemovePiece(MapCoordinate coord)
	{
		MapPiece mp = map.GetExistingPiece(coord);
		pieces.Remove(mp);
		mp.RemoveSection(sectionIndex);
		return mp;
	}

	public bool ContainsPiece(MapCoordinate coord)
	{
		if (Pieces.Exists(p => p.Coord == coord)) { return true; }
		return false;
	}

	private protected virtual void FitSmallArches()
	{
		foreach (MapPiece mp in Pieces)
		{
			FitSmallArch(mp);
		}
	}
	private protected void FitSmallArch(MapPiece piece)
	{
		if (!piece.hasCeiling) { return; }
		// add small arches
		if (piece.HasNorthWall)
		{
			piece.AddExtra(new KeyData() { key = PIECEKEYS.ARCH, dir = MAPDIRECTION.NORTH, variantID = 0 });
		}
		if (piece.HasEastWall)
		{
			piece.AddExtra(new KeyData() { key = PIECEKEYS.ARCH, dir = MAPDIRECTION.EAST, variantID = 0 });
		}
		if (piece.HasSouthWall)
		{
			piece.AddExtra(new KeyData() { key = PIECEKEYS.ARCH, dir = MAPDIRECTION.SOUTH, variantID = 0 });
		}
		if (piece.HasWestWall)
		{
			piece.AddExtra(new KeyData() { key = PIECEKEYS.ARCH, dir = MAPDIRECTION.WEST, variantID = 0 });
		}
	}

	/// <summary>
	/// If the passed piece is pending, It is added to section and all its UNUSED<br/>
	/// neighbors are added the the pieces List for processing.<br/>
	/// That is if they are inside the min/max space of the section
	/// </summary>
	/// <param name="mp"></param>
	private protected void ProcessPiece(MapPiece mp)
	{
		if (mp.State != MAPPIECESTATE.PENDING)
		{
			return;
		}
		mp.Orientation = orientation;
		mp.AddSection(sectionIndex);

		// Do all MAPDIRECTIONs
		for (int i = 1; i < 7; i++)
		{
			MAPDIRECTION processingDirection = (MAPDIRECTION)i;
			MapPiece nb = mp.Neighbour(processingDirection, true);
			if (nb.State == MAPPIECESTATE.UNUSED)
			{
				if (nb.Coord.x >= minX && nb.Coord.x <= maxX
					&& nb.Coord.y >= MinY && nb.Coord.y <= MaxY
					&& nb.Coord.z >= minZ && nb.Coord.z <= maxZ
					)
				{
					// Expand room to tile if within limits
					nb.State = MAPPIECESTATE.PENDING;
					nb.AddSection(sectionIndex);
					nb.sectionFloor = Math.Abs(nb.Coord.y - pieces.First().Coord.y);
					pieces.Add(nb);
					map.SavePiece(nb);
				}
			}
		}
		mp.State = MAPPIECESTATE.LOCKED;
		map.SavePiece(mp);
	}


	public void AddConnection(int id)
	{
		if (id < 1)
		{
			GD.PushError($"SectionBase::AddConnection({id}) INVALID ID!\n{System.Environment.StackTrace}");
		}

		if (!connections.Exists(p => p == id)) { connections.Add(id); }
	}
}// EOF CLASS