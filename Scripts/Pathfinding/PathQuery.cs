// Gone through at v1.3
using System.Collections.Generic;
using MDunGen.Commons;
using MDunGen.Sections;

namespace MDunGen.Pathfinding;

internal class PathQuery
{
	private bool isValid = false;
	private MapData map;
	private int startSection;
	private int endSection;
	internal readonly MapCoordinate startLocation;
	internal readonly MapCoordinate endLocation;
	internal SectionConnection startConnection;
	internal SectionConnection endConnection;
	internal bool IsValid => isValid;
	internal bool IsSectionPath => startSection != endSection;
	internal Dictionary<int, SectionConnection> Connections => map.Connections;
	internal ISection StartSection => map.Sections[startSection];
	internal int StartSectionIndex => map.Sections[startSection].SectionIndex;
	internal ISection EndSection => map.Sections[endSection];
	internal int EndSectionIndex => map.Sections[endSection].SectionIndex;

	//internal List<MapPiece> Extras => BuildExtras();

	internal PathQuery(MapData map, MapPiece mpStart, MapPiece mpEnd)
	{
		this.map = map;
		startSection = mpStart.MainSection;
		if (startSection < 0 && map.Sections.Count > startSection) { return; }
		endSection = mpEnd.MainSection;
		if (endSection < 0 && map.Sections.Count > endSection) { return; }
		startLocation = mpStart.Coord;
		endLocation = mpEnd.Coord;
		if (startSection != endSection)
		{
			startConnection = MakeTempConnection(startSection, startLocation, -1);
			endConnection = MakeTempConnection(endSection, endLocation, -2);
		}
		isValid = true;
	}

	/// <summary>
	/// Creating temporary connections on start and goal map piece is required to<br/>
	/// be able to run a proper connection level path query<br/>
	/// Since they are temporary they are given negative IDs
	/// </summary>
	/// <param name="sectionIndex"></param>
	/// <param name="location"></param>
	/// <param name="tempConnId"></param>
	/// <returns></returns>
	private SectionConnection MakeTempConnection(int sectionIndex, MapCoordinate location, int tempConnId)
	{
		SectionConnection tempConn = new SectionConnection(tempConnId, location, sectionIndex, MAPDIRECTION.ANY);
		// Get connections from section
		ISection section = map.Sections[sectionIndex];
		// Process connection ID's from section
		foreach (int otherConnID in section.Connections)
		{
			SectionConnection otherConn = map.Connections[otherConnID];
			MapPiece mpStart = map.GetExistingPiece(tempConn.coord);
			MapPiece mpEnd = map.GetExistingPiece(otherConn.coord);
			if (sectionIndex == otherConn.sectionID)
			{
				if (Pathing.FindPath(new PathQuery(map, mpStart, mpEnd), out PathAnswer answer))
				{
					if (answer.path.Count > 0)
					{
						tempConn.Add(otherConn.connectionID, otherConn.coord, answer.path.Count);
					}
				}
				else
				{
					// TODO Is this good??? setting this to cost 1???
					Godot.GD.Print("SADDASD");
					tempConn.Add(otherConn.connectionID, otherConn.coord, 1);
				}
			}
		}
		map.Connections[tempConn.connectionID] = tempConn;
		return tempConn;
	}

	internal void OverrideSections(int start, int end)
	{
		Godot.GD.Print($"PathQuery OverrideSections() NO GOAL! startSection[{startSection}]>[{start}]  endSection[{endSection}]>[{end}]");

		startSection = start;
		endSection = end;
	}
}// EOF CLASS