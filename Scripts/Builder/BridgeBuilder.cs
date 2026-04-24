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
		Godot.GD.Print("Bridge Build GOAL!");

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
		// Has to be part of a section
		if (piece.MainSection < 0) { return; }
		// has to have floor
		if (!piece.hasFloor) { return; }
		// The piece has to be a multi section
		if (piece.Sections.Count < 2) { return; }
	
		MAPDIRECTION backwards = DungeonUtils.Flip(piece.Orientation);
	
		MapPiece underPiece = map.GetExistingPiece(piece.Coord.StepDown);

		if (underPiece is null) { return; }

		if (underPiece.hasCeiling) { return; }

		if (piece.MainSection != underPiece.MainSection) { return; }

		bool doEndStub = false;
		bool doStartStub = false;
		// Check for connections
		if (map.GetConnections(piece.Coord, out List<SectionConnection> connections))
		{
			for (int i = 0; i < connections.Count; i++)
			{
				if (piece.Orientation == connections[i].Dir) { doEndStub = true; }
				if (DungeonUtils.Flip(piece.Orientation) == connections[i].Dir) { doStartStub = true; }
				AddBridgeKey(piece, connections[i].Dir, BRIDGES.FOUNDATION, debug);
			}
		}

		if (!doEndStub && !doStartStub)
		{
			AddBridgeKey(piece, piece.Orientation, BRIDGES.LONG, debug);
			AddBridgeKey(piece, DungeonUtils.TwistLeft(piece.Orientation), BRIDGES.HANDRAILLONG, debug);
			AddBridgeKey(piece, DungeonUtils.TwistRight(piece.Orientation), BRIDGES.HANDRAILLONG, debug);
		}
		else
		{
			AddBridgeKey(piece, piece.Orientation, BRIDGES.SECTION, debug);
			AddBridgeKey(piece, DungeonUtils.TwistLeft(piece.Orientation), BRIDGES.HANDRAILSECTION, debug);
			AddBridgeKey(piece, DungeonUtils.TwistRight(piece.Orientation), BRIDGES.HANDRAILSECTION, debug);

			if (doEndStub)
			{
				AddBridgeKey(piece, piece.Orientation, BRIDGES.STUB, debug);
				AddBridgeKey(piece, piece.Orientation, BRIDGES.HANDRAILSTUB, debug);
			}

			if (doStartStub)
			{
				AddBridgeKey(piece, backwards, BRIDGES.STUB, debug);
				AddBridgeKey(piece, backwards, BRIDGES.HANDRAILSTUB, debug);
			}
		}
		RemoveFlooring(piece, debug);
	}

	private void RemoveFlooring(MapPiece piece, bool debug)
	{
		piece.keyFloor = new KeyData() { key = PIECEKEYS.OCCUPIED };
	}


	private void AddBridgeKey(MapPiece piece, MAPDIRECTION dir, BRIDGES key, bool debug)
	{
		Godot.GD.Print($"AddSection GOAL! key[{key}]");
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
				message = $"Bridge [{key}] added ",
				mapLocations = [piece.Coord]
			});
		}
	}
}// EOF CLASS