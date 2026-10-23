#nullable enable

using System;
using Microsoft.Xna.Framework;

public enum WorldLayer
{
    Ground,
    Material,
    Structure
}

public readonly struct WorldPos : IEquatable<WorldPos>
{

    public readonly Point Position;
    public readonly WorldLayer Layer;

    public int X => Position.X;
    public int Y => Position.Y;

    public WorldPos(Point position, WorldLayer layer)
    {
        Position = position;
        Layer = layer;
    }

    public WorldPos(int x, int y, WorldLayer layer)
    {
        Position = new Point(x, y);
        Layer = layer;
    }

    public WorldPos Above => Layer < WorldLayer.Structure ? new(Position, Layer + 1) : this;
    public WorldPos Below => Layer > WorldLayer.Ground ? new(Position, Layer - 1) : this;

    public WorldPos North => new(Position.X, Position.Y - 1, Layer);
    public WorldPos South => new(Position.X, Position.Y + 1, Layer);
    public WorldPos East => new(Position.X + 1, Position.Y, Layer);
    public WorldPos West => new(Position.X - 1, Position.Y, Layer);

    public WorldPos Offset(int dx, int dy, int dLayer = 0)
    {
        int newLayer = Math.Clamp((int)Layer + dLayer, (int)WorldLayer.Ground, (int)WorldLayer.Structure);
        return new(Position.X + dx, Position.Y + dy, (WorldLayer)newLayer);
    }
    public WorldPos Offset(Point offset, int dLayer = 0) => Offset(offset.X, offset.Y, dLayer);

    public int ManhattanDistance(WorldPos other)
    {
        return Math.Abs(X - other.X) + Math.Abs(Y - other.Y) + Math.Abs((int)Layer - (int)other.Layer);
    }

    public int DistanceSquared(WorldPos other)
    {
        int dx = X - other.X;
        int dy = Y - other.Y;
        int dz = (int)Layer - (int)other.Layer;
        return (dx * dx) + (dy * dy) + (dz * dz);
    }

    public static WorldPos operator +(WorldPos pos, Point offset) => new(pos.Position + offset, pos.Layer);
    public static WorldPos operator -(WorldPos pos, Point offset) => new(pos.Position - offset, pos.Layer);

    public static bool operator ==(WorldPos left, WorldPos right) => left.Equals(right);
    public static bool operator !=(WorldPos left, WorldPos right) => !left.Equals(right);


    public bool Equals(WorldPos other) => Position.Equals(other.Position) && Layer == other.Layer;

    public override bool Equals(object? obj) => obj is WorldPos other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(Position, Layer);

    public override string ToString() => $"({X}, {Y} : {Layer})";
}