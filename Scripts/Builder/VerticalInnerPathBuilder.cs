
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MDunGen.Commons;
using MDunGen.Sections;

namespace MDunGen.Builder;
/// <summary>
/// Goes over a section and construct paths between the connections
/// </summary>
internal class VerticalInnerPathBuilder
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
	MapBuilder mapBuilder;
	/// <summary>
	/// The floor RNG generator for the whole build process
	/// </summary>
	PRNGMarsenneTwister sectionRNG;
	Action<BuildLogEventArgument> log;

	internal VerticalInnerPathBuilder(MapBuilder mapBuilder, MapData map, ulong[] seed, Action<BuildLogEventArgument> log)
	{
		this.log = log;
		this.map = map;
		this.mapBuilder = mapBuilder;
		this.seed = seed;
		sectionRNG = new PRNGMarsenneTwister(this.seed);
	}

	internal async Task Build(int sectionIndex, bool debug)
	{
		ISection section = map.Sections[sectionIndex];
		foreach (int connectionID in section.Connections)
		{
			SectionConnection connection = map.Connections[connectionID];
			ProcessConnection(section, connection);
			await Task.Delay(1);
		}
	}

	private void ProcessConnection(ISection section, SectionConnection connection)
	{
		if (connection.coord.y > section.MinCoord.y)
		{
			List<MapPiece> pieces = BuildUtils.GetPiecesDownwardsToFloor(ref map, connection.coord);

			foreach (MapPiece mapPiece in pieces)
			{
				if (mapPiece.HasWall(DungeonUtils.Flip(mapPiece.Orientation)))
				{
					if (mapPiece.WallKey(DungeonUtils.Flip(mapPiece.Orientation)).key == PIECEKEYS.W)
					{

						mapPiece.AddExtra(
							new KeyData()
							{
								key = PIECEKEYS.CLIMBABLE,
								dir = DungeonUtils.Flip(mapPiece.Orientation),
								variantID = 0
							}
						);
					}
					else if (!mapPiece.hasFloor && mapPiece.WallKey(DungeonUtils.Flip(mapPiece.Orientation)).key == PIECEKEYS.WD)
					{
						mapPiece.AddExtra(
							new KeyData()
							{
								key = PIECEKEYS.CLIMBABLE,
								dir = DungeonUtils.Flip(mapPiece.Orientation),
								variantID = 1
							}
						);
					}
				}
			}
		}
	}
}// EOF CLASS