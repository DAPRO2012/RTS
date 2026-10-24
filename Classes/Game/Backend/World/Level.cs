using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RTS;

public class Level(string worldName, int seed = 1)
    {
        protected readonly static SavedStorage WorldStorage = new(new(AppContext.BaseDirectory, "data", "saves"));
        private readonly string _worldName = worldName;
        private readonly ChunkGenerator _generator = new(seed);

        protected Dictionary<Point, Chunk> loadedChunks = [];
        public ReadOnlyDictionary<Point, Chunk> LoadedChunks => loadedChunks.AsReadOnly();

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
                ChunkEvent.Load loadEvent = new ChunkEvent.Load(this, chunkResult.Result().Value);
                Main.EVENT_BUS.Publish(loadEvent);
                if (loadEvent.IsEventEnding())
                    return new DataResult<Chunk>.Error<Chunk>("Pipeline ended.", chunkResult.ResultOrPartial());
                loadedChunks[pos] = chunkResult.Result().Value;
            }
            return chunkResult;
        }
        public Chunk UnloadChunk(Point pos)
        {
            if (loadedChunks.TryGetValue(pos, out Chunk chunk))
            {
                Path chunkFile = GetChunkFile(pos);

                ChunkEvent.Unload unloadEvent = new ChunkEvent.Unload(this, chunk);
                Main.EVENT_BUS.Publish(unloadEvent);
                if (!unloadEvent.IsEventEnding())
                {
                    if (chunk.IsChanged) WorldStorage.WriteCodecToDisk(chunkFile, chunk, Chunk.CODEC);
                    loadedChunks.Remove(pos);
                    chunk.Dispose();
                }
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
    }