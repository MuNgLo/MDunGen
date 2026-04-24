// Gone through at v1.3
#if TOOLS
using Godot;
using MDunGen.Commons;
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
	VIEWERMODE mode = VIEWERMODE.DUNGEON;
	Selection.Manager selection;
	ScreenDungeonVisualizer dunVis;
	EditorFileDialog popup;
	MapData map;

	MapDesignResource design;
	public event EventHandler OnMapDataGenerationStarted;
	public event EventHandler OnMapDataGenerationEnded;
	public event EventHandler<BuildLogEventArgument> OnMapBuildLog;
	internal EventHandler OnMainScreenUIUpdate;
	internal EventHandler<string> OnNotificationPushed;

	public VIEWERMODE Mode => mode;
	public MapData Map { get => map; }
	internal Selection.Manager Selection => selection;
	public Node3D Gizmos => GetNode<Node3D>("SubViewportContainer/SubViewport/Gizmos");
	public Node3D CurrentDungeon => GetNode<Node3D>("SubViewportContainer/SubViewport/Dungeon/Generated");
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
		await Task.Delay(10);

		if (design is null)
		{
			GD.PrintErr($"MainScreen::BuildDungeon() BuildDungeonFailed! design is NULL[{design is null}]");
			return;
		}
		OnMapDataGenerationStarted?.Invoke(EventArgs.Empty, EventArgs.Empty);
		RaiseNotification($"Building Dungeon [---]");
		this.design = design;
		map = new MapData(this.design, addon.MasterConfig.MasterSeed, RaiseBuildLogEvent);
		await map.GenerateMap(() => { dunVis.ReDrawMap();}, debug, UpdateProgress);
		OnMapDataGenerationEnded?.Invoke(EventArgs.Empty, EventArgs.Empty);
	}

	private void UpdateProgress(float normalizedProgression)
	{
		RaiseNotification($"Building Dungeon [{(int)(normalizedProgression * 100.0f)}%]");
	}

	public async void GenerateSection(int levelIndex, string sectionTypeName, SectionResource sectionDef, ulong[] seed, BiomeResource biome, bool debug)
	{
		RaiseNotification($"Building Section {sectionDef.sectionName}");
		this.design = null;
		RaiseNotification($"Generating:" + string.Format("{0:0}", 0) + "%");
		await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
		// TODO fix this so section mode uses the design approach

		BuildSection design = new BuildSection()
		{
			location = LOCATION.CENTER,
			section = sectionDef
		};

		MapDesignResource mapDesign = new MapDesignResource();
		mapDesign.SetSingleSectionDesign(design);

		map = new MapData(mapDesign, seed, RaiseBuildLogEvent);
		await map.GenerateMap(() => { dunVis.ReDrawMap();}, true, UpdateProgress);

		OnMapDataGenerationEnded?.Invoke(EventArgs.Empty, EventArgs.Empty);
	}


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
		switch (Mode)
		{
			case VIEWERMODE.SECTION:
				dunVis.ReDrawMap();
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
		dunVis.ClearAllLevels();
		dunVis.ClearAllLevelsDebug();

		// WORKAROUND! TODO make this better??
		if (selection is null) { selection = new Selection.Manager(addon, this, dunVis); }
		selection.ClearSelection();
		RaiseNotification("CLEARED");
	}


	/// <summary>
	/// Toggles mode between dungeon and section. Defaults to dungeon.
	/// </summary>
	public void ChangeMode()
	{
		ChangeMode(mode == VIEWERMODE.DUNGEON ? VIEWERMODE.SECTION : VIEWERMODE.DUNGEON);
	}
	public void ChangeMode(VIEWERMODE newMode)
	{
		if (newMode != mode)
		{
			mode = newMode;
			RaiseUpdateUI();
		}
	}

#region Export

	/// <summary>
	/// Export Dialogue
	/// </summary>
	public void ShowExportPopup()
	{
		popup = new EditorFileDialog
		{
			AlwaysOnTop = true,
			Title = "Export Dungeon",
			FileMode = EditorFileDialog.FileModeEnum.SaveFile,
			Access = EditorFileDialog.AccessEnum.Resources,
			PopupWindow = true,
			OkButtonText = "Save"
		};
		popup.Confirmed += WhenExportConfirmed;
		popup.WindowInput += WhenExportInput;
		popup.Size = GetViewport().GetWindow().Size / 2;
		EditorInterface.Singleton.GetBaseControl().GetViewport().AddChild(popup);
		popup.MoveToCenter();
		popup.Show();
	}

	private void WhenExportInput(InputEvent @event)
	{
		if(@event is InputEventKey key)
		{
			if(key.Pressed && (key.Keycode == Key.Enter || key.Keycode == Key.KpEnter))
			{
				WhenExportConfirmed();
			}
		}
	}
	/// <summary>
	/// Handle the export
	/// </summary>
	private void WhenExportConfirmed()
	{
		// Check things
		if (CurrentDungeon is null)
		{
			GD.PrintErr($"Dungeons::WhenExportConfirmed() CurrentDungeon Node was NULL!");
			return;
		}
		if (CurrentDungeon.GetChildCount() < 1)
		{
			GD.PrintErr($"Dungeons::WhenExportConfirmed() CurrentDungeon Node has no children to export!");
			return;
		}
		if(popup.GetLineEdit().Text.Length < 1)
		{
			return;
		}
		popup.Confirmed -= WhenExportConfirmed;
		PackedScene sceneToSave = new PackedScene();
		foreach (Node node in CurrentDungeon.GetChildren())
		{
			SetOwner(CurrentDungeon, node);
		}
		Error err = sceneToSave.Pack(CurrentDungeon);
		if (err != Error.Ok)
		{
			GD.PrintErr($"Dungeons::WhenExportConfirmed() err[{err}]");
		}
		sceneToSave.ResourcePath = popup.CurrentPath;
		if (!popup.CurrentPath.Contains(".tscn")) { sceneToSave.ResourcePath = sceneToSave.ResourcePath + ".tscn"; }
		ResourceSaver.Save(sceneToSave);
		popup.QueueFree();
	}
	/// <summary>
	/// Sets the owner of the node and all its children recursively 
	/// Skips children of scene instances
	/// </summary>
	/// <param name="Owner"></param>
	/// <param name="node"></param>
	private void SetOwner(Node Owner, Node node)
	{
		node.Owner = Owner;
		if (node.SceneFilePath != string.Empty) { return; }
		foreach (Node n in node.GetChildren())
		{
			SetOwner(CurrentDungeon, n);
		}
	}
#endregion

}// EOF CLASS
#endif