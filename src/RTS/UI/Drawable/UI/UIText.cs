using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

public class UIText(string name) : UIElement(name)
{
    public Func<string> Getter;

    public int TextSize = 25;
    public Color TextColor = Color.White;
    public float TextRot = 0f;
    public Font Font;
    
    public bool Centered = true;
    public int TextPadding = 5;

    protected override void CustomDraw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        if (Centered)
            Font.DrawCentered(Getter(), BoundingBox.Center.ToVector2(), TextColor, spriteBatch, TextSize, TextRot);
        else
            Font.Draw(Getter(), new Vector2(TextPadding, BoundingBox.Center.Y), TextColor, spriteBatch, TextSize, TextRot);
    }
}