using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

public interface IDrawable
{
    public void Draw(GameTime gameTime, SpriteBatch spriteBatch);
}