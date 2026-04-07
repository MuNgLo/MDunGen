using Godot;
using MDunGen.MS;

namespace MDunGen.UI;
[Tool, GlobalClass]
public partial class UImbtnMSCamera : MenuButton
{
	[Export] bool debug;
	[Export] MainScreen mainScreen;
	[Export] CameraControls cameraControls;
	public override void _Ready()
	{
		mainScreen.OnMainScreenUIUpdate += UpdatePopup;
		GetPopup().IdPressed += WhenOptionsChanged;
		Icon = EditorInterface.Singleton.GetBaseControl().GetThemeIcon("Camera", "EditorIcons");
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
				cameraControls.ResetCamera();
			break;
			case 1:
				mainScreen.addon.MasterConfig.cameraResetOnBuild = pop.IsItemChecked(index);
				ResourceSaver.Save(mainScreen.addon.MasterConfig);
				break;
		}
		//ReleaseFocus();
	}

	private void UpdatePopup(object sender, object args)
	{
		if(debug){ GD.Print($"UIoptbtnMSView::UpdatePopup()");}
		PopupMenu pop = GetPopup();
		pop.HideOnCheckableItemSelection = false;
		pop.HideOnItemSelection = false;
		pop.HideOnStateItemSelection = false;
		pop.SetItemChecked(1, mainScreen.addon.MasterConfig.cameraResetOnBuild);
	}
}// EOF CLASS