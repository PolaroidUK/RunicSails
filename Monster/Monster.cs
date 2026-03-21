using System;
using Godot;

public partial class Monster : Area2D
{
	[Export] public float Damage = 10f;
	[Export] public float Health = 100f;
	[Export] public Sprite2D Sprite;

	public enum EMonster
	{
		Arrow,
		Fish
	}

	[Export] public EMonster MonsterType = EMonster.Arrow;

	[Export] public Color DamageColor = new Color(1f, 0f, 0f, 1f);
	private Color _baseColor = new Color(1f, 1f, 1f, 1f);

	[Export] public float Speed = 0f;
	[Export] public float Acceleration = 5f;
	[Export] public float MaxSpeed = 150f;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		BodyEntered += CheckIfHit;

		_baseColor = MonsterType switch
		{
			EMonster.Arrow => new Color(1f, 0.5f, 0.5f, 1f),
			EMonster.Fish => Colors.Green,
			_ => throw new ArgumentOutOfRangeException()
		};
		Sprite.Modulate = _baseColor;
	}

	private void CheckIfHit(Node2D body)
	{
		if (body is Arrow arrow)
		{
			arrow.QueueFree();
			if (MonsterType == arrow.ArrowType) TakeDamage(arrow);
		}

		
		if (body is Boat boat)
		{
			boat.Damage(Damage);
			Speed = 0;
		}
	}

private void TakeDamage(Arrow arrow)
	{
		Health -= arrow.Damage;
		if (Health <= 0) 
		{
			QueueFree();
			GD.Print("i die");
		}
                    
		CodeAnimations.DamageBlink(0.2f, 5, Sprite, DamageColor, _baseColor);
	}

	private void Chase(double delta)
	{
		Speed = (float)Mathf.Clamp(Speed + Acceleration * delta,0, MaxSpeed);
		var towardsBoat = GetNode<Node2D>("../Boat").Position - Position;
		towardsBoat = towardsBoat.Normalized();
		Position += (float)(Speed * delta) * towardsBoat;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta)
	{
		Chase(delta);
	}
	
}
