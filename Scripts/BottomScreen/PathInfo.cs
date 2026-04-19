#if TOOLS
using Godot;
using MDunGen.Pathfinding;

namespace MDunGen.Bottom;

[Tool]
public partial class PathInfo : Control
{
	[Export] private BottomScreen BS;
	[Export] private RichTextLabel pathDebugInfo;
	public override void _EnterTree()
	{
		Pathing.OnPathDataPushed += WhenPathDataPushed;
	}
    private void WhenPathDataPushed(object sender, PathData e)
    {
        pathDebugInfo.Text = e.ToString();
    }
}// EOF CLASS
#endif