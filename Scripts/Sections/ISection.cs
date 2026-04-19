// Gone through at v1.3
using Godot;
using MDunGen.Commons;
using System;
using System.Collections.Generic;

namespace MDunGen.Sections;

public interface ISection
{
	/// <summary>
	/// Assigned section index on creation
	/// </summary>
	public int SectionIndex { get; }
	/// <summary>
	/// Assigned level index on creation
	/// </summary>
	public int LevelIndex { get; }
	/// <summary>
	/// The name of the section. That should be unique for that resource
	/// </summary>
	public string SectionName { get; }
	/// <summary>
	/// Total count of all map pieces in the section (include empty?)
	/// </summary>
	public int TileCount { get; }

	/// <summary>
	/// All the pieces in the section
	/// Should only be accessed when we create visuals
	/// </summary>
	public List<MapPiece> Pieces { get; }
	/// <summary>
	/// Grows the section into the current MapData
	/// </summary>
	public void Build(Action<BuildLogEventArgument> log);


	public bool AddOpening(MapCoordinate coord, MAPDIRECTION dir, bool wide, bool overrideLocked, Action<BuildLogEventArgument> log);
	/// <summary>
	/// Add inner prop to section
	/// </summary>
	/// <param name="rp"></param>
	/// <param name="pData"></param>
	//[Obsolete]
	//public void AddProp(SectionProp pData);
	//[Obsolete]
	//public bool AddPropOnRandomTile(KeyData keyData, out MapPiece pick);


	/// <summary>
	/// Resolve the worldPosition to closest mapCoordinate and return true if it is part of section
	/// </summary>
	/// <param name="worldPosition"></param>
	/// <returns></returns>
	/// <exception cref="NotImplementedException"></exception>
	public bool IsInside(Vector3 worldPosition);

	public List<int> Connections { get; }

	//public List<SectionProp> Props { get; }
	public MapPiece GetRandomPiece();
	public MapPiece GetRandomFloor();

	//public void PunchBackDoor();
	public void PunchBackDoor() { }

	public MapCoordinate Coord { get; }
	public ROOMCONNECTIONRESPONCE defaultConnectionResponses { get; }
	public bool BridgeAllowed { get; }
	public bool DoorAllowed { get; }

	public bool PlaceArches { get; }
	public List<MapPiece> GetWallPieces(int floor, bool includeCorners = false);


	/// <summary>
	/// Puts wall,floor and ceiling keys against other sections
	/// </summary>
	public void SealSection(int wallVariant = 0, int floorVariant = 0, int ceilingVariant = 0);



	/// <summary>
	/// Assign placers to the section. If placersOverride is valid it will override the SectionResource placers collection
	/// </summary>
	/// <param name="sectionDef"></param>
	/// <param name="placersOverride"></param>
	//void AssignPlacer(SectionResource sectionDef, Array<PlacerEntryResource> placersOverride);


	/// <summary>
	/// Removes the piece from section.<br/>
	/// Should basically never happen
	/// </summary>
	/// <param name="coord"></param>
	/// <returns></returns>
	MapPiece RemovePiece(MapCoordinate coord);

	/// <summary>
	/// Checks both owned and extra pieces for the given coordinate. If it exist, returns true;
	/// </summary>
	/// <param name="parentCoord"></param>
	/// <returns></returns>
	bool ContainsPiece(MapCoordinate parentCoord);
	/// <summary>
	/// Returns the empty neighbor and the direction away from section if successful
	/// </summary>
	/// <param name="neighbour"></param>
	/// <param name="dir"></param>
	/// <param name="includeCorners"></param>
	/// <returns></returns>
	bool GetOuterWallFreeNeighbour(out MapPiece neighbour, out MAPDIRECTION dir, bool includeCorners = false);


	public int ConnectionCount { get; }

	public Node3D SectionContainer { get; set; }
	public MapCoordinate MaxCoord { get; }
	public MapCoordinate MinCoord { get; }
	public PRNGMarsenneTwister RNG { get; }
	//public Array<PlacerEntryResource> Placers { get; }

	public MAPDIRECTION Orientation { get; }

	public float WaterLevel { get; }
	public float WaterDepth { get; }
	public Material WaterMaterial { get; }



	public ATTACHHEIGHT GrowHeight { get; }
	public ATTACHHEIGHT AttachHeight { get; }

	public void AddConnection(int connectionIndex);

}// EOF INTERFACE