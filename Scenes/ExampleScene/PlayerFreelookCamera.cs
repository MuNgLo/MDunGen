using Godot;

namespace MDunGen.Example;

public partial class PlayerFreelookCamera : Camera3D
{
	[Export] float speed = 10.0f;
	[Export] float maxSpeed = 50.0f;
	[Export] float mouseSensitivity = 5.0f;


	Vector2 mVel;
	Vector3 inputVector;

	Vector2 mouseRelative = Vector2.Zero;

	public override void _Ready()
	{
		Input.MouseMode = Input.MouseModeEnum.Captured;
	}
	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventMouseMotion m && Input.MouseMode == Input.MouseModeEnum.Captured)
		{
			mouseRelative += m.Relative;
		}
	}
	public override void _Process(double delta)
	{
		if (Input.IsKeyPressed(Key.Escape))
		{
			if (Input.MouseMode == Input.MouseModeEnum.Captured)
			{
				Input.MouseMode = Input.MouseModeEnum.Visible;
			}
			else
			{
				Input.MouseMode = Input.MouseModeEnum.Captured;
			}
		}

		MouseMove();
		inputVector = Vector3.Zero;
		if (Input.IsKeyPressed(Key.W)) { inputVector += Vector3.Forward; }
		if (Input.IsKeyPressed(Key.A)) { inputVector += Vector3.Left; }
		if (Input.IsKeyPressed(Key.S)) { inputVector += Vector3.Back; }
		if (Input.IsKeyPressed(Key.D)) { inputVector += Vector3.Right; }
		if (Input.IsKeyPressed(Key.Q)) { inputVector += Vector3.Down; }
		if (Input.IsKeyPressed(Key.E)) { inputVector += Vector3.Up; }
		if (inputVector == Vector3.Zero) { return; }

		CastInputVector();

		float multiplier = 1.0f;
		if (Input.IsKeyPressed(Key.Shift)) { multiplier = 2.0f; }
		Position += inputVector.Normalized() * speed * (float)delta * multiplier;
	}
	void CastInputVector()
	{
		inputVector = ToGlobal(inputVector) - ToGlobal(Vector3.Zero);
	}
	void MouseMove()
	{
		Vector3 rot = RotationDegrees;
		rot.Y -= mouseSensitivity * mouseRelative.X * 0.01f; // Rotate this body left/right
		rot.X -= mouseSensitivity * mouseRelative.Y * 0.01f; // Tilt Camera Up/Down
		RotationDegrees = rot;
		mouseRelative = Vector2.Zero;
	}
	void WheelUp()
	{
		speed = Mathf.Clamp(speed + 5.0f, 2.0f, maxSpeed);
	}
	void WheelDown()
	{
		speed = Mathf.Clamp(speed - 5.0f, 2.0f, maxSpeed);
	}
}// EOF CLASS
