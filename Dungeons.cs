// Gone through at v1.3
#if TOOLS
using Godot;
using MDunGen.Bottom;
using MDunGen.MS;
using MDunGen.MS.Selection;
using MDunGen.Resources;

namespace MDunGen;

[Tool]
public partial class Dungeons : EditorPlugin
{

	private VIEWERMODE mode = VIEWERMODE.DUNGEON;
	public VIEWERMODE Mode => mode;
	private readonly string screenName = "Dungeon";
	private AddonSettingsResource masterConfig;
	public AddonSettingsResource MasterConfig => masterConfig;
	private MainScreen screen;
	public MainScreen MS => screen;
	private BottomScreen bScreen;
	private CameraControls cam;
	private PackedScene mainPrefab = ResourceLoader.Load<PackedScene>("res://addons/MDunGen/Scenes/MainScreen.tscn");
	private PackedScene bottomPrefab = ResourceLoader.Load<PackedScene>("res://addons/MDunGen/Scenes/BottomScreen.tscn");
	public ProfileResource Profile = ResourceLoader.Load("res://addons/MDunGen/Config/def_profile.tres") as ProfileResource;
	private EditorFileDialog popup;
	#region Overrides
	public override void _EnterTree()
	{
		GD.Print("Loaded MDunGen Plugin");
		masterConfig = ResourceLoader.Load("res://addons/MDunGen/Config/def_master.tres") as AddonSettingsResource;

		// Center screen
		screen = (MainScreen)mainPrefab.Instantiate();
		screen.addon = this;
		// Add screen instance to the editor
		EditorInterface.Singleton.GetEditorMainScreen().AddChild(screen);
		// Hide the main panel. Very much required.
		_MakeVisible(false);

		cam = screen.FindChild("Camera3D") as CameraControls;

		// Bottom screen
		bScreen = (BottomScreen)bottomPrefab.Instantiate();
		bScreen.Name = "MDunGen";
		bScreen.addon = this;

		// Add bottom screen instance to the editor
		EditorDock bDock = new EditorDock() { DefaultSlot = EditorDock.DockSlot.Bottom };
		bDock.AddChild(bScreen);
		AddDock(bDock);
	}

	Button testBTN;
	private void RunDebugTestThings()
	{
		testBTN = new Button() { Text = "Debug" };
		AddControlToContainer(CustomControlContainer.SpatialEditorMenu, testBTN);
		testBTN.Pressed += DebugDumpToScene;
	}

	private void DebugDumpToScene()
	{
		PackedScene sceneToSave = new PackedScene();
		//GD.Print($"ExportConfirmed() O1[{screen.CurrentDungeon.GetChildren()[0].Owner}] O2[{screen.CurrentDungeon.GetChildren()[1].Owner}]");

		Node copy = testBTN.GetParent().GetParent().GetParent().GetParent().GetParent();


		Error err = sceneToSave.Pack(copy);
		if (err != Error.Ok)
		{
			GD.PrintErr($"Dungeons::DebugDumpToScene() err[{err}]");
		}
		//sceneToSave.ResourcePath = popup.CurrentPath;
		sceneToSave.ResourcePath = "DebugDump2.tscn";
		ResourceSaver.Save(sceneToSave);
	}

	public override void _ExitTree()
	{
		if (testBTN is not null)
		{
			RemoveControlFromContainer(CustomControlContainer.SpatialEditorMenu, testBTN);
		}
		//RemoveControlFromBottomPanel(bScreen);
		RemoveDock(bScreen.GetParent() as EditorDock);
		bScreen.GetParent().QueueFree();
		GD.Print("Unloaded MDunGen Plugin");

		// Release main screen UI
		// The other Side
	}
	public override bool _HasMainScreen()
	{
		return true;
	}
	public override string _GetPluginName()
	{
		return screenName;
	}
	public override Texture2D _GetPluginIcon()
	{
		Texture2D icon = ResourceLoader.Load("res://addons/MDunGen/Icons/AddonIcon.png") as Texture2D;
		return icon;
	}
	public override void _MakeVisible(bool visible)
	{
		if (screen != null)
		{
			screen.Visible = visible;
		}
	}

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventMouseButton)
		{
			InputEventMouseButton b = (InputEventMouseButton)@event;

			if (b.ButtonIndex == MouseButton.Left && b.IsPressed())
			{
				SubViewportContainer cont = screen.GetNode<SubViewportContainer>("SubViewportContainer");


				if (Input.IsKeyPressed(Key.Shift))
				{
					(cont as SelectOnClick).RayCastToMapPiece((mp) => { MS.Selection.SelectPathTargetMapPiece(mp); });
				}
				else
				{
					(cont as SelectOnClick).RayCastToMapPiece((mp) => { MS.Selection.SelectMapPiece(mp); });
				}

			}
		}
	}
	#endregion

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
		popup.Size = screen.GetViewport().GetWindow().Size / 2;
		EditorInterface.Singleton.GetBaseControl().GetViewport().AddChild(popup);
		popup.MoveToCenter();
		popup.Show();
	}
	#region Listeners
	/// <summary>
	/// Handle the export
	/// </summary>
	private void WhenExportConfirmed()
	{
		// Check things
		if (screen.CurrentDungeon is null)
		{
			GD.PrintErr($"Dungeons::WhenExportConfirmed() CurrentDungeon Node was NULL!");
			return;
		}
		if (screen.CurrentDungeon.GetChildCount() < 1)
		{
			GD.PrintErr($"Dungeons::WhenExportConfirmed() CurrentDungeon Node has no children to export!");
			return;
		}
		popup.Confirmed -= WhenExportConfirmed;
		PackedScene sceneToSave = new PackedScene();
		foreach (Node node in screen.CurrentDungeon.GetChildren())
		{
			SetOwner(screen.CurrentDungeon, node);
		}
		Error err = sceneToSave.Pack(screen.CurrentDungeon);
		if (err != Error.Ok)
		{
			GD.PrintErr($"Dungeons::WhenExportConfirmed() err[{err}]");
		}
		sceneToSave.ResourcePath = popup.CurrentPath;
		if (!popup.CurrentPath.Contains(".tscn")) { sceneToSave.ResourcePath = sceneToSave.ResourcePath + ".tscn"; }
		ResourceSaver.Save(sceneToSave);
		popup.QueueFree();
	}
	public bool VerifySectionsFolder()
	{
		if (masterConfig.ProjectResourcePath != string.Empty && DirAccess.DirExistsAbsolute(masterConfig.SectionResourcePath))
		{
			return true;
		}
		if (masterConfig.ProjectResourcePath != string.Empty && DirAccess.DirExistsAbsolute(masterConfig.ProjectResourcePath))
		{
			GD.Print("Dungeons:: Creating Sections folder in the project path");
			DirAccess.MakeDirAbsolute(masterConfig.SectionResourcePath);
			EditorInterface.Singleton.GetResourceFilesystem().Scan();
			return DirAccess.DirExistsAbsolute(masterConfig.SectionResourcePath);
		}
		return false;
	}




	#endregion

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
			SetOwner(screen.CurrentDungeon, n);
		}
	}
	public void ChangeMainScreenToDungeon()
	{
		EditorInterface.Singleton.SetMainScreenEditor(screenName);
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
			screen.RaiseUpdateUI();
		}
	}
}// EOF CLASS
#endif