// Gone through at v1.3
using Godot;
using Godot.Collections;
using MDunGen.Commons;
using System;
using System.Linq;

namespace MDunGen.Resources;

[GlobalClass, Tool]
public partial class BiomeResource : DungeonAddonResource
{
	//[Export] public Vector3I size = Vector3I.One * 6;
	[ExportCategory("Debug")]
	[Export] public BiomeEntryResource[] debug;
	[ExportCategory("Walls")]
	[Export] public Material[] wall_materials;
	[Export] public BiomeEntryResource[] walls;
	[ExportCategory("Floors")]
	[Export] public Material[] floor_materials;
	[Export] public BiomeEntryResource[] floors;
	[ExportCategory("Ceilings")]
	[Export] public Material[] ceiling_materials;
	[Export] public BiomeEntryResource[] ceilings;
	[ExportCategory("Extras")]
	[Export] public BiomeEntryResource[] extras;

	public BiomeResource() { }

	#region Inspector Integration
	public override bool _PropertyCanRevert(StringName property)
	{
		string str = property.ToString();
		if (str == "debug" || str == "walls" || str == "floors" || str == "ceilings" || str == "extras"
		|| str == "wall_materials" || str == "floor_materials" || str == "ceiling_materials")
		{
			return true;
		}
		return base._PropertyCanRevert(property);
	}

	public override Variant _PropertyGetRevert(StringName property)
	{
		switch (property.ToString())
		{
			case "debug":
				return ConstructDefaultDebug();
			case "wall_materials":
				Material[] wallMats = [ResourceLoader.Load<Material>("res://addons/MDunGen/Materials/def_wall.tres")];
				return wallMats;
			case "walls":
				return ConstructDefaultWalls();
			case "floor_materials":
				Material[] floorMats = [ResourceLoader.Load<Material>("res://addons/MDunGen/Materials/def_floor.tres")];
				return floorMats;
			case "floors":
				return ConstructDefaultFloors();
			case "ceiling_materials":
				Material[] ceilingMats = [ResourceLoader.Load<Material>("res://addons/MDunGen/Materials/def_ceiling.tres")];
				return ceilingMats;
			case "ceilings":
				return ConstructDefaultCeilings();
			case "extras":
				return ConstructDefaultExtras();
		}
		return base._PropertyGetRevert(property);
	}

	private Variant ConstructDefaultExtras()
	{
		BiomeEntryResource[] result = [
			new BiomeEntryResource(){
					ResourceName = "Arches",
					key = PIECEKEYS.ARCH,
					resources = [
						ResourceLoader.Load<Resource>("res://addons/MDunGen/Scenes/Standard/def_arch.tscn"),
						ResourceLoader.Load<Resource>("res://addons/MDunGen/Scenes/Standard/def_arch_corner.tscn"),
						ResourceLoader.Load<Resource>("res://addons/MDunGen/Scenes/Standard/def_hallway_arch.tscn"),
					]
				},
			new BiomeEntryResource(){
					ResourceName = "Railings",
					key = PIECEKEYS.RAILING,
					resources = [
						ResourceLoader.Load<Resource>("res://addons/MDunGen/Meshes/Modular/Railings/Default/Long.res"),
						ResourceLoader.Load<Resource>("res://addons/MDunGen/Meshes/Modular/Railings/Default/CornerRounded.res"),
					]
				},
			new BiomeEntryResource(){
					ResourceName = "Ladders",
					key = PIECEKEYS.MISC,
					resources = [
						ResourceLoader.Load<Resource>("res://addons/MDunGen/Meshes/Standard/Standard_LadderFull.res"),
						ResourceLoader.Load<Resource>("res://addons/MDunGen/Meshes/Standard/Standard_LadderStop.res"),
					]
				},
			new BiomeEntryResource(){
					ResourceName = "Supports",
					key = PIECEKEYS.SUPPORT,
					resources = [
						ResourceLoader.Load<Resource>("res://addons/MDunGen/Meshes/Modular/Supports/Default/Long.res"),
						ResourceLoader.Load<Resource>("res://addons/MDunGen/Meshes/Modular/Supports/Default/CornerRounded.res"),
					]
				}
		];
		return result;
	}

	private Variant ConstructDefaultCeilings()
	{
		BiomeEntryResource[] result = [
			new BiomeEntryResource(){
					ResourceName = "Ceilings",
					key = PIECEKEYS.C,
					resources = [
						ResourceLoader.Load<Resource>("res://addons/MDunGen/Scenes/Standard/def_ceiling01.tscn"),
					]
				}
		];
		return result;
	}

