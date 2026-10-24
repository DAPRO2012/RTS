using Microsoft.Xna.Framework.Content;

public class LoadingEvent(
    ContentManager content,
    ResourceManager resourceManager,
    TextureManager textureManager,
    ScreenManager screenManager) : Event
{
    public ContentManager content = content;
    public ResourceManager ResourceManager { get; } = resourceManager;
    public TextureManager TextureManager { get; } = textureManager;
    public ScreenManager ScreenManager { get; } = screenManager;
}

public class RegisteringEvent(
    ContentManager content,
    ResourceManager resourceManager,
    TextureManager textureManager,
    ScreenManager screenManager)
    : LoadingEvent(content, resourceManager, textureManager, screenManager);

public class TextureLoadingEvent(
    ContentManager content,
    ResourceManager resourceManager,
    TextureManager textureManager,
    ScreenManager screenManager)
    : LoadingEvent(content, resourceManager, textureManager, screenManager);