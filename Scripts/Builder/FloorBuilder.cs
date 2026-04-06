using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Godot;
using MDunGen.Commons;
using MDunGen.Pathfinding;
using MDunGen.Resources;
using MDunGen.Sections;

namespace MDunGen.Builder;
/// <summary>
/// The class to instantiate when you want to construct map data<br/>
/// THe build log is fed out through the given Log Action
/// </summary>
internal class FloorBuilder
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
	PRNGMarsenneTwister floorRNG;
	int levelIndex = -1;
	/// <summary>
	/// Action for the builder to be able to push log messages
	/// </summary>
	Action<BuildLogEventArgument> log;


	GenerationSettingsResource Args => map.MapArgs;

	internal FloorBuilder(int levelIndex, MapData map, ulong[] seed, Action<BuildLogEventArgument> log)
	{
		this.levelIndex = levelIndex;
		this.log = log;
		this.map = map;
		this.seed = seed;
		floorRNG = new PRNGMarsenneTwister(this.seed);
	}

	#region FIX IT!
	Vector3I dungeonCenter = Vector3I.Zero;
	int floorIndex = 0;
	int floorHeight = 1;
	private MapCoordinate FloorCenter()
	{
		return new(dungeonCenter + Vector3I.Up * floorIndex * floorHeight);
	}
	#endregion

	internal async Task BuildFloor(FloorResource floor, bool doPathing)
	{
		log.Invoke(new() { source = "FloorBuilder::BuildFloor()", message = "Starting build rules for floor.", levelIndex = levelIndex });

		foreach (BuildRuleResource ruleResource in floor.rules)
		{
			if (ruleResource is null)
			{
				log.Invoke(new() { severity = BUILDLOGSEVERITY.WARNING, source = "FloorBuilder::BuildFloor()", message = "Rule is NULL", levelIndex = levelIndex });
				continue;
			}
			switch (ruleResource.category)
			{
				case CATEGORYRULE.BUILD:
					await ResolveBuildRule(levelIndex, ruleResource);
					break;
			}
		}

		log.Invoke(new() { source = "FloorBuilder::BuildFloor()", message = "Build rules completed.", levelIndex = levelIndex });

		BuildOpeningsFromConnections();


		BuildUtils.FitRoundedCorners(ref map);
		BuildUtils.AddDebugKeys(ref map);
		BuildUtils.LatePassRooms(ref map);
		BuildUtils.RemoveAllEmpty(ref map);
		// Do pathing for all connections
		if (doPathing)
		{
			log.Invoke(new() { source = "FloorBuilder::BuildFloor()", message = "adding pathing.", levelIndex = levelIndex });
			DoPathingPass();
		}
		log.Invoke(new() { source = "MapBuilder::BuildFloorMapData()", message = "Finished." });
	}

	private async Task ResolveBuildRule(int levelIndex, BuildRuleResource rule)
	{
		ISection prevSec = map.Sections.Count > 0 ? map.Sections.Last() : null;
		for (int i = 0; i < rule.amount; i++)
		{
			if (ResolveLocationRule(rule.location, out MapPiece mp, prevSec))
			{
				ISection section = ResolveSectionInstance(levelIndex, mp.Coord, mp.Orientation, rule.section);
				section.Build(log);
				map.Sections.Add(section);
			}
			await Task.Delay(1);
		}
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

	/// <summary>
	/// Returns 0,0,0 as default
	/// </summary>
	/// <param name="location"></param>
	/// <returns></returns>
	private bool ResolveLocationRule(STARTLOCATIONRULE location, out MapPiece mp, ISection prevSec)
	{
		switch (location)
		{
			case STARTLOCATIONRULE.ATTACHEDTOPREVIOUS:
				if (prevSec is null) { break; }
				if (prevSec.GetOuterWallFreeNeighbour(out mp, out MAPDIRECTION dir))
				{
					mp.Orientation = dir;
					return true;
				}
				break;
			case STARTLOCATIONRULE.CENTER:
				mp = map.GetPiece(FloorCenter());
				return true;
		}
		mp = null;
		return false;
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
			sectionSeed = [(ulong)floorRNG.Next(9999), (ulong)floorRNG.Next(9999), (ulong)floorRNG.Next(9999), (ulong)floorRNG.Next(9999)]
		};

		Assembly assembly = Assembly.GetExecutingAssembly();
		Type type = assembly.GetTypes().First(t => t.Name == sectionDef.sectionType);

		object instance = Activator.CreateInstance(type, new object[] { buildArgs, false });

		ISection section = instance as SectionBase;

		//GD.Print($"MapBuilder::ResolveSectionInstance() [{section.GetType().Name}]");
		return section;
	}
}// EOF CLASS