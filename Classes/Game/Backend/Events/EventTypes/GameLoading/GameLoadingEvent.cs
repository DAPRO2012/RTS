using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using SharpDX.Direct2D1;

public class LoadingEvent(ContentManager content) : Event
{
    public ContentManager content = content;
}

public class RegisteringEvent(ContentManager content) : LoadingEvent(content);

public class TextureLoadingEvent(ContentManager content, TextureManager manager) : LoadingEvent(content)
{
    public TextureManager TextureManager = manager;
}