using Godot;
using System;
using System.Threading.Tasks;

public partial class Monster : Area2D
{
	[Export] public float Damage = 10f;
	[Export] public float Health = 100f;
    [Export] public Sprite2D Sprite;

    [Export] public Color DamageColor = new Color(1f, 0f, 0f, 1f);
    private Color _baseColor = new Color(1f, 1f, 1f, 1f);
    
    private Task[] _blinkTask = new Task[5];
    
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		BodyEntered += CheckIfHit;
		_baseColor = Sprite.Modulate;
	}

	private void CheckIfHit(Node2D body)
	{
		if (body is Arrow arrow)
		{
			Health -= arrow.Damage;
			if (Health <= 0) {
				QueueFree();
				GD.Print("i die");
			}
			arrow.QueueFree();
			CodeAnimations.DamageBlink(0.2f, 5, Sprite, DamageColor, _baseColor);
		}

		if (body is Boat boat)
		{
			boat.Damage(Damage);
		}
		
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	
}
