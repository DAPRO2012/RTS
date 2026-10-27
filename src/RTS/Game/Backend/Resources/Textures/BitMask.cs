using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

public class BitMaskTexture : ITexture
{
    Texture2D[] _t;
    public Texture2D[] GetTextures() => _t;

    public BitMaskTexture(Texture2D[] t) => _t = t;

    public void Draw(SpriteBatch spriteBatch, Rectangle rectangle, Color color, bool[][] surroundingTiles)
    {
        
    }
    public void Draw(SpriteBatch spriteBatch, Rectangle rectangle, Color color) =>
        Draw(spriteBatch, rectangle, color, null);
}