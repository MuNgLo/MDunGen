// Gone through at v1.3
#if TOOLS
using System;
using Godot;
using MDunGen.Commons;
namespace MDunGen.MS.Selection;

[Tool]
public partial class SelectOnClick : SubViewportContainer
{
	private MainScreen MS;
	private Camera3D cam;
	private SubViewport subV;
	private Node3D cube;

	/// Testing
	private bool delayedRay = false;
	public override void _Process(double delta)
	{
		if (delayedRay)
		{
			GD.Print("SelectOnClick::_Process()  DELAYED RAY!!");
			delayedRay = false;
		}
	}
	public override void _PhysicsProcess(double delta)
	{
		if (delayedRay)
		{
			GD.Print("SelectOnClick::_PhysicsProcess()  DELAYED RAY!!");
			delayedRay = false;
		}
	}
	public override void _EnterTree()
	{
		MS = GetParent<MainScreen>();
		cam = GetNode<Camera3D>("SubViewport/Camera3D");
		subV = GetNode<SubViewport>("SubViewport");
		cube = FindChild("Dungeon") as Node3D;
	}

	public void RayCastToMapPiece(Action<MapPiece> act)
	{
		actionToCall = act;
		Vector2 position2D = subV.GetMousePosition();
		Vector3 cursorWorldPos = cam.ProjectRayOrigin(position2D);
		Vector3 rayDir = cam.ProjectRayNormal(position2D);
		World3D world = cube.GetWorld3D();
		TryToHit(cursorWorldPos, rayDir, world);
	}

	Action<MapPiece> actionToCall;
	Node3D hit;
	Vector3 point;

	public void TryToHit(Vector3 startPoint, Vector3 dir, World3D world)
	{
		point = Vector3.Zero;
		hit = null;
		Vector3 endPos = startPoint + dir * 1000.0f;
		Godot.Collections.Array<Rid> excluding = new Godot.Collections.Array<Rid> { };
		PhysicsRayQueryParameters3D query = PhysicsRayQueryParameters3D.Create(startPoint, endPos, exclude: excluding);
		//PhysicsDirectSpaceState3D spaceState = PhysicsServer3D.SpaceGetDirectState(world.Space);
		Godot.Collections.Dictionary results = (Godot.Collections.Dictionary)CallDeferredThreadGroup("CastDefferedRay", query, world);
	}
	private void CastDefferedRay(PhysicsRayQueryParameters3D query, World3D world)
	{

		PhysicsDirectSpaceState3D spaceState = PhysicsServer3D.SpaceGetDirectState(world.Space);
		Godot.Collections.Dictionary results = spaceState.IntersectRay(query);
		if (results.Keys.Count > 0)
		{
			GD.Print($"SelectOnClick::CastDefferedRay() results.Keys.Count[{results.Keys.Count}]");
			hit = (results["collider"].AsGodotObject() as Node3D).GetParent<Node3D>();
			point = results["position"].AsVector3();

			(subV.FindChild("Target") as Node3D).GlobalPosition = point;
			ScreenDungeonVisualizer vis = FindChild("Dungeon") as ScreenDungeonVisualizer;
			MapPiece mp = vis.GetMapPiece(DungeonUtils.GlobalSnapCoordinate((Vector3I)point));
			actionToCall.Invoke(mp);
		}
	}
}// EOF
#endif