// Gone through at v1.3
#if TOOLS
using Godot;
using Godot.Collections;
using MDunGen.Commons;
using MDunGen.Design;
using MDunGen.Resources;
using System;
using System.Threading.Tasks;

namespace MDunGen.MS;

/// <summary>
/// The main screen window center editor to generate/view dungeon data
/// </summary>
[Tool, GlobalClass]
public partial class MainScreen : Control
{
	public Dungeons addon;
	private Selection.Manager selection;
	private ScreenDungeonVisualizer dunVis;

	public Node3D CurrentDungeon => GetNode<Node3D>("SubViewportContainer/SubViewport/Dungeon/Generated");
	public Node3D Gizmos => GetNode<Node3D>("SubViewportContainer/SubViewport/Gizmos");

	internal EventHandler OnMainScreenUIUpdate;
	internal EventHandler<string> OnNotificationPushed;
	internal EventHandler<Pathfinding.PathData> OnPathDataPushed;

	internal Selection.Manager Selection => selection;
	public ScreenDungeonVisualizer Visualizer { get => dunVis; }

	public override void _EnterTree()
	{
		dunVis = GetNode<ScreenDungeonVisualizer>("SubViewportContainer/SubViewport/Dungeon");
		selection = new Selection.Manager(addon, this, dunVis);
	}

	public override void _Ready()
	{
		if (addon.MasterConfig.ProjectResourcePath == string.Empty || !DirAccess.DirExistsAbsolute(addon.MasterConfig.ProjectResourcePath))
		{
			PopupInitialSettingsDialogue();
		}
		SetDebugLayer(addon.Profile.showDebugLayer);
		RaiseUpdateUI();
	}

	public void PopupInitialSettingsDialogue()
	{
		PackedScene pScn = ResourceLoader.Load("res://addons/MDunGen/Scenes/InitialPopup.tscn") as PackedScene;
		InitialPopup pop = pScn.Instantiate<InitialPopup>();
		pop.screen = this;
		AddChild(pop);
	}
	public void RaiseUpdateUI()
	{
		EventHandler evt = OnMainScreenUIUpdate;
		evt?.Invoke(this, EventArgs.Empty);
	}
	public void RaiseNotification(string message)
	{
		EventHandler<string> evt = OnNotificationPushed;
		evt?.Invoke(this, message);
	}
	/// <summary>
	/// Generates and display a dungeon in the viewer
	/// </summary>
	/// <param name="design"></param>
	/// <param name="biome"></param>
	internal async Task GenerateDungeon(MapDesignResource design, bool debug)
	{
		await BuildDungeon(design, addon.MasterConfig.MasterSeed, debug);
	}


	internal async Task BuildDungeon(MapDesignResource design, ulong[] seed, bool debug)
	{
		if (design is null)
		{
			GD.PrintErr($"MainScreen::BuildDungeon() BuildDungeonFailed! design is NULL[{design is null}]");
			return;
		}
		OnMapDataGenerationStarted?.Invoke(EventArgs.Empty, EventArgs.Empty);
		RaiseNotification($"Building Dungeon");
		this.design = design;
		map = new MapData(this.design, seed, RaiseBuildLogEvent);
		await map.GenerateMap(() => { dunVis.ReDrawMap();}, debug);
		OnMapDataGenerationEnded?.Invoke(EventArgs.Empty, EventArgs.Empty);
	}


	public void GenerateSection(int levelIndex, string sectionTypeName, SectionResource sectionDef, ulong[] seed, BiomeResource biome, bool debug)
	{
		RaiseNotification($"Building Section {sectionDef.sectionName}");
		BuildSection(levelIndex, sectionTypeName, sectionDef, seed, biome, debug, ReDrawDungeon);
	}
	public async void BuildSection(int levelIndex, string sectionTypeName, SectionResource sectionDef, ulong[] seed, BiomeResource biome, bool debug, Action callback)
	{
		this.design = null;
		RaiseNotification($"Generating:" + string.Format("{0:0}", 0) + "%");
		await ToSignal(GetTree(), "process_frame");
		map = new MapData(design, seed, RaiseBuildLogEvent);
		// TODO fix this so section mode uses the design approach
		//await map.GenerateSection(levelIndex, sectionTypeName, seed, sectionDef, debug, callback);
		OnMapDataGenerationEnded?.Invoke(EventArgs.Empty, EventArgs.Empty);
	}
	private MapData map;
	public MapData Map { get => map; }

	private MapDesignResource design;
	public event EventHandler OnMapDataGenerationStarted;
	public event EventHandler OnMapDataGenerationEnded;
	public event EventHandler<BuildLogEventArgument> OnMapBuildLog;

	public void RaiseBuildLogEvent(BuildLogEventArgument args)
	{
		EventHandler<BuildLogEventArgument> evt = OnMapBuildLog;
		evt?.Invoke(this, args);
	}
	/// <summary>
	/// Allow lookup of current MapData
	/// </summary>
	/// <param name="coord"></param>
	/// <returns></returns>
	public MapPiece GetMapPiece(MapCoordinate coord)
	{
		if (map is null || map.Pieces.Count == 0)
		{
			GD.PushError("Map data needs to be rebuilt");
			return null;
		}
		return map.GetExistingPiece(coord);
	}

















	public void ReDrawDungeon()
	{
		selection.ClearSelection();
		switch (addon.Mode)
		{
			case VIEWERMODE.SECTION:
				dunVis.ReDrawSection();
				selection.SelectFirstSection();
				break;
			case VIEWERMODE.DUNGEON:
			default:
				dunVis.ReDrawMap();
				break;
		}
	}
	/// <summary>
	/// Clears existing dungeon that is being viewed
	/// </summary>
	internal async void WhenClearPressed()
	{
		RaiseNotification("CLEARING...");
		await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
		dunVis.ClearLevel();
		dunVis.ClearLevelDebug();

		// WORKAROUND! TODO make this better??
		if (selection is null) { selection = new Selection.Manager(addon, this, dunVis); }
		selection.ClearSelection();
		RaiseNotification("CLEARED");
	}
	/// <summary>
	/// Sets the state of the debug information
	/// </summary>
	/// <param name="state"></param>
	internal void SetDebugLayer(bool state)
	{
		dunVis.SetDebugLayer(state);
	}
	internal void RaiseOnPathDataPushed(Pathfinding.PathData pathData)
	{
		OnPathDataPushed?.Invoke(this, pathData);
	}



}// EOF CLASS
#endif