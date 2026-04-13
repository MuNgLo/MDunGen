using System;
using Godot;
using Godot.Collections;
using MDunGen.Resources;

namespace MDunGen.Design;

[Tool, GlobalClass]
internal partial class MapDesignResource : DungeonAddonResource
{
	/// <summary>
	/// How many levels the map will have
	/// </summary>
	[Export] internal int nbOfLevel = 1;
	/// <summary>
	/// The biome used when visualizing the data when no other one defined
	/// </summary>
	[Export] internal BiomeResource defaultBiome;
	[ExportToolButton("Add")]
	Callable addDesignResource => Callable.From(AddDesignResource);
	[ExportToolButton("Remove")]
	Callable removeDesignResource => Callable.From(RemoveDesignResource);

	internal Array<Resource> designRules = new Array<Resource>();

	internal MapDesignResource()
	{
		designRules = new Array<Resource>();
		defaultBiome = ResourceLoader.Load<BiomeResource>("res://addons/MDunGen/Config/Biomes/def_biome.tres");
		designRules.Add(ResourceLoader.Load<Resource>("res://addons/MDunGen/BuildRules/StartRoom.tres"));
	}

	internal void SetSingleSectionDesign(DesignResource design)
	{
		designRules = new Array<Resource>(){ design };
	}

	public override Array<Dictionary> _GetPropertyList()
	{
		Array<Dictionary> properties = new Array<Dictionary>();
		string allowedTypes = "Loop,BuildSection,BuildSections,IncreaseLevel";
		for (int i = 0; i < designRules.Count; i++)
		{
			properties.Add(
				new Dictionary
				{
					["name"] = $"designRules/entry_{i}",
					["hint"] = (int)PropertyHint.ResourceType,
					["type"] = (int)Variant.Type.Object,
					["hint_string"] = allowedTypes,
					["usage"] = (long)PropertyUsageFlags.Default
				}
			);
		}
		return properties;
	}



	public override bool _Set(StringName property, Variant value)
	{
		//if (designRules is null) { designRules = new Array<Resource>(); }
		if (value.AsGodotObject() is not DesignResource) { return false; }
		string str = property.ToString();
		if (str.Contains("designRules/entry_"))
		{
			str = str.Replace("designRules/entry_", "");
			if (int.TryParse(str, out int index))
			{
				if (index > designRules.Count - 1)
				{
					designRules.Add(null);
				}
				if (designRules[index] != value.AsGodotObject() as Resource)
				{
					designRules[index] = value.AsGodotObject() as Resource;
					return true;
				}
			}
		}
		return false;
	}



	public override Variant _Get(StringName property)
	{
		string str = property.ToString();
		if (str.Contains("designRules/entry_"))
		{
			str = str.Replace("designRules/entry_", "");
			if (int.TryParse(str, out int index))
			{
				return designRules[index];
			}
		}
		return default;
	}



	public override bool _PropertyCanRevert(StringName property)
	{
		string str = property.ToString();
		if (str == "defaultBiome")
		{
			return true;
		}
		if (str.Contains("designRules/entry_"))
		{
			return true;
		}
		return base._PropertyCanRevert(property);
	}

	public override Variant _PropertyGetRevert(StringName property)
	{
		string str = property.ToString();
		if (str == "defaultBiome")
		{
			return ResourceLoader.Load<Resource>("res://addons/MDunGen/Config/Biomes/def_biome.tres");
		}
		if (str.Contains("designRules/entry_"))
		{
			str = str.Replace("designRules/entry_", "");
			if (int.TryParse(str, out int index))
			{
				if (index == 0)
				{
					return ResourceLoader.Load<Resource>("res://addons/MDunGen/BuildRules/StartRoom.tres");
				}
				return ResourceLoader.Load<Resource>("res://addons/MDunGen/BuildRules/StandardRoom.tres");
			}
		}
		return base._PropertyGetRevert(property);
	}




	private void AddDesignResource()
	{
		designRules.Add(ResourceLoader.Load<Resource>("res://addons/MDunGen/BuildRules/StandardRoom.tres"));
		NotifyPropertyListChanged();
	}
	private void RemoveDesignResource()
	{
		designRules.RemoveAt(designRules.Count - 1);
		NotifyPropertyListChanged();
	}

}// EOF CLASS