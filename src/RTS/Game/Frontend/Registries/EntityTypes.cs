namespace RTS.Game.Frontend.Registries;

public static class EntityTypes
{
    static EntityTypes() { }

    public static readonly EntityType<Player> PLAYER = BuiltInRegistries.ENTITY_TYPES.Register(
        Identifier.fromDefaultNamespace("player"),
        EntityType<Player>.Builder.Of(
            (lvl) => new Player(PLAYER, lvl),
            EntityCategory.MISC
        )
        .Build()
    );

    public static void Register() { }
}