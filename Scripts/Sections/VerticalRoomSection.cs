// Gone through at v1.3
using MDunGen.Commons;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MDunGen.Sections;

public class VerticalRoomSection : SectionBase
{
	public VerticalRoomSection(SectionBuildArguments args, bool debug) : base(args, debug) { }

	#region ISection methods
	public override void Build(Action<BuildLogEventArgument> log)
	{
		log.Invoke(new() { source = "VerticalRoomSection::Build()", message = "Building Room section", sectionIndex = sectionIndex, levelIndex = levelIndex, mapLocations = [coord] });

		MapPiece start = map.GetPiece(coord);

		start.State = MAPPIECESTATE.PENDING;
		pieces.Add(start);
		start.Save();

		MapPiece parent = map.GetExistingPiece(coord + DungeonUtils.Flip(orientation));
		if (parent is not null)
		{
			ISection parentSection = map.Sections[parent.MainSection]; // TODO this should be very broken?? Really??
			int c1 = AddConnection(DungeonUtils.Flip(orientation), parentSection, start.Coord, parent.Coord, true);
			int c2 = parentSection.AddConnection(orientation, this, parent.Coord, start.Coord, true);
			map.Connections[c1].connectedToConnectionID = c2;
			map.Connections[c2].connectedToConnectionID = c1;
		}


		// Loop over pieces and process them. Adding neighbors and growing the section
		int breaker = 0;
		while (pieces.Exists(p => p.State == MAPPIECESTATE.PENDING))
		{
			MapPiece piece = pieces.Find(p => p.State == MAPPIECESTATE.PENDING);
			ProcessPiece(piece);
			breaker++;
			if (breaker > 1000)
			{
				log.Invoke(new()
				{
					severity = BUILDLOGSEVERITY.WARNING,
					source = "RoomVerticalRoomSectionSection::Build()",
					message = "ProcessPiece loop hit breaker!",
					sectionIndex = sectionIndex,
					levelIndex = levelIndex,
					mapLocations = [piece.Coord]
				});
				break;
			}
		}

		SealSection();


		
		if (sectionDefinition.arches) { FitSmallArches(); }
	}

	/*public void PunchBackDoor()
	{
		if (sectionDefinition.backDoorChance < 1 || rng.Next(100) > sectionDefinition.backDoorChance)
		{
			return;
		}
		int breaker = 20;

		List<MapPiece> candidates = GetWallPieces(0, true);
		candidates.RemoveAll(p => p.HasWall(DungeonUtils.Flip(orientation)));

		while (breaker > 0 && candidates.Count > 2)
		{
			breaker--;
			MapPiece pick = candidates[rng.Next(candidates.Count)];
			candidates.Remove(pick);

			MAPDIRECTION dir = pick.OutsideWallDirection();
			if (pick.WallKey(dir).key != PIECEKEYS.W) { continue; }
			MapPiece nb = pick.Neighbour(dir, true);
			if (nb.isEmpty) { continue; }
			if (nb.WallKey(DungeonUtils.Flip(dir)).key != PIECEKEYS.W) { continue; }

			pick.SetError(true);
			nb.SetError(true);

			if (!nb.IsPartOfSection(sectionIndex))
			{
				// TODO Here the nb.SectionIndex been outside valid values
				AddConnection(dir, map.Sections[nb.MainSection], pick.Coord, nb.Coord, true);
				return;
			}


		}
	}*/
	#endregion
}// EOF CLASS