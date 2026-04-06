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
	private protected readonly MapData map;
	/// <summary>
	/// Assigned section index on creation
	/// </summary>
	private protected readonly int sectionIndex;
	/// <summary>
	/// Level the section was grown on
	/// </summary>
	private protected readonly int levelIndex;
	/// <summary>
	/// What type of section is this. Room, Corridor, Void, Pantry?
	/// </summary>
	private protected string sectionStyle = string.Empty;
	/// <summary>
	/// The section name. Might not be unique in the Dungeon
	/// </summary>
	private protected string sectionName = string.Empty;

	private protected List<int> connections;
	public List<int> Connections => connections;
	public int ConnectionCount => connections.Count;

	public MAPDIRECTION Orientation => orientation;


	private protected List<MapPiece> pieces;
	private protected List<MapCoordinate> extraPieces;



	//private SectionProps props;
	//internal SectionProps PropGrid => props;
	//public List<SectionProp> Props => props.props;

	private protected MapCoordinate coord;
	private protected int MinY => minY;
	private protected int MaxY => maxY;
	public MapCoordinate Coord => coord;


	//private Array<PlacerEntryResource> placers;
	//public Array<PlacerEntryResource> Placers => placers;

	#endregion

	#region Needs some work
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
	public MapCoordinate MaxCoord => new MapCoordinate(maxX, MaxY - 1, maxZ);
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
	public string SectionStyle => sectionStyle;
	public string SectionName => sectionName;
	public virtual int TileCount => Pieces.Count;
	public virtual List<MapPiece> Pieces => pieces;
	public virtual List<MapCoordinate> ExtraPieces => extraPieces;
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
		extraPieces = new List<MapCoordinate>();
		connections = new List<int>();
		rng = new PRNGMarsenneTwister(args.Seed);
		//props = new SectionProps(this, args.Seed);

		map = args.map;

		sectionDefinition = args.sectionDefinition;
		levelIndex = args.levelIndex;
		sectionIndex = args.sectionID;

		waterMaterial = args.sectionDefinition.waterMaterial;
		waterDepth = args.sectionDefinition.waterDepth;
		waterLevel = args.sectionDefinition.waterLevel;

		sectionStyle = sectionDefinition.sectionStyle;
		sectionName = sectionDefinition.sectionName;
		coord = args.piece.Coord;
		defaultConnectionResponses = sectionDefinition.defaultResponses;

		orientation = args.piece.Orientation;
		if (orientation == MAPDIRECTION.ANY) { orientation = (MAPDIRECTION)rng.Next(1, 5); }

		sectionDefinition.VerifyValues();
		sizeX = rng.Next(sectionDefinition.sizeWidthMin, sectionDefinition.sizeWidthMax + 1);
		sizeZ = rng.Next(sectionDefinition.sizeDepthMin, sectionDefinition.sizeDepthMax + 1);
		sizeY = rng.Next(sectionDefinition.nbFloorsMin, sectionDefinition.nbFloorsMax + 1);

		ResolveWidthDepth();
		SetMinMaxCoord(coord);

		/*if (args.sectionDefinition.placers != null)
		{
			placers = args.sectionDefinition.placers;
		}
		else
		{
			placers = new Array<PlacerEntryResource>();
		}*/
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
			/*if (!piece.hasFloor)
			{
				StairPlacer stairCase = new StairPlacer(this, piece, dir);
				if (stairCase.isValid)
				{
					stairCase.Build();
				}
			}*/
		}
		else
		{
			piece.AssignWall(new KeyData() { key = PIECEKEYS.WD, dir = dir }, overrideLocked);
			if (!piece.hasFloor)
			{

				/*StairPlacer stairCase = new StairPlacer(this, piece, dir);
				if (stairCase.isValid)
				{
					stairCase.Build();
				}*/


			}
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

	public bool GetOuterWallFreeNeighbour(out MapPiece neighbour, out MAPDIRECTION dir, bool includeCorners = false, bool onlySectionGroundFloor = true)
	{
		// Confirmed
		List<MapPiece> candidates = onlySectionGroundFloor ? GetOutsideWallsOnFloor(Coord.y) : GetOutsideWalls(includeCorners);
		dir = MAPDIRECTION.ANY;
		int breaker = 20;
		while (breaker > 0 && candidates.Count > 0)
		{
			int idx = RNG.Next(candidates.Count);
			neighbour = candidates[idx].Neighbour(candidates[idx].OutsideWallDirection(), true);
			if (neighbour.isEmpty) { dir = candidates[idx].OutsideWallDirection(); return true; }
			breaker--;
		}
		neighbour = null;
		return false;
	}


	private protected void SetMinMaxCoord(MapCoordinate center)
	{
		int XOffset = 0;
		int ZOffset = 0;
		if (sizeX % 2 != 0) { XOffset = 1; }
		if (sizeZ % 2 != 0) { ZOffset = 1; }
		switch (orientation)
		{
			case MAPDIRECTION.NORTH:
				minX = center.x - (int)(sizeX * 0.5f) + 1 - XOffset;
				maxX = center.x + (int)(sizeX * 0.5f);
				minZ = center.z - sizeZ + 1;
				maxZ = center.z;
				break;
			case MAPDIRECTION.SOUTH:
				minX = center.x - (int)(sizeX * 0.5f) + XOffset;
				maxX = center.x + (int)(sizeX * 0.5f);
				minZ = center.z;
				maxZ = center.z + sizeZ - 1;
				break;
			case MAPDIRECTION.EAST:
				minX = center.x;
				maxX = center.x + sizeX - 1;
				minZ = center.z - (int)(sizeZ * 0.5f) + 1 - ZOffset;
				maxZ = center.z + (int)(sizeZ * 0.5f);
				break;
			case MAPDIRECTION.WEST:
				minX = center.x - sizeX + 1;
				maxX = center.x;
				minZ = center.z - (int)(sizeZ * 0.5f) - ZOffset;
				maxZ = center.z + (int)(sizeZ * 0.5f) - 1;
				break;
		}
		minY = center.y;
		maxY = center.y + sizeY;
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
	public void AddConnection(int id)
	{
		if (id < 1)
		{
			GD.PushError($"SectionBase::AddConnection({id}) INVALID ID!\n{System.Environment.StackTrace}");
		}

		if (!connections.Exists(p => p == id)) { connections.Add(id); }
	}
	/// <summary>
	/// Checks for pre-existing connection and add it. Otherwise create a new one.
	/// </summary>
	/// <param name="dir"></param>
	/// <param name="coord"></param>
	/// <param name="overrideLocked"></param>
	public int AddConnection(MAPDIRECTION dir, ISection otherSection, MapCoordinate location, MapCoordinate otherLocation, bool overrideLocked)
	{
		// Create a new connection
		if (map.AddNewConnection(this, otherSection, location, otherLocation, dir, out int cID))
		{
			AddConnection(cID);
			return cID;
		}
		return -1;
	}

	public int AddInverseConnection(MAPDIRECTION dir, ISection otherSection, MapCoordinate location, MapCoordinate otherLocation, bool overrideLocked)
	{
		// Create a new connection
		if (map.AddNewConnection(otherSection, this, otherLocation, location, DungeonUtils.Flip(dir), out int cID))
		{
			AddConnection(cID);
			return cID;
		}
		return -1;
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

	public virtual void RemovePiece(MapCoordinate coord, int newSectionOwner = -1)
	{
		MapPiece mp = map.GetExistingPiece(coord);
		pieces.Remove(mp);
		mp.SectionIndex = newSectionOwner;
		extraPieces.Add(coord);
	}

	public bool ContainsPiece(MapCoordinate coord)
	{
		if (ExtraPieces.Contains(coord)) { return true; }
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
}// EOF CLASS