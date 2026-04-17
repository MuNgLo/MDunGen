// Gone through at v1.3
using System.Collections.Generic;
using System.Linq;
using MDunGen.Commons;
using MDunGen.Sections;

namespace MDunGen;

/// <summary>
/// Single line of locations used to build paths
/// </summary>
internal class Line
{
	private MapData map;
	private List<MapPiece> steps;
	private bool isBlocked = false;
	internal int Count => steps.Count;
	internal bool Blocked => isBlocked;
	internal int Floor => steps[0].Coord.y;
	private protected readonly ISection section;
	private int SectionIndex => section.SectionIndex;

	internal Line(MapData map, ISection section, MapPiece startPiece, PRNGMarsenneTwister rng)
	{
		this.map = map;
		this.section = section;
		steps = new List<MapPiece>();
		startPiece.State = MAPPIECESTATE.PENDING;
		startPiece.AddSection(SectionIndex);
		steps.Add(startPiece);
		this.rng = rng;
	}
	internal MapPiece First => steps.First();
	internal MapPiece Last => steps.Last();
	internal List<MapPiece> Steps => steps;
	//internal MAPDIRECTION Orientation { get => steps.Last().Orientation; set => steps.Last().Orientation = value; }
	private PRNGMarsenneTwister rng;

	/// <summary>
	/// Adds the mapPiece to the line and section
	/// </summary>
	/// <param name="step"></param>
	internal void AddStep(MapPiece step, bool skipOrientation = false)
	{
		if(!skipOrientation){ step.Orientation = Last.Orientation; }
		step.State = MAPPIECESTATE.PENDING;
		step.AddSection(SectionIndex);
		steps.Add(step);
		step.Save();
	}

	internal void Walk(int maxSteps, bool mainline)
	{
		if (steps.Count < 1) { return; }
		WalkNormal(maxSteps, mainline);
	}
	internal void WalkNormal(int maxSteps, bool mainline)
	{
		MapPiece nextStep = Last.Neighbour(Last.Orientation, true);
		// Check if the next piece has a section assigned
		if (nextStep.MainSection >= 0)
		{
			if (!nextStep.IsPartOfSection(SectionIndex) && !nextStep.IsPartOfSection(Last.MainSection))
			{
				// First step into other section
				if (maxSteps > steps.Count)
				{
					if (mainline)
					{
						// TODO FIX IT!
						/*
						ISection nextSection = map.Sections[nextStep.MainSection];
						// Works
						int c1 = section.AddConnection(Last.Orientation, nextSection, Last.Coord, nextStep.Coord, true);
						int c2 = nextSection.AddConnection(DungeonUtils.Flip(Last.Orientation), section, nextStep.Coord, Last.Coord, true);
						map.Connections[c1].connectedToConnectionID = c2;
						map.Connections[c2].connectedToConnectionID = c1;

						// Right side special connection
						MapPiece nextStepRightNB = nextStep.Neighbour(DungeonUtils.TwistRight(Last.Orientation), false);
						if (nextStepRightNB is not null && nextStepRightNB.IsPartOfSection(nextSection.SectionIndex) && !nextStepRightNB.HasWall(DungeonUtils.TwistLeft(Last.Orientation)))
						{
							int cR1 = section.AddConnection(DungeonUtils.TwistRight(Last.Orientation), map.Sections[nextStepRightNB.MainSection], nextStep.Coord, nextStepRightNB.Coord, true);
							int cR2 = map.Sections[nextStepRightNB.MainSection].AddConnection(DungeonUtils.TwistLeft(Last.Orientation), section, nextStepRightNB.Coord, nextStep.Coord, true);
							map.Connections[cR1].connectedToConnectionID = cR2;
							map.Connections[cR2].connectedToConnectionID = cR1;
						}

						// Left side special connection
						MapPiece nextStepLeftNB = nextStep.Neighbour(DungeonUtils.TwistLeft(Last.Orientation), false);
						if (nextStepLeftNB is not null && nextStepLeftNB.IsPartOfSection(nextSection.SectionIndex) && !nextStepLeftNB.HasWall(DungeonUtils.TwistRight(Last.Orientation)))
						{
							int cL1 = section.AddConnection(DungeonUtils.TwistLeft(Last.Orientation), map.Sections[nextStepLeftNB.MainSection], nextStep.Coord, nextStepLeftNB.Coord, false);
							int cL2 = map.Sections[nextStepLeftNB.MainSection].AddConnection(DungeonUtils.TwistRight(Last.Orientation), section, nextStepLeftNB.Coord, nextStep.Coord, true);
							map.Connections[cL1].connectedToConnectionID = cL2;
							map.Connections[cL2].connectedToConnectionID = cL1;
						}
						*/
						AddStep(nextStep);
						return;
					}
				}
				else
				{
					isBlocked = true;
					return;
				}
			}
			else if (nextStep.IsPartOfSection(SectionIndex))
			{
				steps.RemoveAll(p => p.Coord == nextStep.Coord);
				AddStep(nextStep);
				return;
			}
			// Proceed to walk Line through other section
			AddStep(nextStep);
			return;
		}
		/*else if (Last.MainSection >= 0 && !Last.IsPartOfSection(SectionIndex))
		{
			int nextSectionIndex = nextStep.MainSection;
			if (nextSectionIndex < 0) { nextSectionIndex = SectionIndex; }
			// Works
			int c1b = section.AddConnection(DungeonUtils.Flip(Last.Orientation), map.Sections[Last.MainSection], nextStep.Coord, Last.Coord, true);
			int c2b = map.Sections[Last.MainSection].AddConnection(Last.Orientation, section, Last.Coord, nextStep.Coord, true);
			map.Connections[c1b].connectedToConnectionID = c2b;
			map.Connections[c2b].connectedToConnectionID = c1b;
		}*/
		AddStep(nextStep);
		nextStep.Save();
	}

