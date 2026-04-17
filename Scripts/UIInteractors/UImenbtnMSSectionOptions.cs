using Godot;
using MDunGen.MS;

namespace MDunGen.UI;

[Tool, GlobalClass]
public partial class UImenbtnMSSectionOptions : MenuButton
{
	[Export] bool debug;
	[Export] MainScreen mainScreen;
	public override void _Ready()
	{
		mainScreen.OnMainScreenUIUpdate += UpdatePopup;
		PopupMenu pop = GetPopup();
		pop.IdPressed += WhenOptionsChanged;
		pop.HideOnCheckableItemSelection = false;
		pop.HideOnItemSelection = false;
		pop.HideOnStateItemSelection = false;
		Icon = EditorInterface.Singleton.GetBaseControl().GetThemeIcon("ThemeSelectAll", "EditorIcons");
		BuildPopup();
	}

	public override void _ExitTree()
	{
		mainScreen.OnMainScreenUIUpdate -= UpdatePopup;
		GetPopup().IdPressed -= WhenOptionsChanged;
	}

	private void WhenOptionsChanged(long id)
	{
		if (debug) { GD.Print($"UImenbtnMSSectionOptions::WhenOptionsChanged({id})"); }
		PopupMenu pop = GetPopup();
		int index = pop.GetItemIndex((int)id);
		pop.SetItemChecked(index, !pop.IsItemChecked(index));
		switch (id)
		{
			case 0:
				mainScreen.addon.MasterConfig.sectionFirstPieceDoor = pop.IsItemChecked(index);
				ResourceSaver.Save(mainScreen.addon.MasterConfig);
				break;
			case 1:
				mainScreen.addon.MasterConfig.sectionAddAttachment = pop.IsItemChecked(index);
				ResourceSaver.Save(mainScreen.addon.MasterConfig);
				break;
		}
		//mainScreen.ReDrawDungeon();
	}

	private void BuildPopup()
	{
		if (debug) { GD.Print($"UImenbtnMSSectionOptions::UpdatePopup()"); }
		PopupMenu pop = GetPopup();
		pop.Clear();
		pop.AddCheckItem("First piece Door");
		pop.AddCheckItem("With attachment");
	}

	private void UpdatePopup(object sender, object args)
	{
		if (debug) { GD.Print($"UImenbtnMSSectionOptions::UpdatePopup()"); }
		PopupMenu pop = GetPopup();
		pop.SetItemChecked(0, mainScreen.addon.MasterConfig.sectionFirstPieceDoor);
		pop.SetItemChecked(1, mainScreen.addon.MasterConfig.sectionAddAttachment);
	}
}// EOF CLASS