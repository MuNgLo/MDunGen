// Gone through at v1.3
using Godot;
using MDunGen.Commons;
using System;
namespace MDunGen.PropGrid;

[Obsolete]
public struct SectionProp
{
    public PIECEKEYS key;
    /// <summary>
    /// offset relative to room start location tile and the orientation
    /// </summary>
    public Vector3I position;
    public MAPDIRECTION dir;
    public int variantID;
    public SectionProp(KeyData kData, Vector3I pos)
    {
        this.key = kData.key;
        this.dir = kData.dir;
        this.position = pos;
        this.variantID = kData.variantID;
    }
    public SectionProp(PIECEKEYS key, Vector3I position, MAPDIRECTION dir, int variantID = -1)
    {
        this.key = key;
        this.position = position;
        this.dir = dir;
        this.variantID = variantID;
    }
    public SectionProp(PIECEKEYS key, Vector3 position, MAPDIRECTION dir, int variantID = -1)
    {
        this.key = key;
        this.position = (Vector3I)position;
        this.dir = dir;
        this.variantID = variantID;
    }
}// EOF STRUCT