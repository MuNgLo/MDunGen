// Gone through at v1.3
#if TOOLS
using Godot;
using MDunGen.Resources;
using MDunGen.UI;
using System;
namespace MDunGen.MS;

[Tool]
public partial class DungeonViewerToolbar : HBoxContainer
{
	[Export] MainScreen screen;

	[ExportGroup("Section visible")]
	[Export] UIobtnMSSectionType btnSectionTypeList;
	[Export] UIobtnMSSection btnSectionSelector;
	[Export] UIbtnMSInspectSection btnSectionInspect;
	[Export] VSeparator vsLeft;
	[Export] VSeparator vsRight;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		screen.OnMainScreenUIUpdate += UpdateToolbar;
	}
	public override void _ExitTree()
	{
		screen.OnMainScreenUIUpdate -= UpdateToolbar;
	}
	/// <summary>
	/// Update the toolbar to show the correct set of buttons
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	private void UpdateToolbar(object sender, EventArgs e)
	{
		// Update the UI to current states
		switch (screen.addon.Mode)
		{
			case VIEWERMODE.SECTION:
				btnSectionSelector.Show();
				btnSectionTypeList.Show();
				btnSectionInspect.Show();
				vsLeft.Show();
				vsRight.Show();
				break;
			case VIEWERMODE.DUNGEON:
			default:
				btnSectionSelector.Hide();
				btnSectionTypeList.Hide();
				btnSectionInspect.Hide();
				vsLeft.Hide();
				vsRight.Hide();
				break;
		}
	}
}//EOF CLASS
#endif