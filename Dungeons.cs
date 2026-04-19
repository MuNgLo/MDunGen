// Gone through at v1.4
#if TOOLS
using Godot;
using MDunGen.Bottom;
using MDunGen.MS;
using MDunGen.Resources;

namespace MDunGen;

[Tool]
public partial class Dungeons : EditorPlugin
{
	private readonly string screenName = "Dungeon";
	private AddonSettingsResource masterConfig;
	public AddonSettingsResource MasterConfig => masterConfig;
	private MainScreen screen;
	public MainScreen MS => screen;
	private BottomScreen bScreen;
	private CameraControls cam;
	private PackedScene prefabMainScreen = ResourceLoader.Load<PackedScene>("res://addons/MDunGen/Scenes/MainScreen.tscn");
	private PackedScene prefabBottomScreen = ResourceLoader.Load<PackedScene>("res://addons/MDunGen/Scenes/BottomScreen.tscn");
	#region Overrides
	public override void _EnterTree()
	{
		GD.Print("Loaded MDunGen Plugin");
		masterConfig = ResourceLoader.Load("res://addons/MDunGen/Config/def_master.tres") as AddonSettingsResource;

		// Center screen
		screen = (MainScreen)prefabMainScreen.Instantiate();
		screen.addon = this;
		// Add screen instance to the editor
		EditorInterface.Singleton.GetEditorMainScreen().AddChild(screen);
		// Hide the main panel. Very much required.
		_MakeVisible(false);

		cam = screen.FindChild("Camera3D") as CameraControls;

		// Bottom screen
		bScreen = (BottomScreen)prefabBottomScreen.Instantiate();
		bScreen.Name = "MDunGen";
		bScreen.addon = this;

		// Add bottom screen instance to the editor
		EditorDock bDock = new EditorDock() { DefaultSlot = EditorDock.DockSlot.Bottom };
		bDock.AddChild(bScreen);
		AddDock(bDock);
	}

	public override void _ExitTree()
	{
		RemoveDock(bScreen.GetParent() as EditorDock);
		bScreen.GetParent().QueueFree();
		GD.Print("Unloaded MDunGen Plugin");
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
	#endregion

	/// <summary>
	/// Unused right now. Maybe later.
	/// </summary>
	public void ChangeMainScreenToDungeon()
	{
		EditorInterface.Singleton.SetMainScreenEditor(screenName);
	}

}// EOF CLASS
#endif