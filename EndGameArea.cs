using Godot;
using System;

public partial class EndGameArea : Area2D
{
	[Export] public PackedScene MainMenu = null;
	public override void _Ready()
	{
		BodyEntered += CheckBody;
	}

	private void CheckBody(Node2D body)
	{
		if (body is Boat)
		{
			GetTree().ChangeSceneToPacked(MainMenu);
		}
	}

	
}
