using System;
using Godot;

namespace MDunGen.Commons;

public enum BUILDLOGSEVERITY { INFO, WARNING, ERROR }

public class BuildLogEventArgument : EventArgs
{
	public string source = string.Empty;
	public BUILDLOGSEVERITY severity = BUILDLOGSEVERITY.INFO;
	public string message = string.Empty;
	public int levelIndex = -1;
	public int sectionIndex = -1;
	public MapCoordinate[] mapLocations = null;

	public string BuildRichText(bool showSource)
	{
		string str = $"{System.Environment.NewLine}[color={ResolveColor(severity)}]*[/color]";
		if (showSource)
		{
			str += source;
		}
		if(levelIndex != -1)
		{
			str += $"[F{levelIndex}]";
		}
		if(sectionIndex != -1)
		{
			str += $"[S{sectionIndex}]";
		}
		str += message;
		str += AppendLocations();
		return str;
	}

	string AppendLocations()
	{
		string str = string.Empty;
		if (mapLocations is not null && mapLocations.Length > 0)
		{
			foreach (MapCoordinate location in mapLocations)
			{
				str += AppendLocation(location);
			}
		}
		return str;
	}


	string AppendLocation(MapCoordinate coord)
	{
		return $"[color=0093cb][url=location/{coord}]{coord}[/url][/color]";
	}

	private string ResolveColor(BUILDLOGSEVERITY severity)
	{
		switch (severity)
		{
			case BUILDLOGSEVERITY.WARNING:
				return "ece938";
			case BUILDLOGSEVERITY.ERROR:
				return "da3b45";
		}
		return "0093cb";
	}
}// EOF CLASS