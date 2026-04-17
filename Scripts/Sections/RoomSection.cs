// Gone through at v1.3
using Godot;
using MDunGen.Builder;
using MDunGen.Commons;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MDunGen.Sections;

public class RoomSection : SectionBase
{
	public RoomSection(SectionBuildArguments args, bool debug) : base(args, debug) { }

	#region ISection methods
	public override void Build(Action<BuildLogEventArgument> log)
	{
		log.Invoke(new() { source = "RoomSection::Build()", message = "Building Room section", sectionIndex = sectionIndex, levelIndex = levelIndex, mapLocations = [coord] });

		MapPiece start = map.GetPiece(coord);

		start.State = MAPPIECESTATE.PENDING;
		pieces.Add(start);
		start.Save();

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
					source = "RoomSection::Build()",
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

		// Add start connection
		BuildUtils.AddConnection(ref map, coord, coord + DungeonUtils.Flip(Orientation), this);
	}
	#endregion
}// EOF CLASS