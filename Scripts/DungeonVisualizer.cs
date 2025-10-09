#define MConsole
using Godot;
using Munglo.DungeonGenerator.Sections;
using LPlayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Munglo.DungeonGenerator;

[GlobalClass]
public partial class DungeonVisualizer : Node3D
{
    public static EventHandler<ISection> OnSectionVisualized;
    [Export] private BiomeResource biome; // TODO refactor biome into sectionbase

    private Dictionary<PIECEKEYS, Dictionary<int, Resource>> cacheKeyedPieces;
    private NavigationRegion3D mapContainer;
    private Node3D propContainer;
    private Node3D tileContainer;
    private Node3D unsortedContainer;

    public async void ShowMap()
    {
        await VisualizeSections();
        BuildNavMesh();
    }
    public void ClearVisualizer()
    {
        cacheKeyedPieces = new Dictionary<PIECEKEYS, Dictionary<int, Resource>>();
        mapContainer = new NavigationRegion3D();
        mapContainer.NavigationMesh = new NavigationMesh();
        mapContainer.Name = "Generated";
        AddChild(mapContainer, true);

        unsortedContainer = new Node3D();
        unsortedContainer.Name = "UnSorted";
        mapContainer.AddChild(unsortedContainer, true);
    }

    private async Task VisualizeSections()
    {
        Log($"Dungeon : Visualizing sections");

        foreach (ISection section in Core.GetDungeon.Map.Sections)
        {
            await VisualizeSection(section);
        }
    }

    private async Task VisualizeSection(ISection section)
    {
        if (section == null) { return; }
        section.SectionContainer = new Node3D();
        propContainer = new Node3D();
        tileContainer = new Node3D();
        section.SectionContainer.Name = "S" + string.Format("{0:000}", section.SectionIndex);
        propContainer.Name = $"Props[{section.PropCount}]";
        tileContainer.Name = $"Tiles[{section.Pieces.Count}]";
        GetFloorContainer(section.Coord.y).AddChild(section.SectionContainer, true);
        section.SectionContainer.AddChild(propContainer, true);
        section.SectionContainer.AddChild(tileContainer, true);
        // Section Tiles
        int index = 0;
        foreach (MapPiece rp in section.Pieces)
        {
            MapPiece piece = Core.GetDungeon.Map.GetPiece(rp.Coord);
            if (BuildVisualNode(biome, piece, out Node3D visualNode, propContainer, true))
            {
                visualNode.Name = $"S{string.Format("0:000", section.SectionIndex)}-T{index}";
                tileContainer.AddChild(visualNode, true);
                visualNode.Position = DungeonUtils.GlobalPosition(piece);
                visualNode.Show();
                index++;
            }
            await Task.Delay(10);
        }
        // Add water
        if (section.WaterMaterial is not null)
        {
            DungeonUtils.BuildWaterPlane(section);
        }
        OnSectionVisualized?.Invoke(this, section);
    }
    /// <summary>
    /// Gets the floor parent. Creates if needed
    /// </summary>
    /// <param name="y"></param>
    /// <returns></returns>
    private Node3D GetFloorContainer(int y)
    {
        if (mapContainer.GetChildCount() < y + 1)
        {
            Node3D node = new Node3D();
            node.Name = string.Format("{0:000}" + "Floor", y);
            mapContainer.AddChild(node, true);
        }
        return mapContainer.GetChild(y) as Node3D;
    }
    internal void ResetVisuals()
    {
        foreach (Node item in GetNode<NavigationRegion3D>("Generated").GetChildren())
        {
            item.QueueFree();
        }
    }

