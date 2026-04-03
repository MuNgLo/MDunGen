// Gone through at v1.3
#if TOOLS
using Godot;
using Godot.Collections;
using MDunGen.Resources;
using System;

namespace MDunGen.MS;

/// <summary>
/// The main screen window center editor to generate/view dungeon data
/// </summary>
[Tool, GlobalClass]
public partial class MainScreen : Control
{
	/// <summary>
	/// Use this to only react to input if cursor is over screen
	/// </summary>
	public bool cursorIsInside = false;
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
	public MapData Map { get => dunVis.Map; }

	public override void _EnterTree()
	{
		dunVis = GetNode<ScreenDungeonVisualizer>("SubViewportContainer/SubViewport/Dungeon");
		selection = new Selection.Manager(addon, this, dunVis);
	}

	public override void _Ready()
	{
		//GD.Print($"DirExistsAbs [{DirAccess.DirExistsAbsolute(addon.MasterConfig.ProjectResourcePath)}]");

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
	/// <param name="settings"></param>
	/// <param name="biome"></param>
	public void GenerateDungeon(GenerationSettingsResource settings, BiomeResource biome)
	{
		dunVis.BuildDungeon(settings, settings.floorDef, biome);
	}
	public void GenerateSection(string sectionTypeName, SectionResource sectionDef, GenerationSettingsResource settings, BiomeResource biome)
	{
		RaiseNotification($"Building Section {sectionDef.sectionName}");
		Array<PlacerEntryResource> placers = sectionDef.placers;
		GD.Print($"MainScreen::GenerateSection() defIsNull[{sectionDef is null}] placers is Null[{placers is null}]");
		dunVis.BuildSection(sectionTypeName, sectionDef, settings.MasterSeed, settings, biome, ReDrawDungeon);
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