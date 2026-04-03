using Godot;
using MDunGen.MS;

namespace MDunGen.UI;
[Tool, GlobalClass]
public partial class UImenbtnMSView : MenuButton
{
	[Export] bool debug;
	[Export] MainScreen mainScreen;
	public override void _Ready()
	{
		mainScreen.OnMainScreenUIUpdate += UpdatePopup;
		GetPopup().IdPressed += WhenOptionsChanged;
		Icon = EditorInterface.Singleton.GetBaseControl().GetThemeIcon("GuiVisibilityVisible", "EditorIcons");
	}

	public override void _ExitTree()
	{
		mainScreen.OnMainScreenUIUpdate -= UpdatePopup;
		GetPopup().IdPressed -= WhenOptionsChanged;
	}

	private void WhenOptionsChanged(long id)
	{
		if(debug){ GD.Print($"UIoptbtnMSView::WhenOptionsChanged({id})");}
		PopupMenu pop = GetPopup();
		int index = pop.GetItemIndex((int)id);
		pop.SetItemChecked(index, !pop.IsItemChecked(index));
		switch (id)
		{
			case 0:
				mainScreen.addon.MasterConfig.showFloors = pop.IsItemChecked(index);
				ResourceSaver.Save(mainScreen.addon.MasterConfig);
				break;
			case 1:
				mainScreen.addon.MasterConfig.showWalls = pop.IsItemChecked(index);
				ResourceSaver.Save(mainScreen.addon.MasterConfig);
				break;
			case 2:
				mainScreen.addon.MasterConfig.showCeilings = pop.IsItemChecked(index);
				ResourceSaver.Save(mainScreen.addon.MasterConfig);
				break;
			case 4:
				mainScreen.addon.MasterConfig.pathingPass = pop.IsItemChecked(index);
				ResourceSaver.Save(mainScreen.addon.MasterConfig);
				break;
			case 5:
				mainScreen.addon.MasterConfig.showExtras = pop.IsItemChecked(index);
				ResourceSaver.Save(mainScreen.addon.MasterConfig);
				break;
			case 6:
				mainScreen.addon.MasterConfig.showDebug = pop.IsItemChecked(index);
				ResourceSaver.Save(mainScreen.addon.MasterConfig);
				break;
			default:
				break;
		}
		//ReleaseFocus();
		mainScreen.ReDrawDungeon();
	}

	private void UpdatePopup(object sender, object args)
	{
		if(debug){ GD.Print($"UIoptbtnMSView::UpdatePopup()");}
		PopupMenu pop = GetPopup();
		pop.HideOnCheckableItemSelection = false;
		pop.HideOnItemSelection = false;
		pop.HideOnStateItemSelection = false;
		pop.SetItemChecked(0, mainScreen.addon.MasterConfig.showFloors);
		pop.SetItemChecked(1, mainScreen.addon.MasterConfig.showWalls);
		pop.SetItemChecked(2, mainScreen.addon.MasterConfig.showCeilings);
		pop.SetItemChecked(3, mainScreen.addon.MasterConfig.pathingPass);
		pop.SetItemChecked(4, mainScreen.addon.MasterConfig.showExtras);
		pop.SetItemChecked(5, mainScreen.addon.MasterConfig.showDebug);
	}
}// EOF CLASS