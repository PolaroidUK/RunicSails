using Godot;
using System;

public partial class Arrow : RigidBody2D
{
	[Export] public float Damage = 1f;
	[Export] public float Speed = 500f;

	public void SetDamage(float damage)
	{
		Damage = damage;
	}
	public override void _Ready()
	{
		LinearVelocity = Vector2.Up.Rotated(Transform.Rotation) * Speed;
	}
}
