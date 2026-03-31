// Gone through at v1.3
#if TOOLS
using Godot;
using System;

namespace MDunGen.MS;

[Tool]
internal partial class PlacerSelection : EditorResourcePicker
{
	public override void _Ready()
	{
		ResourceChanged += WhenResourceChanged;
		ResourceSelected += WhenResourceSelected;
	}
	private void FocusInspector()
	{
		if (EditedResource is null) { return; }
		EditorInterface.Singleton.InspectObject(EditedResource);
		ReleaseFocus();
	}

	private void WhenResourceSelected(Resource resource, bool inspect)
	{
		FocusInspector();
	}

	private void WhenResourceChanged(Resource resource)
	{
		FocusInspector();
	}
}// EOF CLASS
#endif