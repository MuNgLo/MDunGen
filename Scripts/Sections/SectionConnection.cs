// Gone through at v1.3
using MDunGen.Commons;
using System.Collections.Generic;

namespace MDunGen.Sections;
/// <summary>
/// Represents one side of a door opening you could say.
/// It defines the location in a section that is connected to another section
/// </summary>
public class SectionConnection
{
    public readonly int sectionID;
    public readonly int connectionID;
    public int connectedToConnectionID;
    public MapCoordinate coord;
    private List<ConnectedLocation> connectedLocations;
    /// <summary>
	/// The other connections this connection is connected to
	/// </summary>
	internal List<ConnectedLocation> ConnectedLocations => connectedLocations;
    
    public readonly MAPDIRECTION Dir;

    public SectionConnection(int id, MapCoordinate mapLocation, int inSection, MAPDIRECTION dir)
    {
        connectedLocations = new();
        coord = mapLocation;
        connectionID = id;
        Dir = dir;
        sectionID = inSection;
        connectedToConnectionID = -1;
    }
    public void Add(int connectionID, MapCoordinate location, double cost){
        if(!ConnectedLocations.Exists(p=>p.connectionID == connectionID)){
            ConnectedLocations.Add(new ConnectedLocation(sectionID, connectionID, location, cost));
        }
    }
    /// <summary>
    /// Returns the side of the connection that is in hte given sectionID
    /// </summary>
    /// <param name="sectionID"></param>
    /// <returns></returns>
    internal MapCoordinate GetSide(int sectionID)
    {
        return ConnectedLocations.Find(p=>p.section == sectionID).coord;
    }
  
    public override string ToString(){
        string text = $"uniqueID[{connectionID}] in Section[{sectionID}] paired to other connection[{connectedToConnectionID}]\n";
        text += string.Join(' ', ConnectedLocations);
        return text;
    }

    internal double GetCost(MapCoordinate coord)
    {
        if(ConnectedLocations.Exists(p=>p.coord == coord)){
            return ConnectedLocations.Find(p=>p.coord == coord).cost;
        }
        Godot.GD.PushError($"SectionConnection::GetCost({coord}) coord was not found as connectedLocation. Returning max cost!");
        return double.MaxValue;
    }
}// EOF CLASS