using Godot;

public static class CodeAnimations
{
    public static void DamageBlink(
        float duration, 
        int blinks, 
        Sprite2D sprite, 
        Color damageColor, 
        Color baseColor)
    {
        Tween tween = sprite.GetTree().CreateTween();
        for (int i = 0; i < blinks; i++)
        {
            tween.TweenProperty(sprite, "modulate", damageColor, duration).SetTrans(Tween.TransitionType.Sine);
            tween.TweenProperty(sprite, "modulate", baseColor, duration).SetTrans(Tween.TransitionType.Sine);
        }
    }
}