	internal MapPiece[] GetTurners(int width, MAPDIRECTION dir, bool reversed = false)
	{
		List<MapPiece> turners = new List<MapPiece>();
		for (int i = steps.Count - 1; i > steps.Count - 1 - width; i--)
		{
			turners.Add(steps[i].Neighbour(dir, true));
		}
		if (reversed) { turners.Reverse(); }
		return turners.ToArray();
	}
	/// <summary>
	/// Returns a neighbour piece along the path
	/// </summary>
	/// <param name="dir"></param>
	/// <param name="leftSide"></param>
	/// <param name="rightSide"></param>
	/// <returns></returns>
	internal MapPiece GetRandomAlongPath(out MAPDIRECTION dir, bool leftSide = true, bool rightSide = true)
	{
		dir = MAPDIRECTION.ANY;
		if (Count < 1)
		{
			return null;
		}


		if (leftSide && rightSide)
		{
			int pickIndex = rng.Next(Count * 2);
			if (pickIndex < Count)
			{
				dir = DungeonUtils.TwistLeft(steps[pickIndex].Orientation);
				return steps[pickIndex].Neighbour(dir, true);
			}
			dir = DungeonUtils.TwistRight(steps[pickIndex - Count].Orientation);
			return steps[pickIndex - Count].Neighbour(dir, true);
		}

		if (leftSide && !rightSide)
		{
			int pickIndex = rng.Next(Count);

			dir = DungeonUtils.TwistLeft(steps[pickIndex].Orientation);
			return steps[pickIndex].Neighbour(dir, true);
		}

		if (!leftSide && rightSide)
		{
			int pickIndex = rng.Next(Count);
			dir = DungeonUtils.TwistRight(steps[pickIndex].Orientation);
			return steps[pickIndex].Neighbour(dir, true);
		}

		return null;
	}

	internal void TrimToLength(int length)
	{
		while (Count > length)
		{
			steps.Last().State = MAPPIECESTATE.UNUSED;
			steps.RemoveAt(steps.Count - 1);
		}
	}

	internal void Remove(MapCoordinate coord)
	{
		steps.RemoveAll(p => p.Coord == coord);
	}
	//internal void FilterBySectionID(int id)
	//{
	//	steps.RemoveAll(p => p.SectionIndex == id);
	//}
	internal void InsertAsFirst(MapPiece mapPiece)
	{
		List<MapPiece> newList = new() { mapPiece };
		newList.AddRange(steps);
		steps = newList;
	}
}// EOF CLASS