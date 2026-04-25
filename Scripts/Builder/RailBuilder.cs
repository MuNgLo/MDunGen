
using System;
using System.Threading.Tasks;
using MDunGen.Commons;


namespace MDunGen.Builder;

internal class RailBuilder
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
	public RailBuilder(MapData map, ulong[] seed, Action<BuildLogEventArgument> log)
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
					FitRailing(ref map, map.Pieces[X][Y][Z], debug);
				}
			}
		}
		await Task.Delay(1);
	}
	internal void FitRailing(ref MapData map, MapPiece piece, bool debug)
	{
		// Has to be part of a section
		if (piece.MainSection < 0) { return; }
		// No floor piece needs railing
		if (piece.hasFloor) { return; }
		// Get neighbors
		// These need to NOT create new pieces
		//MapPiece adjacentN = map.GetExistingPiece(piece.Coord.StepNorth);
		//MapPiece adjacentD = map.GetExistingPiece(piece.Coord.StepDown);

		MapPiece lowerPiece = map.GetExistingPiece(piece.Coord + MAPDIRECTION.DOWN);

		// If there is no piece beneath there is no need to check
		if (lowerPiece is null) { return; }

		// Check N,E,S,W for match to a long piece
		for (int i = 1; i < 5; i++)
		{
			// Check for wall underneath
			if (!lowerPiece.HasWall((MAPDIRECTION)i)) { continue; }

			MapPiece otherPiece = map.GetExistingPiece(piece.Coord + (MAPDIRECTION)i);
			if (otherPiece is null) { continue; }
			// If no wall to the direction, Check direction neighbor piece for floor
			if (!piece.HasWall((MAPDIRECTION)i) && otherPiece.hasFloor)
			{
				// Insert long railing
				piece.AddExtra(new KeyData()
				{
					key = PIECEKEYS.RAILING,
					dir = (MAPDIRECTION)i,
					variantID = (int)RAILING.LONG
				});
				if (debug)
				{
					log(new BuildLogEventArgument()
					{
						severity = BUILDLOGSEVERITY.INFO,
						message = $"Railing [{RAILING.LONG}] added ",
						mapLocations = [piece.Coord]
					});
				}
			}
		}

		// Check N,E,S,W for match to a corner piece
		for (int i = 1; i < 5; i++)
		{
			MAPDIRECTION main = (MAPDIRECTION)i;
			MAPDIRECTION other = DungeonUtils.TwistRight(main);

			// Check walls on piece
			if (piece.HasWall(main) || piece.HasWall(other)) { continue; }

			// Check for floor on the diagonal piece
			if (map.GetExistingPiece(piece.Coord + main + other) is null) { continue; }
			if (!map.GetExistingPiece(piece.Coord + main + other).hasFloor) { continue; }

			// Check main direction neighbor wall/floor
			if (map.GetExistingPiece(piece.Coord + main) is null) { continue; }
			if (map.GetExistingPiece(piece.Coord + main).hasFloor) { continue; }
			if (map.GetExistingPiece(piece.Coord + main).HasWall(other)) { continue; }

			// Check other direction neighbor wall/floor
			if (map.GetExistingPiece(piece.Coord + other) is null) { continue; }
			if (map.GetExistingPiece(piece.Coord + other).hasFloor) { continue; }
			if (map.GetExistingPiece(piece.Coord + other).HasWall(main)) { continue; }


			// Insert corner railing
			piece.AddExtra(new KeyData()
			{
				key = PIECEKEYS.RAILING,
				dir = (MAPDIRECTION)i,
				variantID = (int)RAILING.CORNERROUNDED
			});
			if (debug)
			{
				log(new BuildLogEventArgument()
				{
					severity = BUILDLOGSEVERITY.INFO,
					message = $"Railing [{RAILING.CORNERROUNDED}] added ",
					mapLocations = [piece.Coord]
				});
			}
		}

	}
}// EOF CLASS