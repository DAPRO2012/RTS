using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RTS;

public class Texture : DrawableResource
{
    public Texture(Identifier id, ResourceManager resources)
    {
        Resource resource = resources.GetResource(
            ResourceType.Assets,
            id);
        if (resource == null)
            throw new FileNotFoundException($"Texture resource not found: {id}");

        using Stream stream = resource.Open();
        _t = Texture2D.FromStream(Main.GetDevice(), stream);
    }

    public Texture(Resource resource)
    {
        using Stream stream = resource.Open();
        _t = Texture2D.FromStream(Main.GetDevice(), stream);
    }

    public Texture(Texture2D t) => _t = t;

    public void Draw(SpriteBatch spriteBatch, Rectangle rectangle, Color color)
    {
        spriteBatch.Draw(_t, rectangle, color);
    }

    public static Texture2D CreateTexture(int width, int height) => new Texture2D(Device, width, height);
}