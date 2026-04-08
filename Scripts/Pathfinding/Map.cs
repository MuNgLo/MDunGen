// Gone through at v1.3
using System.Collections.Generic;
using MDunGen.Commons;
using MDunGen.Sections;

namespace MDunGen.Pathfinding;

internal class Map
{
    private List<PathLocation> nodes;
    public List<PathLocation> Nodes { get => nodes; }

    // Retrieve a node by its ID
    public PathLocation GetNodeById(MapCoordinate coord)
    {
        return Nodes.Find(node => node.coord == coord);
    }
    internal Map(ISection section) 
	{
        nodes = new List<PathLocation>();
        foreach (MapPiece piece in section.Pieces)
        {
            PathLocation loc = new PathLocation(piece);
            nodes.Add(loc);
        }
    }
    internal void SetNeighbours()
    {
        foreach (PathLocation node in nodes)
        {
            node.SetNeighbours(this);
        }
        //Godot.GD.Print($"Map::Map() Nodes.Count[{Nodes.Count}]");
    }
}// EOF CLASS