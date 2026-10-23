using Microsoft.Xna.Framework.Content;

public class LoadingEvent(
    ContentManager content,
    ResourceManager resourceManager,
    TextureManager textureManager) : Event
{
    public ContentManager content = content;
    public ResourceManager ResourceManager { get; } = resourceManager;
    public TextureManager TextureManager { get; } = textureManager;
}

public class RegisteringEvent(
    ContentManager content,
    ResourceManager resourceManager,
    TextureManager textureManager)
    : LoadingEvent(content, resourceManager, textureManager);

public class TextureLoadingEvent(
    ContentManager content,
    ResourceManager resourceManager,
    TextureManager textureManager)
    : LoadingEvent(content, resourceManager, textureManager);