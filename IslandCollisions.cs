using System.Linq;
using Godot;

public partial class IslandCollisions : Node2D
{
    [Export]
    public CompressedTexture2D collisionBitmap;

    [Export]
    public int AlphaThreshold = 1;

    public Godot.Collections.Array<Vector2[]> polygons;

    public override void _Ready()
    {
        var bmp = new Bitmap();
        bmp.CreateFromImageAlpha(collisionBitmap.GetImage(), AlphaThreshold);

        var rect = new Rect2I(0, 0, collisionBitmap.GetWidth(), collisionBitmap.GetHeight());
        polygons = bmp.OpaqueToPolygons(rect);

        polygons.Select(polygon => polygon.Select(point => point - new Vector2(collisionBitmap.GetWidth() / 2, collisionBitmap.GetHeight() / 2)).ToArray()).ToArray();
        GD.Print(polygons.Count);

        int idx = 0;
        foreach (var polyArray in polygons)
        {
            var polyNode = new CollisionPolygon2D
            {
                Polygon = polyArray,
                Name = $"CollisionPoly_{idx++}"
            };

            AddChild(polyNode);
        }
    }

    public override void _Draw()
    {
        base._Draw();

        foreach (var polygon in polygons)
        {
            DrawPolygon(polygon, [Colors.Red]);
        }
    }
}