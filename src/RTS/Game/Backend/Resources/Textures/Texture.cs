using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RTS;

public class Texture : ITexture
{
    Texture2D[] _t;
    public Texture2D[] GetTextures() => _t;

    public Texture(Resource resource, GraphicsDevice device)
    {
        using Stream stream = resource.Open();
        _t = [Texture2D.FromStream(device, stream)];
    }

    public Texture(Texture2D[] t) => _t = t;

    public void Draw(SpriteBatch spriteBatch, Rectangle rectangle, Color color, bool[][] surroundingTiles) =>
        Draw(spriteBatch, rectangle, color);
    public void Draw(SpriteBatch spriteBatch, Rectangle rectangle, Color color) =>
        spriteBatch.Draw(_t[0], rectangle, color);

    public static Texture2D CreateTexture(int width, int height) => new Texture2D(Main.GetDevice(), width, height);
}