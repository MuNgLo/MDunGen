
using System;
using System.Threading.Tasks;
using MDunGen.Commons;


namespace MDunGen.Builder;

internal class SupportBuilder
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
	PRNGMarsenneTwister supportRNG;
	/// <summary>
	/// Action for the builder to be able to push log messages
	/// </summary>
	Action<BuildLogEventArgument> log;
	public SupportBuilder(MapData map, ulong[] seed, Action<BuildLogEventArgument> log)
	{
		this.log = log;
		this.map = map;
		this.seed = seed;
		supportRNG = new PRNGMarsenneTwister(this.seed);
	}


	internal async Task Build(bool debug)
	{
		foreach (int X in map.Pieces.Keys)
		{
			foreach (int Y in map.Pieces[X].Keys)
			{
				foreach (int Z in map.Pieces[X][Y].Keys)
				{
					FitSupports(ref map, map.Pieces[X][Y][Z], debug);
				}
			}
		}
		await Task.Delay(1);
	}
	internal void FitSupports(ref MapData map, MapPiece piece, bool debug)
	{
		// Has to be part of a section
		if (piece.MainSection < 0) { return; }
		// No ceiling piece needs railing
		if (piece.hasCeiling) { return; }

		MapPiece upperPiece = map.GetExistingPiece(piece.Coord + MAPDIRECTION.UP);

		// If there is no piece above there is no need to check
		if (upperPiece is null) { return; }

		// Check N,E,S,W for match to a long piece
		for (int i = 1; i < 5; i++)
		{
			// Check that this piece has no wall in teh direction
			if (piece.HasWall((MAPDIRECTION)i)) { continue; }

			// Check for wall above
			if (!upperPiece.HasWall((MAPDIRECTION)i)) { continue; }

			MapPiece otherPiece = map.GetExistingPiece(piece.Coord + (MAPDIRECTION)i);
			if (otherPiece is null) { continue; }

			if (otherPiece.hasCeiling)
			{
				// Insert long support
				piece.AddExtra(new KeyData()
				{
					key = PIECEKEYS.SUPPORT,
					dir = (MAPDIRECTION)i,
					variantID = (int)SUPPORT.LONG
				});
				if (debug)
				{
					log(new BuildLogEventArgument()
					{
						severity = BUILDLOGSEVERITY.INFO,
						message = $"Support [{SUPPORT.LONG}] added ",
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

			// Check for ceiling on the diagonal piece
			if (map.GetExistingPiece(piece.Coord + main + other) is null) { continue; }
			if (!map.GetExistingPiece(piece.Coord + main + other).hasCeiling) { continue; }

			// Check walls on piece
			if (piece.HasWall(main) || piece.HasWall(other)) { continue; }

			// Check main direction neighbor wall/ceiling
			if (map.GetExistingPiece(piece.Coord + main) is null) { continue; }
			if (map.GetExistingPiece(piece.Coord + main).hasCeiling) { continue; }
			if (map.GetExistingPiece(piece.Coord + main).HasWall(other)) { continue; }

			// Check other direction neighbor wall/ceiling
			if (map.GetExistingPiece(piece.Coord + other) is null) { continue; }
			if (map.GetExistingPiece(piece.Coord + other).hasCeiling) { continue; }
			if (map.GetExistingPiece(piece.Coord + other).HasWall(main)) { continue; }


			// Insert corner railing
			piece.AddExtra(new KeyData()
			{
				key = PIECEKEYS.SUPPORT,
				dir = (MAPDIRECTION)i,
				variantID = (int)SUPPORT.CORNERROUNDED
			});
			if (debug)
			{
				log(new BuildLogEventArgument()
				{
					severity = BUILDLOGSEVERITY.INFO,
					message = $"Railing [{SUPPORT.CORNERROUNDED}] added ",
					mapLocations = [piece.Coord]
				});
			}
		}

	}
}// EOF CLASS