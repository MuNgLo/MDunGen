using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MDunGen.Commons;
using MDunGen.Pathfinding;
using MDunGen.Resources;
using MDunGen.Sections;

namespace MDunGen.Builder;

internal static class BuildUtils
{
	internal static void AddDebugKeys(ref MapData map)
	{
		foreach (int X in map.Pieces.Keys)
		{
			foreach (int Y in map.Pieces[X].Keys)
			{
				foreach (int Z in map.Pieces[X][Y].Keys)
				{
					AddDebugKeys(map.Pieces[X][Y][Z]);
				}
			}
		}
	}
	internal static void AddDebugKeys(MapPiece piece)
	{
		piece.AddDebug(new KeyData() { key = PIECEKEYS.DEBUG, dir = piece.Orientation, variantID = (int)DEBUGVARIANTS.ARROW });
		if (piece.HasNorthWall) { piece.AddDebug(new KeyData() { key = PIECEKEYS.DEBUG, dir = MAPDIRECTION.NORTH, variantID = (int)DEBUGVARIANTS.WALLFLAGGREEN }); }
		if (piece.HasEastWall) { piece.AddDebug(new KeyData() { key = PIECEKEYS.DEBUG, dir = MAPDIRECTION.EAST, variantID = (int)DEBUGVARIANTS.WALLFLAGGREEN }); }
		if (piece.HasSouthWall) { piece.AddDebug(new KeyData() { key = PIECEKEYS.DEBUG, dir = MAPDIRECTION.SOUTH, variantID = (int)DEBUGVARIANTS.WALLFLAGGREEN }); }
		if (piece.HasWestWall) { piece.AddDebug(new KeyData() { key = PIECEKEYS.DEBUG, dir = MAPDIRECTION.WEST, variantID = (int)DEBUGVARIANTS.WALLFLAGGREEN }); }
	}

	internal static void SectionAddConnectionOnFirst(ref MapData map, ISection section)
	{
		// Add start connection
		AddConnection(
			ref map,
			section.Pieces.First().Coord,
			section.Pieces.First().Coord + DungeonUtils.Flip(section.Orientation),
			section
			);

		/*
		section.Pieces.First().AssignWall(
			new KeyData()
			{
				key = PIECEKEYS.WD,
				dir = DungeonUtils.Flip(section.Pieces.First().Orientation)
			}
				, true);
		section.Pieces.First().Neighbour(
			DungeonUtils.Flip(section.Pieces.First().Orientation), true).
			AssignWall(new KeyData()
			{
				key = PIECEKEYS.WD,
				dir = section.Pieces.
			First().Orientation
			}, true);
		*/
	}
	
