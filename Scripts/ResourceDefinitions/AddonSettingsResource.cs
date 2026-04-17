// Gone through at v1.3
using Godot;
namespace MDunGen.Resources;

/// <summary>
///  Addon's internal settings for remembering picks and things
///  Should never be instanced again
/// </summary>
[Tool, GlobalClass]
public partial class AddonSettingsResource : DungeonAddonResource
{
	[Export] public string lastUsedProfile = "res://addons/MDunGen/Config/def_profile.tres";
	[Export] public string ProjectResourcePath = string.Empty;

	public string SectionResourcePathDefault = "res://addons/MDunGen/Config/Sections/";
	public string SectionResourcePath => ProjectResourcePath + "Sections/";



	public string defaultBiome = "res://addons/MDunGen/Config/def_biome.tres";
	public string defaultSettings = "res://addons/MDunGen/Config/def_settings.tres";

	public string defaultStartRoom = "res://addons/MDunGen/Config/Rooms/DefaultStartRoom.tres";
	public string defaultStandardRoom = "res://addons/MDunGen/Config/Rooms/DefaultStandardRoom.tres";

	[ExportCategory("Visual Levels")]
	[Export] public int visibleLevelsStart = 0;
	[Export] public int maxVisibleLevels = 5;
	public int visibleLevelEnd => visibleLevelsStart + maxVisibleLevels - 1;


	[ExportGroup("Show")]
	[Export] public bool showFloors = true;
	[Export] public bool showWalls = true;
	[Export] public bool showCeilings = true;
	[Export] public bool showExtras = true;
	[Export] public bool showDebug = true;

	[ExportGroup("SectionMode Options")]
	[Export] public bool sectionFirstPieceDoor = false;
	[Export] public bool sectionAddAttachment = false;


	[ExportGroup("Passes")]
	[Export] public bool pathingPass = true;

	[ExportGroup("CameraSettings")]
	[Export] public bool cameraResetOnBuild = true;

	[ExportGroup("Seed")]
	[Export] public int seed1 = 1111;
	[Export] public int seed2 = 2222;
	[Export] public int seed3 = 3333;
	[Export] public int seed4 = 4444;
	public ulong[] MasterSeed => new[] { (ulong)seed1, (ulong)seed2, (ulong)seed3, (ulong)seed4 };

}// EOF CLASS