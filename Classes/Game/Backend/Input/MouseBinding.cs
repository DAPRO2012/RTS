using Microsoft.Xna.Framework.Input;

public class MouseBinding : IBind
{
    public Identifier Id { get; }
    public MouseButton BoundButton { get; set; }
    
    private bool isDown;
    private int pressCount;

    public enum MouseButton { Left, Right, Middle }

    public MouseBinding(Identifier id, MouseButton defaultButton)
    {
        Id = id;
        BoundButton = defaultButton;
    }

    public void Update((KeyboardState keyboard, MouseState mouse) states)
    {
        bool currentlyDown = BoundButton switch
        {
            MouseButton.Left => states.mouse.LeftButton == ButtonState.Pressed,
            MouseButton.Right => states.mouse.RightButton == ButtonState.Pressed,
            MouseButton.Middle => states.mouse.MiddleButton == ButtonState.Pressed,
            _ => false
        };

        if (currentlyDown && !isDown)
        {
            pressCount++;
        }
        
        isDown = currentlyDown;
    }

    public bool IsDown() => isDown;

    public bool ConsumeClick()
    {
        if (pressCount > 0)
        {
            pressCount--;
            return true;
        }
        return false;
    }
}