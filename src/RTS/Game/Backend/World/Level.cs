using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RTS;

public class Level
    {
        protected readonly static SavedStorage WorldStorage = new(new(AppContext.BaseDirectory, "data", "saves"));
        private readonly string _worldName;
        private readonly ChunkGenerator _generator;

        private readonly Pipeline<ChunkEvent.Load> _chunkLoadPipeline = new Pipeline<ChunkEvent.Load>()
            .WithStep(e => Main.EVENT_BUS.Publish(e))
            .WithStep(e => { e.Level.loadedChunks[e.Position] = e.Chunk.Value; });
        private readonly Pipeline<ChunkEvent.Unload> _chunkUnloadPipeline = new Pipeline<ChunkEvent.Unload>()
            .WithStep(e => Main.EVENT_BUS.Publish(e))
            .WithStep(e => { if (e.Chunk.Value.IsChanged) WorldStorage.WriteCodecToDisk(e.Level.GetChunkFile(e.Position), e.Chunk.Value, Chunk.CODEC); })
            .WithStep(e => e.Level.loadedChunks.Remove(e.Position))
            .WithStep(e => e.Chunk.Value.Dispose());

        private int nextEntityId = 1;

        protected Dictionary<Point, Chunk> loadedChunks = [];
        public ReadOnlyDictionary<Point, Chunk> LoadedChunks => loadedChunks.AsReadOnly();

        public Level(string worldName, int seed = 1)
        {
            _worldName = worldName;
            _generator = new(seed);
        }

        public Chunk GetChunk(Point point) => loadedChunks[point];
        public bool IsLoaded(Point point) => loadedChunks.ContainsKey(point);

        private Path GetChunkFile(Point pos) =>
            new(_worldName, "chunks", FormattableString.Invariant($"chunk_{pos.X}_{pos.Y}.dat"));

        public DataResult<Chunk> LoadChunk(Point pos)
        {
            Path chunkFile = GetChunkFile(pos);
            DataResult<Chunk> chunkResult;
            if (loadedChunks.ContainsKey(pos))
                chunkResult = new DataResult<Chunk>.Success<Chunk>(GetChunk(pos));
            else if (chunkFile.IsFile())
                chunkResult = WorldStorage.ReadCodecFromDisk(chunkFile, Chunk.CODEC);
            else
                chunkResult = _generator.Generate(pos);

            if (chunkResult.IsSuccess())
            {
                ChunkEvent.Load loadEvent = new ChunkEvent.Load(this, chunkResult.ResultOrPartial(), pos);
                _chunkLoadPipeline.Run(loadEvent);
                if (loadEvent.IsEventEnding())
                    return new DataResult<Chunk>.Error<Chunk>("Pipeline ended.", chunkResult.ResultOrPartial());
            }
            return chunkResult;
        }
        public Chunk UnloadChunk(Point pos)
        {
            if (loadedChunks.TryGetValue(pos, out Chunk chunk))
            {
                Path chunkFile = GetChunkFile(pos);

                ChunkEvent.Unload unloadEvent = new ChunkEvent.Unload(this, Optional.Of(chunk), pos);
                _chunkUnloadPipeline.Run(unloadEvent);
            }
            return chunk;
        }

        public void BuildCache(SpriteBatch sb, TextureManager tm)
        {
            foreach (Chunk chunk in loadedChunks.Values.AsEnumerable())
                chunk.BuildCache(sb, tm);
        }

        public void Draw(SpriteBatch spriteBatch, TextureManager manager)
        {
            foreach (Chunk chunk in loadedChunks.Values)
            {
                chunk.Draw(spriteBatch);
            }
        }


        public int GetNextEntityId() =>
            nextEntityId;
        public int ConsumeNextEntityId() =>
            nextEntityId++;
    }