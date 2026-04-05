// Gone through at v1.3
using Godot;
using MDunGen.Commons;
using MDunGen.Resources;
using MDunGen.Sections;
using MDunGen.Builder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MDunGen;
/// <summary>
/// On instantiation this sets itself up to be used. Call GenerateMap() to generate dungeon data. Use its callback to
/// do what you want with it.
/// </summary>
public class MapData
{
    private System.Collections.Generic.Dictionary<int, System.Collections.Generic.Dictionary<int, System.Collections.Generic.Dictionary<int, MapPiece>>> pieces;
    private GenerationSettingsResource mapArgs;
    private FloorResource floor;
    internal SectionResource startRoom;
    internal SectionResource standardRoom;
    private List<ISection> sections;
    private System.Collections.Generic.Dictionary<int, SectionConnection> connections;
    //private PRNGMarsenneTwister rng;

    public List<ISection> Sections => sections;
    public System.Collections.Generic.Dictionary<int, SectionConnection> Connections { get => connections; }
    public GenerationSettingsResource MapArgs => mapArgs;

    internal System.Collections.Generic.Dictionary<int, System.Collections.Generic.Dictionary<int, System.Collections.Generic.Dictionary<int, MapPiece>>> Pieces => pieces;
    internal int nbOfPieces => pieces.Values.SelectMany(p => p.Values).Distinct().SelectMany(p => p.Values).Distinct().Count(); // Würkz

	private Action<BuildLogEventArgument> log;

	public void LogBuildEventArgs(BuildLogEventArgument args)
	{
		log.Invoke(args);
	}
    internal MapData(GenerationSettingsResource args, Action<BuildLogEventArgument> log)
    {
		this.log=log;
        sections = new List<ISection>();
        connections = new System.Collections.Generic.Dictionary<int, SectionConnection>();
        pieces = new System.Collections.Generic.Dictionary<int, System.Collections.Generic.Dictionary<int, System.Collections.Generic.Dictionary<int, MapPiece>>>();
        mapArgs = args;
        builder = new MapBuilder(this, MapArgs.MasterSeed, log);
    }
    internal MapData(GenerationSettingsResource args, FloorResource floor, Action<BuildLogEventArgument> log)
    {
		this.log=log;
        sections = new List<ISection>();
        connections = new System.Collections.Generic.Dictionary<int, SectionConnection>();
        pieces = new System.Collections.Generic.Dictionary<int, System.Collections.Generic.Dictionary<int, System.Collections.Generic.Dictionary<int, MapPiece>>>();
        mapArgs = args;
		//rng = new PRNGMarsenneTwister(MapArgs.MasterSeed);
        builder = new MapBuilder(this, MapArgs.MasterSeed, log);
        this.floor = floor;
    }
    internal MapData(GenerationSettingsResource args, SectionResource startRoom, SectionResource standardRoom, Action<BuildLogEventArgument> log)
    {
		this.log=log;
        sections = new List<ISection>();
        connections = new System.Collections.Generic.Dictionary<int, SectionConnection>();
        pieces = new System.Collections.Generic.Dictionary<int, System.Collections.Generic.Dictionary<int, System.Collections.Generic.Dictionary<int, MapPiece>>>();
        mapArgs = args;
        this.startRoom = startRoom;
        this.standardRoom = standardRoom;
        builder = new MapBuilder(this, MapArgs.MasterSeed, log);
    }
    /// <summary>
    /// This generates the data that describes the layout of the dungeon
    /// When done it calls back
    /// </summary>
    /// <param name="callback"></param>
    /// <returns></returns>
    internal async Task GenerateMap(Action callback, bool doPathing)
    {
		log.Invoke(new BuildLogEventArgument(){source = "MapData: GenerateMap()", message = "Generating map......"});
        await builder.BuildFloor(0, MapArgs.floorDef, doPathing); // TODO Will only do bottom floor
        callback.Invoke();
    }
	MapBuilder builder;
    internal async Task GenerateFloor(int floorIndex, FloorResource floorDef, Action callback, bool doPathing)
    {
		log.Invoke(new BuildLogEventArgument(){source = "MapData: GenerateFloor()", message = "Generating floor......", levelIndex = floorIndex });
        await builder.BuildFloor(floorIndex, floorDef, doPathing);
        callback.Invoke();
    }

	internal async Task GenerateSection(int levelIndex, string sectionTypeName, ulong[] seed, SectionResource sectionDef, Action callback)
    {
		log.Invoke(new BuildLogEventArgument(){source = "MapData::GenerateSection()", message = $"Generation started..... defIsNull[{sectionDef is null}]"});
        await builder.BuildSection(levelIndex, sectionTypeName, sectionDef, seed);
        callback.Invoke();
    }

    internal void SavePiece(MapPiece piece)
    {
        pieces[piece.Coord.x][piece.Coord.y][piece.Coord.z] = piece;
    }
    internal MapPiece GetRandomPieceEditor()
    {
        ISection rngSection = Sections[GD.RandRange(0, Sections.Count - 1)];
        return rngSection.GetRandomPiece();
    }
    /// <summary>
    /// Uses piece verification and then return the piece.
    /// Will create piece if needed.
    /// </summary>
    /// <param name="coord"></param>
    /// <returns></returns>
    internal MapPiece GetPiece(MapCoordinate coord)
    {
        VerifyPiece(coord);
        return pieces[coord.x][coord.y][coord.z];
    }


