// Gone through at v1.3
#if TOOLS
using Godot;
namespace MDunGen.MS;

[Tool]
public partial class InitialPopup : Control
{
	public MainScreen screen;
	[Export] private Button changeBtn;
	[Export] private Button closeBtn;
	[Export] private LineEdit pathLine;
	private EditorFileDialog popup;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		changeBtn.Pressed += WhenChangePressed;
		closeBtn.Pressed += QueueFree;
		pathLine.Text = screen.addon.MasterConfig.ProjectResourcePath;

	}
	public override void _ExitTree()
	{
		changeBtn.Pressed -= WhenChangePressed;
		closeBtn.Pressed -= QueueFree;
	}

	private void WhenChangePressed()
	{
		changeBtn.ReleaseFocus();
		popup = new EditorFileDialog();
		popup.AlwaysOnTop = false;
		popup.Title = "Set Project Resource Path";
		popup.FileMode = EditorFileDialog.FileModeEnum.OpenDir;
		popup.Access = EditorFileDialog.AccessEnum.Resources;
		popup.PopupWindow = false;
		popup.OkButtonText = "Select Current Folder";
		popup.Size = screen.GetViewport().GetWindow().Size / 2;
		EditorInterface.Singleton.GetBaseControl().GetViewport().AddChild(popup);
		popup.MoveToCenter();
		popup.Confirmed += WhenConfirmed;
		popup.Show();
	}
	private void WhenConfirmed()
	{
		popup.Confirmed -= WhenConfirmed;
		screen.addon.MasterConfig.ProjectResourcePath = popup.CurrentPath;
		ResourceSaver.Save(screen.addon.MasterConfig);
		pathLine.Text = screen.addon.MasterConfig.ProjectResourcePath;
		popup.QueueFree();
	}
}// EOF CLASS
#endif