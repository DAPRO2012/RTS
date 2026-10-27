using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

public class TextureManager : IDisposable
{
    private const string textureFolder = "textures/";

    private readonly Dictionary<Identifier, Dictionary<Identifier, TextureRegion>> _regions = new();
    private readonly Dictionary<Identifier, Dictionary<Identifier, TextureData>> _definitions = [];
    private readonly List<Texture2D> _atlases = [];

    public void LoadContent(ResourceManager resources, GraphicsDevice device)
    {
        Dictionary<Identifier, Dictionary<Identifier, ITexture>> textures = [];
        Dictionary<Identifier, Dictionary<Identifier, TextureData>> textureData = [];

        try
        {
            foreach (Resource resource in resources.ListResources(ResourceType.Data).Where(r => r.Id.GetPath().StartsWith(textureFolder)))
            {
                string definitionPath = resource.Id.GetPath()[textureFolder.Length..];
                int separator = definitionPath.IndexOf('/');
                if (separator <= 0 || separator == definitionPath.Length - 1)
                    throw new InvalidDataException(
                        $"Texture definition must include a registry and texture path: {resource.Id}");

                Identifier registry = Identifier.fromNamespaceAndPath(
                    resource.Id.GetNamespace(),
                    definitionPath[..separator]);
                string texturePath = System.IO.Path.ChangeExtension(definitionPath[(separator + 1)..], null)
                    .Replace('\\', '/');
                Identifier textureId = Identifier.fromNamespaceAndPath(resource.Id.GetNamespace(), texturePath);

                using Stream stream = resource.Open();
                DataResult<TextureData> currTexDataResult = TextureData.CODEC.Decode(JsonNode.Parse(stream));
                if (currTexDataResult.IsError())
                    continue;
                TextureData currTexData = currTexDataResult.Result().Value;

                if (!textureData.TryGetValue(registry, out Dictionary<Identifier, TextureData> registryTextures))
                {
                    registryTextures = [];
                    textureData.Add(registry, registryTextures);
                }
                registryTextures.Add(textureId, currTexData);
            }

            foreach (Resource resource in resources.ListResources(ResourceType.Assets))
            {
                if (!TryGetTextureIds(resource.Id, out Identifier registryId, out Identifier textureId))
                    continue;

                if (!textures.TryGetValue(registryId, out Dictionary<Identifier, ITexture> registryTextures))
                {
                    registryTextures = [];
                    textures.Add(registryId, registryTextures);
                }

                if (!registryTextures.TryAdd(textureId, new Texture(resource, device)))
                    throw new InvalidDataException($"Multiple texture files resolve to {registryId}/{textureId}.");
            }

            foreach (KeyValuePair<Identifier, Dictionary<Identifier, ITexture>> registryTextures in textures)
                CreateAtlas(registryTextures.Key, registryTextures.Value, textureData.GetValueOrDefault(registryTextures.Key, []), device);
        }
        finally
        {
            foreach (ITexture texture in textures.Values.SelectMany(registryTextures => registryTextures.Values))
            foreach (Texture2D texture2D in texture.GetTextures())
                texture2D.Dispose();
        }
    }

    public TextureRegion GetRegion(Identifier registryId, Identifier id)
    {
        if (id != null && _regions.TryGetValue(registryId, out var registry) && registry.TryGetValue(id, out var region))
        {
            return region;
        }
        else if (_regions.TryGetValue(Identifier.fromDefaultNamespace("textures"), out var missingRegistry) && missingRegistry.TryGetValue(Identifier.fromDefaultNamespace("missing"), out var missingRegion))
        {
            return missingRegion;
        }
        return null;
    }

    public TextureRegion GetRegion(Identifier resourceId)
    {
        if (resourceId == null)
            return GetRegion(null, null);
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

    public TextureData GetTextureData(Identifier registryId, Identifier id)
    {
        return _definitions.TryGetValue(registryId, out var definitions)
            && definitions.TryGetValue(id, out var data)
                ? data
                : null;
    }

    public TextureData GetTextureData(Identifier resourceId)
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

        return GetTextureData(registryId, textureId);
    }

    public Texture2D CreateAtlas(Identifier registryId, Dictionary<Identifier, ITexture> textures, Dictionary<Identifier, TextureData> data, GraphicsDevice device)
    {
        if (textures.Count == 0) return null;
        const int AtlasTextureColumns = 20;
        
        if (!_definitions.TryGetValue(registryId, out Dictionary<Identifier, TextureData> definitions))
        {
            definitions = data;
            _definitions.Add(registryId, definitions);
        }
        else foreach (var dataKey in data)
            definitions[dataKey.Key] = dataKey.Value;
            

        if (!_regions.ContainsKey(registryId))
        {
            _regions[registryId] = new Dictionary<Identifier, TextureRegion>();
        }

        int tileWidth = 0;
        int tileHeight = 0;
        foreach (var tex in textures.Values)
        foreach (Texture2D texture2D in tex.GetTextures())
        {
            tileWidth = Math.Max(tileWidth, texture2D.Width);
            tileHeight = Math.Max(tileHeight, texture2D.Height);
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
                foreach (KeyValuePair<Identifier, ITexture> item in textures)
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

    public void Draw(SpriteBatch spriteBatch, Vector2 position, Color color, Point? size = null, float rotation = 0f, float depth = 0f)
    {
        Point drawSize = size ?? new Point(Chunk.TileSize);
        if (rotation == 0f)
        {
            spriteBatch.Draw(Atlas, new Rectangle(position.ToPoint(), drawSize), SourceRect, color);
            return;
        }

        Vector2 center = position + new Vector2(drawSize.X / 2f, drawSize.Y / 2f);
        Vector2 origin = new(SourceRect.Width / 2f, SourceRect.Height / 2f);
        Vector2 scale = new(
            drawSize.X / (float)SourceRect.Width,
            drawSize.Y / (float)SourceRect.Height);
        spriteBatch.Draw(Atlas, center, SourceRect, color, rotation, origin, scale, SpriteEffects.None, depth);
    }

    public TextureRegion(Texture2D atlas, Rectangle sourceRect)
    {
        Atlas = atlas;
        SourceRect = sourceRect;
    }
}