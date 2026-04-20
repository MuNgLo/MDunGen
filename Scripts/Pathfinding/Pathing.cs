// Gone through at v1.4
using System;
using System.Collections.Generic;
using System.Linq;
using MDunGen.Commons;
using MDunGen.Sections;

namespace MDunGen.Pathfinding;

/// <summary>
/// The main pathfinding class in MDunGen
/// </summary>
internal static class Pathing
{
	#region Events
	internal static EventHandler<PathData> OnPathDataPushed;
	internal static void RaiseOnPathDataPushed(PathData pathData)
	{
		OnPathDataPushed?.Invoke(null, pathData);
	}
	#endregion

	#region Exposed Methods
	internal static void FindPath(PathQuery query, Action<PathAnswer> callback)
	{
		if (query.IsSectionPath)
		{
			PathAnswer answer = new PathAnswer(query);
			answer.connectionPath = RunConnectionPathing(query.startConnection, query.endConnection, query.Connections);
			callback.Invoke(answer);
		}
		else
		{
			PathAnswer answer = new PathAnswer(query);
			answer.path = RunPathing(query.StartSection, query.startLocation, query.endLocation);
			callback.Invoke(answer);
		}
	}
	internal static bool FindPath(PathQuery query, out PathAnswer answer)
	{
		if(!query.IsValid){ answer = null; return false; }
		if (query.IsSectionPath)
		{
			answer = new PathAnswer(query);
			answer.connectionPath = RunConnectionPathing(query.startConnection, query.endConnection, query.Connections);
		}
		else
		{
			answer = new PathAnswer(query);
			answer.path = RunPathing(query.StartSection, query.startLocation, query.endLocation);
		}
		if (answer.path.Count > 0 || answer.connectionPath.Count > 0) { return true; }
		return false;
	}
	#endregion


	/// <summary>
	/// Build a path between 2 coordinates in same section
	/// </summary>
	/// <param name="section"></param>
	/// <param name="from"></param>
	/// <param name="to"></param>
	/// <returns></returns>
	private static List<PathLocation> RunPathing(ISection section, MapCoordinate from, MapCoordinate to)
	{
		Map map = new Map(section);
		map.SetNeighbours();
		PathLocation start = new PathLocation(section.Pieces.Find(p => p.Coord == from));
		start.SetNeighbours(map); // TODO try without this?
		PathLocation goal = new PathLocation(section.Pieces.Find(p => p.Coord == to));
		goal.SetNeighbours(map); // TODO try without this?
		return AStar(start, goal, map);
	}
	/// <summary>
	/// Finds a path of connection ID's in the Dictionary of connections passed to it.
	/// </summary>
	/// <param name="startConnection"></param>
	/// <param name="goalConnection"></param>
	/// <param name="connections"></param>
	/// <returns></returns>
	private static List<int> RunConnectionPathing(SectionConnection startConnection, SectionConnection goalConnection, Dictionary<int, SectionConnection> connections)
	{
		if(startConnection is null)
		{
			Godot.GD.PushError($"Pathing::RunConnectionPathing() startConnection is null. goalConnection is also null[{goalConnection is null}]");
			Godot.GD.Print(Environment.StackTrace);
			return new List<int>();
		}

		var openList = new List<int> { startConnection.connectionID };
		var closedList = new HashSet<int>();

		// Dictionaries to hold g(n), h(n), and parent pointers
		var gScore = new Dictionary<int, double> { [startConnection.connectionID] = 0 };
		var hScore = new Dictionary<int, double> { [startConnection.connectionID] = StepDistance(startConnection.coord, goalConnection.coord) };
		var parentMap = new Dictionary<int, int>();

		while (openList.Count > 0)
		{
			//Godot.GD.Print($"Pathing::RunConnectionPathing() while openList...");

			// Find node in open list with the lowest F score
			int current = openList.OrderBy(node => gScore[node] + hScore[node]).First();

			if (current == goalConnection.connectionID)
			{
				//Godot.GD.Print($"Pathing::AStarOverConnections() current[{current}] goalConnection[{goalConnection.connectionID}] parentMap KeyCount[{parentMap.Keys.Count}]");
				return ReconstructSectionPath(parentMap, current);
			}

			openList.Remove(current);
			closedList.Add(current);

			if (connections[current].sectionID == goalConnection.sectionID)
			{
				double tentativeGScore = gScore[current] + goalConnection.ConnectedLocations.Find(p => p.connectionID == current).cost;
				if (!gScore.ContainsKey(goalConnection.connectionID) || tentativeGScore < gScore[goalConnection.connectionID])
				{
					// Update gScore and hScore
					gScore[goalConnection.connectionID] = tentativeGScore;
					hScore[goalConnection.connectionID] = tentativeGScore + StepDistance(goalConnection.coord, connections[current].coord);
					// Set the current node as the parent of the neighbor
					parentMap[goalConnection.connectionID] = current;
					if (!openList.Contains(goalConnection.connectionID))
					{
						openList.Add(goalConnection.connectionID);
					}
				}
			}
			// Iterate over the precalculated locations
			foreach (ConnectedLocation nbLocation in connections[current].ConnectedLocations)
			{
				SectionConnection nbSectionConnection = connections[nbLocation.connectionID];
				// Tentative gScore (current gScore + distance to neighbor)
				double tentativeGScore = double.MaxValue;
				if (nbSectionConnection.sectionID == goalConnection.sectionID)
				{
					// Workaround for the connections in the goal section to read the cost from the endConnection instead
					tentativeGScore = gScore[current] + goalConnection.ConnectedLocations.Find(p => p.connectionID == current).cost;
				}
				else
				{
					tentativeGScore = gScore[current] + nbLocation.cost;
				}


				if (!gScore.ContainsKey(nbSectionConnection.connectionID) || tentativeGScore < gScore[nbSectionConnection.connectionID])
				{
					// Update gScore and hScore
					gScore[nbSectionConnection.connectionID] = tentativeGScore;
					hScore[nbSectionConnection.connectionID] = tentativeGScore + StepDistance(nbSectionConnection.coord, connections[current].coord);

					// Set the current node as the parent of the neighbor
					parentMap[nbSectionConnection.connectionID] = current;

					if (!openList.Contains(nbSectionConnection.connectionID))
					{
						openList.Add(nbSectionConnection.connectionID);
						if (nbSectionConnection.connectedToConnectionID > 0)
						{
							parentMap[nbSectionConnection.connectedToConnectionID] = nbSectionConnection.connectionID;
							// Update gScore and hScore
							gScore[nbSectionConnection.connectedToConnectionID] = tentativeGScore + 1;
							hScore[nbSectionConnection.connectedToConnectionID] = 1000000000;
							openList.Add(nbSectionConnection.connectedToConnectionID);
						}
					}
				}
			}
		}
		Godot.GD.PushError($"Pathing::RunConnectionPathing() Failed to path! from[{startConnection.coord}] to[{goalConnection.coord}]");
		connections.Remove(startConnection.connectionID);
		connections.Remove(goalConnection.connectionID);
		return new List<int>(); // No path found
	}

