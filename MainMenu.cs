using Godot;
using System;

public partial class MainMenu : Control
{
    [Export] public PackedScene MainScene = null;
    public void StartGame()
    {
        GetTree().ChangeSceneToPacked(MainScene);
    }
    public void ExitGame()
    {
        GetTree().Quit();
    }
}
