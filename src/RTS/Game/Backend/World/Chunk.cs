using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RTS;
using SharpDX.Direct2D1.Effects;
using SharpDX.Win32;

public class Chunk
{
    private static readonly Logger LOGGER = LogUtils.GetLogger();

    public const int ChunkSize = 64;
    public const int TileSize = 32;
    public const int LayerCount = 3;

    public Point GridPosition { get; private set; }
    private readonly int[][][] _tiles;

    private RenderTarget2D _cachedTexture;

    public bool IsDirty { get; set; } = true;
    public bool IsChanged { get; set; } = false;
    private bool _isDisposed = false;

    private static int[][][] CreateEmptyTiles()
    {
        int[][][] tiles = new int[LayerCount][][];
        for (int l = 0; l < LayerCount; l++)
        {
            tiles[l] = new int[ChunkSize][];
            for (int x = 0; x < ChunkSize; x++)
            {
                tiles[l][x] = new int[ChunkSize];
            }
        }
        return tiles;
    }

    public Chunk(Point gridPosition)
        : this(gridPosition, CreateEmptyTiles())
    {
    }

    public Chunk(Point gridPosition, int[][][] tiles)
    {
        GridPosition = gridPosition;
        _tiles = (int[][][])tiles.Clone();

        _cachedTexture = new RenderTarget2D(
            Main.GetDevice(),
            (ChunkSize + 2) * TileSize,
            (ChunkSize + 2) * TileSize
        );
    }

    public void SetTile(int x, int y, WorldLayer layer, Identifier tileID)
    {
        _tiles[(int)layer][x][y] = BuiltInRegistries.STRUCTURES.GetId(tileID);
        IsDirty = true;
        IsChanged = true;
    }

    public int GetTile(int x, int y, WorldLayer layer)
    {
        return _tiles[(int)layer][x][y];
    }

    public void BuildCache(SpriteBatch spriteBatch, TextureManager textures, Level level)
    {
        if (!IsDirty || _isDisposed) return;

        Main.GetDevice().SetRenderTarget(_cachedTexture);
        Main.GetDevice().Clear(Color.Transparent);

        spriteBatch.Begin(samplerState: SamplerState.PointClamp);

        List<AutoTileUtil.OverlayDraw> overlays = [];
        for (int l = 0; l < LayerCount; l++)
        {
            for (int x = 0; x < ChunkSize; x++)
            {
                for (int y = 0; y < ChunkSize; y++)
                {
                    int tileId = _tiles[l][x][y];
                    if (tileId == 0) continue;

                    Identifier registryId;
                    Identifier blockId;
                    if (l == (int)WorldLayer.Structure)
                    {
                        registryId = BuiltInRegistries.STRUCTURES.GetRegistryIdentifier();
                        blockId = BuiltInRegistries.STRUCTURES.ById(tileId);
                    }
                    else if (l == (int)WorldLayer.Material)
                    {
                        registryId = BuiltInRegistries.MATERIALS.GetRegistryIdentifier();
                        blockId = BuiltInRegistries.MATERIALS.ById(tileId);
                    }
                    else
                    {
                        registryId = BuiltInRegistries.GROUNDS.GetRegistryIdentifier();
                        blockId = BuiltInRegistries.GROUNDS.ById(tileId);
                    }

                    TextureRegion tileTexture;
                    TextureData tileData = textures.GetTextureData(registryId, blockId);
                    if (tileData != null)
                    {
                        int worldX = GridPosition.X * ChunkSize + x;
                        int worldY = GridPosition.Y * ChunkSize + y;
                        var textureId = AutoTileUtil.GetRequiredTexture(tileData, new WorldPos(worldX, worldY, (WorldLayer)l), level);
                        foreach (var overlay in  textureId.Overlays)
                            overlays.Add(
                                new(
                                    overlay.Texture,
                                    new Point(x + overlay.TileOffset.X, y + overlay.TileOffset.Y),
                                    overlay.Rotation
                                )
                            );

                        tileTexture = textures.GetRegion(textureId.BaseTexture);
                    } else
                        tileTexture = textures.GetRegion(registryId, blockId);
                    tileTexture?.Draw(
                            spriteBatch,
                            new Vector2((x + 1) * TileSize, (y + 1) * TileSize),
                            Color.White);
                    
                }
            }
        }
        foreach (var overlay in overlays)
        {
            TextureRegion tileTexture = textures.GetRegion(overlay.Texture);
            tileTexture?.Draw(
                spriteBatch,
                new Vector2((overlay.TileOffset.X + 1) * TileSize, (overlay.TileOffset.Y + 1) * TileSize),
                Color.White,
                rotation:overlay.Rotation);
        }

        spriteBatch.End();
        Main.GetDevice().SetRenderTarget(null);
        IsDirty = false;
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        if (_isDisposed) return;
        
        Vector2 worldPosition = new(
            GridPosition.X * ChunkSize * TileSize - TileSize,
            GridPosition.Y * ChunkSize * TileSize - TileSize);
        spriteBatch.Draw(_cachedTexture, worldPosition, Color.White);
    }

    public void Dispose()
    {
        if (!_isDisposed)
        {
            _cachedTexture?.Dispose();
            _isDisposed = true;
        }
    }

    public static readonly Codec<Chunk> CODEC = RecordCodecBuilder<Chunk>.Create()
        .Field(Codec.INT.FieldOf("x"), data => data.GridPosition.X)
        .Field(Codec.INT.FieldOf("y"), data => data.GridPosition.Y)
        .Field(Codec.INT.ArrayOf().ArrayOf().ArrayOf().FieldOf("tiles"),
            data => data._tiles)
        .Build(fields =>
            new Chunk(
                new Point((int)fields[0], (int)fields[1]),
                (int[][][])fields[2]));
}