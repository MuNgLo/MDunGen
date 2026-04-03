// Gone through at v1.3
#if TOOLS
using System;
using Godot;
using MDunGen.Commons;
namespace MDunGen.MS.Selection;

[Tool]
public partial class SelectOnClick : SubViewportContainer
{
	[Export] bool debug;

	[Export] MainScreen mainScreen;
	[Export] Camera3D camera;
	[Export] SubViewport subViewPort;
	[Export] ScreenDungeonVisualizer visualizer;

	Action<MapPiece> actionToCall;
	Node3D hit;
	Vector3 point;

	public void RayCastToMapPiece(Action<MapPiece> act)
	{
		actionToCall = act;
		Vector2 position2D = subViewPort.GetMousePosition();
		Vector3 cursorWorldPos = camera.ProjectRayOrigin(position2D);
		Vector3 rayDir = camera.ProjectRayNormal(position2D);
		World3D world = visualizer.GetWorld3D();
		TryToHit(cursorWorldPos, rayDir, world);
	}

	public void TryToHit(Vector3 startPoint, Vector3 dir, World3D world)
	{
		point = Vector3.Zero;
		hit = null;
		Vector3 endPos = startPoint + dir * 1000.0f;
		Godot.Collections.Array<Rid> excluding = new Godot.Collections.Array<Rid> { };
		PhysicsRayQueryParameters3D query = PhysicsRayQueryParameters3D.Create(startPoint, endPos, exclude: excluding);
		CallDeferredThreadGroup("CastDeferredRay", query, world);
	}
	private void CastDeferredRay(PhysicsRayQueryParameters3D query, World3D world)
	{
		PhysicsDirectSpaceState3D spaceState = PhysicsServer3D.SpaceGetDirectState(world.Space);
		Godot.Collections.Dictionary results = spaceState.IntersectRay(query);
		if (results.Keys.Count > 0)
		{
			if(debug) { GD.Print($"SelectOnClick::CastDeferredRay() results.Keys.Count[{results.Keys.Count}]"); }
			hit = (results["collider"].AsGodotObject() as Node3D).GetParent<Node3D>();
			point = results["position"].AsVector3();

			(subViewPort.FindChild("Target") as Node3D).GlobalPosition = point;
			MapPiece mp = visualizer.GetMapPiece(DungeonUtils.GlobalSnapCoordinate((Vector3I)point));
			actionToCall.Invoke(mp);
		}
	}
}// EOF
#endif