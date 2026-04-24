// Gone through at v1.3
#if TOOLS
using Godot;
using MDunGen.Commons;
using MDunGen.Resources;
using MDunGen.Sections;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MDunGen.MS;

/// <summary>
/// The class that builds and updates the visual representation of the map data in the Dungeon Viewer main screen dock
/// </summary>
[Tool, GlobalClass]
public partial class ScreenDungeonVisualizer : Node3D
{
	[Export] bool debug;

	private MainScreen mainScreen;
	private AddonSettingsResource MasterConfig => mainScreen.addon.MasterConfig;
	private Node3D mapContainer;
	private Node3D propContainer;
	private Node3D tileContainer;
	private Node3D debugContainer;



	public override void _EnterTree()
	{
		mainScreen = GetParent().GetParent().GetParent() as MainScreen;
	}



	/// <summary>
	/// Updates the visuals
	/// Obeying the level start and level end
	/// </summary>
	public async void ReDrawMap()
	{
		mainScreen.RaiseNotification($"Generating Visuals");
		if (debug) { GD.Print($"ScreenDungeonVisualizer::ReDrawMap() MapData.Levels[{mainScreen.Map.Levels}]"); }
		cacheKeyedPieces = new Dictionary<PIECEKEYS, Dictionary<int, Resource>>();
		mapContainer = FindChild("Generated") as Node3D;
		debugContainer = FindChild("GeneratedDebug") as Node3D;
		for (int i = 0; i < mainScreen.Map.Levels; i++)
		{
			GetLevelContainer(i);
			GetLevelDebugContainer(i);
			if (i < MasterConfig.visibleLevelsStart || i > MasterConfig.visibleLevelEnd) { ClearLevel(i); ClearDebugLevel(i); continue; }
			VisualizeLevel(i, mainScreen.Map.DefaultBiome);
			await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
		}
		mainScreen.RaiseNotification($"Done");
	}

	private void VisualizeLevel(int level, BiomeResource biome)
	{
		//GD.Print($"ScreenDungeonVisualizer::VisualizeLevel({level})");
		ClearLevel(level);

		foreach (ISection section in mainScreen.Map.Sections)
		{
			if (section.LevelIndex == level)
			{
				VisualizeSection(section, biome);
			}
		}
	}


	/// <summary>
	/// Removes the visuals for a specific level
	/// </summary>
	/// <param name="levelIndex"></param>
	internal void ClearLevel(int levelIndex)
	{
		Node3D levelContainer = GetLevelContainer(levelIndex);

		if (levelContainer == null)
		{
			GD.Print($"ScreenDungeonVisualizer::ClearLevel()  level container node missing!");
			return;
		}
		foreach (Node child in levelContainer.GetChildren())
		{
			if (child is Node3D)
			{
				child.QueueFree();
			}
		}
	}
	/// <summary>
	/// Removes the visuals for a specific level
	/// </summary>
	/// <param name="level"></param>
	internal void ClearDebugLevel(int level)
	{
		Node3D levelContainer = GetLevelDebugContainer(level);

		if (levelContainer == null)
		{
			GD.Print($"ScreenDungeonVisualizer::ClearDebugLevel()  level container node missing!");
			return;
		}
		foreach (Node child in levelContainer.GetChildren())
		{
			if (child is Node3D)
			{
				child.QueueFree();
			}
		}
	}
	/// <summary>
	/// Clears All the visuals
	/// </summary>
	internal void ClearAllLevels()
	{
		Node3D generated = GetNode<Node3D>("Generated");
		if (generated == null)
		{
			GD.Print($"ScreenDungeonVisualizer::ClearAllLevels()  Generated node missing!");
			return;
		}
		foreach (Node child in generated.GetChildren())
		{
			if (child is Node3D)
			{
				child.QueueFree();
			}
		}
	}
	/// <summary>
	/// Clears All the debug visuals 
	/// </summary>
	internal void ClearAllLevelsDebug()
	{
		Node3D generated = GetNode<Node3D>("GeneratedDebug");
		if (generated == null)
		{
			GD.Print($"ScreenDungeonVisualizer::ClearAllLevelsDebug()  GeneratedDebug node missing!");
			return;
		}
		foreach (Node child in generated.GetChildren())
		{
			if (child is Node3D)
			{
				child.QueueFree();
			}
		}
	}
	/// <summary>
	/// Gets the level parent. Creates if needed
	/// </summary>
	/// <param name="level"></param>
	/// <returns></returns>
	private Node3D GetLevelContainer(int level)
	{
		if (mapContainer.GetChildCount() < level + 1)
		{
			Node3D node = new Node3D() { Name = string.Format("{0:000}" + "Level", level) };
			mapContainer.AddChild(node, true);
		}
		return mapContainer.GetChild(level) as Node3D;
	}
	/// <summary>
	/// Gets the level parent. Creates if needed
	/// </summary>
	/// <param name="level"></param>
	/// <returns></returns>
	private Node3D GetLevelDebugContainer(int level)
	{
		if (debugContainer.GetChildCount() < level + 1)
		{
			Node3D node = new Node3D();
			node.Name = string.Format("{0:000}" + "Level", level);
			debugContainer.AddChild(node, true);
		}
		return debugContainer.GetChild(level) as Node3D;
	}



