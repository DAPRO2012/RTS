public class ChunkEvent(Level level, Chunk chunk) : LevelEvent(level)
{
    public readonly Chunk Chunk = chunk;
    public Chunk GetChunk() => Chunk;

    public new class Load(Level level, Chunk chunk) : ChunkEvent(level, chunk);
    public new class Unload(Level level, Chunk chunk) : ChunkEvent(level, chunk);
}