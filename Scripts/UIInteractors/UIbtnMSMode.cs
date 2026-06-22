#if TOOLS
using Godot;
using MDunGen.MS;

namespace MDunGen.UI;
[Tool, GlobalClass]
public partial class UIbtnMSMode : Button
{
	[Export] MainScreen mainScreen;
	public override void _Ready()
	{
		Icon = EditorInterface.Singleton.GetBaseControl().GetThemeIcon("TexturePreviewChannels", "EditorIcons");
		Pressed += WhenBtnPressed;
	}

	public override void _ExitTree()
	{
		Pressed -= WhenBtnPressed;
	}

	private void WhenBtnPressed()
	{
		mainScreen.ChangeMode();
	}
}// EOF CLASS
#endif
