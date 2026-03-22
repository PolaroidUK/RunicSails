using Godot;
using System;

public partial class EndGameArea : Area2D
{
	public override void _Ready()
	{
		BodyEntered += CheckBody;
	}

	private void CheckBody(Node2D body)
	{
		if (body is Boat)
		{
			GetTree().ChangeSceneToFile("res://MainMenu.tscn");
		}
	}

	
}
