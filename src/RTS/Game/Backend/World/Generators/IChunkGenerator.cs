using Microsoft.Xna.Framework;

public interface IChunkGenerator
{
    public DataResult<Chunk> Generate(Point gridPos);
}