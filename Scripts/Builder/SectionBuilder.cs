using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using MDunGen.Commons;
using MDunGen.Design;
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

	MapBuilder mapBuilder;

	/// <summary>
	/// The floor RNG generator for the whole build process
	/// </summary>
	PRNGMarsenneTwister sectionRNG;
	int levelIndex = -1;
	/// <summary>
	/// Action for the builder to be able to push log messages
	/// </summary>
	Action<BuildLogEventArgument> log;

	MapDesignResource Args => map.Design;


	internal SectionBuilder(MapBuilder mapBuilder, int levelIndex, MapData map, ulong[] seed, Action<BuildLogEventArgument> log)
	{
		this.levelIndex = levelIndex;
		this.log = log;
		this.map = map;
		this.mapBuilder = mapBuilder;
		this.seed = seed;
		sectionRNG = new PRNGMarsenneTwister(this.seed);
	}

	internal async Task Build(BuildSection designRule, bool debug)
	{
		if (BuildUtils.ResolveLocationRule(map, mapBuilder, designRule, log, out MapPiece piece))
		{
			if (designRule.direction != MAPDIRECTION.PIECE)
			{
				if (designRule.direction == MAPDIRECTION.ANY)
				{
					piece.Orientation = (MAPDIRECTION)sectionRNG.Next(1, 5);
				}
				else
				{
					piece.Orientation = designRule.direction;
				}
			}

			SectionBuildArguments buildArgs = new SectionBuildArguments()
			{
				map = map,
				piece = piece,
				sectionID = map.Sections.Count,
				levelIndex = this.levelIndex,
				cfg = Args,
				sectionDefinition = designRule.section,
				sectionSeed = seed
			};

			Assembly assembly = Assembly.GetExecutingAssembly();
			Type type = assembly.GetTypes().First(t => t.Name == buildArgs.sectionDefinition.sectionType);

			object instance = Activator.CreateInstance(type, new object[] { buildArgs, debug });

			ISection section = instance as SectionBase;

			section.Build(log);
			map.Sections.Add(section);

			if (debug)
			{
				if (Godot.Engine.IsEditorHint())
				{
					AddonSettingsResource config = Godot.ResourceLoader.Load<AddonSettingsResource>("res://addons/MDunGen/Config/def_master.tres");
					if (config.sectionFirstPieceDoor) { BuildUtils.SectionAddConnectionOnFirst(ref map, section); }
					if (config.sectionAddAttachment) { BuildUtils.SectionAddFakeAttachment(section); }
				}
			}
			await Task.Delay(1);
		}
	}
}// EOF CLASS