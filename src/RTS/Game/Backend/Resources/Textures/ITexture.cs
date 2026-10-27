using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

public interface ITexture
{
    public Texture2D[] GetTextures();

    public void Draw(SpriteBatch spriteBatch, Rectangle rectangle, Color color, bool[][] surroundingTiles);
    public void Draw(SpriteBatch spriteBatch, Rectangle rectangle, Color color);

}