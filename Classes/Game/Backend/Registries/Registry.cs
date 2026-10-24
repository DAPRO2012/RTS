using System;
using System.Collections.Generic;
using System.Linq;

public class Registry<T>
{
    private readonly Dictionary<Identifier, T> _entries = new();
    private readonly Dictionary<Identifier, int> _idToNumeric = new();
    private readonly Dictionary<int, Identifier> _numericToId = new();

    private bool _isFrozen = false;
    private int _nextAvailableId = 1;

    public IReadOnlyDictionary<Identifier, T> Entries => _entries;

    internal int Register(Identifier id, T value)
    {
        if (_isFrozen) 
            throw new InvalidOperationException($"Registry {GetRegistryIdentifier()} is frozen! Cannot register {id}.");
        
        if (_entries.ContainsKey(id)) 
            throw new ArgumentException($"Duplicate registry key {id}");
            
        int numericId = _nextAvailableId++;

        _entries[id] = value;
        _idToNumeric[id] = numericId;
        _numericToId[numericId] = id;

        return numericId;
    }

    public int Size() => _entries.Count;

    public T Get(Identifier id)
    {
        if (_entries.TryGetValue(id, out var value))
            return value;
            
        return default;
    }

    public T Get(int numericId)
    {
        if (_numericToId.TryGetValue(numericId, out var id))
            return _entries[id];
        return default;
    }

    public int GetNumericId(Identifier id)
    {
        if (id != null && _idToNumeric.TryGetValue(id, out int numericId))
            return numericId;
            
        return 0;
    }
    public int GetNumericId(T item)
    {
        Identifier id = GetKey(item);
        if (id != null && _idToNumeric.TryGetValue(id, out int numericId))
            return numericId;
            
        return 0;
    }

    public Identifier GetIdentifier(int numericId)
    {
        if (_numericToId.TryGetValue(numericId, out var id))
            return id;
            
        return null;
    }

    public Identifier GetKey(T item)
    {
        if (item == null)
            throw new ArgumentNullException();

        if (_entries.ContainsValue(item))
            return _entries.First(x => x.Value.Equals(item)).Key;
        throw new Exception("Registry doesnt contain " + item.ToString());
    }

    public Identifier GetRegistryIdentifier() => BuiltInRegistries.GetRegistryKey(this);

    public void Freeze()
    {
        _isFrozen = true;
    }
}