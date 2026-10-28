using Microsoft.Xna.Framework;

public interface IChunkGenerator
{
    public DataResult<ServerChunk> Generate(Point gridPos);
}