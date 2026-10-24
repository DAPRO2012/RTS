using System.Collections.Generic;
using System.Linq;

public sealed class ResourceManager : System.IDisposable
{
    private readonly IReadOnlyList<IResourcePack> _packs;

    public ResourceManager(IReadOnlyList<IResourcePack> packs)
    {
        _packs = packs;
    }

    public Resource GetResource(ResourceType type, ResourceId id)
    {
        for (int i = _packs.Count - 1; i >= 0; i--)
        {
            IResourcePack pack = _packs[i];

            if (pack.Contains(type, id))
                return new Resource(pack, type, id);
        }

        return null;
    }

    public IReadOnlyList<Resource> GetResourceStack(ResourceType type, ResourceId id)
    {
        List<Resource> result = [];

        foreach (IResourcePack pack in _packs)
        {
            if (pack.Contains(type, id))
                result.Add(new Resource(pack, type, id));
        }

        return result;
    }

    public IReadOnlyList<Resource> ListResources(ResourceType type)
    {
        Dictionary<ResourceId, Resource> resources = [];

        foreach (IResourcePack pack in _packs)
        {
            foreach (string namespaceName in pack.GetNamespaces(type))
            {
                foreach (ResourceId id in pack.ListResources(type, namespaceName, string.Empty))
                    resources[id] = new Resource(pack, type, id);
            }
        }

        return resources.Values
            .OrderBy(resource => resource.Id.Namespace, System.StringComparer.Ordinal)
            .ThenBy(resource => resource.Id.Path, System.StringComparer.Ordinal)
            .ToArray();
    }

    public void Dispose()
    {
        foreach (IResourcePack pack in _packs)
            pack.Dispose();
    }
}