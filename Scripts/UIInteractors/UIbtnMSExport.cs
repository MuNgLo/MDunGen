using Godot;
using MDunGen.MS;

namespace MDunGen.UI;
[Tool, GlobalClass]
public partial class UIbtnMSExport : Button
{
	[Export] MainScreen mainScreen;
	public override void _Ready()
	{
		Icon = EditorInterface.Singleton.GetBaseControl().GetThemeIcon("Save", "EditorIcons");
		Pressed += WhenBtnPressed;
	}

	public override void _ExitTree()
	{
		Pressed -= WhenBtnPressed;
	}

	private void WhenBtnPressed()
	{
		mainScreen.addon.ShowExportPopup();
		ReleaseFocus();
	}
}// EOF CLASS