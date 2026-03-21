using Godot;
using System;

public partial class RuneIsland : StaticBody2D
{
	[Export] public Area2D Area;
	public override void _Ready()
	{
		Area.BodyEntered += (body) => Area.Show();
		Area.BodyExited += (body) => Area.Hide();
	}
}
