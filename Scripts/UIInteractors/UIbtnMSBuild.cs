using Godot;
using MDunGen.Commons;
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
			mainScreen.addon.MasterConfig.seed1 = GD.RandRange(1111, 9999);
			mainScreen.addon.MasterConfig.seed2 = GD.RandRange(1111, 9999);
			mainScreen.addon.MasterConfig.seed3 = GD.RandRange(1111, 9999);
			mainScreen.addon.MasterConfig.seed4 = GD.RandRange(1111, 9999);
			ResourceSaver.Save(mainScreen.addon.MasterConfig);
		}

		DungeonUtils.globalOffset = mainScreen.addon.Profile.globalOffset;

		switch (mainScreen.addon.Mode)
		{
			case VIEWERMODE.SECTION:

				string sectionTypeName = sectionTypeSelector.GetItemText(sectionTypeSelector.Selected);
				SectionResource sectionDef = sectionSelector.GetSelectedResource();
				mainScreen.GenerateSection(0, sectionTypeName, sectionDef, mainScreen.addon.MasterConfig.MasterSeed, mainScreen.addon.Profile.design.defaultBiome, true);
				break;
			default:
			case VIEWERMODE.DUNGEON:
				await mainScreen.GenerateDungeon(mainScreen.addon.Profile.design, true);
				break;
		}
		//ReleaseFocus();
		mainScreen.RaiseUpdateUI();
	}
}// EOF CLASS


