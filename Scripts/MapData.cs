// Gone through at v1.3
using Godot;
using MDunGen.Commons;
using MDunGen.Resources;
using MDunGen.Sections;
using MDunGen.Builder;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MDunGen;
/// <summary>
/// On instantiation this sets itself up to be used. Call GenerateMap() to generate dungeon data. Use its callback to
/// do what you want with it.
/// </summary>
public class MapData
{
	private Dictionary<int, Dictionary<int, Dictionary<int, MapPiece>>> pieces;
	private MapDesignResource design;
	private List<ISection> sections;
	private Dictionary<int, SectionConnection> connections;

	public List<ISection> Sections => sections;
	public Dictionary<int, SectionConnection> Connections { get => connections; }
	internal MapDesignResource Design => design;

	internal Dictionary<int, Dictionary<int, Dictionary<int, MapPiece>>> Pieces => pieces;
	//internal int nbOfPieces => pieces.Values.SelectMany(p => p.Values).Distinct().SelectMany(p => p.Values).Distinct().Count();
	MapBuilder builder;

	private Action<BuildLogEventArgument> log;

	internal BiomeResource DefaultBiome => (BiomeResource)design.defaultBiome;
	internal int Levels => design.nbOfLevel;

	internal MapData(MapDesignResource design, ulong[] seed, Action<BuildLogEventArgument> log)
	{
		this.log = log;
		sections = new List<ISection>();
		connections = new Dictionary<int, SectionConnection>();
		pieces = new Dictionary<int, Dictionary<int, Dictionary<int, MapPiece>>>();
		this.design = design;
		builder = new MapBuilder(this, seed, log);
	}
	/*internal MapData(MapDesignResource args, ulong[] seed, BuildLevel floor, Action<BuildLogEventArgument> log)
    {
		this.log=log;
        sections = new List<ISection>();
        connections = new Dictionary<int, SectionConnection>();
        pieces = new Dictionary<int, Dictionary<int, Dictionary<int, MapPiece>>>();
        design = args;
		//rng = new PRNGMarsenneTwister(MapArgs.MasterSeed);
        builder = new MapBuilder(this, seed, log);
    }*/
	/// <summary>
	/// This generates the data that describes the layout of the dungeon
	/// When done it calls back
	/// </summary>
	/// <param name="callback"></param>
	/// <returns></returns>
	internal async Task GenerateMap(Action callback, bool debug)
	{
		log.Invoke(new BuildLogEventArgument() { source = "MapData: GenerateMap()", message = "Generating map......" });
		await builder.Build(design, debug);
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
	/// Makes one if needed and then return the piece.
	/// </summary>
	/// <param name="coord"></param>
	/// <returns></returns>
	internal MapPiece GetPiece(MapCoordinate coord, bool verbose = false)
	{
		if (pieces == null) { pieces = new Dictionary<int, Dictionary<int, Dictionary<int, MapPiece>>>(); }
		if (!pieces.Keys.Contains(coord.x)) { pieces[coord.x] = new Dictionary<int, Dictionary<int, MapPiece>>(); }
		if (!pieces[coord.x].Keys.Contains(coord.y)) { pieces[coord.x][coord.y] = new Dictionary<int, MapPiece>(); }
		if (!pieces[coord.x][coord.y].Keys.Contains(coord.z))
		{
			if (verbose) { GD.PrintErr($"VerifyPieceSpace", $"insert blank piece [{coord.x}.{coord.y}.{coord.z}]"); }
			pieces[coord.x][coord.y][coord.z] = new MapPiece(this, coord);
		}
		return pieces[coord.x][coord.y][coord.z];
	}




	/*public MapPiece GetNextPieceOver(MapPiece startPiece, MAPDIRECTION orientation)
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
	}*/

	#region Connection and Opening related
	internal bool AddNewConnection(int fromSectionIndex, int toSectionIndex, MapCoordinate fromLocation, MapCoordinate toLocation, MAPDIRECTION dir, out int id)
	{
		id = Connections.Keys.Count + 1;
		connections[id] = new SectionConnection(id, fromLocation, fromSectionIndex, dir);
		connections[id].ConnectedLocations.Add(new ConnectedLocation(toSectionIndex, id, toLocation, 1));
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
	internal bool GetConnection(MapCoordinate coord, out SectionConnection conn)
	{
		conn = null;
		foreach (KeyValuePair<int, SectionConnection> c in Connections)
		{
			if (c.Value.coord == coord)
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
		sections[p1.MainSection].AddOpening(p1.Coord, connection.Dir, false, overrideLocked, log);
	}
	#endregion
}// EOF CLASS
