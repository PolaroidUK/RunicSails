using Godot;
using System;

public partial class Boat : CharacterBody2D
{
	public const float Speed = 300.0f;
	public const float rotationSpeed = 1.0f;
	private float wantedDirection;
	private Vector2 currentDirection = Vector2.Up;
	public override void _Process(double delta)
	{
		base._Process(delta);
		if (Input.IsActionPressed("ui_right"))
		{
			wantedDirection = Mathf.Pi/2;
		}
		if (Input.IsActionPressed("ui_left"))
		{
			wantedDirection = Mathf.Pi*1.5f;
		}
		if (Input.IsActionPressed("ui_down"))
		{
			wantedDirection = Mathf.Pi;
		}
		if (Input.IsActionPressed("ui_up"))
		{
			wantedDirection = 0;
		}
	}
	public override void _PhysicsProcess(double delta)
	{
		Rotation = RotateTowardTarget(Rotation, wantedDirection, (float)(rotationSpeed * delta));
		currentDirection = Vector2.Up.Rotated(Transform.Rotation);
		Velocity = currentDirection * Speed;
		MoveAndSlide();
	}
	float RotateTowardTarget(float currentRotation, float targetRotation, float step)
	{
		float diff = (targetRotation - currentRotation) % Mathf.Tau;
		if (diff > Mathf.Pi)
		{
			diff -= Mathf.Tau;
		}
		if (Mathf.Abs(diff) <= step)
		{
			return targetRotation;
		}
		return currentRotation + Mathf.Sign(diff) * step;
	}
}