	private static List<PathLocation> AStar(PathLocation start, PathLocation goal, Map map)
	{
		var openList = new List<PathLocation> { start };
		var closedList = new HashSet<PathLocation>();

		// Dictionaries to hold g(n), h(n), and parent pointers
		var gScore = new Dictionary<MapCoordinate, double> { [start.coord] = 0 };
		var hScore = new Dictionary<MapCoordinate, double> { [start.coord] = StepDistance(start.coord, goal.coord) };
		var parentMap = new Dictionary<MapCoordinate, PathLocation>();

		while (openList.Count > 0)
		{
			// Find node in open list with the lowest F score
			var current = openList.OrderBy(node => gScore[node.coord] + hScore[node.coord]).First();

			if (current.coord == goal.coord)
			{
				return ReconstructPath(parentMap, current);
			}
			openList.Remove(current);
			closedList.Add(current);

			foreach (MapCoordinate nbCoord in current.Neighbors.Keys)
			{
				PathLocation neighbor = map.GetNodeById(nbCoord);
				if (neighbor == null || closedList.Contains(neighbor)) continue;
			
			// don't cut diagonally through corners
				/*MapCoordinate direction = current.coord - neighborId;
                if (direction == MapCoordinate.NorthEast)
                {
                    if (map.GetNodeById(current.coord + MapCoordinate.North) is null)
                    {
                        Godot.GD.Print($"Pathing::AStar(North) Skipped NE diagonal because corner! direction[{direction}]");
                        continue;
                    }
                    if (map.GetNodeById(current.coord + MapCoordinate.East) is null)
                    {
                        Godot.GD.Print($"Pathing::AStar(East) Skipped NE diagonal because corner! direction[{direction}]");
                        continue;
                    }
                }
                if (direction == MapCoordinate.NorthWest)
                {
                    if (map.GetNodeById(current.coord + MapCoordinate.North) is null)
                    {
                        Godot.GD.Print($"Pathing::AStar(North) Skipped NW diagonal because corner! direction[{direction}]");
                        continue;
                    }
                    if (map.GetNodeById(current.coord + MapCoordinate.West) is null)
                    {
                        Godot.GD.Print($"Pathing::AStar(West) Skipped NW diagonal because corner! direction[{direction}]");
                        continue;
                    }
                }


                  if (direction == MapCoordinate.SouthEast)
                {
                    if (map.GetNodeById(current.coord + MapCoordinate.South) is null)
                    {
                        Godot.GD.Print($"Pathing::AStar(South) Skipped SE diagonal because corner! direction[{direction}]");
                        continue;
                    }
                    if (map.GetNodeById(current.coord + MapCoordinate.East) is null)
                    {
                        Godot.GD.Print($"Pathing::AStar(East) Skipped SE diagonal because corner! direction[{direction}]");
                        continue;
                    }
                }
                if (direction == MapCoordinate.SouthWest)
                {
                    if (map.GetNodeById(current.coord + MapCoordinate.South) is null)
                    {
                        Godot.GD.Print($"Pathing::AStar(South) Skipped SW diagonal because corner! direction[{direction}]");
                        continue;
                    }
                    if (map.GetNodeById(current.coord + MapCoordinate.West) is null)
                    {
                        Godot.GD.Print($"Pathing::AStar(West) Skipped SW diagonal because corner! direction[{direction}]");
                        continue;
                    }
                }
                */

				// Tentative gScore (current gScore + distance to neighbor)
				double tentativeGScore = gScore[current.coord] + current.Neighbors[nbCoord];

				if (!gScore.ContainsKey(neighbor.coord) || tentativeGScore < gScore[neighbor.coord])
				{
					// Update gScore and hScore
					gScore[neighbor.coord] = tentativeGScore;
					hScore[neighbor.coord] = StepDistance(neighbor.coord, goal.coord);

					// Set the current node as the parent of the neighbor
					parentMap[neighbor.coord] = current;

					if (!openList.Contains(neighbor))
					{
						openList.Add(neighbor);
					}
				}
			}
		}

		return new List<PathLocation>(); // No path found
	}




	

