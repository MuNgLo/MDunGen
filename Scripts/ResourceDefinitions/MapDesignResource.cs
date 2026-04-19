using System;
using Godot;
using Godot.Collections;

namespace MDunGen.Resources;

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
	[Export(PropertyHint.ResourceType, "BiomeResource")]
	internal Resource defaultBiome;
	[ExportToolButton("Add")]
	Callable addDesignResource => Callable.From(AddDesignResource);
	[ExportToolButton("Remove")]
	Callable removeDesignResource => Callable.From(RemoveDesignResource);
	[ExportToolButton("Next Level")]
	Callable addNextLevel => Callable.From(AddNextLevel);
	[ExportToolButton("Loop")]
	Callable addLoop => Callable.From(AddLoop);



	internal Array<Resource> designRules = new Array<Resource>();

	internal MapDesignResource()
	{
		designRules = new Array<Resource>();
		defaultBiome = ResourceLoader.Load<Resource>("res://addons/MDunGen/Config/Biomes/def_biome.tres");
		designRules.Add(ConstructNewStartRoom());
	}

	internal void SetSingleSectionDesign(DesignResource design)
	{
		designRules = new Array<Resource>() { design };
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
					return ConstructNewStartRoom();
				}
				return ConstructNewStandardRoom();
			}
		}
		return base._PropertyGetRevert(property);
	}

	private void AddDesignResource()
	{
		if (designRules.Count < 1)
		{
			designRules.Add(ConstructNewStartRoom());
		}
		else
		{
			designRules.Add(ConstructNewStandardRoom());
		}
		NotifyPropertyListChanged();
	}
	private void AddNextLevel()
	{
		designRules.Add(ConstructNewNextLevel());
		NotifyPropertyListChanged();
	}

	private void AddLoop()
	{
		designRules.Add(new Loop());
		NotifyPropertyListChanged();
	}

	private Resource ConstructNewNextLevel()
	{
		return new IncreaseLevel()
		{
			ResourceName = "Next Level " + (CountRules<IncreaseLevel>() + 1)
		};
	}
	private Resource ConstructNewStartRoom()
	{
		return new BuildSection()
		{
			ResourceName = "DefaultStartRoom",
			location = Commons.LOCATION.CENTER,
			direction = Commons.MAPDIRECTION.ANY,
			section = new SectionResource()
			{
				ResourceName = "DefaultStartRoom",
				sectionType = "RoomSection",
				sectionName = "DefaultStartRoom",
				sizeWidthMin = 5,
				sizeWidthMax = 5,
				sizeDepthMin = 5,
				sizeDepthMax = 5,
				nbFloorsMin = 2,
				nbFloorsMax = 2
			}
		};
	}
	private Resource ConstructNewStandardRoom()
	{
		return new BuildSection()
		{
			ResourceName = "DefaultStandardRoom",
			location = Commons.LOCATION.ATTACHEDTOPREVIOUSSECTION,
			direction = Commons.MAPDIRECTION.PIECE,
			section = new SectionResource()
			{
				ResourceName = "DefaultStandardRoom",
				sectionType = "RoomSection",
				sectionName = "DefaultStandardRoom",
				sizeWidthMin = 3,
				sizeWidthMax = 5,
				sizeDepthMin = 3,
				sizeDepthMax = 5,
				nbFloorsMin = 1,
				nbFloorsMax = 3
			}
		};
	}

	private void RemoveDesignResource()
	{
		if (designRules.Count > 0)
		{
			designRules.RemoveAt(designRules.Count - 1);
			NotifyPropertyListChanged();
		}
	}



	private int CountRules<T>()
	{
		int count = 0;
		for (int i = 0; i < designRules.Count; i++)
		{
			if (designRules[i] is T) { count++; }
		}
		return count;
	}
}// EOF CLASS