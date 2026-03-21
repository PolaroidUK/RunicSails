using Godot;
using System;

public partial class Arrow : RigidBody2D
{
	public override void _Ready()
	{
		LinearVelocity = Vector2.Up.Rotated(Transform.Rotation)*500f;
	}
}
