using Microsoft.Xna.Framework.Graphics;
using RTS;

public class DrawableResource
{
    protected static GraphicsDevice Device => Main.GetDevice();
    protected Texture2D _t;

    public Texture2D GetTexture2D() => _t;
    public int Width => _t.Width;
    public int Height => _t.Height;
}