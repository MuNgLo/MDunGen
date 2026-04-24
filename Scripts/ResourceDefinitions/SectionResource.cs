// Gone through at v1.3
using Godot;
using MDunGen.Commons;

namespace MDunGen.Resources;

[GlobalClass, Tool]
public partial class SectionResource : DungeonAddonResource
{
	[Export] public string sectionName = string.Empty;
	[Export] public string sectionType = string.Empty;
	
	[ExportGroup("General")]
	[Export] public int sizeWidthMin = 3; // Used to have blank get/set. Dunno why
	[Export] public int sizeWidthMax = 5;
	[Export] public int sizeDepthMin = 3;
	[Export] public int sizeDepthMax = 5;

	[Export] public int nbFloorsMin = 1;
	[Export] public int nbFloorsMax = 2;

	[Export] public ATTACHHEIGHT GrowHeight = ATTACHHEIGHT.BOTTOM;
	[Export] public ATTACHHEIGHT AttachHeight = ATTACHHEIGHT.BOTTOM;
	
	[ExportGroup("Responses")]
	[Export] public ROOMCONNECTIONRESPONCE defaultResponses = (ROOMCONNECTIONRESPONCE)15;

	[ExportGroup("Extras")]
	[Export] public bool arches = true;
	[ExportGroup("Water")]
	[Export] public float waterLevel = 1.1f;
	[Export] public float waterDepth = 1.0f;
	[Export] public Material waterMaterial;
	//[ExportGroup("Debug")]
	//[Export] public bool debug = false;
	//[Export] public int nbDoorsPerFloorMin = 0;
	//[Export] public int nbDoorsPerFloorMax = 0;


	internal void VerifyValues()
	{
		if (sizeWidthMax < sizeWidthMin) { sizeWidthMax = Mathf.Clamp(sizeWidthMax, sizeWidthMin, 1000); }
		sizeWidthMin = Mathf.Clamp(sizeWidthMin, 1, sizeWidthMax);

		if (sizeDepthMax < sizeDepthMin) { sizeDepthMax = Mathf.Clamp(sizeDepthMax, sizeDepthMin, 1000); }
		sizeDepthMin = Mathf.Clamp(sizeDepthMin, 1, sizeDepthMax);

		if (nbFloorsMax < nbFloorsMin) { nbFloorsMax = Mathf.Clamp(nbFloorsMax, nbFloorsMin, 1000); }
		nbFloorsMin = Mathf.Clamp(nbFloorsMin, 1, nbFloorsMax);

		//backDoorChance = Mathf.Clamp(backDoorChance, 0, 100);
	}
}// EOF CLASS