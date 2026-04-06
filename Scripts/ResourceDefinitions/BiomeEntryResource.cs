// Gone through at v1.3
using Godot;

namespace MDunGen.Resources;

[GlobalClass, Tool]
public partial class BiomeEntryResource : DungeonAddonResource
{
	[Export] public PIECEKEYS key { get; set; } = PIECEKEYS.NONE;
	/// <summary>
	/// DON'T access this directly Use the GetResource
	/// </summary>
	[Export] public Resource[] resources { get; set; } = null;
	public Resource GetResource(int index)
	{
		if (index < 0 || index >= resources.Length)
		{
			GD.PrintErr($"BiomeEntry::GetResource([{index}]) Variant index out of range for [{key}]. Falling back on default index 0.");
			index = 0;
		}
		;
		if (resources[index] == null)
		{
			GD.PrintErr($"BiomeEntry::GetResource([{index}]) Resource entry is NULL for [{key}]." + (index != 0 ? "Falling back on default index 0." : ""));
			if (index != 0) { return GetResource(0); }
			return null;
		}
		return resources[index];
	}
}// EOF CLASS