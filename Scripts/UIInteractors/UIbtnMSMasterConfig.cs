using Godot;
using MDunGen.MS;

namespace MDunGen.UI;
[Tool, GlobalClass]
public partial class UIbtnMSMasterConfig : Button
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
		mainScreen.PopupInitialSettingsDialogue();
		//ReleaseFocus();
	}
}// EOF CLASS