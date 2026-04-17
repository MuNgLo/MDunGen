// Gone through at v1.3
using MDunGen.Commons;
using MDunGen.Design;
using MDunGen.Resources;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MDunGen.Builder;

/// <summary>
/// The class that map data instantiates to construct itself<br/>
/// THe build log is fed out through the given Log Action
/// </summary>
internal class MapBuilder
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
	/// The top RNG generator for the whole build process
	/// </summary>
	PRNGMarsenneTwister masterRNG;
	/// <summary>
	/// Action for the builder to be able to push log messages
	/// </summary>
	Action<BuildLogEventArgument> log;

	int currentLevel = 0;

	public MapBuilder(MapData map, ulong[] seed, Action<BuildLogEventArgument> log)
	{
		this.log = log;
		this.map = map;
		this.seed = seed;
		masterRNG = new PRNGMarsenneTwister(this.seed);
	}

	/// <summary>
	/// Make sure that the order you pop new seeds from the master seed is in proper order
	/// </summary>
	/// <returns>the next seed from the master seed</returns>
	private ulong[] NewSeed()
	{
		return [(ulong)masterRNG.Next(9999), (ulong)masterRNG.Next(9999), (ulong)masterRNG.Next(9999), (ulong)masterRNG.Next(9999)];
	}

	/// <summary>
	/// Keep track of loops by design index and store how many<br/>
	/// times it has looped.
	/// </summary>
	Dictionary<int, int> loopCounter;
	/// <summary>
	/// As rules generates sections a lookup table is generated
	/// </summary>
	internal Dictionary<int, int> indexToSection;
	internal async Task Build(MapDesignResource mapDesign, bool debug)
	{
		// Reset loop counter
		loopCounter = new Dictionary<int, int>();
		// Reset section lookup table
		indexToSection = new Dictionary<int, int>();

		for (int i = 0; i < mapDesign.designRules.Count; i++)
		{
			BuildUtils.RemoveAllEmpty(ref map);
			DesignResource designRule = (DesignResource)mapDesign.designRules[i];
			switch (designRule.GetType().Name)
			{
				case nameof(BuildSections):
					Godot.GD.Print($"Goal BuildSections BuildSections");
					await BuildLevel(designRule as BuildSections);
					break;
				case nameof(BuildSection):
					//Godot.GD.Print($"Goal BuildSection BuildSection");
					//if(!indexToSection.ContainsKey(i)){ indexToSection[i] = -1; }
					indexToSection[i] = map.Sections.Count;
					await BuildSection(designRule as BuildSection, debug);
					break;
				case nameof(Loop):
					// Verify that the internal count entry exists
					if (!loopCounter.ContainsKey(i)) { loopCounter[i] = 0; }
					// Increment counter on loop
					loopCounter[i]++;
					// If the loop is hit again after it already ran once. Reset and do again
					if (loopCounter[i] > (designRule as Loop).loop + 1)
					{
						log(new BuildLogEventArgument()
						{
							severity = BUILDLOGSEVERITY.INFO,
							message = $"Loop reset on Index[{i}]"
						});
						loopCounter[i] = 1;
					}
					// Cause the step back
					if (loopCounter[i] < (designRule as Loop).loop + 1)
					{
						int newIDX = i - (designRule as Loop).stepBack;
						log(new BuildLogEventArgument()
						{
							severity = BUILDLOGSEVERITY.INFO,
							message = $"Loop[{i}] triggered Going back to [{newIDX}]"
						});
						i = newIDX - 1;
					}
					break;
				case nameof(IncreaseLevel):
					Godot.GD.Print($"Goal IncreaseLevel IncreaseLevel");
					currentLevel++;
					break;
			}
		}

		BuildUtils.BuildOpeningsFromConnections(ref map);
		BuildUtils.FitRoundedCorners(ref map);
		BuildUtils.AddDebugKeys(ref map);
		BuildUtils.LatePassRooms(ref map);
		BuildUtils.RemoveAllEmpty(ref map);
	}

	async Task BuildLevel(BuildSections levelDef)
	{
		LevelBuilder levelBuilder = new(currentLevel, map, this, NewSeed(), log);
		await levelBuilder.Build(levelDef);
	}

	async Task BuildSection(BuildSection designRule, bool debug)
	{
		SectionBuilder sectionBuilder = new(this, currentLevel, map, NewSeed(), log);
		await sectionBuilder.Build(designRule, debug);
	}
}// EOF CLASS

