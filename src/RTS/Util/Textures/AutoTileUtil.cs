using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

public static class AutoTileUtil
{
    public readonly record struct OverlayDraw(
        Identifier Texture,
        Point TileOffset,
        float Rotation);

    public readonly record struct AutoTileResult(
        Identifier BaseTexture,
        IReadOnlyList<OverlayDraw> Overlays);

    public static AutoTileResult GetRequiredTexture(TextureData data, WorldPos tilePos, ILevel level)
    {
        if (data.Method == AutoTileType.None)
            return new(data.TextureIdentifiers[0], []);

        int sides = 0;
        List<OverlayDraw> overlays = [];
        if (data.Method == AutoTileType.Bitmask)
        {
            int tile = level.GetTile(tilePos.Position, tilePos.Layer);
            int north = level.GetTile(tilePos.North.Position, tilePos.Layer);
            int west = level.GetTile(tilePos.West.Position, tilePos.Layer);
            int south = level.GetTile(tilePos.South.Position, tilePos.Layer);
            int east = level.GetTile(tilePos.East.Position, tilePos.Layer);

            if (tile != north) sides += 1;
            if (tile != west) sides += 2;
            if (tile != south) sides += 4;
            if (tile != east) sides += 8;
        }
        if (data.Method == AutoTileType.Overlay)
        {

            int tile = level.GetTile(tilePos.Position, tilePos.Layer);
            int north = level.GetTile(tilePos.North.Position, tilePos.Layer);
            int west = level.GetTile(tilePos.West.Position, tilePos.Layer);
            int south = level.GetTile(tilePos.South.Position, tilePos.Layer);
            int east = level.GetTile(tilePos.East.Position, tilePos.Layer);

            if (tile != north) overlays.Add(new(
                data.Overlay,
                new Point(0, -1),
                0f
            ));
            if (tile != west) overlays.Add(new(
                data.Overlay,
                new Point(-1, 0),
                MathHelper.ToRadians(270)
            ));
            if (tile != south) overlays.Add(new(
                data.Overlay,
                new Point(0, 1),
                MathHelper.ToRadians(180)
            ));
            if (tile != east) overlays.Add(new(
                data.Overlay,
                new Point(1, 0),
                MathHelper.ToRadians(90)
            ));
        }

        if (sides >= data.TextureIdentifiers.Length)
            return new(null, overlays);
        return new(data.TextureIdentifiers[sides], overlays);
    }
}