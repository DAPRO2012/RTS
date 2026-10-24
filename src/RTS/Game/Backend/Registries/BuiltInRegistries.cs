using System.Collections.Generic;
using System.Linq;

public static class BuiltInRegistries
{
    private static readonly Dictionary<Identifier, object> _registries = [];

    public static readonly Registry<Scene> SCENES = RegisterSimple<Scene>("scene");

    public static readonly Registry<Structure> STRUCTURES = RegisterSimple<Structure>("structure");
    public static readonly Registry<Material> MATERIALS = RegisterSimple<Material>("material");
    public static readonly Registry<Ground> GROUNDS = RegisterSimple<Ground>("ground");
    public static readonly Registry<BiomeDefinition> BIOMES = RegisterSimple<BiomeDefinition>("biome");

    internal static Registry<T> RegisterSimple<T>(string name) => InternalRegister<T>(Identifier.fromDefaultNamespace(name));
    private static Registry<T> InternalRegister<T>(Identifier id) {
        Registry<T> registry = new Registry<T>();
        _registries.Add(id, registry);
        return registry;
    }

    public static Registry<T> GetRegistry<T>(Identifier id)
    {
        if (_registries.TryGetValue(id, out object registry))
            return (Registry<T>)registry;
        throw new KeyNotFoundException(id.ToString() + " isn't a valid registry");
    }

    internal static Identifier GetRegistryKey<T>(Registry<T> registry)
    {
        if (_registries.ContainsValue(registry))
            return _registries.First(x => x.Value == registry).Key;
        throw new KeyNotFoundException();
    }
}