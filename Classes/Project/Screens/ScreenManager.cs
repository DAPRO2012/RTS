#nullable enable

using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

public class ScreenManager
{
    private readonly Stack<Screen> _screens = new();

    public void Push(Screen screen) => _screens.Push(screen);
    public Screen? Pop() => _screens.Count > 0 ? _screens.Pop() : null;
    public Screen? Peek() => _screens.Count > 0 ? _screens.Peek() : null;


    public void Update(GameTime gameTime)
    {
        foreach (Screen screen in _screens)
            screen.Update(gameTime);
    }
    public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        var screens = _screens.ToArray();
        spriteBatch.Begin();
            for (int i = screens.Length - 1; i >= 0; i--)
                screens[i].Draw(gameTime, spriteBatch);
        spriteBatch.End();
    }
}