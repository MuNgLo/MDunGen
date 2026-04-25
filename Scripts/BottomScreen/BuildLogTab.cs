using Godot;
using MDunGen.Commons;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MDunGen.Bottom;

[Tool, GlobalClass]
public partial class BuildLogTab : MarginContainer
{
	[Export] bool debug = false;
	[Export] BottomScreen BS;
	[Export] PackedScene prefabLogEntry;
	[Export] RichTextLabel logRichText;
	[Export] LineEdit leFilter;
	[Export] CheckBox cbShowSource;

	List<BuildLogEventArgument> entries;
	public override void _Ready()
	{
		if (debug) { GD.Print($"BuildLogTab::_EnterTree()"); }
		entries = new List<BuildLogEventArgument>();
		logRichText.MetaClicked += WhenMetaClicked;
		leFilter.TextChanged += WhenFilterChanged;
		BS.addon.MS.OnMapDataGenerationStarted += ClearLog;
		BS.addon.MS.OnMapDataGenerationEnded += AddBuildEnd;
		BS.addon.MS.OnMapBuildLog += WhenBuildLog;
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
		ClearLog();
		foreach (BuildLogEventArgument args in entries)
		{
			if (args.message.ToLower().Contains(newText.ToLower()))
			{
				AddEntry(args);
			}
		}
	}

	private void WhenBuildLog(object sender, BuildLogEventArgument args)
	{
		if (debug) { GD.Print($"BuildLogTab::AddBuildLog() {args.message}"); }
		entries.Add(args);
		if (leFilter.Text == string.Empty || args.message.ToLower().Contains(leFilter.Text.ToLower()))
		{
			AddEntry(args);
		}
	}

	void AddBuildEnd(object sender, EventArgs e)
	{
		if (debug) { GD.Print($"BuildLogTab::AddBuildEnd()"); }
	}

	void ClearLog(object sender, EventArgs args)
	{
		entries = new List<BuildLogEventArgument>();
		ClearLog();
	}
	internal void ClearLog()
	{
		if (debug) { GD.Print($"BuildLogTab::ClearLog()"); }
		logRichText.Text = string.Empty;
		logRichText.Clear();
	}

	void AddEntry(BuildLogEventArgument args)
	{
		if (debug) { GD.Print($"BuildLogTab::AddEntry()"); }
		logRichText.Text += args.BuildRichText(cbShowSource.ButtonPressed);
		PushToEnd();
	}

	private async void PushToEnd()
	{
		await Task.Delay(5);
		ScrollContainer sc = logRichText.GetParent<ScrollContainer>();
		sc.ScrollVertical = 20000000;
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