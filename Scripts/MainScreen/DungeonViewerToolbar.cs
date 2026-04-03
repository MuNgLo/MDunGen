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
	[Export] Button btnModeToggle;
	[Export] Button btnClear;
	[Export] Button btnRandomToggle;
	[ExportGroup("Section visible")]
	[Export] UIobtnMSSectionType btnSectionTypeList;
	[Export] UIobtnMSSection btnSectionSelector;
	[Export] UIbtnMSInspectSection btnSectionInspect;
	[Export] VSeparator vsLeft;
	[Export] VSeparator vsRight;
	ProfileResource Profile => screen.addon.Profile;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		screen.OnMainScreenUIUpdate += UpdateToolbar;
		btnModeToggle.Pressed += WhenModeTogglePressed;
		btnClear.Pressed += WhenMSClearPressed;
		btnRandomToggle.Pressed += WhenRandomSeedPressed;
		btnClear.Icon = EditorInterface.Singleton.GetBaseControl().GetThemeIcon("RotateLeft", "EditorIcons");
		btnRandomToggle.Icon = EditorInterface.Singleton.GetBaseControl().GetThemeIcon("RandomNumberGenerator", "EditorIcons");
		btnModeToggle.Icon = EditorInterface.Singleton.GetBaseControl().GetThemeIcon("TexturePreviewChannels", "EditorIcons");
	}
	public override void _ExitTree()
	{
		screen.OnMainScreenUIUpdate -= UpdateToolbar;
		btnModeToggle.Pressed -= WhenModeTogglePressed;
		btnClear.Pressed -= WhenMSClearPressed;
		btnRandomToggle.Pressed -= WhenRandomSeedPressed;
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

	private void WhenModeTogglePressed()
	{
		screen.addon.ChangeMode();
	}

	private void WhenMSClearPressed()
	{
		screen.WhenClearPressed();
	}
	private void WhenRandomSeedPressed()
	{
		Profile.useRandomSeed = !Profile.useRandomSeed;
		ResourceSaver.Save(Profile);
		screen.RaiseNotification("Generate " + (Profile.useRandomSeed ? "Random" : "Seeded"));
	}
}//EOF CLASS
#endif