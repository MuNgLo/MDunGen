// Gone through at v1.3
using System.Collections.Generic;
using System.Linq;
using MDunGen.Builder;
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
		if (!skipOrientation) { step.Orientation = Last.Orientation; }
		step.State = MAPPIECESTATE.PENDING;
		step.AddSection(SectionIndex);
		steps.Add(step);
		step.Save();
	}

	internal void Walk(int maxSteps, bool mainline)
	{
		if (steps.Count < 1) { return; }
		MapPiece oldLast = Last;
		WalkNormal(maxSteps, mainline);

		// Insert connection backwards when leaving other section
		if (oldLast.MainSection != SectionIndex && oldLast.MainSection != Last.MainSection)
		{
			BuildUtils.AddConnection(ref map, Last.Coord, oldLast.Coord, section);
		}
	}
	internal void WalkNormal(int maxSteps, bool mainline)
	{
		MapPiece nextStep = Last.Neighbour(Last.Orientation, true);
		// Check if the next piece has a section assigned
		if (nextStep.MainSection > -1)
		{
			if (!nextStep.IsPartOfSection(SectionIndex) && !nextStep.IsPartOfSection(Last.MainSection))
			{
				// First step into other section
				if (maxSteps > steps.Count)
				{
					if (mainline)
					{
						BuildUtils.AddConnection(ref map, Last.Coord, nextStep.Coord, section);
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