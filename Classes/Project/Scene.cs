using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using RTS;

public abstract class Scene
{
    //Handling
    private static Scene _currScene = null;
    public static void Set(Identifier id)
    {
        Scene changeTo = BuiltInRegistries.SCENES.Get(id);
        OnSceneChangeEvent @event = new OnSceneChangeEvent(changeTo);
        Main.EVENT_BUS.Publish(@event);
        if (@event.AllowChange) _currScene = changeTo;
    }
    public static Scene Get() => _currScene;

    public static void UpdateScene(GameTime gameTime) => _currScene?.Update(gameTime);
    public static void DrawScene(GameTime gameTime, SpriteBatch spriteBatch) => _currScene?.Draw(gameTime, spriteBatch);

    //Scene Logic
    protected List<IElement> drawables = [];
    protected void UpdateDrawables(GameTime gameTime) => drawables.ForEach((d) => d.Update(gameTime));
    protected void DrawDrawables(GameTime gameTime, SpriteBatch spriteBatch) => drawables.ForEach((d) => d.Draw(gameTime, spriteBatch));

    public Scene(ContentManager content)
    {
        Initialize(content);
    }

    public abstract void Initialize(ContentManager content);
    public abstract void Update(GameTime gameTime);
    public abstract void Draw(GameTime gameTime, SpriteBatch spriteBatch);
}