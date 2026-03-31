// Gone through at v1.3
namespace MDunGen.Commons;

public struct ConnectedLocation
{
    public int section;
    public int connectionID;
    public MapCoordinate coord;
    public double cost;
    public override string ToString()
    {
        return $"[S{section}]{coord}[cost:{cost}]";
    }
    public ConnectedLocation(int inSection, int connectionID, MapCoordinate location, double cost)
    {
        section = inSection; this.connectionID = connectionID; coord = location; this.cost = cost;
    }
}// EOF STRUCT