	/// <summary>
	/// Add a fake attachment when debugging a section
	/// </summary>
	/// <param name="section"></param>
	/// <exception cref="NotImplementedException"></exception>
	internal static void SectionAddFakeAttachment(ISection section)
	{
		if (section.GetOuterWallFreeNeighbour(out MapPiece mp, out MAPDIRECTION dir))
		{
			// Change the wall on the section piece
			mp.Neighbour(DungeonUtils.Flip(dir), true).AssignWall(new KeyData() { key = PIECEKEYS.WD, dir = dir }, true);
		}
	}
	/// <summary>
	/// Parses through the map data, finds and inserts rounded corners where needed
	/// </summary>
	/// <param name="map"></param>
	internal static void FitRoundedCorners(ref MapData map)
	{
		foreach (int X in map.Pieces.Keys)
		{
			foreach (int Y in map.Pieces[X].Keys)
			{
				foreach (int Z in map.Pieces[X][Y].Keys)
				{
					FitRoundedCorners(ref map, map.Pieces[X][Y][Z]);
				}
			}
		}
	}
	/// <summary>
	/// Fits rounded corners to the passed in map piece
	/// </summary>
	/// <param name="map"></param>
	/// <param name="piece"></param>
	internal static void FitRoundedCorners(ref MapData map, MapPiece piece)
	{
		MapPiece adjacentN = map.GetExistingPiece(piece.Coord.StepNorth); // These need to NOT create new pieces
		MapPiece adjacentE = map.GetExistingPiece(piece.Coord.StepEast);
		MapPiece adjacentS = map.GetExistingPiece(piece.Coord.StepSouth);
		MapPiece adjacentW = map.GetExistingPiece(piece.Coord.StepWest);
		bool NE = false, SE = false, SW = false, NW = false;
		//check inner corners
		if (!piece.HasNorthWall && !piece.HasEastWall) // seems fine
		{
			if (adjacentN is not null && adjacentE is not null)
			{
				if (!adjacentN.HasSouthWall && !adjacentE.HasWestWall)
				{
					if (adjacentN.HasEastWall && adjacentE.HasNorthWall)
					{
						NE = true;
					}
				}
			}
		}
		if (!piece.HasSouthWall && !piece.HasEastWall)
		{
			if (adjacentE is not null && adjacentS is not null)
			{
				if (!adjacentS.HasNorthWall && !adjacentE.HasWestWall)
				{
					if (adjacentS.HasEastWall && adjacentE.HasSouthWall)
					{
						SE = true;
					}
				}
			}
		}
		if (!piece.HasSouthWall && !piece.HasWestWall)
		{
			if (adjacentS is not null && adjacentW is not null)
			{
				if (!adjacentS.HasNorthWall && !adjacentW.HasEastWall)
				{
					if (adjacentS.HasWestWall && adjacentW.HasSouthWall)
					{
						SW = true;
					}
				}
			}
		}
		if (!piece.HasNorthWall && !piece.HasWestWall)
		{
			if (adjacentN is not null && adjacentW is not null)
			{
				if (!adjacentN.HasSouthWall && !adjacentW.HasEastWall)
				{
					NW = true;
				}
			}
		}
		// add the keys
		if (NE)
		{
			piece.AssignWall(new KeyData() { key = PIECEKEYS.WCI, dir = MAPDIRECTION.NORTH }, true);
			if (piece.hasCeiling && map.Sections[piece.MainSection].PlaceArches)
			{
				piece.AddExtra(new KeyData() { key = PIECEKEYS.ARCH, dir = MAPDIRECTION.NORTH, variantID = 1 });
			}
		}
		if (SE)
		{
			piece.AssignWall(new KeyData() { key = PIECEKEYS.WCI, dir = MAPDIRECTION.EAST }, true);
			if (piece.hasCeiling && map.Sections[piece.MainSection].PlaceArches)
			{
				piece.AddExtra(new KeyData() { key = PIECEKEYS.ARCH, dir = MAPDIRECTION.EAST, variantID = 1 });
			}
		}
		if (SW)
		{
			piece.AssignWall(new KeyData() { key = PIECEKEYS.WCI, dir = MAPDIRECTION.SOUTH }, true);
			if (piece.hasCeiling && map.Sections[piece.MainSection].PlaceArches)
			{
				piece.AddExtra(new KeyData() { key = PIECEKEYS.ARCH, dir = MAPDIRECTION.SOUTH, variantID = 1 });
			}
		}
		if (NW)
		{
			if (adjacentN.HasWestWall && adjacentW.HasNorthWall)
			{
				piece.AssignWall(new KeyData() { key = PIECEKEYS.WCI, dir = MAPDIRECTION.WEST }, true);
				if (piece.hasCeiling && map.Sections[piece.MainSection].PlaceArches)
				{
					piece.AddExtra(new KeyData() { key = PIECEKEYS.ARCH, dir = MAPDIRECTION.WEST, variantID = 1 });
				}
			}
		}
	}
	/// <summary>
	/// Passing over the map data, section by section<br/>
	/// TODO Doesn't do anything right now
	/// </summary>
	/// <param name="map"></param>
	internal static void LatePassRooms(ref MapData map)
	{
		// Props pass of rooms
		/*
		foreach (ISection room in map.Sections)
		{
			if (room is not RoomSection) { continue; }
			//PlaceBridges(room);
		}
		*/
	}
	/// <summary>
	/// Will go through the map data and insert openings between sections as indicated by<br/>
	/// the connection data.
	/// </summary>
	/// <param name="map"></param>
	internal static void BuildOpeningsFromConnections(ref MapData map)
	{
		// TODO maybe move this last chance for a section to get a connection in
		for (int i = 0; i < map.Sections.Count; i++)
		{
			map.Sections[i].PunchBackDoor();
		}
		foreach (KeyValuePair<int, SectionConnection> con in map.Connections)
		{
			map.AddOpeningBetweenSections(con.Value, true);
		}
	}
	/// <summary>
	/// Removes all pieces in map data that isEmpty
	/// Run this as last step of the generation.
	/// </summary>
	internal static void RemoveAllEmpty(ref MapData map)
	{
		List<MapCoordinate> toDelete = new List<MapCoordinate>();
		foreach (int X in map.Pieces.Keys)
		{
			foreach (int Y in map.Pieces[X].Keys)
			{
				foreach (int Z in map.Pieces[X][Y].Keys)
				{
					if (map.Pieces[X][Y][Z].isEmpty && map.Pieces[X][Y][Z].MainSection == -1)
					{
						toDelete.Add(map.Pieces[X][Y][Z].Coord);
					}
				}
			}
		}
		foreach (MapCoordinate c in toDelete)
		{
			map.Pieces[c.x][c.y].Remove(c.z);
		}
	}
	/// <summary>
	/// If successful this will resolve the location for the section and return true with the<br/>
	/// map piece
	/// </summary>
	/// <param name="map"></param>
	/// <param name="builder"></param>
	/// <param name="rule"></param>
	/// <param name="log"></param>
	/// <param name="mp"></param>
	/// <returns></returns>
	internal static bool ResolveLocationRule(MapData map, MapBuilder builder, BuildSection rule, Action<BuildLogEventArgument> log, out MapPiece mp)
	{
		ISection prevSec; MAPDIRECTION dir;
		switch (rule.location)
		{
			case LOCATION.ATTACHEDTOPREVIOUSSECTION:
				prevSec = map.Sections.Last();
				if (prevSec is null) { break; }
				if (prevSec.GetOuterWallFreeNeighbour(out mp, out dir))
				{
					mp.Orientation = dir;
					return true;
				}
				break;
			case LOCATION.ATTACHEDTOSECTION:
				int targetIndex = rule.targetedIndex;
				bool hasKey = builder.indexToSection.ContainsKey(targetIndex);
				if (!hasKey)
				{
					log(new BuildLogEventArgument()
					{
						severity = BUILDLOGSEVERITY.ERROR,
						message = $"MapBuilder index to section lookup failed for targetIndex[{targetIndex}] sectionIndex came back as [{builder.indexToSection[rule.targetedIndex]}]"
					});

					prevSec = map.Sections.Last();
				}
				else
				{
					int sectionIndex = builder.indexToSection[rule.targetedIndex];
					if (sectionIndex < map.Sections.Count && sectionIndex > -1)
					{
						prevSec = map.Sections[builder.indexToSection[rule.targetedIndex]];
					}
					else
					{
						log(new BuildLogEventArgument()
						{
							severity = BUILDLOGSEVERITY.ERROR,
							message = $"Map sectionIndex[{sectionIndex}] OutOfRange. There is [{map.Sections.Count}] sections in MapData"
						});

						prevSec = map.Sections.Last();
					}
				}

				if (prevSec is null) { break; }
				if (prevSec.GetOuterWallFreeNeighbour(out mp, out dir))
				{
					mp.Orientation = dir;
					return true;
				}
				break;
			case LOCATION.CENTER:
				mp = map.GetPiece(MapCoordinate.Zero); // TODO fix this
				return true;
		}
		mp = null;
		return false;
	}
	
