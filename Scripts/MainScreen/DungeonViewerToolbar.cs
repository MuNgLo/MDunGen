// Gone through at v1.3
#if TOOLS
using Godot;
using MDunGen.Commons;
using MDunGen.Resources;
using MDunGen.UI;
using System;
namespace MDunGen.MS;

[Tool]
public partial class DungeonViewerToolbar : HBoxContainer
{
	[Export] MainScreen screen;

	[ExportGroup("Section visible")]
	[Export] Control[] visibleWhenInSectionMode;

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
		switch (screen.Mode)
		{
			case VIEWERMODE.SECTION:
				foreach (Control item in visibleWhenInSectionMode)
				{
					item.Show();
				}
				break;
			case VIEWERMODE.DUNGEON:
			default:
				foreach (Control item in visibleWhenInSectionMode)
				{
					item.Hide();
				}
				break;
		}
	}
}//EOF CLASS
#endif