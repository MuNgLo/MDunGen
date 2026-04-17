// Gone through at v1.3
#if TOOLS
using Godot;
using MDunGen.Resources;
namespace MDunGen.MS;

[Tool]
public partial class UIMSFloorFilterBar : HBoxContainer
{
	[Export] bool debug;
	[Export] MainScreen mainScreen;
	[ExportGroup("Tool References")]
	[Export] SpinBox sbStartFloor;
	[Export] TextureButton tbAddFloor;
	[Export] TextureButton tbFloorBox;
	[Export] TextureButton tbSubtractFloor;
	[Export] Label lblFloorEnd;
	private AddonSettingsResource MasterConfig => mainScreen.addon.MasterConfig;
	private ProfileResource Profile => mainScreen.addon.Profile;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		sbStartFloor.ValueChanged += WhenSpinBoxValueChanged;
		tbAddFloor.Pressed += WhenPlusPressed;
		tbSubtractFloor.Pressed += WhenMinusPressed;
		lblFloorEnd.Text = MasterConfig.maxVisibleLevels.ToString();
	}
	public override void _ExitTree()
	{
		sbStartFloor.ValueChanged -= WhenSpinBoxValueChanged;
		tbAddFloor.Pressed -= WhenPlusPressed;
		tbSubtractFloor.Pressed -= WhenMinusPressed;
	}
	private void UpdateUI()
	{
		// Get the OG floor box
		TextureButton FloorBox = FindChild("tbFloorBox") as TextureButton;
		// Get Index of OG box. After this the rest will be inserted/removed
		int startInsertIndex = FloorBox.GetIndex() + 1;
		// Calculate end floor
		int endFloor = MasterConfig.visibleLevelsStart + MasterConfig.maxVisibleLevels - 1;
		lblFloorEnd.Text = endFloor.ToString();
		int currentCount = GetChildCount();
		int goalCount = endFloor - MasterConfig.visibleLevelsStart + 5; // 5 extra because not all are floor boxes

		if (currentCount > goalCount)
		{
			// Subtract
			for (int i = 0; i < currentCount - goalCount; i++)
			{
				GetChild(startInsertIndex + i).QueueFree();
			}
		}
		if (currentCount < goalCount)
		{
			// Add
			for (int i = 0; i < goalCount - currentCount; i++)
			{
				TextureButton copy = FloorBox.Duplicate() as TextureButton;
				AddChild(copy);
				MoveChild(copy, startInsertIndex + i);
			}
		}
		if(debug) { GD.Print($"UIMSFloorFilterBar::UpdateUI() startInsertIndex[{startInsertIndex}] currentCount[{currentCount}] goalCount[{goalCount}]"); }
	}
	private void WhenSpinBoxValueChanged(double value)
	{
		MasterConfig.visibleLevelsStart = Mathf.Clamp((int)value, 0, 100);
		ResourceSaver.Save(MasterConfig);
		ShowFloorsNotification();
		UpdateUI();
		mainScreen.ReDrawDungeon();
		//tbAddFloor.GrabFocus(); // Block the spin box line edit from grabbing keystrokes
	}
	private void WhenPlusPressed()
	{
		MasterConfig.maxVisibleLevels = Mathf.Clamp(MasterConfig.maxVisibleLevels + 1, 1, 10);
		ResourceSaver.Save(MasterConfig);
		ShowFloorsNotification();
		UpdateUI();
		mainScreen.ReDrawDungeon();
	}
	private void WhenMinusPressed()
	{
		MasterConfig.maxVisibleLevels = Mathf.Clamp(MasterConfig.maxVisibleLevels - 1, 1, 10);
		ResourceSaver.Save(MasterConfig);
		ShowFloorsNotification();
		UpdateUI();
		mainScreen.ReDrawDungeon();
	}
	private void ShowFloorsNotification()
	{
		mainScreen.RaiseNotification($"Showing floor {MasterConfig.visibleLevelsStart + 1}" + (MasterConfig.maxVisibleLevels > 1 ? $" through {MasterConfig.visibleLevelsStart + MasterConfig.maxVisibleLevels}" : string.Empty));
	}
}// EOF CLASS
#endif