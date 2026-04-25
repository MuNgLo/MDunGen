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

	public static KeyData Empty => new KeyData() { key = PIECEKEYS.NONE, dir = MAPDIRECTION.ANY, variantID = -1 };

	public static KeyData Faulty(MAPDIRECTION dir)
	{
		return new KeyData()
		{
			key = PIECEKEYS.DEBUG,
			dir = dir,
			variantID = (int)DEBUGVARIANTS.FAULTY
		};
	}
	public static KeyData End(MAPDIRECTION dir)
	{
		return new KeyData()
		{
			key = PIECEKEYS.DEBUG,
			dir = dir,
			variantID = (int)DEBUGVARIANTS.END
		};
	}
}// EOF STRUCT