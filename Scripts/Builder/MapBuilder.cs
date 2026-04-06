// Gone through at v1.3
using MDunGen.Commons;
using MDunGen.Resources;
using System;
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

	internal async Task BuildFloor(int levelIndex, FloorResource floorDef, bool doPathing)
	{
		FloorBuilder floorBuilder = new(levelIndex, map, NewSeed(), log);
		await floorBuilder.BuildFloor(floorDef, doPathing);
	}

	internal async Task BuildSection(int levelIndex, string sectionTypeName, SectionResource sectionDef, ulong[] seed, bool debug)
	{
		SectionBuilder sectionBuilder = new(levelIndex, map, NewSeed(), log);
		await sectionBuilder.BuildSection(sectionTypeName, sectionDef, seed, debug);
	}
}// EOF CLASS

