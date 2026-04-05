using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
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


	internal async Task BuildSection(int levelIndex, string sectionTypeName, SectionResource sectionDef, ulong[] usedSeed)
	{
		MapPiece piece = map.GetPiece(MapCoordinate.Zero);
		piece.Orientation = MAPDIRECTION.NORTH;

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

		object instance = Activator.CreateInstance(type, new object[] { buildArgs });

		ISection section = instance as SectionBase;

		//GD.Print($"MapBuilder::BuildSection() [{section.GetType().Name}]");

		section.Build(log);
		map.Sections.Add(section);

		BuildUtils.FitRoundedCorners(ref map);
		BuildUtils.AddDebugKeys(ref map);
		BuildUtils.LatePassRooms(ref map);
		BuildUtils.RemoveAllEmpty(ref map);
		await Task.Delay(1);
	}
}// EOF CLASS