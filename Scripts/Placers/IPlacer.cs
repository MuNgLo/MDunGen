// Gone through at v1.3
using Godot;
using MDunGen.Sections;
namespace MDunGen.Placers;

[System.Obsolete("Potentially removable onces props stuff is cleared out")]
public interface IPlacer
{
	public string ResourceName { get; set; }
	public bool PickRandomProp(out PackedScene asset, out int count);
	public void Place(ISection section);
	public void Place(ISection section, Node3D node);
	public bool Fit(ISection section);
	public bool Fit(ISection section, Node3D node);
}// EOF INTERFACE