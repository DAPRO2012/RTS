using System;
using System.Collections.Generic;
using System.IO;

public enum ResourceType
{
    Assets,
    Data
}

public readonly record struct ResourceId(string Namespace, string Path);

public interface IResourcePack : IDisposable
{
    string Name { get; }

    IEnumerable<string> GetNamespaces(ResourceType type);

    bool Contains(ResourceType type, ResourceId id);

    Stream Open(ResourceType type, ResourceId id);

    IEnumerable<ResourceId> ListResources(
        ResourceType type,
        string namespaceName,
        string pathPrefix);
}

public sealed record Resource(IResourcePack Source, ResourceType Type, ResourceId Id)
{
    public Stream Open() => Source.Open(Type, Id);
}