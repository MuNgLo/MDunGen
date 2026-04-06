using Godot;
using MDunGen.Commons;
using System;
using System.Collections.Generic;

namespace MDunGen.Bottom;
[Tool]
public partial class BuildLogTab : MarginContainer
{
	[Export] bool debug = false;
	[Export] BottomScreen BS;
	[Export] PackedScene prefabLogEntry;
	[Export] RichTextLabel logRichText;
	[Export] LineEdit leFilter;
	[Export] CheckBox cbShowSource;
	public override void _Ready()
	{
		if(debug){ GD.Print($"BuildLogTab::_EnterTree()"); }
		logRichText.MetaClicked += WhenMetaClicked;
		leFilter.TextChanged += WhenFilterChanged;
		BS.addon.MS.Visualizer.OnMapBuildFloorStarted += ClearLog;
		BS.addon.MS.Visualizer.OnMapBuildEnded += AddBuildEnd;
		BS.addon.MS.Visualizer.OnMapBuildLog += AddBuildLog;
		RequestReady();
	}
	MS.CameraControls camera;
	public void CoordinateClicked(MapCoordinate coord)
	{
		camera = BS.addon.MS.FindChild("CameraControls") as MS.CameraControls; 
		camera.FocusOnMapCoordinate(coord);
	}

	private void WhenFilterChanged(string newText)
	{
		/*
		foreach (Node child in container.GetChildren())
		{
			if(child is BuildLogEntry entry)
			{
				entry.Visible = IsInFilter(entry);
			}
		}*/
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

	void ClearLog(object sender, EventArgs args)
	{
		if(debug){ GD.Print($"BuildLogTab::ClearLog()"); }
		logRichText.Text = string.Empty;
		logRichText.Clear();
	}

	void AddEntry(BuildLogEventArgument args)
	{
		if(debug){ GD.Print($"BuildLogTab::AddEntry()"); }
		logRichText.Text += args.BuildRichText(cbShowSource.ButtonPressed);
	}
	bool IsInFilter(BuildLogEntry entry)
	{
		return entry.Text.ToLower().Contains(leFilter.Text.ToLower());
	}

	private void WhenMetaClicked(Variant meta)
	{
		if (meta.AsString().Contains("location"))
		{
			string[] parts = meta.AsString().Split('/');
			if (parts.Length > 1)
			{
				string[] locStr = parts[1].Split('.');
				if (int.TryParse(locStr[0], out int x))
				{
					if (int.TryParse(locStr[1], out int y))
					{
						if (int.TryParse(locStr[2], out int z))
						{
							Vector3I loc = new Vector3I(x, y, z);
							CoordinateClicked(new MapCoordinate(loc));
						}
					}
				}
			}
		}
	}
}// EOF CLASS