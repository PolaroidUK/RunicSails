using System;
using Godot;

public partial class Boat : CharacterBody2D
{
	public float Speed = 0f;
	public float Accelaration = 10f;
	public float MaxSpeed = 100.0f;
	public const float rotationSpeed = 0.01f;
	private float wantedDirection;
	private Vector2 currentDirection = Vector2.Up;

	[Export] public float Health = 100f;
	[Export] public Sprite2D Sprite;
	[Export] public Color DamageColorMod = new Color(1f, 0f, 0f, 1f);
	private Color _baseColorMod = new Color(1f, 1f, 1f, 1f);
	
	[Export] public float ArrowDamage = 25f;
	[Export] public int BurstAmount = 3;
	[Export] public float ArrowCooldown = 0.5f;
	private float _arrowCooldownTime = 0f;
	private bool _sails = true;
	private bool _lights = true;
	private bool _oars = false;

	[Export] public Node2D Ship;
	
	[Export] public PackedScene arrow;
	[Export] private GameUI ui;
	
	[Export] private AudioStreamPlayer2D lanternSound;
	[Export] private AudioStreamPlayer2D arrowFireSound;
	[Export] private AudioStreamPlayer2D sailUpSound;
	[Export] private AudioStreamPlayer2D crackSound;
	
	[Export] private Node2D spawnPoint1;
	[Export] private Node2D spawnPoint2;
	[Export] private Node2D spawnPoint3;
	[Export] private Node2D spawnPoint4;
	
	public override void _Ready()
	{
		base._Ready();
		ui.RuneMade += ActivateRune;
		Spawn();
		GD.Randi();
	}

	private void Spawn()
	{
		Health = 100f;
		int i = (int)(GD.Randi()%4);
		switch (i)
		{
			case 0:
				Position = spawnPoint1.Position;
				break;
			case 1:
				Position = spawnPoint2.Position;
				break;
			case 2:
				Position = spawnPoint3.Position;
				break;
			case 3:
				Position = spawnPoint4.Position;
				break;
			default:
				Position = spawnPoint1.Position;
				break;
		}
	}

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
		if (Input.IsActionJustPressed("ui_accept"))
		{
			ShootArrowBarrage(Monster.EMonster.Arrow);
		}
		
	}

	[Export] public LineEdit runeNumberInput;
	
	public void TakeDebugInput()
	{
		//ActivateRune(Convert.ToInt32(runeNumberInput.Text));
	}
	public void ActivateRune(int runeID)
	{
		Runes rune = (Runes)runeID;
		GD.Print("rune made :"+rune);
		switch (rune)
		{
			case Runes.Sails:
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
			case Runes.Oars:
				_oars = !_oars;
				break;
			
			case Runes.None:
				break;
			case Runes.Arrows:
				ShootArrowBarrage(Monster.EMonster.Arrow);
				break;
			case Runes.Lights:
				_lights = !_lights;
				if (_lights)
				{
					lanternSound.Play();
				}
				break;
			case Runes.North:
				wantedDirection = 0;
				break;
			case Runes.South:
				wantedDirection = Mathf.Pi;
				break;
			case Runes.East:
				wantedDirection = Mathf.Pi/2;
				break;
			case Runes.West:
				wantedDirection = Mathf.Pi*1.5f;
				break;
			case Runes.Fish:
				ShootArrowBarrage(Monster.EMonster.Fish);
				break;
			default:
				throw new ArgumentOutOfRangeException();
		}
		RecalculateSpeed();
		Ship.Call("update_visuals", Speed, _lights);
	}

	public void RaiseSails()
	{
		GD.Print("Raise Sails");
		Ship.Call("raise_sails");
		sailUpSound.Play();
	}

	public void DropSails()
	{
		GD.Print("Drop Sails");
		Ship.Call("drop_sails");
		sailUpSound.Play();
	}
	
	private void RecalculateSpeed()
	{
		MaxSpeed = 0;
		Accelaration = 0;
		if (_oars)
		{
			Accelaration += 10f;
			MaxSpeed = 50.0f;
		}
		if (_sails)
		{
			Accelaration += 10f;
			MaxSpeed = 100.0f;
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
		if (Health <= 0)
		{
			Spawn();
		}

		foreach (var node in Ship.GetChildren())
		{
			if (node is Sprite2D sprite)
			{
				CodeAnimations.DamageBlink(0.2f, 5, sprite, DamageColorMod, _baseColorMod);
			}
		}
	}


	private void ShootArrowBarrage(Monster.EMonster type)
	{
		if (_arrowCooldownTime + ArrowCooldown * 1000 > Time.GetTicksMsec()) return;
		
		for (int i = 0; i < BurstAmount; i++)
		{
			var rng = new RandomNumberGenerator();
			ShootArrow(Rotation - Mathf.Pi/2 + rng.RandfRange(-0.3f, 0.3f), type);
		}
		for (int i = 0; i < BurstAmount; i++)
		{
			var rng = new RandomNumberGenerator();
			ShootArrow(Rotation + Mathf.Pi/2 + rng.RandfRange(-0.3f, 0.3f), type);
		}

		if (type == Monster.EMonster.Arrow)
		{
			
			arrowFireSound.Play();
		}
		else
		{
			arrowFireSound.Play();
		}
		_arrowCooldownTime = Time.GetTicksMsec();
	}
	private void ShootArrow(float rotation, Monster.EMonster type)
	{
		Arrow newArrow = arrow.Instantiate<Arrow>();
		newArrow.SetDamage(ArrowDamage);
		newArrow.SetType(type);
		newArrow.GlobalPosition = GlobalPosition;
		newArrow.Rotation = rotation;
		GetParent().AddChild(newArrow);
	}
}
