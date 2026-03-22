using Godot;
using System;

public partial class MainMenu : Control
{
    public void StartGame()
    {
        GetTree().ChangeSceneToFile("res://MainScene.tscn");
    }
    public void ExitGame()
    {
        GetTree().Quit();
    }
}