	#region  Connections
	/// <summary>
	/// Will insert a connection between 2 map pieces in the map data<br/>
	/// It wont check if it is valid so when in section mode a partial connection<br/>
	/// can be visualized.<br/>
	/// The connection generated in mirrored direction will not be made if end piece is empty
	/// </summary>
	/// <param name="map"></param>
	/// <param name="from"></param>
	/// <param name="to"></param>
	internal static bool AddConnection(ref MapData map, MapCoordinate from, MapCoordinate to, ISection startSection)
	{
		MAPDIRECTION dir = from.DirectionTo(to);
		int c1 = -1;
		MapPiece start = map.GetExistingPiece(from);
		MapPiece end = map.GetExistingPiece(to);
		if (start is not null)
		{
			// Adds the start to end side of the connection
			if (end is not null && !end.isEmpty && map.AddNewConnection(start.MainSection, end.MainSection, from, to, dir, out c1))
			{
				startSection.AddConnection(c1);
				int c2 = -1;
				// Adds the end to start side of the connection
				if (map.AddNewConnection(end.MainSection, start.MainSection,
				to, from, DungeonUtils.Flip(dir), out c2))
				{
					map.Sections[end.MainSection].AddConnection(c2);
				}
			}
		}
		return c1 != -1;
	}
	internal static void ProcessMapConnections(ref MapData map, Action<BuildLogEventArgument> log)
	{
		// Process all connections
		foreach (KeyValuePair<int, SectionConnection> connPair in map.Connections)
		{
			if (connPair.Value.sectionID < 0 || connPair.Value.sectionID >= map.Sections.Count)
			{
				GD.PushError($"MapBuilder::DoPathingPass() missing section for connection Connection[key:{connPair.Key}][value.sectionID:{connPair.Value.sectionID}] Map has [{map.Sections.Count}] sections.");
				log(new BuildLogEventArgument()
				{
					source = $"MapBuilder::DoPathingPass()",
					message = $"missing section for connection Connection[key:{connPair.Key}][value.sectionID:{connPair.Value.sectionID}] Map has [{map.Sections.Count}] sections."
				});
				continue;
			}

			ISection section = map.Sections[connPair.Value.sectionID];
			if(map.Connections[connPair.Key].connectedToConnectionID < 0)
			{
				if(map.GetConnection(map.Connections[connPair.Key].coord + map.Connections[connPair.Key].Dir, out SectionConnection conn))
				{
					map.Connections[connPair.Key].connectedToConnectionID = conn.connectionID;
				}
			}

		}
		foreach (ISection item in map.Sections)
		{
			ConnectInternalSectionConnections(ref map, item, log);
		}
	}
	/// <summary>
	/// Calculate distance cost between all the connections in the section<br/>
	/// Has to run after the section has been added to the map data
	/// </summary>
	static void ConnectInternalSectionConnections(ref MapData map, ISection section, Action<BuildLogEventArgument> log)
	{
		//GD.Print($"BuildUtils::ConnectInternalSectionConnections() section[{section.SectionIndex}] section.Connections[{section.Connections.Count}]");
		foreach (int fromID in section.Connections)
		{
			SectionConnection conn = map.Connections[fromID];
			// path to the other connections in the same section
			foreach (int toID in section.Connections)
			{
				if (fromID == toID) { continue; }
				SectionConnection to = map.Connections[toID];
				if(section.SectionIndex != to.sectionID){ 
					log(new BuildLogEventArgument()
					{
						severity = BUILDLOGSEVERITY.ERROR,
						message = $"MapBuilder::DoPathingPass() Section miss match! Skipping!",
						mapLocations = [conn.coord, to.coord]
					});
				    continue;
				}
				MapPiece mpStart = map.GetExistingPiece(conn.coord);
				MapPiece mpEnd = map.GetExistingPiece(to.coord);
				if (Pathing.FindPath(
				new PathQuery(map, mpStart, mpEnd), out PathAnswer answer))
				{
					if (answer.path.Count > 0)
					{
						conn.Add(to.connectionID, to.coord, answer.path.Count);
					}
				}
			}
		}
	}

	internal static List<MapPiece> GetPiecesDownwardsToFloor(ref MapData map, MapCoordinate coord)
	{
		List<MapPiece> result = [map.GetExistingPiece(coord)];
		while (result.Last() is not null && !result.Last().hasFloor)
		{
			result.Add(map.GetExistingPiece(result.Last().Coord + MAPDIRECTION.DOWN));
		}
		return result;
	}
	#endregion
}// EOF CLASS