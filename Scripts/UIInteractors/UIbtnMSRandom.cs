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
		ButtonPressed = mainScreen.addon.Profile.useRandomSeed;
		Pressed += WhenRandomSeedPressed;
		Icon = EditorInterface.Singleton.GetBaseControl().GetThemeIcon("RandomNumberGenerator", "EditorIcons");
	}

	public override void _ExitTree()
	{
		Pressed -= WhenRandomSeedPressed;
	}

	private void WhenRandomSeedPressed()
	{
		mainScreen.addon.Profile.useRandomSeed = !mainScreen.addon.Profile.useRandomSeed;
		ResourceSaver.Save(mainScreen.addon.Profile);
		mainScreen.RaiseNotification("Generate " + (mainScreen.addon.Profile.useRandomSeed ? "Random" : "Seeded"));
	}
	
}// EOF CLASS


