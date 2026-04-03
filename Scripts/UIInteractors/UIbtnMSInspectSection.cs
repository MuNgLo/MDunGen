using Godot;
using MDunGen.MS;

namespace MDunGen.UI;
[Tool, GlobalClass]
public partial class UIbtnMSInspectSection : Button
{
	[Export] MainScreen mainScreen;
	[Export] UIobtnMSSection sectionSelector;
	public override void _Ready()
	{
		Pressed += WhenBtnPressed;
		Icon = EditorInterface.Singleton.GetBaseControl().GetThemeIcon("EditorInspector", "EditorIcons");
	}

	public override void _ExitTree()
	{
		Pressed -= WhenBtnPressed;
	}

	private void WhenBtnPressed()
	{
		EditorInterface.Singleton.InspectObject(sectionSelector.GetSelectedResource());
		//ReleaseFocus();
	}
}// EOF CLASS
