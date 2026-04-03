using Godot;
using MDunGen.MS;
using MDunGen.Resources;

namespace MDunGen.UI;
[Tool, GlobalClass]
public partial class UIbtnMSBuild : Button
{
	[Export] MainScreen mainScreen;
	[Export] UIobtnMSSectionType sectionTypeSelector;
	[Export] UIobtnMSSection sectionSelector;
	public override void _Ready()
	{
		Pressed += WhenBtnPressed;
		Icon = EditorInterface.Singleton.GetBaseControl().GetThemeIcon("BuildCSharp", "EditorIcons");
	}

	public override void _ExitTree()
	{
		Pressed -= WhenBtnPressed;
	}

	private async void WhenBtnPressed()
	{
		mainScreen.WhenClearPressed();
		if (mainScreen.addon.Profile.useRandomSeed)
		{
			mainScreen.addon.Profile.settings.seed1 = GD.RandRange(1111, 9999);
			mainScreen.addon.Profile.settings.seed2 = GD.RandRange(1111, 9999);
			mainScreen.addon.Profile.settings.seed3 = GD.RandRange(1111, 9999);
			mainScreen.addon.Profile.settings.seed4 = GD.RandRange(1111, 9999);
		}

		DungeonUtils.globalOffset = mainScreen.addon.Profile.globalOffset;

		switch (mainScreen.addon.Mode)
		{
			case VIEWERMODE.SECTION:

				string sectionTypeName = sectionTypeSelector.GetItemText(sectionTypeSelector.Selected);
				SectionResource sectionDef = sectionSelector.GetSelectedResource();
				mainScreen.GenerateSection(0, sectionTypeName, sectionDef, mainScreen.addon.Profile.settings, mainScreen.addon.Profile.biome);
				break;
			default:
			case VIEWERMODE.DUNGEON:
				await mainScreen.GenerateDungeon(mainScreen.addon.Profile.settings, mainScreen.addon.Profile.biome);
				break;
		}
		//ReleaseFocus();
		mainScreen.RaiseUpdateUI();
	}
}// EOF CLASS


