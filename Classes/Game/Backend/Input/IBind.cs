using Microsoft.Xna.Framework.Input;

public interface IBind
{
    public Identifier Id { get; }

    public void Update((KeyboardState keyboard, MouseState mouse) states);

    public bool IsDown();
    public bool ConsumeClick();
}