using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using RTS;

public class KeyboardManager
{
    public enum Action
    {
        CLICKED,
        RELEASED,
        REPEAT
    }

    public KeyboardState PrevState =  new();
    public KeyboardState State = Keyboard.GetState();

    public void Update(GameTime gameTime)
    {
        PrevState = State;
        State = Keyboard.GetState();

        var prevPressedKeys = PrevState.GetPressedKeys();
        var pressedKeys = State.GetPressedKeys();

        foreach (Keys item in Enum.GetValues(typeof(Keys)))
        {
            if (prevPressedKeys.Contains(item) && pressedKeys.Contains(item))
                Main.EVENT_BUS.Publish(new KeyPressedEvent(item, Action.REPEAT));
            else if (pressedKeys.Contains(item))
                Main.EVENT_BUS.Publish(new KeyPressedEvent(item, Action.CLICKED));
            else if (prevPressedKeys.Contains(item))
                Main.EVENT_BUS.Publish(new KeyPressedEvent(item, Action.RELEASED));
        }
    }

    public bool IsClicked(Keys key) => PrevState.IsKeyUp(key) && State.IsKeyDown(key);
    public bool IsRepeat(Keys key) => PrevState.IsKeyDown(key) && State.IsKeyDown(key);
    public bool IsReleased(Keys key) => PrevState.IsKeyDown(key) && State.IsKeyUp(key);
}