	private void VisualizeSection(ISection section, BiomeResource biome)
	{
		//GD.Print($"ScreenDungeonVisualizer::VisualizeSection() section is null[{section is null}]");
		if (section == null) { return; }
		section.SectionContainer = new Node3D();
		tileContainer = new Node3D();
		section.SectionContainer.Name = "S" + string.Format("{0:000}", section.SectionIndex);
		tileContainer.Name = $"Tiles[{section.Pieces.Count}]";
		GetLevelContainer(section.LevelIndex).AddChild(section.SectionContainer, true);
		section.SectionContainer.AddChild(tileContainer, true);
		// Section Tiles
		int index = 0;
		foreach (MapPiece rp in section.Pieces)
		{
			MapPiece piece = mainScreen.Map.GetPiece(rp.Coord);
			if (BuildVisualNode(biome, piece, out Node3D visualNode, propContainer, true))
			{
				visualNode.Name = $"S{string.Format("0:000", section.SectionIndex)}-T{index}";
				tileContainer.AddChild(visualNode, true);
				visualNode.Position = DungeonUtils.GlobalPosition(piece);
				visualNode.Show();
				index++;
			}
		}
		//GD.Print($"ScreenVis::VisualizeSection() section.WaterMaterial is null[{section.WaterMaterial is null}]");
		// Add water
		if (section.WaterMaterial is not null)
		{
			DungeonUtils.BuildWaterPlane(section);
		}
	}
	/// <summary>
	/// Decodes and instantiates the nodes needed for the map piece data
	/// </summary>
	/// <param name="biome"></param>
	/// <param name="piece"></param>
	/// <param name="makeCollider"></param>
	internal bool BuildVisualNode(BiomeResource biome, MapPiece piece, out Node3D visualNode, Node3D propParent, bool makeCollider = true)
	{
		visualNode = new Node3D();
		visualNode.Name = piece.CoordString;

		// generate floors visuals
		if (mainScreen.addon.MasterConfig.showFloors)
		{
			if (piece.keyFloor.key != PIECEKEYS.NONE && piece.keyFloor.key != PIECEKEYS.OCCUPIED &&
				GetByKey(piece.keyFloor, biome, out Node3D floor, makeCollider))
			{
				DungeonUtils.ApplyMaterialOverrides(floor, biome.floor_materials);
				visualNode.AddChild(floor, true);
			}
		}
		// generate ceiling
		if (mainScreen.addon.MasterConfig.showCeilings)
		{
			if (piece.keyCeiling.key != PIECEKEYS.NONE && GetByKey(piece.keyCeiling, biome, out Node3D ceiling, makeCollider))
			{
				DungeonUtils.ApplyMaterialOverrides(ceiling, biome.ceiling_materials);
				visualNode.AddChild(ceiling, true);
			}
		}
		// generate walls
		if (mainScreen.addon.MasterConfig.showWalls)
		{
			for (int i = 1; i < 9; i *= 2)
			{
				if (piece.Walls.HasFlag((WALLS)i))
				{
					if (GetByKey(piece.WallKey((MAPDIRECTION)Math.Log2(i) + 1), biome, out Node3D wall, makeCollider))
					{
						DungeonUtils.ApplyMaterialOverrides(wall, biome.wall_materials);
						visualNode.AddChild(wall, true);
					}
				}
			}
			SpecialCaseRoundedCorners(piece, visualNode, biome, makeCollider);
		}
		// generate extras
		if (mainScreen.addon.MasterConfig.showExtras)
		{
			foreach (KeyData extra in piece.Extras)
			{
				if (GetByKey(extra, biome, out Node3D ext, makeCollider))
				{
					DungeonUtils.ApplyMaterialOverrides(ext, biome.wall_materials);
					visualNode.AddChild(ext, true);
				}
			}
		}
		if (mainScreen.addon.MasterConfig.showDebug)
		{
			foreach (KeyData dbg in piece.Debug)
			{
				if (GetByKey(dbg, biome, out Node3D ext, false))
				{
					visualNode.AddChild(ext, true);
				}
			}
		}
		return true;
	}

