using Godot;
using MDunGen.Commons;
using System;

namespace MDunGen.Bottom;
[Tool]
public partial class BuildLogTab : MarginContainer
{
	[Export] bool debug = false;
	[Export] BottomScreen BS;
	[Export] PackedScene prefabLogEntry;
	[Export] GridContainer container;
	[Export] LineEdit leFilter;
	[Export] CheckBox cbShowSource;
	public override void _Ready()
	{
		if(debug){ GD.Print($"BuildLogTab::_EnterTree()"); }
		leFilter.TextChanged += WhenFilterChanged;
		BS.addon.MS.Visualizer.OnMapBuildStarted += ClearLog;
		BS.addon.MS.Visualizer.OnMapBuildEnded += AddBuildEnd;
		BS.addon.MS.Visualizer.OnMapBuildLog += AddBuildLog;
		RequestReady();
	}
	MS.CameraControls camera;
	public void CoordinateClicked(MapCoordinate coord)
	{
		camera = BS.addon.MS.FindChild("Camera3D") as MS.CameraControls; 
		camera.FocusOnMapCoordinate(coord);
	}

	private void WhenFilterChanged(string newText)
	{
		foreach (Node child in container.GetChildren())
		{
			if(child is BuildLogEntry entry)
			{
				entry.Visible = IsInFilter(entry);
			}
		}
	}

	private void AddBuildLog(object sender, BuildLogEventArgument e)
	{
		if(debug){ GD.Print($"BuildLogTab::AddBuildLog() {e.message}"); }
		AddEntry(e);
	}

	void AddBuildEnd(object sender, EventArgs e)
	{
		if(debug){ GD.Print($"BuildLogTab::AddBuildEnd()"); }
	}

	void ClearLog(object sender, EventArgs e)
	{
		if(debug){ GD.Print($"BuildLogTab::ClearLog()"); }
		foreach (Node child in container.GetChildren())
		{
			child.QueueFree();
		}
	}

	void AddEntry(BuildLogEventArgument args)
	{
		if(debug){ GD.Print($"BuildLogTab::AddEntry()"); }
		BuildLogEntry newEntry = prefabLogEntry.Instantiate<BuildLogEntry>();
		newEntry.SetBuildLog(this, args, cbShowSource.ButtonPressed);
		newEntry.Visible = IsInFilter(newEntry);
		container.AddChild(newEntry);
	}
	bool IsInFilter(BuildLogEntry entry)
	{
		return entry.Text.ToLower().Contains(leFilter.Text.ToLower());
	}
}// EOF CLASS