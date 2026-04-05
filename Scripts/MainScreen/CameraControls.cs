#if TOOLS
using Godot;
using MDunGen.Commons;
using System;
namespace MDunGen.MS;

[Tool]
public partial class CameraControls : Node
{
	public enum CAMERAMODE { LOCKED, FREELOOK }
	[Export] MainScreen mainScreen;
	[Export] Camera3D camera;
	[Export] float speed = 20.0f;
	[Export] float maxSpeed = 200.0f;
	[Export] float mouseSensitivity = 10.0f;
	[Export] SubViewportContainer subV;

	/// <summary>
	/// Use this to only react to input if cursor is over screen
	/// </summary>
	public bool cursorIsInside = false;
	Vector2 storedCursorPosition;

	CAMERAMODE state = CAMERAMODE.LOCKED;
	Vector2 mVel;
	Vector3 inV;
	Vector3 ogPos;
	Vector3 ogRot;
	bool skipNextMouseMovement = false;
	Vector2 mouseRelative = Vector2.Zero;

	public CAMERAMODE State => state;

	public override void _EnterTree()
	{
		ogPos = camera.Position;
		ogRot = camera.Rotation;
	}
	public override void _Ready()
	{
		mainScreen.Visualizer.OnMapBuildEnded += WhenNewMapBuilt;
		subV.MouseEntered += WhenMouseEnterMain;
		subV.MouseExited += WhenMouseExitMain;
	}
	public override void _ExitTree()
	{
		subV.MouseEntered -= WhenMouseEnterMain;
		subV.MouseExited -= WhenMouseExitMain;
	}

	public override void _Input(InputEvent @event)
	{

		if (@event is InputEventMouseMotion && state == CAMERAMODE.FREELOOK)
		{
			InputEventMouseMotion m = (InputEventMouseMotion)@event;
			mouseRelative += m.Relative;
		}
		if (@event is InputEventMouseButton)
		{
			InputEventMouseButton b = (InputEventMouseButton)@event;

			if (b.ButtonIndex == MouseButton.Right)
			{
				if (b.Pressed && cursorIsInside && state == CAMERAMODE.LOCKED) { GoFreeLook(); }
				if (b.IsReleased() && state == CAMERAMODE.FREELOOK) { GoLocked(); }
			}

			if (b.ButtonIndex == MouseButton.WheelUp && cursorIsInside)
			{
				if (b.Pressed) { WheelUp(); }
			}
			if (b.ButtonIndex == MouseButton.WheelDown && cursorIsInside)
			{
				if (b.Pressed) { WheelDown(); }
			}
		}
	}
	public override void _Process(double delta)
	{
		if (state != CAMERAMODE.FREELOOK) { return; }

		if (skipNextMouseMovement)
		{
			mouseRelative = Vector2.Zero;
			skipNextMouseMovement = false;
		}
		MouseMove();
		if (storedCursorPosition.DistanceTo(GetViewport().GetMousePosition()) > 400)
		{
			//SnapCursorBack();
			CallDeferred(nameof(SnapCursorBack));
		}


		Vector3 inputVector = Vector3.Zero;
		if (Input.IsKeyPressed(Key.W)) { inputVector += Vector3.Forward; }
		if (Input.IsKeyPressed(Key.S)) { inputVector += Vector3.Back; }
		if (Input.IsKeyPressed(Key.A)) { inputVector += Vector3.Left; }
		if (Input.IsKeyPressed(Key.D)) { inputVector += Vector3.Right; }
		if (Input.IsKeyPressed(Key.E)) { inputVector += Vector3.Up; }
		if (Input.IsKeyPressed(Key.Q)) { inputVector += Vector3.Down; }
		//if (Input.IsKeyPressed(Key.Shift)) { screen.shiftIsPressed; }
		inputVector = inputVector.Normalized();
		InputVector(inputVector);
		float multiplier = 1.0f;
		if (Input.IsKeyPressed(Key.Shift)) { multiplier = 2.0f; }

		camera.Position += inV.Normalized() * speed * (float)delta * multiplier;
		inV = Vector3.Zero;


	}




	private void WhenNewMapBuilt(object sender, EventArgs e)
	{
		ResetCamera();
	}

	private void ResetCamera()
	{
		camera.Position = ogPos;
		camera.Rotation = ogRot;
	}

	void MouseMove()
	{
		if (state == CAMERAMODE.LOCKED) { return; }
		if (skipNextMouseMovement) { GD.Print("MouseMove SKIPPY!!"); return; }

		//GD.Print($"CameraControls::MouseMove() relative[{relative}]");
		Vector3 rot = camera.RotationDegrees;
		// MouseInput
		rot.Y -= mouseSensitivity * mouseRelative.X * 0.01f; // Rotate this body left/right
		rot.X -= mouseSensitivity * mouseRelative.Y * 0.01f; // Tilt Camera Up/Down
		camera.RotationDegrees = rot;
		mouseRelative = Vector2.Zero;
	}

	void InputVector(Vector3 inputVector)
	{
		if (state == CAMERAMODE.LOCKED) { inV = Vector3.Zero; return; }
		inV = camera.ToGlobal(inputVector) - camera.ToGlobal(Vector3.Zero);
	}
	void GoFreeLook()
	{
		state = CAMERAMODE.FREELOOK;
		mouseRelative = Vector2.Zero;
		storedCursorPosition = GetViewport().GetMousePosition();
		Input.MouseMode = Input.MouseModeEnum.ConfinedHidden;
	}

	void GoLocked()
	{
		CallDeferred(nameof(SnapCursorToCenter));
		mouseRelative = Vector2.Zero;
		state = CAMERAMODE.LOCKED;
		Input.MouseMode = Input.MouseModeEnum.Visible;
	}

	void SnapCursorToCenter()
	{
		Input.WarpMouse(storedCursorPosition);
	}
	void SnapCursorBack()
	{
		Input.WarpMouse(storedCursorPosition);
		skipNextMouseMovement = true;
	}
	void WheelUp()
	{
		speed = Mathf.Clamp(speed + 5.0f, 2.0f, maxSpeed);
		mainScreen.RaiseNotification($"Speed:" + string.Format("{0:0.0}", speed));
	}

	void WheelDown()
	{
		speed = Mathf.Clamp(speed - 5.0f, 2.0f, maxSpeed);
		mainScreen.RaiseNotification($"Speed:" + string.Format("{0:0.0}", speed));
	}

	internal void FocusOnMapCoordinate(MapCoordinate coord)
	{
		Vector3 focusPoint = DungeonUtils.GlobalPosition(coord);
		camera.GlobalPosition = focusPoint + Vector3.Up * 30.0f;
		camera.Rotation = ogRot;
	}

	private void WhenMouseEnterMain()
	{
		cursorIsInside = true;
	}
	private void WhenMouseExitMain()
	{
		cursorIsInside = false;
	}

}// EOF CLASS
#endif