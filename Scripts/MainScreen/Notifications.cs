// Gone through at v1.3
#if TOOLS
using Godot;
namespace MDunGen.MS;

[Tool]
public partial class Notifications : RichTextLabel
{
	[Export] MainScreen screen;

	double notificationTTL;

	public override void _Process(double delta)
	{
		// Notification timer
		if (notificationTTL > 0) { notificationTTL -= delta; if (notificationTTL < 0) { Text = string.Empty; } }
	}
	/// <summary>
	/// Show a message on the bottom of the viewer. Only one can be shown so it overwrites the existing one.
	/// </summary>
	/// <param name="message"></param>
	public void ScreenNotify(object obj, string message)
	{
		Text = message;
		notificationTTL = 1.5f;
	}
}// EOF CLASS
#endif