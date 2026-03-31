// Gone through at v1.3
using Godot;

namespace MDunGen.Resources;

[GlobalClass, Tool]
public partial class FloorResource : DungeonAddonResource
{
    [Export] public BuildRuleResource[] rules;
}// EOF CLASS