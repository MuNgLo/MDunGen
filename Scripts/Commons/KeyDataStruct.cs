// Gone through at v1.3
namespace MDunGen.Commons;
/// <summary>
/// Struct for the key, direction and variant ID
/// </summary>
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