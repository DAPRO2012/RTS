using System.Linq;
using Microsoft.Xna.Framework;

public sealed class BiomeGenerator
{
    private readonly FastNoiseLite _temperatureNoise = new();
    private readonly FastNoiseLite _humidityNoise = new();

    public BiomeGenerator(int seed = 1)
    {
        ConfigureNoise(_temperatureNoise, seed);
        ConfigureNoise(_humidityNoise, seed + 1);
    }

    public DataResult<BiomeDefinition[][]> Generate(Point gridPos)
    {
        BiomeDefinition[] biomes = BuiltInRegistries.BIOMES.Entries
            .OrderBy(entry => entry.Key.ToString(), System.StringComparer.Ordinal)
            .Select(entry => entry.Value)
            .ToArray();
        if (biomes.Length == 0)
            return new DataResult<BiomeDefinition[][]>.Error<BiomeDefinition[][]>(
                "No biomes are registered.",
                Optional.Empty<BiomeDefinition[][]>());

        BiomeDefinition[][] result = new BiomeDefinition[IChunk.ChunkSize][];
        for (int x = 0; x < IChunk.ChunkSize; x++)
        {
            result[x] = new BiomeDefinition[IChunk.ChunkSize];
            for (int y = 0; y < IChunk.ChunkSize; y++)
            {
                int worldX = gridPos.X * IChunk.ChunkSize + x;
                int worldY = gridPos.Y * IChunk.ChunkSize + y;
                float temperature = _temperatureNoise.GetNoise(worldX, worldY);
                float humidity = _humidityNoise.GetNoise(worldX, worldY);
                result[x][y] = FindClosestBiome(biomes, temperature, humidity);
            }
        }

        return new DataResult<BiomeDefinition[][]>.Success<BiomeDefinition[][]>(result);
    }

    private static void ConfigureNoise(FastNoiseLite noise, int seed)
    {
        noise.SetSeed(seed);
        noise.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
        noise.SetFractalType(FastNoiseLite.FractalType.FBm);
        noise.SetFrequency(0.01f);
    }

    private static BiomeDefinition FindClosestBiome(
        BiomeDefinition[] biomes,
        float temperature,
        float humidity)
    {
        BiomeDefinition closest = biomes[0];
        float closestDistance = float.MaxValue;
        foreach (BiomeDefinition biome in biomes)
        {
            float temperatureDelta = temperature - biome.Temperature;
            float humidityDelta = humidity - biome.Humidity;
            float distance = temperatureDelta * temperatureDelta + humidityDelta * humidityDelta;
            if (distance < closestDistance)
            {
                closest = biome;
                closestDistance = distance;
            }
        }

        return closest;
    }
}
