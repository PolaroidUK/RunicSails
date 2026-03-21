using Godot;
using System;

public partial class Boat : CharacterBody2D
{
	public float Speed = 0f;
	public float Accelaration = 5f;
	public float MaxSpeed = 300.0f;
	public const float rotationSpeed = 0.01f;
	private float wantedDirection;
	private Vector2 currentDirection = Vector2.Up;

	[Export] public float Health = 100f;
	[Export] public Color DamageColorMod = new Color(1f, 0f, 0f, 1f);
	private Color _baseColorMod = new Color(1f, 1f, 1f, 1f);
	[Export] public float ArrowDamage = 1f;
	[Export] public Sprite2D Sprite;

	private bool _sails = false;
	private bool _lights = true;
	private bool _oars = false;

	[Export] public Node2D Ship;
	
	[Export] public PackedScene arrow;
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
		if (Input.IsActionPressed("ui_accept"))
		{
			Arrow newArrow = arrow.Instantiate<Arrow>();
			newArrow.SetDamage(ArrowDamage);
			newArrow.GlobalPosition = GlobalPosition;
			newArrow.Rotation = -Mathf.Pi/2 +Rotation;
			GetParent().AddChild(newArrow);
		}
	}

	[Export] public LineEdit runeNumberInput;
	
	public void TakeDebugInput()
	{
		ActivateRune(Convert.ToInt32(runeNumberInput.Text));
	}
	public void ActivateRune(int runeID)
	{
		switch (runeID)
		{
			case 1:
				if (_sails)
				{
					_sails = false;
					RaiseSails();
				}
				else
				{
					_sails = true;
					DropSails();
				}
				break;
			case 2:
				_oars = !_oars;
				break;
		}
		RecalculateSpeed();
		Ship.Call("update_visuals", Speed, _lights);
	}

	public void RaiseSails()
	{
		GD.Print("Raise Sails");
		Ship.Call("raise_sails");
	}

	public void DropSails()
	{
		GD.Print("Drop Sails");
		Ship.Call("drop_sails");
	}
	
	private void RecalculateSpeed()
	{
		MaxSpeed = 0;
		Accelaration = 0;
		if (_oars)
		{
			Accelaration += 5f;
			MaxSpeed = 100.0f;
		}
		if (_sails)
		{
			Accelaration += 5f;
			MaxSpeed = 300.0f;
		}

		
	}
	public override void _PhysicsProcess(double delta)
	{
		Rotation = RotateTowardTarget(Rotation, wantedDirection, (float)(rotationSpeed*Speed * delta));
		currentDirection = Vector2.Up.Rotated(Transform.Rotation);
		Speed = (float)Mathf.Clamp(Speed+Accelaration*delta,0,MaxSpeed);
		Velocity = currentDirection * Speed;
		MoveAndSlide();
		Ship.Call("update_visuals", Speed, _lights);
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

	public void HitSomething(Node2D body)
	{
		Speed = 0f;
	}

	public void Damage(float damage)
	{
		Health -= damage;
		CodeAnimations.DamageBlink(0.2f, 5, Sprite, DamageColorMod, _baseColorMod);
		if (Health <= 0)
		{
			QueueFree();
		}
	}
}
