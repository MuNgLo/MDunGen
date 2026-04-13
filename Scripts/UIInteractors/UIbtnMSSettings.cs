using Godot;
using MDunGen.MS;

namespace MDunGen.UI;
[Tool, GlobalClass]
public partial class UIbtnMSSettings : Button
{
	[Export] MainScreen mainScreen;
	public override void _Ready()
	{
		Pressed += WhenBtnPressed;
	}

	public override void _ExitTree()
	{
		Pressed -= WhenBtnPressed;
	}

	private void WhenBtnPressed()
	{
		EditorInterface.Singleton.InspectObject(mainScreen.addon.Profile.design);
		//ReleaseFocus();
	}
}// EOF CLASS