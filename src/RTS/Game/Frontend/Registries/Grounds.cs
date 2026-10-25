namespace RTS.Game.Frontend.Registries;

public static class Grounds
{
    public static readonly Ground GRASS = BuiltInRegistries.GROUNDS.Register(Identifier.fromDefaultNamespace("grass"), new Ground());
    public static readonly Ground STONE = BuiltInRegistries.GROUNDS.Register(Identifier.fromDefaultNamespace("stone"), new Ground());
    public static readonly Ground SAND = BuiltInRegistries.GROUNDS.Register(Identifier.fromDefaultNamespace("sand"), new Ground());
}