    /// <summary>
    /// Decodes and instantiates the nodes needed for the map piece data
    /// </summary>
    /// <param name="biome"></param>
    /// <param name="piece"></param>
    /// <param name="makeCollider"></param>
    /// 
    internal bool BuildVisualNode(BiomeResource biome, MapPiece piece, out Node3D visualNode, Node3D propParent, bool makeCollider = true)
    {
        visualNode = new Node3D();
        visualNode.Name = piece.CoordString;

        // generate floors
        if (piece.keyFloor.key != PIECEKEYS.NONE && piece.keyFloor.key != PIECEKEYS.OCCUPIED &&
            GetByKey(piece.keyFloor, biome, out Node3D floor, makeCollider))
        {
            DungeonUtils.ApplyMaterialOverrides(floor, biome.floorMaterials);
            visualNode.AddChild(floor);
        }
        // generate walls
        if (piece.Walls.HasFlag(WALLS.N))
        {
            if (GetByKey(piece.WallKeyNorth, biome, out Node3D wall, makeCollider))
            {
                DungeonUtils.ApplyMaterialOverrides(wall, biome.wallMaterials);
                visualNode.AddChild(wall);
            }
            ;
        }
        if (piece.Walls.HasFlag(WALLS.E))
        {
            if (GetByKey(piece.WallKeyEast, biome, out Node3D wall, makeCollider))
            {
                DungeonUtils.ApplyMaterialOverrides(wall, biome.wallMaterials);
                visualNode.AddChild(wall);
            }
            ;
        }
        if (piece.Walls.HasFlag(WALLS.S))
        {
            if (GetByKey(piece.WallKeySouth, biome, out Node3D wall, makeCollider))
            {
                DungeonUtils.ApplyMaterialOverrides(wall, biome.wallMaterials);
                visualNode.AddChild(wall);
            }
            ;
        }
        if (piece.Walls.HasFlag(WALLS.W))
        {
            if (GetByKey(piece.WallKeyWest, biome, out Node3D wall, makeCollider))
            {
                DungeonUtils.ApplyMaterialOverrides(wall, biome.wallMaterials);
                visualNode.AddChild(wall);
            }
            ;
        }



        // Not flagged as wall but check for rounded corner keys
        if (piece.WallKeyNorth.key == PIECEKEYS.WCI)
        {
            if (GetByKey(piece.WallKeyNorth, biome, out Node3D wall, makeCollider))
            {
                DungeonUtils.ApplyMaterialOverrides(wall, biome.wallMaterials);
                visualNode.AddChild(wall);
            }
        }
        if (piece.WallKeyEast.key == PIECEKEYS.WCI)
        {
            if (GetByKey(piece.WallKeyEast, biome, out Node3D wall, makeCollider))
            {
                DungeonUtils.ApplyMaterialOverrides(wall, biome.wallMaterials);

                visualNode.AddChild(wall);
            }

        }
        if (piece.WallKeySouth.key == PIECEKEYS.WCI)
        {
            if (GetByKey(piece.WallKeySouth, biome, out Node3D wall, makeCollider))
            {
                DungeonUtils.ApplyMaterialOverrides(wall, biome.wallMaterials);
                visualNode.AddChild(wall);
            }

        }
        if (piece.WallKeyWest.key == PIECEKEYS.WCI)
        {
            if (GetByKey(piece.WallKeyWest, biome, out Node3D wall, makeCollider))
            {
                DungeonUtils.ApplyMaterialOverrides(wall, biome.wallMaterials);
                visualNode.AddChild(wall);
            }

        }




        // generate ceiling
        if (piece.keyCeiling.key != PIECEKEYS.NONE && GetByKey(piece.keyCeiling, biome, out Node3D ceiling, makeCollider))
        {
            DungeonUtils.ApplyMaterialOverrides(ceiling, biome.ceilingMaterials);
            visualNode.AddChild(ceiling);
        }



        foreach (KeyData extra in piece.Extras)
        {
            if (GetByKey(extra, biome, out Node3D ext, makeCollider))
            {
                DungeonUtils.ApplyMaterialOverrides(ext, biome.wallMaterials);
                visualNode.AddChild(ext, true);
            }
        }
        return true;
    }



    /// <summary>
    /// returns a Node with the correct rotation
    /// </summary>
    /// <param name="data"></param>
    /// <param name="biome"></param>
    /// <param name="obj"></param>
    /// <param name="makeCollider"></param>
    /// <returns></returns>
    internal bool GetByKey(KeyData data, BiomeResource biome, out Node3D obj, bool makeCollider)
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
    private Resource ResolveAndCache(KeyData data, BiomeResource biome)
    {
        if (cacheKeyedPieces == null) { cacheKeyedPieces = new Dictionary<PIECEKEYS, Dictionary<int, Resource>>(); }

        if (!cacheKeyedPieces.ContainsKey(data.key)) { cacheKeyedPieces[data.key] = new Dictionary<int, Resource>(); }

        if (!cacheKeyedPieces[data.key].ContainsKey(data.variantID))
        {
            if (biome.GetResource(data.key, data.variantID, out Resource result))
            {
                cacheKeyedPieces[data.key][data.variantID] = result;
            }

            if (biome.debug.Where(p => p.key == data.key).Count() > 0)
            {
                cacheKeyedPieces[data.key][data.variantID] = biome.debug.Where(p => p.key == data.key).First().GetResource(data.variantID);
            }
            else if (biome.walls.Where(p => p.key == data.key).Count() > 0)
            {
                cacheKeyedPieces[data.key][data.variantID] = biome.walls.Where(p => p.key == data.key).First().GetResource(data.variantID);
            }
            else if (biome.floors.Where(p => p.key == data.key).Count() > 0)
            {
                cacheKeyedPieces[data.key][data.variantID] = biome.floors.Where(p => p.key == data.key).First().GetResource(data.variantID);
            }
            else if (biome.ceilings.Where(p => p.key == data.key).Count() > 0)
            {
                cacheKeyedPieces[data.key][data.variantID] = biome.ceilings.Where(p => p.key == data.key).First().GetResource(data.variantID);
            }
            else if (biome.extras.Where(p => p.key == data.key).Count() > 0)
            {
                cacheKeyedPieces[data.key][data.variantID] = biome.extras.Where(p => p.key == data.key).First().GetResource(data.variantID);
            }

        }
        if (!cacheKeyedPieces.ContainsKey(data.key))
        {
            GD.PrintErr($"ResolveAndCache Key [{data.key}] was not found!");
            return null;
        }
        if (!cacheKeyedPieces[data.key].ContainsKey(data.variantID))
        {
            if (!cacheKeyedPieces[data.key].ContainsKey(0))
            {
                GD.PrintErr($"ResolveAndCache", $"Key [{data.key}] Variant [{data.variantID}] was not found! And Default fallback failed!");
                return null;
            }
            GD.PrintErr($"ResolveAndCache", $"Key [{data.key}] Variant [{data.variantID}] was not found! Default used as fallback.");
            return cacheKeyedPieces[data.key][0];
        }
        return cacheKeyedPieces[data.key][data.variantID];
    }

    public void BuildNavMesh()
    {
        GetNode<NavigationRegion3D>("Generated").BakeNavigationMesh();
    }

    private void Log(string msg)
    {
        MConsole.GameConsole.AddLine(msg);
    }

}// EOF CLASS
