using Godot;
using MDunGen.Bottom;
using MDunGen.MS;

namespace MDunGen.UI;
[Tool, GlobalClass]
public partial class UIbtnBSClearLog : Button
{
	[Export] BuildLogTab buildLog;
	public override void _Ready()
	{
		Icon = EditorInterface.Singleton.GetBaseControl().GetThemeIcon("Clear", "EditorIcons");
		Pressed += WhenBtnPressed;
	}

	public override void _ExitTree()
	{
		Pressed -= WhenBtnPressed;
	}

	private void WhenBtnPressed()
	{
		buildLog.ClearLog();
	}
}// EOF CLASS