	/// <summary>
	/// Not flagged as wall but check for rounded corner keys
	/// </summary>
	/// <param name="piece"></param>
	/// <param name="visualNode">parent</param>
	/// <param name="biome"></param>
	/// <param name="makeCollider"></param>
	private void SpecialCaseRoundedCorners(MapPiece piece, Node3D visualNode, BiomeResource biome, bool makeCollider)
	{
		if (piece.WallKeyNorth.key == PIECEKEYS.WCI)
		{
			if (GetByKey(piece.WallKeyNorth, biome, out Node3D wall, makeCollider))
			{
				DungeonUtils.ApplyMaterialOverrides(wall, biome.wall_materials);

				visualNode.AddChild(wall, true);
			}
		}
		if (piece.WallKeyEast.key == PIECEKEYS.WCI)
		{
			if (GetByKey(piece.WallKeyEast, biome, out Node3D wall, makeCollider))
			{
				DungeonUtils.ApplyMaterialOverrides(wall, biome.wall_materials);
				visualNode.AddChild(wall, true);
			}
		}
		if (piece.WallKeySouth.key == PIECEKEYS.WCI)
		{
			if (GetByKey(piece.WallKeySouth, biome, out Node3D wall, makeCollider))
			{
				DungeonUtils.ApplyMaterialOverrides(wall, biome.wall_materials);
				visualNode.AddChild(wall, true);
			}
		}
		if (piece.WallKeyWest.key == PIECEKEYS.WCI)
		{
			if (GetByKey(piece.WallKeyWest, biome, out Node3D wall, makeCollider))
			{
				DungeonUtils.ApplyMaterialOverrides(wall, biome.wall_materials);
				visualNode.AddChild(wall, true);
			}
		}
	}
	/// <summary>
	/// returns a Node with the correct rotation
	/// </summary>
	/// <param name="data"></param>
	/// <param name="biome"></param>
	/// <param name="obj"></param>
	/// <param name="makeCollider"></param>
	/// <returns></returns>
	private bool GetByKey(KeyData data, BiomeResource biome, out Node3D obj, bool makeCollider)
	{
		if (data.key == PIECEKEYS.NONE || data.key == PIECEKEYS.OCCUPIED) { obj = null; return false; }
		Resource res = ResolveAndCache(data, biome);
		if (res == null) { obj = null; return false; }


		// Split depending if Mesh or Prefab
		if (res is Mesh)
		{
			obj = new MeshInstance3D() { Mesh = res as Mesh };
			if (makeCollider) { (obj as MeshInstance3D).CreateConvexCollision(); }
		}
		else
		{
			obj = (res as PackedScene).Instantiate() as Node3D;
			if (obj == null)
			{
				GD.Print($"DungeonGenerator::GetByKey() Key was {data.key} resolving packed scene resulted in NULL!");
				return false;
			}
		}
		obj.Name = data.key.ToString() + "-" + data.dir.ToString();
		if (data.dir != MAPDIRECTION.ANY) { obj.RotationDegrees = DungeonUtils.ResolveRotation(data.dir); } else { obj.RotationDegrees = Vector3.Up * 45.0f; }
		return true;
	}

	private Dictionary<PIECEKEYS, Dictionary<int, Resource>> cacheKeyedPieces;

	private Resource ResolveAndCache(KeyData data, BiomeResource biome)
	{
		if (biome is null) { GD.PushError("ScreenDungeonVisualizer::ResolveAndCache() BIOME GIVEN AS NULL!!"); return null; }

		if (cacheKeyedPieces == null) { cacheKeyedPieces = new Dictionary<PIECEKEYS, Dictionary<int, Resource>>(); }

		if (!cacheKeyedPieces.ContainsKey(data.key)) { cacheKeyedPieces[data.key] = new Dictionary<int, Resource>(); }

		if (!cacheKeyedPieces[data.key].ContainsKey(data.variantID))
		{
			if (biome.GetResource(data.key, data.variantID, out Resource result))
			{
				cacheKeyedPieces[data.key][data.variantID] = result;
			}
			if (biome.debug.Any(p => p.key == data.key))
			{
				cacheKeyedPieces[data.key][data.variantID] = biome.debug.First(p => p.key == data.key).GetResource(data.variantID);
			}
			else if (biome.walls.Any(p => p.key == data.key))
			{
				cacheKeyedPieces[data.key][data.variantID] = biome.walls.First(p => p.key == data.key).GetResource(data.variantID);
			}
			else if (biome.floors.Any(p => p.key == data.key))
			{
				cacheKeyedPieces[data.key][data.variantID] = biome.floors.First(p => p.key == data.key).GetResource(data.variantID);
			}
			else if (biome.ceilings.Any(p => p.key == data.key))
			{
				cacheKeyedPieces[data.key][data.variantID] = biome.ceilings.First(p => p.key == data.key).GetResource(data.variantID);
			}
			else if (biome.extras.Any(p => p.key == data.key))
			{
				cacheKeyedPieces[data.key][data.variantID] = biome.extras.First(p => p.key == data.key).GetResource(data.variantID);
			}
		}
		if (!cacheKeyedPieces.ContainsKey(data.key))
		{
			GD.PushError($"ResolveAndCache Key [{data.key}] was not found!");
			return null;
		}
		if (!cacheKeyedPieces[data.key].ContainsKey(data.variantID))
		{
			if (!cacheKeyedPieces[data.key].ContainsKey(0))
			{
				GD.PushError($"ResolveAndCache Key [{data.key}] Variant [{data.variantID}] was not found! And Default fallback failed!");
				return null;
			}
			GD.PushError($"ResolveAndCache Key [{data.key}] Variant [{data.variantID}] was not found! Default used as fallback.");
			return cacheKeyedPieces[data.key][0];
		}
		if(cacheKeyedPieces[data.key][data.variantID] is null)
		{
			GD.PushError($"ResolveAndCache Key [{data.key}] Variant [{data.variantID}] is NULL.");
		}
		return cacheKeyedPieces[data.key][data.variantID];
	}

}// eof class
#endif