using Microsoft.Xna.Framework;

public interface ILayerGenerator<T>
{
    public DataResult<T[][]> Generate(Point gridPos, int[][][] partialChunk);
}