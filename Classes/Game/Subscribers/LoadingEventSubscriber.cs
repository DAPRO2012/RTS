using System;

[EventBusSubscriber]
public class LoadingEventSubscriber
{
    [SubscribeEvent]
    public void Loading(RegisteringEvent e)
    {
        Console.WriteLine("Loading");

        BuiltInRegistries.GROUNDS.Register(Identifier.fromDefaultNamespace("grass"), new Ground());

        BuiltInRegistries.SCENES.Register(Identifier.fromDefaultNamespace("menu"), new MenuScene(e.content));
        BuiltInRegistries.SCENES.Register(Identifier.fromDefaultNamespace("game"), new GameScene(e.content));

        Scene.Set(Identifier.fromDefaultNamespace("menu"));
    }
}