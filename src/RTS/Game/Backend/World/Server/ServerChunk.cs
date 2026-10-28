using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RTS;
using SharpDX.Direct2D1.Effects;
using SharpDX.Win32;

public class ServerChunk : IChunk
{
    private static readonly Logger LOGGER = LogUtils.GetLogger();

    public Point GridPosition { get; set; }
    private readonly int[][][] _tiles;

    public bool IsChanged { get; set; } = false;
    private bool _isDirty = true;
    public bool IsDirty
    {
        get
        {
            bool returnBool = _isDirty;
            _isDirty = false;
            return returnBool;
        }
        set =>
            _isDirty = value;
    }

    public ServerChunk(Point gridPosition)
        : this(gridPosition, IChunk.CreateEmptyTiles())
    {
    }

    public ServerChunk(Point gridPosition, int[][][] tiles)
    {
        GridPosition = gridPosition;
        _tiles = (int[][][])tiles.Clone();
    }

    public void SetTile(int x, int y, WorldLayer layer, Identifier tileID)
    {
        _tiles[(int)layer][x][y] = BuiltInRegistries.STRUCTURES.GetId(tileID);
        IsChanged = true;
    }

    public int GetTile(int x, int y, WorldLayer layer)
    {
        return _tiles[(int)layer][x][y];
    }

    public void UpdateWithSnapshot(ChunkSnapshot snapshot)
    {
        for (int l = 0; l < snapshot.chunkTiles.Length; l++)
        for (int x = 0; x < snapshot.chunkTiles[0].Length; x++)
        for (int y = 0; y < snapshot.chunkTiles[0][0].Length; y++)
        {
            if (snapshot.chunkTiles[l][x][y] != _tiles[l][x][y])
                _tiles[l][x][y] = snapshot.chunkTiles[l][x][y];
        }
    }
    public ChunkSnapshot GenerateSnapshot() =>
        new ChunkSnapshot(_tiles);

    public static readonly Codec<ServerChunk> CODEC = RecordCodecBuilder<ServerChunk>.Create()
        .Field(Codec.INT.FieldOf("x"), data => data.GridPosition.X)
        .Field(Codec.INT.FieldOf("y"), data => data.GridPosition.Y)
        .Field(Codec.INT.ArrayOf().ArrayOf().ArrayOf().FieldOf("tiles"),
            data => data._tiles)
        .Build(fields =>
            new ServerChunk(
                new Point((int)fields[0], (int)fields[1]),
                (int[][][])fields[2]));
}