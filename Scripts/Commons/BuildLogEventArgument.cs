using System;
using Godot;

namespace MDunGen.Commons;
public enum BUILDLOGSEVERITY { INFO, WARNING, ERROR }

public class BuildLogEventArgument : EventArgs
{
	public string source = string.Empty;
	public BUILDLOGSEVERITY severity = BUILDLOGSEVERITY.INFO;
	public string message = string.Empty;
	public MapCoordinate[] mapLocations = null;

}// EOF CLASS