using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Godot;
using MDunGen.Commons;
using MDunGen.Resources;
using MDunGen.Sections;

namespace MDunGen.Builder;
/// <summary>
/// The class to instantiate when you want to construct map data<br/>
/// THe build log is fed out through the given Log Action
/// </summary>
internal class SectionBuilder
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
	/// The floor RNG generator for the whole build process
	/// </summary>
	PRNGMarsenneTwister sectionRNG;
	int levelIndex = -1;
	/// <summary>
	/// Action for the builder to be able to push log messages
	/// </summary>
	Action<BuildLogEventArgument> log;

	GenerationSettingsResource Args => map.MapArgs;

	internal SectionBuilder(int levelIndex, MapData map, ulong[] seed, Action<BuildLogEventArgument> log)
	{
		this.levelIndex = levelIndex;
		this.log = log;
		this.map = map;
		this.seed = seed;
		sectionRNG = new PRNGMarsenneTwister(this.seed);
	}


	internal async Task BuildSection(string sectionTypeName, SectionResource sectionDef, ulong[] usedSeed, bool debug)
	{
		MapPiece piece = map.GetPiece(MapCoordinate.Zero);
		// Seems tpo be only for the debug generation of a section so use random direction
		piece.Orientation = MAPDIRECTION.ANY;

		SectionBuildArguments buildArgs = new SectionBuildArguments()
		{
			map = map,
			piece = piece,
			sectionID = map.Sections.Count,
			levelIndex = levelIndex,
			cfg = Args,
			sectionDefinition = sectionDef,
			sectionSeed = usedSeed
		};

		Assembly assembly = Assembly.GetExecutingAssembly();
		Type type = assembly.GetTypes().First(t => t.Name == sectionTypeName);

		object instance = Activator.CreateInstance(type, new object[] { buildArgs, debug});

		ISection section = instance as SectionBase;

		section.Build(log);
		map.Sections.Add(section);

		BuildUtils.FitRoundedCorners(ref map);
		BuildUtils.AddDebugKeys(ref map);
		BuildUtils.LatePassRooms(ref map);
		BuildUtils.RemoveAllEmpty(ref map);
		await Task.Delay(1);
	}
}// EOF CLASS