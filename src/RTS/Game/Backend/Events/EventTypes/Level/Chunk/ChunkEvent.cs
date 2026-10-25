using Microsoft.Xna.Framework;

public class ChunkEvent(Level level, Optional<Chunk> chunk, Point position) : LevelEvent(level)
{
    public Optional<Chunk> Chunk { get; private set; } = chunk;
    public Optional<Chunk> GetChunk() => Chunk;
    public readonly Point Position = position;

    public new class Load(Level level, Optional<Chunk> chunk, Point position) : ChunkEvent(level, chunk, position);
    public new class Unload(Level level, Optional<Chunk> chunk, Point position) : ChunkEvent(level, chunk, position);
}