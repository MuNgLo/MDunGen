

using System.Collections.Generic;
using MDunGen.Commons;
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
		// TODO get wall debug as variants under the debug key
		piece.AddDebug(new KeyData() { key = PIECEKEYS.DEBUG, dir = piece.Orientation });
		if (piece.HasNorthWall) { piece.AddDebug(new KeyData() { key = PIECEKEYS.DEBUG, dir = MAPDIRECTION.NORTH }); }
		if (piece.HasEastWall) { piece.AddDebug(new KeyData() { key = PIECEKEYS.DEBUG, dir = MAPDIRECTION.EAST }); }
		if (piece.HasSouthWall) { piece.AddDebug(new KeyData() { key = PIECEKEYS.DEBUG, dir = MAPDIRECTION.SOUTH }); }
		if (piece.HasWestWall) { piece.AddDebug(new KeyData() { key = PIECEKEYS.DEBUG, dir = MAPDIRECTION.WEST }); }
	}


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
			if (piece.hasCeiling && piece.Section.PlaceArches)
			{
				piece.AddExtra(new KeyData() { key = PIECEKEYS.ARCH, dir = MAPDIRECTION.NORTH, variantID = 1 });
			}
		}
		if (SE)
		{
			piece.AssignWall(new KeyData() { key = PIECEKEYS.WCI, dir = MAPDIRECTION.EAST }, true);
			if (piece.hasCeiling && piece.Section.PlaceArches)
			{
				piece.AddExtra(new KeyData() { key = PIECEKEYS.ARCH, dir = MAPDIRECTION.EAST, variantID = 1 });
			}
		}
		if (SW)
		{
			piece.AssignWall(new KeyData() { key = PIECEKEYS.WCI, dir = MAPDIRECTION.SOUTH }, true);
			if (piece.hasCeiling && piece.Section.PlaceArches)
			{
				piece.AddExtra(new KeyData() { key = PIECEKEYS.ARCH, dir = MAPDIRECTION.SOUTH, variantID = 1 });
			}
		}
		if (NW)
		{
			if (adjacentN.HasWestWall && adjacentW.HasNorthWall)
			{
				piece.AssignWall(new KeyData() { key = PIECEKEYS.WCI, dir = MAPDIRECTION.WEST }, true);
				if (piece.hasCeiling && piece.Section.PlaceArches)
				{
					piece.AddExtra(new KeyData() { key = PIECEKEYS.ARCH, dir = MAPDIRECTION.WEST, variantID = 1 });
				}
			}
		}
	}

	internal static void LatePassRooms(ref MapData map)
	{
		// Props pass of rooms
		foreach (ISection room in map.Sections)
		{
			if (room is not RoomSection) { continue; }
			//PlaceBridges(room);
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
					if (map.Pieces[X][Y][Z].isEmpty)
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


}// EOF CLASS