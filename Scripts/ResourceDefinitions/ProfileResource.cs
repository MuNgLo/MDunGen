// Gone through at v1.3
using Godot;
using MDunGen.Design;

namespace MDunGen.Resources;

[GlobalClass, Tool, System.Obsolete("Replaced by MapDesignResource")]
public partial class ProfileResource : DungeonAddonResource
{
	[Export] public bool useRandomSeed = true;
	[Export] public bool showDebugLayer = false;
	[Export] public Vector3 globalOffset = Vector3.Zero;
	[Export] internal MapDesignResource design;
}// EOF CLASS