// Gone through at v1.3
using Godot;
using MDunGen.Commons;

namespace MDunGen.Resources;
/// <summary>
/// How to add a section to the map data
/// </summary>
[GlobalClass, Tool]
internal partial class BuildSection : DesignResource
{
    // Where to start this rule
    [Export] public LOCATION location = LOCATION.ATTACHEDTOPREVIOUSSECTION;
    [Export] public int targetedIndex = -1;
    // Direction
    [Export] public MAPDIRECTION direction = MAPDIRECTION.ANY;
    // Section to insert
    [Export] public SectionResource section;
}// EOF CLASS