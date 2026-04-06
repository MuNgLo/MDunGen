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
	[Export] private bool spawnInOnLaunch = false;
	[Export] private Vector3 globalOffset = Vector3.Zero;
	[Export] private GenerationSettingsResource dunSettings;
	[Export] private DungeonVisualizer visualizer;
	internal Dictionary<int, Dictionary<int, Dictionary<int, MapPiece>>> Pieces => map.Pieces;
	internal List<MapPiece> PendingPieces => map.GetPieces(MAPPIECESTATE.PENDING);
	internal List<MapPiece> LockedPieces => map.GetPieces(MAPPIECESTATE.LOCKED);
	internal MapData Map => map;

	private MapData map;

	public event EventHandler<BuildLogEventArgument> OnMapBuildLog;

	public override void _Ready()
	{
		DungeonUtils.globalOffset = globalOffset;
		visualizer.ClearVisualizer();
		if (spawnInOnLaunch) { GenerateMapData(dunSettings); }
	}

	private void GenerateMapData(GenerationSettingsResource dunSettings)
	{
		Log("Dungeon : Generating layout....");
		BuildMapData(dunSettings, () => { GeneratedMapData(); });
	}
	private void GeneratedMapData()
	{
		Log($"Dungeon : Generation Completed #Pieces[{map.Pieces.Count}]");
		visualizer.ShowMap();
	}

	public async void BuildMapData(GenerationSettingsResource settings, Action callback)
	{
		if (settings is null)
		{
			Log($"Fail! settings is NULL[{settings is null}]");
			return;
		}
		map = new MapData(settings, RaiseBuildLogEvent);
		await map.GenerateMap(callback, settings.calculatePathing);
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