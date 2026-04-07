
#if TOOLS
using Godot;
using MDunGen.Commons;
using MDunGen.Sections;
using System;

namespace MDunGen.Bottom;
/// <summary>
/// The bottom window center editor to view and change dungeon configuration
/// </summary>
[Tool, GlobalClass]
public partial class BottomScreen : Control
{
    public Dungeons addon;
    [Export] private Label sectionInfo;
    [Export] private Label mapPieceInfo;
    [Export] private Label connectionInfo;
    public override void _EnterTree()
    {
        addon.MS.Selection.OnSelectionChanged += WhenSelectionChanged;
        connectionInfo.Text = string.Empty;
    }

    private void WhenSelectionChanged(object sender, EventArgs e)
    {
        MapPiece mp = addon.MS.Selection.SelectedMapPiece;
        ISection ss = addon.MS.Selection.SelectedSection;
        SectionConnection sc = addon.MS.Selection.SelectedConnection;
        if (mp is not null)
        {
            mapPieceInfo.Text = $"MapPiece Info:\n MapPiece[{mp.Coord}] section[{mp.SectionIndex}] floor[{mp.hasFloor}]";
        }
        if (ss is not null)
        {
            sectionInfo.Text = $"Section Info:\n Section[{ss.SectionIndex}] has [{ss.ConnectionCount}] connections. Section Min/Max [{ss.MinCoord} / {ss.MaxCoord}]";
        }
        if (sc is not null)
        {
            connectionInfo.Text = $"Connection Info:\n{sc}";
        }
        else
        {
            connectionInfo.Text = string.Empty;
        }
    }
}// EOF CLASS
#endif