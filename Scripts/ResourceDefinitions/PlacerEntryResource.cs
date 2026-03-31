// Gone through at v1.3
using Godot;
using System;

namespace MDunGen.Resources;

[Tool, GlobalClass, Obsolete("Placers not a thing anymore")]
public partial class PlacerEntryResource : DungeonAddonResource
{
	[Export] public bool active = true;
	[Export] public int count = 1;
	[Export] public PlacerResource placer;


	public string Name { get { return placer is not null ? placer.ResourceName : "UnNamed"; } }


}// EOF CLASS