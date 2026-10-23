using Microsoft.Xna.Framework.Input;

public class KeyPressedEvent(Keys key, KeyboardManager.Action action) : Event
{
    public Keys Key = key;
    public KeyboardManager.Action Action { get; private set; } = action;
}