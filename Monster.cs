using Godot;
using System;

public partial class Monster : Area2D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		BodyEntered += CheckIfHit;
	}

	private void CheckIfHit(Node2D body)
	{
		if (body is Arrow arrow)
		{
			GD.Print("i die");
			QueueFree();
			arrow.QueueFree();
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	
}
