using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

public class MouseManager
{
    public MouseState PrevState = new();
    public MouseState State = Mouse.GetState();

    public (ButtonState Current, ButtonState Previous) LEFT => (State.LeftButton, PrevState.LeftButton);
    public (ButtonState Current, ButtonState Previous, int Scroll, int PreviousScroll) MIDDLE => (State.MiddleButton, PrevState.MiddleButton, State.ScrollWheelValue, PrevState.ScrollWheelValue);
    public (ButtonState Current, ButtonState Previous) RIGHT => (State.RightButton, PrevState.RightButton);

    public (Point Current, Point Previous) POSITION => (State.Position, PrevState.Position);

    public void Update(GameTime gameTime)
    {
        PrevState = State;
        State = Mouse.GetState();
    }
}