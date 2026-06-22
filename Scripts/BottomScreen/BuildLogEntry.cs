#if TOOLS
using Godot;
using MDunGen.Commons;
using System;

namespace MDunGen.Bottom;

[Tool]
public partial class BuildLogEntry : PanelContainer
{
	[Export] RichTextLabel richText;
	[Export] ColorRect crSeverity;

	public string Text => richText.Text;
	BuildLogTab buildLog;

	public override void _Ready()
	{
		richText.BbcodeEnabled = true;
		richText.MetaClicked += WhenMetaClicked;
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
							buildLog.CoordinateClicked(new MapCoordinate(loc));
						}
					}
				}
			}
		}
	}
	public void SetBuildLog(BuildLogTab buildLog, BuildLogEventArgument arg, bool showSource)
	{
		this.buildLog = buildLog;
		richText.Text = string.Empty;
		if (showSource)
		{
			richText.Text += arg.source;
		}
		richText.Text += arg.message;
		crSeverity.Color = ResolveColor(arg.severity);
		if (arg.mapLocations is not null && arg.mapLocations.Length > 0)
		{
			foreach (MapCoordinate location in arg.mapLocations)
			{
				AppendLocation(location);
			}
		}
	}

	private Color ResolveColor(BUILDLOGSEVERITY severity)
	{
		switch (severity)
		{
			case BUILDLOGSEVERITY.WARNING:
				return Color.FromHtml("ece938");
			case BUILDLOGSEVERITY.ERROR:
				return Color.FromHtml("da3b45");
		}
		return Color.FromHtml("0093cb");
	}

	private void AppendLocation(MapCoordinate location)
	{
		richText.Text += $"[color=0093cb][url=location/{location}]{location}[/url][/color]";
	}
}// EOF CLASS
#endif