    /// <summary>
    /// Get piece if it exists or return null
    /// Use this when iterating across map to not change map
    /// </summary>
    /// <param name="coord"></param>
    /// <returns></returns>
    internal MapPiece GetExistingPiece(MapCoordinate coord)
    {
        if (pieces.ContainsKey(coord.x))
        {
            if (pieces[coord.x].ContainsKey(coord.y))
            {
                if (pieces[coord.x][coord.y].ContainsKey(coord.z))
                {
                    return pieces[coord.x][coord.y][coord.z];
                }
            }
        }
        return null;
    }

    /// <summary>
    /// Returns a List of pieces with the state
    /// </summary>
    /// <param name="queriedState"></param>
    /// <returns></returns>
    internal List<MapPiece> GetPieces(MAPPIECESTATE queriedState)
    {
        List<MapPiece> picked = new List<MapPiece>();

        foreach (int keyX in pieces.Keys)
        {
            foreach (int keyY in pieces[keyX].Keys)
            {
                foreach (int keyZ in pieces[keyX][keyY].Keys)
                {
                    if (pieces[keyX][keyY][keyZ].State == queriedState)
                    {
                        picked.Add(pieces[keyX][keyY][keyZ]);
                    }
                }
            }
        }
        return picked;
    }

    /// <summary>
    /// Verifies piece instance exists. Makes one if needed
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="z"></param>
    /// <param name="verbose"></param>
    private void VerifyPiece(MapCoordinate coord, bool verbose = false)
    {
        if (pieces == null) { pieces = new System.Collections.Generic.Dictionary<int, System.Collections.Generic.Dictionary<int, System.Collections.Generic.Dictionary<int, MapPiece>>>(); }

        if (!pieces.Keys.Contains(coord.x)) { pieces[coord.x] = new System.Collections.Generic.Dictionary<int, System.Collections.Generic.Dictionary<int, MapPiece>>(); }
        if (!pieces[coord.x].Keys.Contains(coord.y)) { pieces[coord.x][coord.y] = new System.Collections.Generic.Dictionary<int, MapPiece>(); }
        if (!pieces[coord.x][coord.y].Keys.Contains(coord.z))
        {
            if (verbose) { GD.PrintErr($"VerifyPieceSpace", $"insert blank piece [{coord.x}.{coord.y}.{coord.z}]"); }

            pieces[coord.x][coord.y][coord.z] = new MapPiece(this, coord);
        }

    }




    public MapPiece GetNextPieceOver(MapPiece startPiece, MAPDIRECTION orientation)
    {

        switch (orientation)
        {
            case MAPDIRECTION.NORTH:
                startPiece = GetPiece(startPiece.Coord.StepNorth);
                break;
            case MAPDIRECTION.EAST:
                startPiece = GetPiece(startPiece.Coord.StepEast);
                break;
            case MAPDIRECTION.SOUTH:
                startPiece = GetPiece(startPiece.Coord.StepSouth);
                break;
            case MAPDIRECTION.WEST:
                startPiece = GetPiece(startPiece.Coord.StepWest);
                break;
            case MAPDIRECTION.UP:
                startPiece = GetPiece(startPiece.Coord.StepUp);
                break;
            case MAPDIRECTION.DOWN:
                startPiece = GetPiece(startPiece.Coord.StepDown);
                break;
        }
        return startPiece;
    }

    #region Functional to manipulate mappieces
    internal void AddDoorWide(MapPiece piece1, bool overrideLocked)
    {
        MapPiece piece2 = piece1.Neighbour(DungeonUtils.TwistRight(piece1.Orientation), true);
        piece1.AssignWall(new KeyData() { key = PIECEKEYS.OCCUPIED, dir = DungeonUtils.Flip(piece1.Orientation) }, overrideLocked);
        piece2.AssignWall(new KeyData() { key = PIECEKEYS.WDW, dir = DungeonUtils.Flip(piece1.Orientation) }, overrideLocked);
        piece1.Neighbour(DungeonUtils.Flip(piece1.Orientation), true).AssignWall(new KeyData() { key = PIECEKEYS.WDW, dir = piece1.Orientation }, overrideLocked);
        piece2.Neighbour(DungeonUtils.Flip(piece1.Orientation), true).AssignWall(new KeyData() { key = PIECEKEYS.OCCUPIED, dir = piece1.Orientation }, overrideLocked);
    }
    internal void MovePieceOwnershipToSection(MapPiece piece, int newOwnerSection)
    {
        ISection oldOwner = sections[piece.SectionIndex];
        oldOwner.RemovePiece(piece.Coord, newOwnerSection);
    }
    #endregion


    #region Connection and Opening related

    internal bool AddNewConnection(ISection fromSection, ISection toSection, MapCoordinate fromLocation, MapCoordinate toLocation, MAPDIRECTION dir, out int id)
    {
        id = Connections.Keys.Count + 1;
        connections[id] = new SectionConnection(id, fromLocation, fromSection.SectionIndex, dir);
        connections[id].ConnectedLocations.Add(new ConnectedLocation(toSection.SectionIndex, id, toLocation, 1));
        return true;
    }
    internal bool GetConnection(int sectionID, MapCoordinate coord, out SectionConnection conn)
    {
        conn = null;
        foreach (KeyValuePair<int, SectionConnection> c in Connections)
        {
            if (c.Value.coord == coord && c.Value.sectionID == sectionID)
            {
                conn = c.Value;
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Adds opening between to locations
    /// </summary>
    /// <param name="connection"></param>
    /// <param name="overrideLocked"></param>
    internal void AddOpeningBetweenSections(SectionConnection connection, bool overrideLocked)
    {
        MapPiece p1 = GetExistingPiece(connection.coord);
        sections[p1.SectionIndex].AddOpening(p1.Coord, connection.Dir, false, overrideLocked, log);
    }






    #endregion
}// EOF CLASS
