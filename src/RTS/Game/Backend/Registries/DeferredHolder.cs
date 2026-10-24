// Holds the reference to your registered item
using System;
using System.Collections.Generic;
using System.Reflection;

public class DeferredHolder<T>
{
    public Identifier Id { get; }
    private T _value;
    private readonly Func<T> _factory;
    private readonly Registry<T> _registry;

    public DeferredHolder(Identifier id, Func<T> factory, Registry<T> registry)
    {
        Id = id;
        _factory = factory;
        _registry = registry;
    }

    public T Value => _value ??= _registry.Get(Id); 

    internal void CreateAndRegister()
    {
        _value = _factory();
        _registry.Register(Id, _value);
    }
}

public class DeferredRegister<T>
{
    private readonly Registry<T> _registry;
    private readonly string _namespace;
    private readonly List<DeferredHolder<T>> _unregisteredEntries = new();

    //Helper Classes
    
    
    private DeferredRegister(Registry<T> registry, string @namespace)
    {
        _registry = registry;
        _namespace = @namespace;
    }

    public static DeferredRegister<T> Create(Registry<T> registry, string @namespace)
    {
        return new DeferredRegister<T>(registry, @namespace);
    }

    public DeferredHolder<T> Register(string path, Func<T> factory)
    {
        var id = Identifier.fromNamespaceAndPath(_namespace, path);
        var holder = new DeferredHolder<T>(id, factory, _registry);
        _unregisteredEntries.Add(holder);
        
        return holder;
    }

    public void Apply()
    {
        foreach (var entry in _unregisteredEntries)
        {
            entry.CreateAndRegister();
        }
        _unregisteredEntries.Clear();
    }
}