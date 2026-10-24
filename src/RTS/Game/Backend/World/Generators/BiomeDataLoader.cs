using System;
using System.IO;
using System.Text.Json;

public static class BiomeDataLoader
{
    private const string BiomePathPrefix = "worldgen/biome/";

    public static void Load(ResourceManager resources)
    {
        foreach (Resource resource in resources.ListResources(ResourceType.Data))
        {
            string path = resource.Id.Path;
            if (!path.StartsWith(BiomePathPrefix, StringComparison.Ordinal)
                || !path.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                continue;

            Identifier biomeId = Identifier.fromNamespaceAndPath(
                resource.Id.Namespace,
                System.IO.Path.ChangeExtension(path[BiomePathPrefix.Length..], null));

            using Stream stream = resource.Open();
            JsonDocument document;
            try
            {
                document = JsonDocument.Parse(stream);
            }
            catch (JsonException exception)
            {
                throw new InvalidDataException(
                    $"Invalid JSON in biome definition '{resource.Id.Namespace}:{resource.Id.Path}'.",
                    exception);
            }

            using (document)
            {
                BiomeDefinition biome = Decode(document.RootElement, resource.Id);
                BuiltInRegistries.BIOMES.Register(biomeId, biome);
            }
        }

        if (BuiltInRegistries.BIOMES.Entries.Count == 0)
            throw new InvalidDataException(
                $"No biome definitions found under data/<namespace>/{BiomePathPrefix}.");
    }

    private static BiomeDefinition Decode(JsonElement root, ResourceId resourceId)
    {
        if (root.ValueKind != JsonValueKind.Object)
            throw InvalidDefinition(resourceId, "Expected a JSON object.");

        Identifier surfaceGround = ParseIdentifier(
            RequiredString(root, "surface", resourceId),
            resourceId,
            "surface");
        if (BuiltInRegistries.GROUNDS.Get(surfaceGround) == null)
            throw InvalidDefinition(resourceId, $"Surface ground is not registered: {surfaceGround}.");

        float temperature = RequiredFloat(root, "temperature", resourceId);
        float humidity = RequiredFloat(root, "humidity", resourceId);
        if (temperature is < -1f or > 1f || humidity is < -1f or > 1f)
            throw InvalidDefinition(resourceId, "Temperature and humidity must be between -1 and 1.");

        if (!root.TryGetProperty("features", out JsonElement featuresElement)
            || featuresElement.ValueKind != JsonValueKind.Array)
            throw InvalidDefinition(resourceId, "Expected a 'features' array.");

        MaterialFeature[] features = new MaterialFeature[featuresElement.GetArrayLength()];
        int index = 0;
        foreach (JsonElement featureElement in featuresElement.EnumerateArray())
        {
            if (featureElement.ValueKind != JsonValueKind.Object)
                throw InvalidDefinition(resourceId, $"Feature at index {index} must be an object.");

            Identifier material = ParseIdentifier(
                RequiredString(featureElement, "material", resourceId),
                resourceId,
                $"features[{index}].material");
            if (BuiltInRegistries.MATERIALS.Get(material) == null)
                throw InvalidDefinition(resourceId, $"Feature material is not registered: {material}.");

            float chance = RequiredFloat(featureElement, "chance", resourceId);
            if (chance is < 0f or > 1f)
                throw InvalidDefinition(resourceId, $"Feature chance at index {index} must be between 0 and 1.");

            int seed = 0;
            if (featureElement.TryGetProperty("seed", out JsonElement seedElement)
                && (seedElement.ValueKind != JsonValueKind.Number || !seedElement.TryGetInt32(out seed)))
                throw InvalidDefinition(resourceId, $"Feature seed at index {index} must be an integer.");

            features[index++] = new MaterialFeature(material, chance, seed);
        }

        return new BiomeDefinition(surfaceGround, temperature, humidity, features);
    }

    private static string RequiredString(JsonElement element, string property, ResourceId resourceId)
    {
        if (!element.TryGetProperty(property, out JsonElement value)
            || value.ValueKind != JsonValueKind.String
            || string.IsNullOrWhiteSpace(value.GetString()))
            throw InvalidDefinition(resourceId, $"Expected a non-empty '{property}' string.");

        return value.GetString()!;
    }

    private static float RequiredFloat(JsonElement element, string property, ResourceId resourceId)
    {
        if (!element.TryGetProperty(property, out JsonElement value)
            || value.ValueKind != JsonValueKind.Number
            || !value.TryGetSingle(out float number)
            || float.IsNaN(number)
            || float.IsInfinity(number))
            throw InvalidDefinition(resourceId, $"Expected a finite '{property}' number.");

        return number;
    }

    private static Identifier ParseIdentifier(string value, ResourceId resourceId, string property)
    {
        string[] parts = value.Split(':', 2);
        if (parts.Length != 2 || string.IsNullOrWhiteSpace(parts[0]) || string.IsNullOrWhiteSpace(parts[1]))
            throw InvalidDefinition(resourceId, $"'{property}' must use a namespaced identifier.");

        return Identifier.fromNamespaceAndPath(parts[0], parts[1]);
    }

    private static InvalidDataException InvalidDefinition(ResourceId resourceId, string message) =>
        new($"Invalid biome definition '{resourceId.Namespace}:{resourceId.Path}': {message}");
}