	static private void ProcessNeighbour(MapCoordinate nc, PathLocation loc, ref List<PathLocation> locations, ref List<PathLocation> locationsToProcess)
	{
		if (locations.Exists(p => p.coord == nc))
		{
			PathLocation n = locations.Find(p => p.coord == nc);
			if (loc.calculatedCost + 1 < n.calculatedCost)
			{
				n.parentCoord = loc.coord;
				n.isClosed = false;
				if (!locationsToProcess.Exists(p => p.coord == n.coord))
				{
					locationsToProcess.Add(n);
				}
			}
			if (n.calculatedCost + 1 < loc.calculatedCost)
			{
				loc.parentCoord = n.coord;
			}
			loc.isClosed = true;
		}
	}

	

	/// <summary>
	/// Converts the connection ID dictionary to a path
	/// </summary>
	/// <param name="parentMap"></param>
	/// <param name="current"></param>
	/// <returns></returns>
	static private List<int> ReconstructSectionPath(Dictionary<int, int> parentMap, int current)
	{
		var path = new List<int> { current };
		while (parentMap.ContainsKey(current))
		{
			if(parentMap[current] == current)
			{
				Godot.GD.PushError($"Pathing::ReconstructSectionPath() path entry is looping to it self.");
				break;
			}
			current = parentMap[current];
			path.Add(current);
		}
		path.Reverse();
		return path;
	}
	/// <summary>
	/// Converts the path location dictionary to a path
	/// </summary>
	/// <param name="parentMap"></param>
	/// <param name="current"></param>
	/// <returns></returns>
	static private List<PathLocation> ReconstructPath(Dictionary<MapCoordinate, PathLocation> parentMap, PathLocation current)
	{
		var path = new List<PathLocation> { current };
		while (parentMap.ContainsKey(current.coord))
		{
			current = parentMap[current.coord];
			path.Add(current);
		}
		path.Reverse();
		return path;
	}
	/// <summary>
	/// Manhattan distance heuristic cost
	/// </summary>
	/// <param name="coord"></param>
	/// <param name="to"></param>
	/// <returns></returns>
	static private int StepDistance(MapCoordinate start, MapCoordinate goal)
	{
		MapCoordinate cd = start - goal;
		int x = Math.Abs(cd.x);
		int y = Math.Abs(cd.y);
		return x + y;
	}



