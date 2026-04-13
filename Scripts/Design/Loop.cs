// Gone through at v1.3
using Godot;

namespace MDunGen.Design;

[GlobalClass, Tool]
internal partial class Loop : DesignResource
{
    [Export] public int loop = 0;
    [Export] public int stepBack = 1; // 1 being previous rule
}// EOF CLASS