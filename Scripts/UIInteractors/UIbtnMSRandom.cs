#if TOOLS
using Godot;
using MDunGen.MS;
using MDunGen.Resources;

namespace MDunGen.UI;
[Tool, GlobalClass]
public partial class UIbtnMSRandom : Button
{
	[Export] MainScreen mainScreen;

	public override void _Ready()
	{
		ButtonPressed = mainScreen.addon.MasterConfig.useRandomSeed;
		Pressed += WhenRandomSeedPressed;
		Icon = EditorInterface.Singleton.GetBaseControl().GetThemeIcon("RandomNumberGenerator", "EditorIcons");
	}

	public override void _ExitTree()
	{
		Pressed -= WhenRandomSeedPressed;
	}

	private void WhenRandomSeedPressed()
	{
		mainScreen.addon.MasterConfig.useRandomSeed = !mainScreen.addon.MasterConfig.useRandomSeed;
		ResourceSaver.Save(mainScreen.addon.MasterConfig);
		mainScreen.RaiseNotification("Generate " + (mainScreen.addon.MasterConfig.useRandomSeed ? "Random" : "Seeded"));
	}
	
}// EOF CLASS

#endif

