using Microsoft.Xna.Framework;

public struct AABB(float minX, float minY, float maxX, float maxY)
{
    public readonly float MinX = minX;
    public readonly float MinY = minY;
    public readonly float MaxX = maxX;
    public readonly float MaxY = minY;

    public readonly float Width => MaxX - MinX;
    public readonly float Height => MaxY - MinY;
    public readonly Vector2 Size => new Vector2(Width, Height);
}