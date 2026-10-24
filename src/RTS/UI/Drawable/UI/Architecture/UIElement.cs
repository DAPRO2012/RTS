#nullable enable

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using RTS;

public class UIElement(string name) : IElement
{
    //Static Vars

    //Instance Vars
    public string Name { get; } = name;
    public UIElement? Parent = null;
    protected readonly List<UIElement> _children = [];

    public void AddChild(UIElement child)
    {
        _children.Add(child);
        child.Parent = this;
    }

    public Point Position = Point.Zero;
    public Point Size = Point.Zero;
    public UIAnchor? Anchor = null;

    public Rectangle BoundingBox => new(GetScreenPosition(), Size);

    //Graphics
    public Identifier? BackgroundTextureId = null;
    public TextureManager? TextureManager = null;
    public Color BackgroundColor = Color.White;

    public bool IsActive = true;
    public bool IsVisible = true;

    public bool IsHovering { get; private set; } = false;

    //Instance Methods
    public Point GetScreenUnanchoredPosition() => Position + (Parent?.GetScreenPosition() ?? Point.Zero);
    public Point GetScreenPosition() => Anchor?.GetScreenPosition(this) ?? GetScreenUnanchoredPosition();

    //Overridable Methods
    public Action OnClicked = () => {};
    public Action OnUpdate = () => {};

    public virtual void Update(GameTime gameTime)
    {
        MouseState ms = Mouse.GetState();
        IsHovering = BoundingBox.Contains(ms.Position) && IsActive;

        if (IsHovering && InputManager.Instance.LeftClick.ConsumeClick())
        {
            OnClicked();
        }

        OnUpdate();
        _children.ForEach((c) => c.Update(gameTime));
    }

    protected virtual void CustomDraw(GameTime gameTime, SpriteBatch spriteBatch) {}
    public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        if (BackgroundTextureId is not null
            && TextureManager?.GetRegion(BackgroundTextureId) is TextureRegion backgroundTexture)
        {
            backgroundTexture.Draw(spriteBatch, BoundingBox, BackgroundColor);
        }

        CustomDraw(gameTime, spriteBatch);

        _children.ForEach((c) => c.Draw(gameTime, spriteBatch));
    }
}