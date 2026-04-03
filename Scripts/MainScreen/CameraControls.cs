#if TOOLS
using Godot;
using MDunGen.Commons;
using System;
namespace MDunGen.MS;

[Tool]
public partial class CameraControls : Camera3D
{
	MainScreen screen;
	public enum CAMERAMODE { LOCKED, FREELOOK }
	private CAMERAMODE state = CAMERAMODE.LOCKED;
	[Export] private float speed = 20.0f;
	[Export] private float maxSpeed = 200.0f;
	[Export] private float mouseSensitivity = 10.0f;
	private Vector2 mVel;
	private Vector3 inV;
	private Vector3 ogPos;
	private Vector3 ogRot;

	public CAMERAMODE State => state;

	public override void _EnterTree()
	{
		screen = GetParent().GetParent().GetParent() as MainScreen;
		ogPos = Position;
		ogRot = Rotation;
	}
	public override void _Ready()
	{
		screen.Visualizer.OnMapBuildEnded += WhenNewMapBuilt;
		base._Ready();
	}

	private void WhenNewMapBuilt(object sender, EventArgs e)
	{
		ResetCamera();
	}

	private void ResetCamera()
	{
		Position = ogPos;
		Rotation = ogRot;
	}

	public override void _Process(double delta)
	{
		float multiplier = 1.0f;
		if (Input.IsKeyPressed(Key.Shift)) { multiplier = 2.0f; }

		Position += inV.Normalized() * speed * (float)delta * multiplier;
		inV = Vector3.Zero;
	}

	internal void MouseMove(Vector2 relative)
	{
		if (state == CAMERAMODE.LOCKED) { return; }
		//GD.Print($"CameraControls::MouseMove() relative[{relative}]");
		Vector3 rot = RotationDegrees;
		// MouseInput
		rot.Y -= mouseSensitivity * relative.X * 0.01f; // Rotate this body left/right
		rot.X -= mouseSensitivity * relative.Y * 0.01f; // Tilt Camera Up/Down
		RotationDegrees = rot;
	}

	internal void InputVector(Vector3 inputvector)
	{
		if (state == CAMERAMODE.LOCKED) { inV = Vector3.Zero; return; }
		inV = ToGlobal(inputvector) - ToGlobal(Vector3.Zero);
	}

	internal void GoFreeLook()
	{
		state = CAMERAMODE.FREELOOK;
		Input.MouseMode = Input.MouseModeEnum.Captured;
	}

	internal void GoLocked()
	{
		state = CAMERAMODE.LOCKED;
		Input.MouseMode = Input.MouseModeEnum.Visible;

	}

	internal void WheelUp()
	{
		speed = Mathf.Clamp(speed + 5.0f, 2.0f, maxSpeed);
		screen.RaiseNotification($"Speed:" + string.Format("{0:0.0}", speed));
	}

	internal void WheelDown()
	{
		speed = Mathf.Clamp(speed - 5.0f, 2.0f, maxSpeed);
		screen.RaiseNotification($"Speed:" + string.Format("{0:0.0}", speed));
	}

	internal void FocusOnMapCoordinate(MapCoordinate coord)
	{
		Vector3 focusPoint = DungeonUtils.GlobalPosition(coord);
		GlobalPosition = focusPoint + Vector3.Up * 30.0f;
		Rotation = ogRot;
	}
}// EOF CLASS
#endif