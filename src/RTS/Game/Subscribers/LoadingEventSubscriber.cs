using System;
using System.Collections.Generic;

[EventBusSubscriber]
public class LoadingEventSubscriber
{
    [SubscribeEvent]
    public void Registering(RegisteringEvent e)
    {
        IReadOnlyList<Resource> assets = e.ResourceManager.ListResources(ResourceType.Assets);
        foreach (Resource resource in assets)
        {
            if (!TextureManager.TryGetTextureIds(resource.Id, out Identifier registryId, out Identifier textureId))
                continue;

            if (registryId.GetPath() == "ground" && BuiltInRegistries.GROUNDS.Get(textureId) == null)
                BuiltInRegistries.GROUNDS.Register(textureId, new Ground());
            else if (registryId.GetPath() == "material" && BuiltInRegistries.MATERIALS.Get(textureId) == null)
                BuiltInRegistries.MATERIALS.Register(textureId, new Material());
        }
        BiomeDataLoader.Load(e.ResourceManager);

        BuiltInRegistries.SCENES.Register(Identifier.fromDefaultNamespace("menu"), new MenuScene(e.content, e.TextureManager, e.ScreenManager));
        BuiltInRegistries.SCENES.Register(Identifier.fromDefaultNamespace("game"), new GameScene(e.content, e.TextureManager, e.ScreenManager));

        Scene.Set(Identifier.fromDefaultNamespace("menu"), e.ScreenManager);
    }
}