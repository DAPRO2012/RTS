using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public sealed class Pack : IResourcePack
{
    private readonly Dictionary<ResourceType, Dictionary<ResourceId, string>> _resources = [];

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
            .Select(id => id.Namespace)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(name => name, StringComparer.Ordinal);

    public bool Contains(ResourceType type, ResourceId id) =>
        _resources[type].ContainsKey(id);

    public Stream Open(ResourceType type, ResourceId id)
    {
        if (!_resources[type].TryGetValue(id, out string path))
            throw new FileNotFoundException($"Resource not found in pack '{Name}': {type}/{id.Namespace}:{id.Path}");

        return File.OpenRead(path);
    }

    public IEnumerable<ResourceId> ListResources(
        ResourceType type,
        string namespaceName,
        string pathPrefix)
    {
        return _resources[type].Keys
            .Where(id => id.Namespace == namespaceName
                && (pathPrefix.Length == 0
                    || id.Path.Equals(pathPrefix, StringComparison.Ordinal)
                    || id.Path.StartsWith(pathPrefix + "/", StringComparison.Ordinal)))
            .OrderBy(id => id.Path, StringComparer.Ordinal);
    }

    private void IndexResources(ResourceType type, string sectionPath)
    {
        Dictionary<ResourceId, string> indexedResources = [];
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
                    ResourceId id = new(namespaceName, resourcePath);

                    if (!indexedResources.TryAdd(id, filePath))
                        throw new InvalidDataException($"Duplicate resource in pack '{Name}': {id.Namespace}:{id.Path}");
                }
            }
        }

        _resources.Add(type, indexedResources);
    }

    public void Dispose()
    {
    }
}
