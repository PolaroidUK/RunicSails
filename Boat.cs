using Godot;

public partial class Boat : CharacterBody2D
{
	public float Speed = 0f;
	public float Accelaration = 5f;
	public float MaxSpeed = 300.0f;
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
		if (Input.IsActionJustPressed("ui_accept"))
		{
			ShootArrow();
		}
	}
	public override void _PhysicsProcess(double delta)
	{
		Rotation = RotateTowardTarget(Rotation, wantedDirection, (float)(rotationSpeed*Speed * delta));
		currentDirection = Vector2.Up.Rotated(Transform.Rotation);
		Speed = (float)Mathf.Clamp(Speed+Accelaration*delta,0,MaxSpeed);
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

	public void HitSomething(Node2D body)
	{
		Speed = 0f;
	}

	public void Damage(float damage)
	{
		Health -= damage;
		if (Health <= 0)
		{
			QueueFree();
		}
		CodeAnimations.DamageBlink(0.2f, 5, Sprite, DamageColorMod, _baseColorMod);
	}

	private void ShootArrow()
	{
		if (_arrowCooldownTime + ArrowCooldown * 1000 > Time.GetTicksMsec()) return;
			
		for (int i = 0; i < BurstAmount; i++)
		{
			Arrow newArrow = arrow.Instantiate<Arrow>();
			newArrow.SetDamage(ArrowDamage);
			newArrow.GlobalPosition = GlobalPosition;
			var rng = new RandomNumberGenerator();
			// Bagbord
			//newArrow.Rotation = -Mathf.Pi/2 +Rotation;
			// Ahead
			newArrow.Rotation = Rotation + rng.RandfRange(-0.3f, 0.3f);
			GetParent().AddChild(newArrow);
		}
		
		_arrowCooldownTime = Time.GetTicksMsec();
	}
}
