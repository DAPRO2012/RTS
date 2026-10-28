using Microsoft.Xna.Framework;

public class ChunkEvent(ServerLevel level, Optional<ServerChunk> chunk, Point position) : LevelEvent<ServerLevel>(level)
{
    public Optional<ServerChunk> Chunk { get; private set; } = chunk;
    public Optional<ServerChunk> GetChunk() => Chunk;
    public readonly Point Position = position;

    public new class Load(ServerLevel level, Optional<ServerChunk> chunk, Point position) : ChunkEvent(level, chunk, position);
    public new class Unload(ServerLevel level, Optional<ServerChunk> chunk, Point position) : ChunkEvent(level, chunk, position);
}