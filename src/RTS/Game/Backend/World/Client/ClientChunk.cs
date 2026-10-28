using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RTS;

public class ClientChunk : IChunk
{
    public Point GridPosition { get; set; }
    private readonly int[][][] _tiles;

    private RenderTarget2D _cachedTexture;
    private RenderTarget2D _cachedOverlayTexture;

    public bool IsDirty { get; set; } = true;
    public bool IsChanged { get; set; } = false;
    private bool _isDisposed = false;

    public ClientChunk(Point gridPosition)
    {
        _tiles = IChunk.CreateEmptyTiles();
        GridPosition = gridPosition;

        _cachedTexture = new RenderTarget2D(
            Main.GetDevice(),
            (IChunk.ChunkSize + 2) * IChunk.TileSize,
            (IChunk.ChunkSize + 2) * IChunk.TileSize
        );
        _cachedOverlayTexture = new RenderTarget2D(
            Main.GetDevice(),
            (IChunk.ChunkSize + 2) * IChunk.TileSize,
            (IChunk.ChunkSize + 2) * IChunk.TileSize
        );
    }

    public void BuildCache(SpriteBatch spriteBatch, TextureManager textures, ILevel level)
    {
        if (!IsDirty || _isDisposed) return;

        Main.GetDevice().SetRenderTarget(_cachedTexture);
        Main.GetDevice().Clear(Color.Transparent);

        spriteBatch.Begin(samplerState: SamplerState.PointClamp);

        List<AutoTileUtil.OverlayDraw> overlays = [];
        for (int l = 0; l < IChunk.LayerCount; l++)
        {
            for (int x = 0; x < IChunk.ChunkSize; x++)
            {
                for (int y = 0; y < IChunk.ChunkSize; y++)
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
                        int worldX = GridPosition.X * IChunk.ChunkSize + x;
                        int worldY = GridPosition.Y * IChunk.ChunkSize + y;
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
                            new Vector2((x + 1) * IChunk.TileSize, (y + 1) * IChunk.TileSize),
                            Color.White);
                    
                }
            }
        }
        spriteBatch.End();

        Main.GetDevice().SetRenderTarget(_cachedOverlayTexture);
        Main.GetDevice().Clear(Color.Transparent);
        spriteBatch.Begin(samplerState: SamplerState.PointClamp);
        foreach (var overlay in overlays)
        {
            TextureRegion tileTexture = textures.GetRegion(overlay.Texture);
            tileTexture?.Draw(
                spriteBatch,
                new Vector2((overlay.TileOffset.X + 1) * IChunk.TileSize, (overlay.TileOffset.Y + 1) * IChunk.TileSize),
                Color.White,
                rotation: overlay.Rotation);
        }
        spriteBatch.End();

        Main.GetDevice().SetRenderTarget(null);
        IsDirty = false;
    }

    public void DrawBase(SpriteBatch spriteBatch)
    {
        if (_isDisposed) return;
        
        Vector2 worldPosition = new(
            GridPosition.X * IChunk.ChunkSize * IChunk.TileSize - IChunk.TileSize,
            GridPosition.Y * IChunk.ChunkSize * IChunk.TileSize - IChunk.TileSize);
        spriteBatch.Draw(
            _cachedTexture,
            worldPosition,
            null,
            Color.White,
            0f,
            Vector2.Zero,
            1f,
            SpriteEffects.None,
            1f);
    }

    public void DrawOverlays(SpriteBatch spriteBatch)
    {
        if (_isDisposed) return;

        Vector2 worldPosition = new(
            GridPosition.X * IChunk.ChunkSize * IChunk.TileSize - IChunk.TileSize,
            GridPosition.Y * IChunk.ChunkSize * IChunk.TileSize - IChunk.TileSize);
        spriteBatch.Draw(
            _cachedOverlayTexture,
            worldPosition,
            null,
            Color.White,
            0f,
            Vector2.Zero,
            1f,
            SpriteEffects.None,
            0f);
    }

    public void Dispose()
    {
        if (!_isDisposed)
        {
            _cachedTexture?.Dispose();
            _cachedOverlayTexture?.Dispose();
            _isDisposed = true;
        }
    }

    public void SetTile(int x, int y, WorldLayer layer, Identifier tileID)
    {
        _tiles[(int)layer][x][y] = BuiltInRegistries.STRUCTURES.GetId(tileID);
        IsChanged = true;
        IsDirty = true;
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
        IsChanged = false;
    }
    public ChunkSnapshot GenerateSnapshot() =>
        new ChunkSnapshot(_tiles);
}