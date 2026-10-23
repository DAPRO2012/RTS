using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using RTS;

public class Texture : DrawableResource
{
    public Texture(Identifier id, ContentManager content)
    {
        string rootDir =
             System.IO.Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory, 
                content.RootDirectory,
                "assets"
            );


        string path = Data.GetPath(rootDir, id);
        _t = Texture2D.FromFile(Main.GetDevice(), path);
    }

    public Texture(Texture2D t) => _t = t;

    public void Draw(SpriteBatch spriteBatch, Rectangle rectangle, Color color)
    {
        spriteBatch.Draw(_t, rectangle, color);
    }

    public static Texture2D CreateTexture(int width, int height) => new Texture2D(Device, width, height);
}