		/// <summary>
	/// ONLY use this in path query constructor and MapBuilder pathing pass
	/// </summary>
	/// <param name="section"></param>
	/// <param name="from"></param>
	/// <param name="to"></param>
	/// <param name="path"></param>
	/// <returns></returns>
	/*private static bool FindPathForQuery(ISection section, MapPiece from, MapPiece to, List<MapPiece> extras, out List<PathLocation> path)
	{
		Map map = new Map(section);
		map.SetNeighbours();
		PathLocation start = new PathLocation(from);
		start.SetNeighbours(map); // TODO try without this?
		PathLocation goal = new PathLocation(to);
		goal.SetNeighbours(map); // TODO try without this?
		path = AStar(start, goal, map);
		return path is not null;
	}*/

	/*static private void SetStraightPathCost(ref List<PathLocation> locations, MapCoordinate to)
    {
        for (int i = 0; i < locations.Count; i++)
        {
            if (locations[i].isWalkable == false)
            {
                locations[i].straightLineCost = int.MaxValue;
            }
            locations[i].straightLineCost = StepDistance(locations[i], to);
        }
    }*/

	/*
	static private List<PathLocation> BuildLocations(ISection section)
	{
		List<PathLocation> locations = new List<PathLocation>();
		for (int i = 0; i < section.Pieces.Count; i++)
		{
			locations.Add(new PathLocation(section.Pieces[i]));
		}
		return locations;
	}
	*/


		/*static private void ProcessLocation(PathLocation loc, MapCoordinate from, MapCoordinate to, ref List<PathLocation> locations, ref List<PathLocation> locationsToProcess)
	{
		if (loc.coord == to) { loc.isClosed = true; loc.calculatedCost = 0; }

		ProcessNeighbour(loc.coord + MapCoordinate.North, loc, ref locations, ref locationsToProcess);
		ProcessNeighbour(loc.coord + MapCoordinate.North + MapCoordinate.East, loc, ref locations, ref locationsToProcess);
		ProcessNeighbour(loc.coord + MapCoordinate.East, loc, ref locations, ref locationsToProcess);
		ProcessNeighbour(loc.coord + MapCoordinate.East + MapCoordinate.South, loc, ref locations, ref locationsToProcess);
		ProcessNeighbour(loc.coord + MapCoordinate.South, loc, ref locations, ref locationsToProcess);
		ProcessNeighbour(loc.coord + MapCoordinate.South + MapCoordinate.West, loc, ref locations, ref locationsToProcess);
		ProcessNeighbour(loc.coord + MapCoordinate.West, loc, ref locations, ref locationsToProcess);
		ProcessNeighbour(loc.coord + MapCoordinate.West + MapCoordinate.North, loc, ref locations, ref locationsToProcess);
	}*/

/*
        public static bool FindPath(MapData map, MapCoordinate from, MapCoordinate to, out List<MapCoordinate> path, bool leaveSection = false)
        {
            path = new List<MapCoordinate>();
            MapPiece fmp = map.GetPiece(from);
            MapPiece tmp = map.GetPiece(to);
            if (leaveSection == false && fmp.SectionIndex != tmp.SectionIndex)
            {
                // target mappiece oputside section. Not allowed! 
                return false;
            }
            List<PathLocation> locations = BuildLocations(map.Sections[fmp.SectionIndex]);
            if (locations.Count < 1) { return false; }
            List<PathLocation> locationsToProcess = new();
            //SetStraightPathCost(ref locations, to);
            locations.RemoveAll(p => p.straightLineCost == int.MaxValue);
            locations = locations.OrderBy(p => p.straightLineCost).ToList();
            locationsToProcess.Add(locations[0]);

            int breaker = 500;

            while (locations.Exists(p => p.isClosed == false))
            {
                if (locationsToProcess.Count < 1) { locationsToProcess.Add(locations.Find(p => p.isClosed == false)); }
                ProcessLocation(locationsToProcess[0], from, to, ref locations, ref locationsToProcess);
                locationsToProcess.RemoveAll(p => p.isClosed);
                breaker--;
                if (breaker < 1)
                {
                    Godot.GD.PrintErr($"Pathing::FindPath() FAILED! Breaker tripped. locations.Count[{locations.Count}] locationsToProcess.Count[{locationsToProcess.Count}]");
                    break;
                }
            }

            return false;
        }
    */

}// EOF CLASS