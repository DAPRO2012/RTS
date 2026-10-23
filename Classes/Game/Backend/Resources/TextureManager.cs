using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

public class TextureManager
{
    private Texture2D _blockAtlas;
    private Dictionary<Identifier, Dictionary<Identifier, TextureRegion>> _regions = new();

    public void LoadContent(GraphicsDevice graphics, Texture2D loadedAtlas)
    {
        _blockAtlas = loadedAtlas;
    }

    public TextureRegion GetRegion(Identifier registryId, Identifier id)
    {
        if (id != null && _regions.TryGetValue(registryId, out var registry) && registry.TryGetValue(id, out var region))
        {
            return region;
        }
            
        return null;
    }

    public Texture2D CreateAtlas(Identifier registryId, Dictionary<Identifier, Texture> textures, GraphicsDevice device)
    {
        if (textures.Count == 0) return null;
        const int AtlasTextureColumns = 20;

        if (!_regions.ContainsKey(registryId))
        {
            _regions[registryId] = new Dictionary<Identifier, TextureRegion>();
        }

        int tileWidth = 0;
        int tileHeight = 0;
        foreach (var tex in textures.Values)
        {
            tileWidth = Math.Max(tileWidth, tex.Width);
            tileHeight = Math.Max(tileHeight, tex.Height);
        }

        int totalTextures = textures.Count;
        int columns = Math.Min(totalTextures, AtlasTextureColumns);
        int rows = (int)Math.Ceiling((float)totalTextures / AtlasTextureColumns);

        int atlasWidth = columns * tileWidth;
        int atlasHeight = rows * tileHeight;

        RenderTarget2D atlas = new RenderTarget2D(device, atlasWidth, atlasHeight);

        device.SetRenderTarget(atlas);
        device.Clear(Color.Transparent);

        using (SpriteBatch sb = new SpriteBatch(device))
        {
            sb.Begin(samplerState: SamplerState.PointClamp);

            int currentIndex = 0;
            foreach (KeyValuePair<Identifier, Texture> item in textures)
            {
                int currentColumn = currentIndex % AtlasTextureColumns;
                int currentRow = currentIndex / AtlasTextureColumns;

                Rectangle destinationRect = new Rectangle(
                    currentColumn * tileWidth,
                    currentRow * tileHeight,
                    tileWidth,
                    tileHeight
                );

                item.Value.Draw(sb, destinationRect, Color.White);

                _regions[registryId][item.Key] = new TextureRegion(atlas, destinationRect);

                currentIndex++;
            }

            sb.End();
        }

        device.SetRenderTarget(null);

        _blockAtlas = atlas;
        return atlas;
    }
}

public class TextureRegion
{
    public Texture2D Atlas { get; }
    public Rectangle SourceRect { get; }

    public TextureRegion(Texture2D atlas, Rectangle sourceRect)
    {
        Atlas = atlas;
        SourceRect = sourceRect;
    }
}