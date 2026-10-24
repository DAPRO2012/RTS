using Microsoft.Xna.Framework;

public sealed class MaterialGenerator
{
    public DataResult<Material[][]> Generate(Point gridPos, BiomeDefinition[][] biomes, int worldSeed)
    {
        Material[][] materials = new Material[Chunk.ChunkSize][];
        for (int i = 0; i < Chunk.ChunkSize; i++)
            materials[i] = new Material[Chunk.ChunkSize];

        for (int x = 0; x < Chunk.ChunkSize; x++)
        for (int y = 0; y < Chunk.ChunkSize; y++)
        {
            BiomeDefinition biome = biomes[x][y];
            if (biome == null)
                return new DataResult<Material[][]>.Error<Material[][]>(
                    "Material generation received an invalid biome.",
                    Optional.Empty<Material[][]>());

            int worldX = gridPos.X * Chunk.ChunkSize + x;
            int worldY = gridPos.Y * Chunk.ChunkSize + y;
            foreach (MaterialFeature feature in biome.Features)
            {
                if (GetPlacementValue(worldX, worldY, worldSeed ^ feature.Seed) >= feature.Chance)
                    continue;

                Material material = BuiltInRegistries.MATERIALS.Get(feature.Material);
                if (material == null)
                    return new DataResult<Material[][]>.Error<Material[][]>(
                        $"Biome feature references an unregistered material: {feature.Material}.",
                        Optional.Empty<Material[][]>());

                materials[x][y] = material;
                break;
            }
        }

        return new DataResult<Material[][]>.Success<Material[][]>(materials);
    }

    private static float GetPlacementValue(int x, int y, int seed)
    {
        uint hash = Mix((uint)x) ^ RotateLeft(Mix((uint)y), 16) ^ Mix((uint)seed);
        hash = Mix(hash);
        return (hash >> 8) * (1f / 16777216f);
    }

    private static uint Mix(uint value)
    {
        value ^= value >> 16;
        value *= 0x7feb352d;
        value ^= value >> 15;
        value *= 0x846ca68b;
        return value ^ (value >> 16);
    }

    private static uint RotateLeft(uint value, int amount) =>
        (value << amount) | (value >> (32 - amount));
}