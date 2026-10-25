using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public sealed class Pack : IResourcePack
{
    private readonly Dictionary<ResourceType, Dictionary<Identifier, string>> _resources = [];

    public string Name { get; }

    public Pack(string name, string rootPath)
    {
        Name = name;
        string fullRootPath = System.IO.Path.GetFullPath(rootPath);

        if (!Directory.Exists(fullRootPath))
            throw new DirectoryNotFoundException($"Resource pack directory not found: {fullRootPath}");

        IndexResources(ResourceType.Assets, System.IO.Path.Combine(fullRootPath, "assets"));
        IndexResources(ResourceType.Data, System.IO.Path.Combine(fullRootPath, "data"));
    }

    public IEnumerable<string> GetNamespaces(ResourceType type) =>
        _resources[type].Keys
            .Select(id => id.GetNamespace())
            .Distinct(StringComparer.Ordinal)
            .OrderBy(name => name, StringComparer.Ordinal);

    public bool Contains(ResourceType type, Identifier id) =>
        _resources[type].ContainsKey(id);

    public Stream Open(ResourceType type, Identifier id)
    {
        if (!_resources[type].TryGetValue(id, out string path))
            throw new FileNotFoundException(
                $"Resource not found in pack '{Name}': {type}/{id.GetNamespace()}:{id.GetPath()}");

        return File.OpenRead(path);
    }

    public IEnumerable<Identifier> ListResources(
        ResourceType type,
        string namespaceName,
        string pathPrefix)
    {
        return _resources[type].Keys
            .Where(id => id.GetNamespace() == namespaceName
                && (pathPrefix.Length == 0
                    || id.GetPath().Equals(pathPrefix, StringComparison.Ordinal)
                    || id.GetPath().StartsWith(pathPrefix + "/", StringComparison.Ordinal)))
            .OrderBy(id => id.GetPath(), StringComparer.Ordinal);
    }

    private void IndexResources(ResourceType type, string sectionPath)
    {
        Dictionary<Identifier, string> indexedResources = [];
        if (Directory.Exists(sectionPath))
        {
            foreach (string namespacePath in Directory.EnumerateDirectories(sectionPath)
                         .OrderBy(path => path, StringComparer.Ordinal))
            {
                string namespaceName = System.IO.Path.GetFileName(namespacePath);
                foreach (string filePath in Directory.EnumerateFiles(
                             namespacePath, "*", SearchOption.AllDirectories)
                         .OrderBy(path => path, StringComparer.Ordinal))
                {
                    string resourcePath = System.IO.Path.GetRelativePath(namespacePath, filePath)
                        .Replace('\\', '/');
                    Identifier id = Identifier.fromNamespaceAndPath(namespaceName, resourcePath);

                    if (!indexedResources.TryAdd(id, filePath))
                        throw new InvalidDataException(
                            $"Duplicate resource in pack '{Name}': {id.GetNamespace()}:{id.GetPath()}");
                }
            }
        }

        _resources.Add(type, indexedResources);
    }

    public void Dispose()
    {
    }
}
