using System;
using System.Collections.Generic;
using RTS.Game.Frontend.Registries;

[EventBusSubscriber]
public class LoadingEventSubscriber
{
    [SubscribeEvent]
    public void Registering(RegisteringEvent e)
    {
        Grounds.Register();
        Materials.Register();

        BiomeDataLoader.Load(e.ResourceManager);

        BuiltInRegistries.SCENES.Register(Identifier.fromDefaultNamespace("menu"), new MenuScene(e.content, e.TextureManager, e.ScreenManager));
        BuiltInRegistries.SCENES.Register(Identifier.fromDefaultNamespace("game"), new GameScene(e.content, e.TextureManager, e.ScreenManager));

        Scene.Set(Identifier.fromDefaultNamespace("menu"), e.ScreenManager);
    }
}