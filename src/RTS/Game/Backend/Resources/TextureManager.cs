using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

public class TextureManager : IDisposable
{
    private readonly Dictionary<Identifier, Dictionary<Identifier, TextureRegion>> _regions = new();
    private readonly List<Texture2D> _atlases = [];

    public void LoadContent(ResourceManager resources, GraphicsDevice device)
    {
        Dictionary<Identifier, Dictionary<Identifier, Texture>> textures = [];

        try
        {
            foreach (Resource resource in resources.ListResources(ResourceType.Assets))
            {
                if (!TryGetTextureIds(resource.Id, out Identifier registryId, out Identifier textureId))
                    continue;

                if (!textures.TryGetValue(registryId, out Dictionary<Identifier, Texture> registryTextures))
                {
                    registryTextures = [];
                    textures.Add(registryId, registryTextures);
                }

                if (!registryTextures.TryAdd(textureId, new Texture(resource)))
                    throw new InvalidDataException($"Multiple texture files resolve to {registryId}/{textureId}.");
            }

            foreach (KeyValuePair<Identifier, Dictionary<Identifier, Texture>> registryTextures in textures)
                CreateAtlas(registryTextures.Key, registryTextures.Value, device);
        }
        finally
        {
            foreach (Texture texture in textures.Values.SelectMany(registryTextures => registryTextures.Values))
                texture.GetTexture2D().Dispose();
        }
    }

    public TextureRegion GetRegion(Identifier registryId, Identifier id)
    {
        if (id != null && _regions.TryGetValue(registryId, out var registry) && registry.TryGetValue(id, out var region))
        {
            return region;
        }
            
        return null;
    }

    public TextureRegion GetRegion(Identifier resourceId)
    {
        string resourcePath = resourceId.GetPath();
        int separator = resourcePath.IndexOf('/');
        if (separator <= 0 || separator == resourcePath.Length - 1)
            return null;

        Identifier registryId = Identifier.fromNamespaceAndPath(
            resourceId.GetNamespace(),
            resourcePath[..separator]);
        string texturePath = System.IO.Path.ChangeExtension(resourcePath[(separator + 1)..], null)
            .Replace('\\', '/');
        Identifier textureId = Identifier.fromNamespaceAndPath(resourceId.GetNamespace(), texturePath);

        return GetRegion(registryId, textureId);
    }

    public TextureRegion GetRequiredRegion(Identifier resourceId)
    {
        return GetRegion(resourceId)
            ?? throw new KeyNotFoundException($"Texture atlas region not found: {resourceId}");
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
        int rows = (int)Math.Ceiling((float)totalTextures / columns);

        int atlasWidth = columns * tileWidth;
        int atlasHeight = rows * tileHeight;

        RenderTarget2D atlas = new RenderTarget2D(device, atlasWidth, atlasHeight);
        try
        {
            device.SetRenderTarget(atlas);
            device.Clear(Color.Transparent);

            using (SpriteBatch sb = new SpriteBatch(device))
            {
                sb.Begin(samplerState: SamplerState.PointClamp);

                int currentIndex = 0;
                foreach (KeyValuePair<Identifier, Texture> item in textures)
                {
                    int currentColumn = currentIndex % columns;
                    int currentRow = currentIndex / columns;

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
        }
        catch
        {
            atlas.Dispose();
            throw;
        }
        finally
        {
            device.SetRenderTarget(null);
        }

        _atlases.Add(atlas);
        return atlas;
    }

    public static bool TryGetTextureIds(
        Identifier resourceId,
        out Identifier registryId,
        out Identifier textureId)
    {
        string resourcePath = resourceId.GetPath();
        string extension = System.IO.Path.GetExtension(resourcePath);
        if (!extension.Equals(".png", StringComparison.OrdinalIgnoreCase)
            && !extension.Equals(".jpg", StringComparison.OrdinalIgnoreCase)
            && !extension.Equals(".jpeg", StringComparison.OrdinalIgnoreCase)
            && !extension.Equals(".bmp", StringComparison.OrdinalIgnoreCase))
        {
            registryId = null;
            textureId = null;
            return false;
        }

        int separator = resourcePath.IndexOf('/');
        if (separator <= 0 || separator == resourcePath.Length - 1)
            throw new InvalidDataException(
                $"Texture resource must include a registry and texture path: {resourceId}");

        registryId = Identifier.fromNamespaceAndPath(resourceId.GetNamespace(), resourcePath[..separator]);
        string texturePath = System.IO.Path.ChangeExtension(resourcePath[(separator + 1)..], null)
            .Replace('\\', '/');
        textureId = Identifier.fromNamespaceAndPath(resourceId.GetNamespace(), texturePath);
        return true;
    }

    public void Dispose()
    {
        foreach (Texture2D atlas in _atlases)
            atlas.Dispose();
        _atlases.Clear();
        _regions.Clear();
    }
}

public class TextureRegion
{
    public Texture2D Atlas { get; }
    public Rectangle SourceRect { get; }

    public void Draw(SpriteBatch spriteBatch, Rectangle destination, Color color) =>
        spriteBatch.Draw(Atlas, destination, SourceRect, color);

    public void Draw(SpriteBatch spriteBatch, Vector2 position, Color color, Point? size = null) =>
        spriteBatch.Draw(Atlas, new Rectangle(position.ToPoint(), size ?? new Point(Chunk.TileSize)), SourceRect, color);

    public TextureRegion(Texture2D atlas, Rectangle sourceRect)
    {
        Atlas = atlas;
        SourceRect = sourceRect;
    }
}