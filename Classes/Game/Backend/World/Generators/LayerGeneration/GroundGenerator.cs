using System;
using Microsoft.Xna.Framework;

public class GroundGenerator(int seed = 1) : LayerGenerator<Ground>(seed)
{
    public override DataResult<Ground[][]> Generate(Point gridPos)
    {
        Ground[][] grounds = new Ground[Chunk.ChunkSize][];
        for (int i = 0; i < Chunk.ChunkSize; i++)
            grounds[i] = new Ground[Chunk.ChunkSize];
        
        int registrySize = BuiltInRegistries.GROUNDS.Size();
        for (int x = 0; x < Chunk.ChunkSize; x++)
        for (int y = 0; y < Chunk.ChunkSize; y++)
        {
            float noiseVal = _noise.GetNoise(x, y) + 1f / 2f;
            int groundIndex = (int)Math.Round(noiseVal * registrySize, 1);

            Ground ground = BuiltInRegistries.GROUNDS.Get(groundIndex + 1);
            grounds[x][y] = ground;
        }

        return new DataResult<Ground[][]>.Success<Ground[][]>(grounds);
    }
}