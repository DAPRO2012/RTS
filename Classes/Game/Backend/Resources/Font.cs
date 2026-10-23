using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using SpriteFontPlus;

public class Font : DrawableResource
{
    protected SpriteFont _spriteFont;

    public Font(Identifier id, ContentManager content)
    {
        string rootDir = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, content.RootDirectory, "assets");
        string path = Data.GetPath(rootDir, id);

        byte[] ttfData = File.ReadAllBytes(path);

        TtfFontBakerResult bakedFont = TtfFontBaker.Bake(
            ttfData,
            100,
            1024, 1024,
            [ 
                CharacterRange.BasicLatin,
                CharacterRange.Latin1Supplement,
                CharacterRange.LatinExtendedA 
            ]
        );

        _spriteFont = bakedFont.CreateSpriteFont(Device);
    }

    public void Draw(string text, Vector2 position, Color color, SpriteBatch spriteBatch, int size = 25, float rot = 0f)
    {
        float scale = size / 100f;
        Vector2 textSize = _spriteFont.MeasureString(text) * scale;
        textSize.X = 0;

        spriteBatch.DrawString(_spriteFont, text, position - (textSize / 2f), color, rot, new(0, 0), scale, SpriteEffects.None, 0f);
    }
    public void DrawCentered(string text, Vector2 position, Color color, SpriteBatch spriteBatch, int size = 25, float rot = 0f)
    {   
        float scale = size / 100f;
        Vector2 textSize = _spriteFont.MeasureString(text) * scale;

        spriteBatch.DrawString(_spriteFont, text, position - (textSize / 2f), color, rot, new(0, 0), scale, SpriteEffects.None, 0f);
    }
}