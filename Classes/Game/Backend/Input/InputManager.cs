using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;

public class InputManager
{
    public static InputManager Instance { get; private set; } = new();

    private readonly Dictionary<Identifier, IBind> bindings = [];

    public KeyBinding MoveForward { get; private set; }
    public KeyBinding MoveBackward { get; private set; }
    public KeyBinding MoveLeft { get; private set; }
    public KeyBinding MoveRight { get; private set; }

    public MouseBinding LeftClick { get; private set; }

    public InputManager()
    {
        MoveForward = Register(new KeyBinding(Identifier.fromDefaultNamespace("Forward"), Keys.W));
        MoveBackward = Register(new KeyBinding(Identifier.fromDefaultNamespace("Backward"), Keys.S));

        MoveLeft = Register(new KeyBinding(Identifier.fromDefaultNamespace("Left"), Keys.A));
        MoveRight = Register(new KeyBinding(Identifier.fromDefaultNamespace("Right"), Keys.D));


        LeftClick = Register(new MouseBinding(Identifier.fromDefaultNamespace("LeftClick"), MouseBinding.MouseButton.Left));
    }

    private T Register<T>(T binding) where T : IBind
    {
        bindings.Add(binding.Id, binding);
        return binding;
    }

    public void Update()
    {
        KeyboardState kstate = Keyboard.GetState();
        MouseState mstate = Mouse.GetState();

        foreach (IBind binding in bindings.Values)
        {
            binding.Update((kstate, mstate));
        }
    }
}