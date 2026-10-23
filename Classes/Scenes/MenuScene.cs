using System;
using System.Drawing.Imaging;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using RTS;

public class MenuScene(ContentManager content) : Scene(content)
{
    private Texture gradient;
    public Font pixelFont;
    public UIText ContinueButton = new("playButton")
    {
        Anchor = new SideAnchor(SideAnchor.SideHorizontal.Middle, SideAnchor.SideVerical.Bottom),
        Size = new(100, 75),
        Position = new(0, -25),
        BackgroundTexture = new Texture(Identifier.fromDefaultNamespace("textures/blank.png"), content),
        BackgroundColor = Color.Black,
        OnClicked = () => Set(Identifier.fromDefaultNamespace("game")),

        Getter = () => "Continue"
    };


    public override void Initialize(ContentManager content)
    {
        pixelFont = new(Identifier.fromDefaultNamespace("fonts/Silkscreen-Regular.ttf"), content);
        gradient = new Texture(Identifier.fromDefaultNamespace("textures/scenes/menu/menu_grad.png"), content);

        ContinueButton.Font = pixelFont;
        drawables.Add(ContinueButton);
    }

    public override void Update(GameTime gt)
    {
        UpdateDrawables(gt);
    }

    public override void Draw(GameTime gt, SpriteBatch sb)
    {
        sb.GraphicsDevice.Clear(Color.White);

        sb.Begin(samplerState:SamplerState.PointClamp);
            int gradHeight = (int)Math.Floor(Main.Dimensions.Height / 2f);
            gradient.Draw(sb, new Rectangle(0, Main.Dimensions.Height - gradHeight, Main.Dimensions.Width, gradHeight), Color.White);
            DrawDrawables(gt, sb);
        sb.End();
    }
}