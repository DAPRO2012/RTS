using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Xna.Framework;
using RTS;

public class ServerLevel : ILevel
{
    private readonly Logger LOGGER = LogUtils.GetLogger();

    private int nextEntityId = 1;

    protected readonly static SavedStorage WorldStorage = new(new(AppContext.BaseDirectory, "data", "saves"));
    private readonly string _worldName;
    private readonly ChunkGenerator _generator;

    private readonly Dictionary<int, Entity> _entities = [];
    public void AddFreshEntity(Entity entity, Vector2? position = null)
    {
        int id = entity.GetId();
        if (id >= nextEntityId || !_entities.TryAdd(id, entity))
            LOGGER.Error("Tried adding entity with already registered Id.");
        else if (position != null)
            entity.SetPosRaw(position.Value.X, position.Value.Y);
    }

    protected Dictionary<Point, ServerChunk> loadedChunks = [];
    public ReadOnlyDictionary<Point, IChunk> GetChunks() =>
        new(
            loadedChunks.ToDictionary(entry => entry.Key, entry => (IChunk)entry.Value)
        );
    public IChunk GetChunk(Point position) =>
        loadedChunks[position];

    public bool IsLoaded(Point point) => loadedChunks.ContainsKey(point);

    private Path GetChunkFile(Point pos) =>
        new(_worldName, "Chunks", FormattableString.Invariant($"Chunk_{pos.X}_{pos.Y}.dat"));

    private readonly Pipeline<ChunkEvent.Load> _chunkLoadPipeline = new Pipeline<ChunkEvent.Load>()
        .WithStep(e => Main.EVENT_BUS.Publish(e))
        .WithStep(e => { e.Level.loadedChunks[e.Position] = e.Chunk.Value; });
    private readonly Pipeline<ChunkEvent.Unload> _chunkUnloadPipeline = new Pipeline<ChunkEvent.Unload>()
        .WithStep(e => Main.EVENT_BUS.Publish(e))
        .WithStep(e => { if (e.Chunk.Value.IsChanged) WorldStorage.WriteCodecToDisk(e.Level.GetChunkFile(e.Position), e.Chunk.Value, ServerChunk.CODEC); })
        .WithStep(e => e.Level.loadedChunks.Remove(e.Position));

    public DataResult<ServerChunk> LoadChunk(Point pos)
    {
        Path ChunkFile = GetChunkFile(pos);
        DataResult<ServerChunk> ChunkResult;
        if (loadedChunks.ContainsKey(pos))
            ChunkResult = new DataResult<ServerChunk>.Success<ServerChunk>((ServerChunk)GetChunk(pos));
        else if (ChunkFile.IsFile())
            ChunkResult = WorldStorage.ReadCodecFromDisk(ChunkFile, ServerChunk.CODEC);
        else
            ChunkResult = _generator.Generate(pos);

        if (ChunkResult.IsSuccess())
        {
            ChunkEvent.Load loadEvent = new ChunkEvent.Load(this, ChunkResult.ResultOrPartial(), pos);
            _chunkLoadPipeline.Run(loadEvent);
            if (loadEvent.IsEventEnding())
                return new DataResult<ServerChunk>.Error<ServerChunk>("Pipeline ended.", ChunkResult.ResultOrPartial());
        }
        return ChunkResult;
    }
    public ServerChunk UnloadChunk(Point pos)
    {
        if (loadedChunks.TryGetValue(pos, out ServerChunk Chunk))
        {
            Path ChunkFile = GetChunkFile(pos);

            ChunkEvent.Unload unloadEvent = new ChunkEvent.Unload(this, Optional.Of(Chunk), pos);
            _chunkUnloadPipeline.Run(unloadEvent);
        }
        return Chunk;
    }

    public ServerLevel(string worldName, int seed = 1)
    {
        _worldName = worldName;
        _generator = new(seed);
    }

    public int GetTile(Point position, WorldLayer layer) =>
        GetTile(position.X, position.Y, layer);
    public int GetTile(int x, int y, WorldLayer layer)
    {
        Point ChunkPos = new Point(
            (int)Math.Floor(x / (float)IChunk.ChunkSize),
            (int)Math.Floor(y / (float)IChunk.ChunkSize)
        );
        Point inChunkPos = new Point(
            x - ChunkPos.X * IChunk.ChunkSize,
            y - ChunkPos.Y * IChunk.ChunkSize
        );

        ServerChunk ServerChunk = LoadChunk(ChunkPos).Result().GetValue();
        return ServerChunk.GetTile(inChunkPos.X, inChunkPos.Y, layer);
    }

    public ImmutableList<AABB> GetBlockCollisions(Entity source, AABB boundingBox)
    {
        throw new NotImplementedException();
    }

    public int GetNextEntityId() =>
        nextEntityId;
    public int ConsumeNextEntityId() =>
        nextEntityId++;
}