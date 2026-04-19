// Gone through at v1.3
using Godot;
using MDunGen.Commons;
using MDunGen.Resources;

namespace MDunGen.Sections;

public class SectionBuildArguments
{
	public SectionResource sectionDefinition;
	public MapData map;
	public MapPiece piece;
	public int sectionID;
	public int levelIndex = -1;
	internal MapDesignResource cfg;
	public Material waterMaterial;
	public ulong[] sectionSeed;
	//public ulong[] Seed => sectionSeed is null ? cfg.Seed : sectionSeed;
	public ulong[] Seed => sectionSeed;
}// EOF CLASS