// Gone through at v1.3
using Godot;

namespace MDunGen.Resources;
/// <summary>
/// Contains a collection of BuildSection
/// </summary>
[GlobalClass, Tool]
internal partial class BuildSections : DesignResource
{
    [Export] internal BuildSection[] rules;
}// EOF CLASS