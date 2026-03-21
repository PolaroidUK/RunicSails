using Godot;
using System;

public partial class Arrow : RigidBody2D
{
	[Export] public float Damage = 1f;
	[Export] public float Speed = 500f;

	[Export] public Sprite2D Sprite;
	[Export] public Texture2D ArrowTexture;
	[Export] public Texture2D FishTexture;
	
	public enum EArrow
	{
		Arrow,
		Fish
	}
	
	[Export] public Monster.EMonster ArrowType = Monster.EMonster.Arrow;

	public void SetDamage(float damage)
	{
		Damage = damage;
	}

	public void SetType(Monster.EMonster type)
	{
		ArrowType = type;

		Sprite.Texture = ArrowType switch
		{
			Monster.EMonster.Arrow => ArrowTexture,
			Monster.EMonster.Fish => FishTexture,
			_ => throw new ArgumentOutOfRangeException()
		};
	}
	
	public override void _Ready()
	{
		LinearVelocity = Vector2.Up.Rotated(Transform.Rotation) * Speed;
	}
}
