// Gone through at v1.3
using System;
using System.Linq;
using MDunGen.Commons;
using MDunGen.Resources;

namespace MDunGen.Sections;

public class HallwaySection : SectionBase
{
	public HallwaySection(SectionBuildArguments args, bool debug = false) : base(args, debug)
	{
		PathResource pathRes = args.sectionDefinition as PathResource;
	}
	public override void Build(Action<BuildLogEventArgument> log)
	{
		log.Invoke(new()
		{
			source = "RoomSection::Build()",
			message = $"Building Hallway section{(debug == true ? " with debug flag" : "")}",
			sectionIndex = sectionIndex,
			levelIndex = levelIndex,
			mapLocations = [coord]
		});

		//SetMinMaxCoord(coord + DungeonUtils.TwistRight(orientation) + MapCoordinate.Down);
		// Shift start coord one down for this section type
		SetMinMaxCoord(coord + MapCoordinate.Down); 

		BuildHallwayStart(coord, orientation);
		MapCoordinate stepLocation = coord + orientation;
		for (int i = 0; i < Depth - 2; i++)
		{
			AddHallwayStep(stepLocation, orientation);
			stepLocation = stepLocation + orientation;
		}
		BuildHallwayEnd(stepLocation, orientation);
		SealSection(0, -1, 0);
		foreach (MapPiece mapPiece in pieces)
		{
			mapPiece.AddSection(sectionIndex);
			map.SavePiece(mapPiece);
		}

		if (debug)
		{
			Godot.GD.Print($"HallwaySection:: Orientation[{pieces.First().Orientation}] Depth[{Depth}] Width[{Width}]");
		}
	}
	private void BuildHallwayStart(MapCoordinate startCoord, MAPDIRECTION dir)
	{
		ClaimSlice(startCoord, dir);
		MapPiece start = map.GetPiece(startCoord + DungeonUtils.TwistLeft(dir));
		start.State = MAPPIECESTATE.PENDING;
		start.AddExtra(new KeyData() { key = PIECEKEYS.ARCH, dir = dir, variantID = 2 });
		start.keyFloor = new KeyData() { key = PIECEKEYS.F, dir = dir, variantID = 3 };
		pieces.Add(start);
		start.Save();
	}
	private void BuildHallwayEnd(MapCoordinate endCoord, MAPDIRECTION dir)
	{
		ClaimSlice(endCoord, dir);
		MapPiece end = map.GetPiece(endCoord + DungeonUtils.TwistRight(dir) + DungeonUtils.TwistRight(dir));
		end.AddExtra(new KeyData() { key = PIECEKEYS.ARCH, dir = DungeonUtils.Flip(dir), variantID = 2 });
		end.State = MAPPIECESTATE.PENDING;
		end.keyFloor = new KeyData() { key = PIECEKEYS.F, dir = DungeonUtils.Flip(dir), variantID = 3 };
		pieces.Add(end);
		end.Save();
	}
	private void AddHallwayStep(MapCoordinate stepCoord, MAPDIRECTION dir)
	{
		ClaimSlice(stepCoord, dir);

		MapPiece stepLeft = map.GetPiece(stepCoord + DungeonUtils.TwistLeft(dir));
		stepLeft.State = MAPPIECESTATE.PENDING;
		stepLeft.AddExtra(new KeyData() { key = PIECEKEYS.ARCH, dir = dir, variantID = 2 });
		stepLeft.keyFloor = new KeyData() { key = PIECEKEYS.F, dir = dir, variantID = 4 };
		pieces.Add(stepLeft);
		stepLeft.Save();
		MapPiece stepRight = map.GetPiece(stepCoord + DungeonUtils.TwistRight(dir) + DungeonUtils.TwistRight(dir));
		stepRight.State = MAPPIECESTATE.PENDING;
		stepRight.keyFloor = new KeyData() { key = PIECEKEYS.F, dir = DungeonUtils.Flip(dir), variantID = 4 };
		pieces.Add(stepRight);
		stepRight.Save();
	}

	private void ClaimSlice(MapCoordinate stepCoord, MAPDIRECTION dir)
	{
		// start on the location under step coord and work upwards
		MapCoordinate startCoord = stepCoord + MapCoordinate.Down;
		MAPDIRECTION r = DungeonUtils.TwistRight(dir);
		// do it by row
		for (int i = 0; i < 3; i++)
		{
			ClaimPiece(startCoord + DungeonUtils.TwistLeft(dir), dir);
			ClaimPiece(startCoord, dir);
			ClaimPiece(startCoord + r, dir);
			ClaimPiece(startCoord + r + r, dir);
			startCoord += MapCoordinate.Up;
		}
	}
	private void ClaimPiece(MapCoordinate pieceCoord, MAPDIRECTION dir)
	{
		MapPiece mp = map.GetPiece(pieceCoord);
		mp.State = MAPPIECESTATE.PENDING;
		mp.Orientation = dir;
		pieces.Add(mp);
		mp.Save();
	}
}// EOF CLASS