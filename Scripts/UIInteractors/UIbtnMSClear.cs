using Godot;
using MDunGen.MS;

namespace MDunGen.UI;
[Tool, GlobalClass]
public partial class UIbtnMSClear : Button
{
	[Export] MainScreen mainScreen;
	public override void _Ready()
	{
		Icon = EditorInterface.Singleton.GetBaseControl().GetThemeIcon("RotateLeft", "EditorIcons");
		Pressed += WhenBtnPressed;
	}

	public override void _ExitTree()
	{
		Pressed -= WhenBtnPressed;
	}

	private void WhenBtnPressed()
	{
		mainScreen.WhenClearPressed();
	}
}// EOF CLASS