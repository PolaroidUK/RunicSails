using Godot;
using System;

public partial class SpawnPoint : Node2D
{
    public bool IsOnScreen = false;

    public void SetOnScreen()
    {
        IsOnScreen = true;
    }

    public void SetNotOnScreen()
    {
        IsOnScreen = false;
    }

}
