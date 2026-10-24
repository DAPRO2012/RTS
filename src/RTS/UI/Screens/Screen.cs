using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

public class Screen : IElement
{
    private readonly ScreenManager _manager;

    private readonly UIText _title = new UIText("screenTitle");
    private readonly string _titleText;

    protected readonly List<IElement> _widgets = [];
    protected void AddWidget(IElement widget) => _widgets.Add(widget);

    public Screen(string title, ContentManager content, ScreenManager manager)
    {
        _titleText = title;

        _title.Getter = () => _titleText;
        _title.Font = new Font(Identifier.fromDefaultNamespace("fonts/Silkscreen-Regular.ttf"), content);

        _manager = manager;
    }

    public virtual void DrawTitle(GameTime gt, SpriteBatch sb) =>
        _title.Draw(gt, sb);
    public virtual void DrawWidgets(GameTime gt, SpriteBatch sb) =>
        _widgets.ForEach(w => w.Draw(gt, sb));
    public virtual void DrawBackground(GameTime gt, SpriteBatch sb) { }
    public virtual void Update(GameTime gameTime) =>
        _widgets.ForEach(w => w.Update(gameTime));

    public virtual void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        DrawBackground(gameTime, spriteBatch);
        DrawWidgets(gameTime, spriteBatch);
        DrawTitle(gameTime, spriteBatch);
    }

    public void Open() => _manager.Push(this);
}