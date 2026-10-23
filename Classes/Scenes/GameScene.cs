using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

public class GameScene(ContentManager content) : Scene(content)
{
    Level lvl = new("test");
    TextureManager textureManager = new();

    public override void Initialize(ContentManager content)
    {
        lvl.LoadChunk(Point.Zero);
        lvl.UnloadChunk(Point.Zero);
    }

    public override void Update(GameTime gameTime)
    {
    }

    public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        spriteBatch.GraphicsDevice.Clear(Color.White);

        lvl.BuildCache(spriteBatch, textureManager);
        spriteBatch.Begin();
            lvl.Draw(spriteBatch, textureManager);
        spriteBatch.End();
    }
}