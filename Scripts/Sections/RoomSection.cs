// Gone through at v1.3
using Godot;
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
		start.keyFloor = new KeyData() { key = PIECEKEYS.F, dir = orientation, variantID = 0 };
		pieces.Add(start);
		start.Save();

		MapPiece parent = map.GetExistingPiece(coord + DungeonUtils.Flip(orientation));
		if (parent is not null)
		{
			ISection parentSection = map.Sections[parent.MainSection]; // TODO this should be very broken??
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


		if (sectionDefinition.firstPieceDoor)
		{
			pieces.First().AssignWall(new KeyData() { key = PIECEKEYS.WD, dir = DungeonUtils.Flip(pieces.First().Orientation) }, true);
			pieces.First().Neighbour(DungeonUtils.Flip(pieces.First().Orientation), true).AssignWall(new KeyData() { key = PIECEKEYS.WD, dir = pieces.First().Orientation }, true);
		}
		if (sectionDefinition.arches) { FitSmallArches(); }
	}

	private void ProcessPiece(MapPiece mp)
	{
		if (mp.State != MAPPIECESTATE.PENDING)
		{
			return;
		}
		mp.Orientation = orientation;
		mp.AddSection(sectionIndex);

		// Do all MAPDIRECTIONs
		for (int i = 1; i < 7; i++)
		{
			MAPDIRECTION processingDirection = (MAPDIRECTION)i;
			MapPiece nb = mp.Neighbour(processingDirection, true);
			if (nb.State == MAPPIECESTATE.UNUSED)
			{
				if (nb.Coord.x >= minX && nb.Coord.x <= maxX
					&& nb.Coord.y >= MinY && nb.Coord.y < MaxY
					&& nb.Coord.z >= minZ && nb.Coord.z <= maxZ
					)
				{
					// Not bottom floor so have to have same section index underneath
					if (nb.Coord.y > MinY && !pieces.Exists(p => p.Coord == nb.Coord + MAPDIRECTION.DOWN))
					{
						// blocked by piece no part of room so wall it
						//rp.AssignWall(new KeyData() { key = PIECEKEYS.W, dir = processingDirection }, false);
						mp.State = MAPPIECESTATE.LOCKED;
						map.SavePiece(mp);
						return;
					}

					// Expand room to tile if within limits
					nb.State = MAPPIECESTATE.PENDING;
					nb.AddSection(sectionIndex);
					nb.sectionFloor = Math.Abs(nb.Coord.y - pieces.First().Coord.y);

					if (nb.sectionFloor == 0 || (sectionDefinition.allFloor && !nb.hasFloor && nb.sectionFloor < sizeY - 1))
					{
						//nb.keyFloor = new KeyData() { key = PIECEKEYS.F, dir = orientation, variantID = 0 };
					}

					pieces.Add(nb);
					map.SavePiece(nb);
				}
				else
				{
					if (mp.Coord + MAPDIRECTION.UP == nb.Coord)
					{
						//rp.keyCeiling = new KeyData() { key = PIECEKEYS.C, dir = processingDirection };
					}
					// cant expand so set wall
					//rp.AssignWall(new KeyData() { key = PIECEKEYS.W, dir = processingDirection }, false);
				}
			}
			else
			{
				if (!pieces.Exists(p => p.Coord == nb.Coord))
				{
					// blocked by piece no part of room so wall it
					//rp.AssignWall(new KeyData() { key = PIECEKEYS.W, dir = processingDirection }, false);
				}
			}
		}
		mp.State = MAPPIECESTATE.LOCKED;
		map.SavePiece(mp);
	}

	public void PunchBackDoor()
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
	}
	#endregion
}// EOF CLASS