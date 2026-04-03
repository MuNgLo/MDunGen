// Gone through at v1.3
#if TOOLS
using Godot;
using MDunGen.Resources;
using System;
namespace MDunGen.MS;

[Tool]
public partial class DungeonViewerToolbar : HBoxContainer
{
	private MainScreen screen;

	[Export] private Button btnModeToggle;
	[Export] private Button btnClear;
	[Export] private Button btnBuild;
	[Export] private Button btnRandomToggle;
	[Export] private MenuButton btnView;
	[ExportGroup("Section visible")]
	[Export] private SectionTypeListButton btnSectionTypeList;
	[Export] private SectionSelector btnSectionSelector;
	[Export] VSeparator vsLeft;
	[Export] VSeparator vsRight;
	public ProfileResource Profile => screen.addon.Profile;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		screen = GetParent<MainScreen>();
		screen.OnMainScreenUIUpdate += UpdateUI;
		btnModeToggle.Pressed += WhenModeTogglePressed;
		btnBuild.Pressed += WhenTBBuildPressed;
		btnClear.Pressed += WhenMSClearPressed;
		btnRandomToggle.Pressed += WhenMSRNGSeedPressed;
		btnView.GetPopup().IdPressed += WhenMSShowChanged;

		btnBuild.Icon = EditorInterface.Singleton.GetBaseControl().GetThemeIcon("BuildCSharp", "EditorIcons");
		btnClear.Icon = EditorInterface.Singleton.GetBaseControl().GetThemeIcon("RotateLeft", "EditorIcons");
		btnRandomToggle.Icon = EditorInterface.Singleton.GetBaseControl().GetThemeIcon("RandomNumberGenerator", "EditorIcons");
		btnView.Icon = EditorInterface.Singleton.GetBaseControl().GetThemeIcon("ViewportTexture", "EditorIcons");
		btnModeToggle.Icon = EditorInterface.Singleton.GetBaseControl().GetThemeIcon("TexturePreviewChannels", "EditorIcons");
		


	}

	private void UpdateUI(object sender, EventArgs e)
	{
		// Update the UI to current states
		switch (screen.addon.Mode)
		{
			case VIEWERMODE.SECTION:
				btnSectionSelector.Show();
				btnSectionTypeList.Show();
				vsLeft.Show();
				vsRight.Show();
				break;
			case VIEWERMODE.DUNGEON:
			default:
				btnSectionSelector.Hide();
				btnSectionTypeList.Hide();
				vsLeft.Hide();
				vsRight.Hide();
				break;
		}

		PopupMenu pop = btnView.GetPopup();
		pop.HideOnCheckableItemSelection = false;
		pop.HideOnItemSelection = false;
		pop.HideOnStateItemSelection = false;
		pop.SetItemChecked(0, screen.addon.MasterConfig.showFloors);
		pop.SetItemChecked(1, screen.addon.MasterConfig.showWalls);
		pop.SetItemChecked(2, screen.addon.MasterConfig.showCeilings);
		pop.SetItemChecked(3, screen.addon.MasterConfig.pathingPass);
		pop.SetItemChecked(4, screen.addon.MasterConfig.showExtras);
		pop.SetItemChecked(5, screen.addon.MasterConfig.showDebug);
	}

	private void WhenModeTogglePressed()
	{
		screen.addon.ChangeMode();
	}

	public override void _ExitTree()
	{
		btnBuild.Pressed -= WhenTBBuildPressed;
		btnClear.Pressed -= WhenMSClearPressed;
		btnRandomToggle.Pressed -= WhenMSRNGSeedPressed;
		btnView.GetPopup().IdPressed -= WhenMSShowChanged;
	}

	private void WhenTBBuildPressed()
	{
		GD.Print("DungeonViewerToolbar::WhenTBBuildPressed()");
		screen.WhenClearPressed();
		if (Profile.useRandomSeed)
		{
			Profile.settings.seed1 = GD.RandRange(1111, 9999);
			Profile.settings.seed2 = GD.RandRange(1111, 9999);
			Profile.settings.seed3 = GD.RandRange(1111, 9999);
			Profile.settings.seed4 = GD.RandRange(1111, 9999);
		}

		DungeonUtils.globalOffset = Profile.globalOffset;

		switch (screen.addon.Mode)
		{
			case VIEWERMODE.SECTION:

				string sectionTypeName = btnSectionTypeList.GetItemText(btnSectionTypeList.Selected);
				SectionResource sectionDef = btnSectionSelector.GetSelectedResource();
				screen.GenerateSection(sectionTypeName, sectionDef, Profile.settings, Profile.biome);
				break;
			default:
			case VIEWERMODE.DUNGEON:
				screen.GenerateDungeon(Profile.settings, Profile.biome);
				break;
		}
		btnBuild.ReleaseFocus();
		screen.RaiseUpdateUI();
	}
	private void WhenMSClearPressed()
	{
		GD.Print("DungeonViewerToolbar::WhenMSClearPressed()");
		screen.WhenClearPressed();
		btnClear.ReleaseFocus();
	}
	private void WhenMSRNGSeedPressed()
	{
		GD.Print("DungeonViewerToolbar::WhenMSRNGSeedPressed()");
		Profile.useRandomSeed = !Profile.useRandomSeed;
		ResourceSaver.Save(Profile);
		screen.RaiseNotification("Generate " + (Profile.useRandomSeed ? "Random" : "Seeded"));
		btnRandomToggle.ReleaseFocus();
	}
	private void WhenMSShowChanged(long id)
	{
		GD.Print($"DungeonViewerToolbar::WhenMSShowChanged({id})");
		PopupMenu pop = btnView.GetPopup();
		int index = pop.GetItemIndex((int)id);
		pop.SetItemChecked(index, !pop.IsItemChecked(index));
		switch (id)
		{
			case 0:
				screen.addon.MasterConfig.showFloors = pop.IsItemChecked(index);
				ResourceSaver.Save(Profile.settings);
				break;
			case 1:
				screen.addon.MasterConfig.showWalls = pop.IsItemChecked(index);
				ResourceSaver.Save(Profile.settings);
				break;
			case 2:
				screen.addon.MasterConfig.showCeilings = pop.IsItemChecked(index);
				ResourceSaver.Save(Profile.settings);
				break;
			case 4:
				screen.addon.MasterConfig.pathingPass = pop.IsItemChecked(index);
				ResourceSaver.Save(Profile.settings);
				break;
			case 5:
				screen.addon.MasterConfig.showExtras = pop.IsItemChecked(index);
				ResourceSaver.Save(Profile.settings);
				break;
			case 6:
				screen.addon.MasterConfig.showDebug = pop.IsItemChecked(index);
				ResourceSaver.Save(Profile.settings);
				break;
			default:
				break;
		}
		btnView.ReleaseFocus();
		screen.ReDrawDungeon();
	}
}//EOF CLASS
#endif