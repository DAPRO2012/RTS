namespace RTS.Game.Frontend.Registries;

public static class Materials
{
    static Materials() { }

    public static readonly Material LOG = BuiltInRegistries.MATERIALS.Register(Identifier.fromDefaultNamespace("log"), new Material());

    public static void Register() { }
}