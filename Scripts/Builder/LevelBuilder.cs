using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Godot;
using MDunGen.Commons;
using MDunGen.Design;
using MDunGen.Pathfinding;
using MDunGen.Resources;
using MDunGen.Sections;

namespace MDunGen.Builder;
/// <summary>
/// The class to instantiate when you want to construct map data<br/>
/// The build log is fed out through the given Log Action
/// </summary>
internal class LevelBuilder
{
	/// <summary>
	/// The seed for the build<br>
	/// Set on object instantiation and never changed
	/// </summary>
	readonly ulong[] seed;
	/// <summary>
	/// The map data the level is part of
	/// </summary>
	MapData map;

	MapBuilder mapBuilder;


	/// <summary>
	/// The level RNG generator for the whole build process
	/// </summary>
	readonly PRNGMarsenneTwister levelRNG;
	/// <summary>
	/// The level index is relative to the generation. Starting on 0<br/>
	/// for the first level generated
	/// </summary>
	readonly int levelIndex = -1;
	/// <summary>
	/// Action for the builder to be able to push log messages
	/// </summary>
	readonly Action<BuildLogEventArgument> log;

	MapDesignResource Args => map.Design;

	internal LevelBuilder(int levelIndex, MapData map, MapBuilder mapBuilder, ulong[] seed, Action<BuildLogEventArgument> log)
	{
		this.levelIndex = levelIndex;
		this.log = log;
		this.map = map;
		this.mapBuilder = mapBuilder;
		this.seed = seed;
		levelRNG = new PRNGMarsenneTwister(this.seed);
	}

	

	internal async Task Build(BuildSections level)
	{
		log.Invoke(new() { source = "FloorBuilder::BuildFloor()", message = $"Starting build rules for floor. Count[{level.rules.Length}]", levelIndex = levelIndex });

		foreach (BuildSection ruleResource in level.rules)
		{
			if (ruleResource is null)
			{
				log.Invoke(new() { severity = BUILDLOGSEVERITY.WARNING, source = "FloorBuilder::BuildFloor()", message = "Rule is NULL", levelIndex = levelIndex });
				continue;
			}
			//switch (ruleResource.category)
			//{
			//	case CATEGORYRULE.BUILD:
			//		await ResolveBuildRule(levelIndex, ruleResource);
			//		break;
			//}
		}

		log.Invoke(new() { source = "FloorBuilder::BuildFloor()", message = "Build rules completed.", levelIndex = levelIndex });

		BuildOpeningsFromConnections();


		BuildUtils.FitRoundedCorners(ref map);
		BuildUtils.AddDebugKeys(ref map);
		BuildUtils.LatePassRooms(ref map);
		BuildUtils.RemoveAllEmpty(ref map);
		// TODO pathing for all connections
		//if (doPathing)
		//{
		//	log.Invoke(new() { source = "FloorBuilder::BuildFloor()", message = "adding pathing.", levelIndex = levelIndex });
		//	DoPathingPass();
		//}
		log.Invoke(new() { source = "MapBuilder::BuildFloorMapData()", message = "Finished." });
	}

	private async Task ResolveBuildRule(int levelIndex, BuildSection rule)
	{
		ISection prevSec = map.Sections.Count > 0 ? map.Sections.Last() : null;
		//for (int i = 0; i < rule.amount; i++)
		//{
			if (BuildUtils.ResolveLocationRule(map, mapBuilder, rule, log, out MapPiece mp))
			{
				ISection section = ResolveSectionInstance(levelIndex, mp.Coord, mp.Orientation, rule.section);
				section.Build(log);
				map.Sections.Add(section);
			}
			await Task.Delay(1);
		//}
	}
	private void BuildOpeningsFromConnections()
	{
		// TODO maybe move this last chance for a section to get a connection in
		for (int i = 0; i < map.Sections.Count; i++)
		{
			map.Sections[i].PunchBackDoor();
		}
		foreach (KeyValuePair<int, SectionConnection> con in map.Connections)
		{
			map.AddOpeningBetweenSections(con.Value, true);
		}
	}
	private void DoPathingPass()
	{
		// Process all connections
		foreach (KeyValuePair<int, SectionConnection> connPair in map.Connections)
		{
			if (connPair.Value.sectionID < 0 || connPair.Value.sectionID >= map.Sections.Count)
			{
				GD.PushError($"MapBuilder::DoPathingPass() missing section for connection Connection[key:{connPair.Key}][value.sectionID:{connPair.Value.sectionID}] Map has [{map.Sections.Count}] sections.");
				log(new BuildLogEventArgument()
				{
					source = $"MapBuilder::DoPathingPass()",
					message = $"missing section for connection Connection[key:{connPair.Key}][value.sectionID:{connPair.Value.sectionID}] Map has [{map.Sections.Count}] sections."
				});
				continue;
			}

			ISection section = map.Sections[connPair.Value.sectionID];
			//section.AddConnection(connection.Key);
		}

		foreach (ISection section in map.Sections)
		{
			foreach (int fromID in section.Connections)
			{
				SectionConnection conn = map.Connections[fromID];

				// path to the other connections in the same section
				foreach (int toID in section.Connections)
				{
					if (fromID == toID) { continue; }
					SectionConnection to = map.Connections[toID];

					//if(section.SectionIndex != to.sectionID){ 
					//    Godot.GD.PushError($"MapBuilder::DoPathingPass() Section miss match! Skipping!");
					//    continue;
					//}
					MapPiece mpStart = map.GetExistingPiece(conn.coord);
					MapPiece mpEnd = map.GetExistingPiece(to.coord);
					if (Pathing.FindPath(
					new PathQuery(map, mpStart, mpEnd), out PathAnswer answer))
					{
						if (answer.path.Count > 0)
						{
							conn.Add(to.connectionID, to.coord, answer.path.Count);
						}
					}
				}
			}
		}
		//GD.Print("pathing Pass in builder!");
		//GD.Print(map.Connections.First().ToString());
	}

	

	private ISection ResolveSectionInstance(int levelIndex, MapCoordinate location, MAPDIRECTION direction, SectionResource sectionDef)
	{
		MapPiece piece = map.GetPiece(location);
		piece.Orientation = direction;

		SectionBuildArguments buildArgs = new SectionBuildArguments()
		{
			map = map,
			piece = piece,
			sectionID = map.Sections.Count,
			levelIndex = levelIndex,
			cfg = Args,
			sectionDefinition = sectionDef,
			sectionSeed = [(ulong)levelRNG.Next(9999), (ulong)levelRNG.Next(9999), (ulong)levelRNG.Next(9999), (ulong)levelRNG.Next(9999)]
		};

		Assembly assembly = Assembly.GetExecutingAssembly();
		Type type = assembly.GetTypes().First(t => t.Name == sectionDef.sectionType);

		object instance = Activator.CreateInstance(type, new object[] { buildArgs, false });

		ISection section = instance as SectionBase;

		//GD.Print($"MapBuilder::ResolveSectionInstance() [{section.GetType().Name}]");
		return section;
	}
}// EOF CLASS