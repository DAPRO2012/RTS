using Microsoft.Xna.Framework.Input;

public class KeyBinding : IBind
{
    public Identifier Id { get; }
    public Keys BoundKey { get; set; }
    
    private bool isDown;
    private int pressTime;

    public KeyBinding(Identifier id, Keys defaultKey)
    {
        Id = id;
        BoundKey = defaultKey;
    }

    public void Update((KeyboardState keyboard, MouseState mouse) states)
    {
        bool currentlyDown = states.keyboard.IsKeyDown(BoundKey);
        
        if (currentlyDown && !isDown)
        {
            pressTime++;
        }
        
        isDown = currentlyDown;
    }

    public bool IsDown() => isDown;

    public bool ConsumeClick()
    {
        if (pressTime > 0)
        {
            pressTime--;
            return true;
        }
        return false;
    }
}