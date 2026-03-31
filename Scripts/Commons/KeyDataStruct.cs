// Gone through at v1.3
namespace MDunGen.Commons;

public struct KeyData
{
    public PIECEKEYS key;
    public MAPDIRECTION dir;
    public int variantID;

    public override string ToString()
    {
        return $"key[{key}] dir[{dir}]";
    }
}// EOF STRUCT