using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MDunGen.Commons;
using MDunGen.Sections;


namespace MDunGen.Builder;

internal class BridgeBuilder
{
	/// <summary>
	/// The seed for the build<br>
	/// Set on object instantiation and never changed
	/// </summary>
	readonly ulong[] seed;
	/// <summary>
	/// The actual map generated
	/// </summary>
	MapData map;
	/// <summary>
	/// The RNG generator for the whole build instance
	/// </summary>
	PRNGMarsenneTwister railRNG;
	/// <summary>
	/// Action for the builder to be able to push log messages
	/// </summary>
	Action<BuildLogEventArgument> log;

	public BridgeBuilder(MapData map, ulong[] seed, Action<BuildLogEventArgument> log)
	{
		this.log = log;
		this.map = map;
		this.seed = seed;
		railRNG = new PRNGMarsenneTwister(this.seed);
	}

	internal async Task Build(bool debug)
	{
		foreach (int X in map.Pieces.Keys)
		{
			foreach (int Y in map.Pieces[X].Keys)
			{
				foreach (int Z in map.Pieces[X][Y].Keys)
				{
					FitBridge(ref map, map.Pieces[X][Y][Z], debug);
				}
			}
		}
		await Task.Delay(1);
	}

	private void FitBridge(ref MapData map, MapPiece piece, bool debug)
	{
		if (!IsBridge(ref map, piece, debug)) { return; }

		MAPDIRECTION backwards = DungeonUtils.Flip(piece.Orientation);
		MAPDIRECTION left = DungeonUtils.TwistLeft(piece.Orientation);
		MAPDIRECTION right = DungeonUtils.TwistRight(piece.Orientation);

		// If there is a ceiling that needs to be removed, do so
		if (piece.hasCeiling)
		{
			MapPiece upperPiece = map.GetExistingPiece(piece.Coord.StepUp);
			if (upperPiece is not null)
			{
				if (piece.MainSection == upperPiece.MainSection)
				{
					piece.keyCeiling = KeyData.Empty;
				}
			}
		}

		// Remember if center piece is long or section
		bool isLong = true;

		List<SectionConnection> connections = new List<SectionConnection>();

		// Check for connections around and insert foundations
		if (map.GetConnections(piece.Coord, out connections))
		{
			for (int i = 0; i < connections.Count; i++)
			{
				// If there is connection in front/back, flag so we use section instead of long
				if (piece.Orientation == connections[i].Dir) { isLong = false; }
				if (backwards == connections[i].Dir) { isLong = false; }
				// Add the foundation in the direction of the connection
				AddBridgeKey(piece, connections[i].Dir, BRIDGES.FOUNDATION, debug);

				if (isLong)
				{
					if (connections[i].Dir != piece.Orientation && connections[i].Dir != backwards)
					{
						AddBridgeKey(piece, connections[i].Dir, BRIDGES.HANDRAILLONGOPEN, debug);
					}
				}
			}
		}

		if (isLong)
		{
			AddBridgeKey(piece, piece.Orientation, BRIDGES.LONG, debug);
		}
		else
		{
			AddBridgeKey(piece, piece.Orientation, BRIDGES.SECTION, debug);
		}

		// Check all 4 directions and insert stubs when needed
		for (int i = 1; i < 5; i++)
		{
			MapPiece nb = map.GetExistingPiece(piece.Coord + (MAPDIRECTION)i);
			if (nb is not null)
			{
				if (IsBridge(ref map, nb, debug))
				{
					AddBridgeKey(piece, (MAPDIRECTION)i, BRIDGES.STUB, debug);
					AddBridgeKey(piece, (MAPDIRECTION)i, BRIDGES.HANDRAILSTUB, debug);
				}
			}


			if ((MAPDIRECTION)i != piece.Orientation && (MAPDIRECTION)i != backwards)
			{
				if (!connections.Exists(p => p.Dir == (MAPDIRECTION)i))
				{
					if (nb is not null && IsBridge(ref map, nb, debug))
					{
						if (isLong) { AddBridgeKey(piece, (MAPDIRECTION)i, BRIDGES.HANDRAILLONGOPEN, debug); }
					}
					else
					{
						AddBridgeKey(piece, (MAPDIRECTION)i, isLong ? BRIDGES.HANDRAILLONG : BRIDGES.HANDRAILSECTION, debug);
					}
				}
			}
		}

		// Check where posts are needed
		for (int i = 1; i < 5; i++)
		{
			if ((MAPDIRECTION)i != piece.Orientation && (MAPDIRECTION)i != backwards)
			{
				MapPiece nb = map.GetExistingPiece(piece.Coord + (MAPDIRECTION)i);
				if (nb is null) { continue; }
				if (IsBridge(ref map, nb, debug) || connections.Exists(p => p.Dir == (MAPDIRECTION)i))
				{
					AddBridgeKey(piece, DungeonUtils.TwistRight((MAPDIRECTION)i), BRIDGES.HANDRAILPOST, debug);
					AddBridgeKey(piece, DungeonUtils.Flip((MAPDIRECTION)i), BRIDGES.HANDRAILPOST, debug);
				}
			}
		}
		RemoveFlooring(piece, debug);
	}

	private bool IsBridge(ref MapData map, MapPiece piece, bool debug)
	{
		// Has to be part of a section
		if (piece.MainSection < 0) { return false; }
		// has to have floor
		//if (!piece.hasFloor) { return; }
		// The piece has to be a multi section
		if (piece.Sections.Count < 2) { return false; }


		MapPiece underPiece = map.GetExistingPiece(piece.Coord.StepDown);

		if (underPiece is null) { return false; }

		if (underPiece.hasCeiling) { return false; }

		if (piece.MainSection != underPiece.MainSection) { return false; }

		return true;
	}

	private void RemoveFlooring(MapPiece piece, bool debug)
	{
		piece.keyFloor = new KeyData() { key = PIECEKEYS.OCCUPIED };
	}


	private void AddBridgeKey(MapPiece piece, MAPDIRECTION dir, BRIDGES key, bool debug)
	{
		piece.AddExtra(new KeyData()
		{
			key = PIECEKEYS.BRIDGE,
			dir = dir,
			variantID = (int)key
		});
		if (debug)
		{
			log(new BuildLogEventArgument()
			{
				severity = BUILDLOGSEVERITY.INFO,
				message = $"Bridge [{key}] added Direction [{dir}] on  ",
				mapLocations = [piece.Coord]
			});
		}
	}
}// EOF CLASS