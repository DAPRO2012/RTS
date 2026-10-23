using Microsoft.Xna.Framework;

public abstract class LayerGenerator<T> : ILayerGenerator<T>
{
    protected readonly FastNoiseLite _noise = new();
    
    public LayerGenerator(int seed = 1)
    {
        _noise.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
        _noise.SetFractalType(FastNoiseLite.FractalType.FBm);
        _noise.SetFrequency(0.01f);
        _noise.SetSeed(seed);
    }

    public abstract DataResult<T[][]> Generate(Point gridPos);
}