	private Variant ConstructDefaultFloors()
	{
		BiomeEntryResource[] result = [
			new BiomeEntryResource(){
				ResourceName = "Floors",
				key = PIECEKEYS.F,
				resources = [
					ResourceLoader.Load<Resource>("res://addons/MDunGen/Scenes/Standard/def_floor01.tscn"),
					ResourceLoader.Load<Resource>("res://addons/MDunGen/Scenes/Standard/def_floor02.tscn"),
					ResourceLoader.Load<Resource>("res://addons/MDunGen/Scenes/Standard/def_floor03.tscn"),
					ResourceLoader.Load<Resource>("res://addons/MDunGen/Scenes/Standard/def_hallway_floor01.tscn"),
					ResourceLoader.Load<Resource>("res://addons/MDunGen/Scenes/Standard/def_hallway_floor01.tscn"),
				]
			}
		];
		return result;
	}

	private Variant ConstructDefaultWalls()
	{
		BiomeEntryResource[] result = [
		new BiomeEntryResource(){
				ResourceName = "Walls",
				key = PIECEKEYS.W,
				resources = [
					ResourceLoader.Load<Resource>("res://addons/MDunGen/Scenes/Standard/def_wall01.tscn"),
					ResourceLoader.Load<Resource>("res://addons/MDunGen/Scenes/Standard/def_wall02.tscn"),
					ResourceLoader.Load<Resource>("res://addons/MDunGen/Scenes/Standard/def_wall03.tscn"),
				]
			},
		new BiomeEntryResource(){
				ResourceName = "Openings",
				key = PIECEKEYS.WD,
				resources = [
					ResourceLoader.Load<Resource>("res://addons/MDunGen/Scenes/Standard/def_wall_opening01.tscn"),
					ResourceLoader.Load<Resource>("res://addons/MDunGen/Scenes/Standard/def_wall_opening02.tscn"),
				]
			},
		new BiomeEntryResource(){
				ResourceName = "Wide Openings",
				key = PIECEKEYS.WDW,
				resources = [
					ResourceLoader.Load<Resource>("res://addons/MDunGen/Scenes/Standard/def_wall_opening_wide.tscn"),
				]
			},
		new BiomeEntryResource(){
				ResourceName = "Corners",
				key = PIECEKEYS.WCI,
				resources = [
					ResourceLoader.Load<Resource>("res://addons/MDunGen/Scenes/Standard/def_wall_corner.tscn"),
				]
			}
	];
		return result;
	}

	private Variant ConstructDefaultDebug()
	{
		BiomeEntryResource[] result = [
			new BiomeEntryResource(){
				ResourceName = "Debug Visuals",
				key = PIECEKEYS.DEBUG,
				resources = [
					ResourceLoader.Load<Resource>("res://addons/MDunGen/Meshes/Standard/Standard_dbError.res"),
					ResourceLoader.Load<Resource>("res://addons/MDunGen/Meshes/Standard/Standard_dbArrow.res"),
					ResourceLoader.Load<Resource>("res://addons/MDunGen/Meshes/Standard/Standard_dbWallFlagGreen.res"),
					ResourceLoader.Load<Resource>("res://addons/MDunGen/Meshes/Standard/Standard_dbWallFlagRed.res"),
					ResourceLoader.Load<Resource>("res://addons/MDunGen/Meshes/Standard/Standard_dbFaulty.res"),
					ResourceLoader.Load<Resource>("res://addons/MDunGen/Meshes/Standard/Standard_dbEnd.res")
				]
			}
		];
		return result;
	}
	#endregion

	#region Access
	internal bool GetResource(PIECEKEYS key, int variantID, out Resource result)
	{
		result = null;
		BiomeEntryResource entry = null;
		if (debug.Any(p => p.key == key)) { entry = debug.First(p => p.key == key); }
		if (entry is null) { if (walls.Any(p => p.key == key)) { entry = walls.First(p => p.key == key); } }
		if (entry is null) { if (floors.Any(p => p.key == key)) { entry = floors.First(p => p.key == key); } }
		if (entry is null) { if (ceilings.Any(p => p.key == key)) { entry = ceilings.First(p => p.key == key); } }
		if (entry is null) { if (extras.Any(p => p.key == key)) { entry = extras.First(p => p.key == key); } }
		if (entry is null) { return false; }
		if (entry.resources.Length < 1)
		{
			GD.PrintErr($"BiomeDefinition::GetResource({key}, {variantID}) No resources setup under that key in [{ResourcePath}]");
			return false;
		}
		if (variantID > 0 && variantID < entry.resources.Length)
		{
			result = entry.resources[variantID];
			return true;
		}
		if (variantID >= entry.resources.Length)
		{
			GD.PrintErr($"BiomeDefinition::GetResource({key}, {variantID}) Variant don't exist in [{ResourcePath}]");
		}
		result = entry.resources[0];
		return true;
	}
	#endregion
}// EOF CLASS