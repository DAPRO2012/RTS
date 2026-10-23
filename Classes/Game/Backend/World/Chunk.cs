using System;
using System.Text.Json.Nodes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RTS;
using SharpDX.Win32;

public class Chunk
{
    public const int ChunkSize = 32;
    public const int TileSize = 32;
    public const int LayerCount = 3;

    public Point GridPosition { get; private set; }
    private readonly int[][][] _tiles;

    private RenderTarget2D _cachedTexture;

    public bool IsDirty { get; set; } = true;
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
            ChunkSize * TileSize, 
            ChunkSize * TileSize
        );
    }

    public void SetTile(int x, int y, WorldLayer layer, Identifier tileID)
    {
        _tiles[(int)layer][x][y] = BuiltInRegistries.STRUCTURES.GetNumericId(tileID);
        IsDirty = true; 
    }

    public int GetTile(int x, int y, WorldLayer layer)
    {
        return _tiles[(int)layer][x][y];
    }

    public void BuildCache(SpriteBatch spriteBatch, TextureManager textures)
    {
        if (!IsDirty || _isDisposed) return;

        Main.GetDevice().SetRenderTarget(_cachedTexture);
        Main.GetDevice().Clear(Color.Transparent);

        spriteBatch.Begin(samplerState: SamplerState.PointClamp);

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
                        blockId = BuiltInRegistries.STRUCTURES.GetIdentifier(tileId);
                    }
                    else if (l == (int)WorldLayer.Material)
                    {
                        registryId = BuiltInRegistries.MATERIALS.GetRegistryIdentifier();
                        blockId = BuiltInRegistries.MATERIALS.GetIdentifier(tileId);
                    }
                    else
                    {
                        registryId = BuiltInRegistries.GROUNDS.GetRegistryIdentifier();
                        blockId = BuiltInRegistries.GROUNDS.GetIdentifier(tileId);
                    }

                    TextureRegion tileTexture = textures.GetRegion(registryId, blockId);

                    if (tileTexture != null)
                    {
                        tileTexture.Draw(
                            spriteBatch,
                            new Vector2(x * TileSize, y * TileSize),
                            Color.White);
                    }
                }
            }
        }

        spriteBatch.End();
        Main.GetDevice().SetRenderTarget(null);
        IsDirty = false;
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        if (_isDisposed) return;
        
        Vector2 worldPosition = new(GridPosition.X * ChunkSize * TileSize, GridPosition.Y * ChunkSize * TileSize);
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