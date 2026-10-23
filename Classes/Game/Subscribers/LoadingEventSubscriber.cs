using System;

[EventBusSubscriber]
public class LoadingEventSubscriber
{
    [SubscribeEvent]
    public void Registering(RegisteringEvent e)
    {
        BuiltInRegistries.GROUNDS.Register(Identifier.fromDefaultNamespace("grass"), new Ground());
        BuiltInRegistries.GROUNDS.Register(Identifier.fromDefaultNamespace("stone"), new Ground());
        BuiltInRegistries.GROUNDS.Register(Identifier.fromDefaultNamespace("sand"), new Ground());

        BuiltInRegistries.SCENES.Register(Identifier.fromDefaultNamespace("menu"), new MenuScene(e.content, e.TextureManager));
        BuiltInRegistries.SCENES.Register(Identifier.fromDefaultNamespace("game"), new GameScene(e.content, e.TextureManager));

        Scene.Set(Identifier.fromDefaultNamespace("menu"));
    }
}