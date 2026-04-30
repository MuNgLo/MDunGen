// Gone through at v1.3
//#define MConsole // If MConsole is in the project, comment this out to get log messages pushed to it
using Godot;
using MDunGen.Commons;
using MDunGen.Resources;
using System;
using System.Collections.Generic;

namespace MDunGen;
/// <summary>
/// Dungeon runtime class
/// Use this node to generate the map data.
/// Use the DungeonVisualizer Node to see the data.
/// </summary>
[GlobalClass]
public partial class Dungeon : Node
{
	[Export] private bool debug = false;
	[Export] private bool spawnInOnConnect = true;
	[Export] private Vector3 globalOffset = Vector3.Zero;
	[Export] private MapDesignResource dunSettings;

	[ExportGroup("Seed")]
	[Export] public int seed1 = 1111;
	[Export] public int seed2 = 2222;
	[Export] public int seed3 = 3333;
	[Export] public int seed4 = 4444;

	DungeonVisualizer visualizer => Core.DungeonVisualizer;
	public ulong[] MasterSeed => new[] { (ulong)seed1, (ulong)seed2, (ulong)seed3, (ulong)seed4 };

	internal Dictionary<int, Dictionary<int, Dictionary<int, MapPiece>>> Pieces => map.Pieces;
	internal List<MapPiece> PendingPieces => map.GetPieces(MAPPIECESTATE.PENDING);
	internal List<MapPiece> LockedPieces => map.GetPieces(MAPPIECESTATE.LOCKED);
	internal MapData Map => map;

	private MapData map;

	public event EventHandler<BuildLogEventArgument> OnMapBuildLog;

	public override void _Ready()
	{
		Core.Lobby.LobbyEvents.OnHostSetupReady += (o, s) =>
		{
			if (spawnInOnConnect) { GenerateMapData(dunSettings, MasterSeed); }
		};
		Core.Lobby.LobbyEvents.OnConnectedToServer += (o, s) =>
		{
			if (spawnInOnConnect) { GenerateMapData(dunSettings, MasterSeed); }
		};
		DungeonUtils.globalOffset = globalOffset;
		visualizer.ClearVisualizer();
	}

	internal void GenerateMapData(MapDesignResource design, ulong[] seed)
	{
		//Log("Dungeon : Generating layout....");
		BuildMapData(design, seed, () => { MapDataReady(); }, debug);
	}
	private void MapDataReady()
	{
		//Log($"Dungeon : Generation Completed");
		visualizer.ShowMap();
	}

	internal async void BuildMapData(MapDesignResource design, ulong[] seed, Action callback, bool debug)
	{
		if (design is null)
		{
			Log($"Fail! settings is NULL[{design is null}]");
			return;
		}
		map = new MapData(design, seed, RaiseBuildLogEvent);
		await map.GenerateMap(callback, debug, UpdateProgress);
	}

	private void UpdateProgress(float normalizedProgression)
	{
		//GD.Print($"Building Dungeon [{(normalizedProgression * 100).ToString("00")}%]");
	}

	/// <summary>
	/// This is how the addon logs build messages in runtime
	/// </summary>
	/// <param name="msg"></param>
	private void Log(string msg)
	{
#if MConsole
		MConsole.GameConsole.AddLine(msg);
#else
		GD.Print(msg);
#endif
	}

	public void RaiseBuildLogEvent(BuildLogEventArgument args)
	{
		EventHandler<BuildLogEventArgument> evt = OnMapBuildLog;
		evt?.Invoke(this, args);
	}
}// EOF CLASS