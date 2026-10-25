using System;
using System.Collections.Generic;
using System.IO;

public enum ResourceType
{
    Assets,
    Data
}

public interface IResourcePack : IDisposable
{
    string Name { get; }

    IEnumerable<string> GetNamespaces(ResourceType type);

    bool Contains(ResourceType type, Identifier id);

    Stream Open(ResourceType type, Identifier id);

    IEnumerable<Identifier> ListResources(
        ResourceType type,
        string namespaceName,
        string pathPrefix);
}

public sealed record Resource(IResourcePack Source, ResourceType Type, Identifier Id)
{
    public Stream Open() => Source.Open(Type, Id);
}