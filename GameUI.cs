using Godot;
using System;

public partial class GameUI : CanvasLayer
{
	[Signal]
	public delegate void RuneMadeEventHandler(int runeID);
}
