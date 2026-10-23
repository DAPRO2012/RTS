using System;
using Microsoft.Xna.Framework;

public class GroundGenerator(int seed = 1) : LayerGenerator<Ground>(seed)
{
    private const int NoiseSampleResolution = 512;
    private const int NoiseSampleSpacing = 4;
    private float[] _groundThresholds = [];
    private int _thresholdRegistrySize;

    public override DataResult<Ground[][]> Generate(Point gridPos)
    {
        Ground[][] grounds = new Ground[Chunk.ChunkSize][];
        for (int i = 0; i < Chunk.ChunkSize; i++)
            grounds[i] = new Ground[Chunk.ChunkSize];
        
        int registrySize = BuiltInRegistries.GROUNDS.Size();
        if (registrySize == 0)
            return new DataResult<Ground[][]>.Error<Ground[][]>(
                "No ground types are registered.",
                Optional.Empty<Ground[][]>());

        if (_thresholdRegistrySize != registrySize)
            CalculateGroundThresholds(registrySize);

        for (int x = 0; x < Chunk.ChunkSize; x++)
        for (int y = 0; y < Chunk.ChunkSize; y++)
        {
            float noise = _noise.GetNoise(
                (gridPos.X * Chunk.ChunkSize) + x,
                (gridPos.Y * Chunk.ChunkSize) + y);
            int groundIndex = GetGroundIndex(noise);
            grounds[x][y] = BuiltInRegistries.GROUNDS.Get(groundIndex + 1);
        }

        return new DataResult<Ground[][]>.Success<Ground[][]>(grounds);
    }

    private void CalculateGroundThresholds(int registrySize)
    {
        float[] noiseSamples = new float[NoiseSampleResolution * NoiseSampleResolution];
        int sampleIndex = 0;
        for (int x = 0; x < NoiseSampleResolution; x++)
        for (int y = 0; y < NoiseSampleResolution; y++)
            noiseSamples[sampleIndex++] = _noise.GetNoise(
                x * NoiseSampleSpacing,
                y * NoiseSampleSpacing);

        Array.Sort(noiseSamples);
        _groundThresholds = new float[registrySize - 1];
        for (int i = 1; i < registrySize; i++)
            _groundThresholds[i - 1] = noiseSamples[i * noiseSamples.Length / registrySize];

        _thresholdRegistrySize = registrySize;
    }

    private int GetGroundIndex(float noise)
    {
        int low = 0;
        int high = _groundThresholds.Length;
        while (low < high)
        {
            int middle = low + (high - low) / 2;
            if (noise >= _groundThresholds[middle])
                low = middle + 1;
            else
                high = middle;
        }

        return low;
    }
}