using Godot;
using System;
using System.Threading.Tasks;

public static class CodeAnimations
{
    // Sorry not sorry
    public static async void DamageBlink(float duration, int blinks, Sprite2D sprite, Color damageColor, Color baseColor)
    {
        for (int i = 0; i < blinks; i++)
        {
            sprite.Modulate = damageColor;
            await Task.Delay(TimeSpan.FromSeconds(duration));
            sprite.Modulate = baseColor;
            await Task.Delay(TimeSpan.FromSeconds(duration));
        }
    }
}
