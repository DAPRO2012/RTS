using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RTS;

public class ClientLevel : ILevel
{
    private readonly Dictionary<int, Entity> _entities = [];
    protected Dictionary<Point, ClientChunk> loadedChunks = [];
    public ReadOnlyDictionary<Point, IChunk> GetChunks() =>
        new(
            loadedChunks.ToDictionary(entry => entry.Key, entry => (IChunk)entry.Value)
        );
    public IChunk GetChunk(Point position) =>
        loadedChunks[position];

    public bool RemoveChunk(Point position) =>
        loadedChunks.Remove(position);
    public void SetChunk(Point position, ClientChunk chunk) =>
        loadedChunks[position] = chunk;
    public void SetChunk(Point position, ChunkSnapshot snapshot)
    {
        if (!loadedChunks.TryGetValue(position, out ClientChunk chunk))
        {
            chunk = new(position);
            loadedChunks.Add(position, chunk);
        }
        chunk.UpdateWithSnapshot(snapshot);
    }

    public void BuildCache(SpriteBatch sb, TextureManager tm)
    {
        foreach (ClientChunk chunk in loadedChunks.Values.ToArray())
            chunk.BuildCache(sb, tm, this);
    }

    public void Draw(SpriteBatch spriteBatch, TextureManager manager)
    {
        foreach (ClientChunk chunk in loadedChunks.Values)
        {
            chunk.DrawBase(spriteBatch);
        }
        foreach (ClientChunk chunk in loadedChunks.Values)
        {
            chunk.DrawOverlays(spriteBatch);
        }
    }

    public int GetTile(Point position, WorldLayer layer) =>
        GetTile(position.X, position.Y, layer);
    public int GetTile(int x, int y, WorldLayer layer)
    {
        Point ChunkPos = new Point(
            (int)Math.Floor(x / (float)IChunk.ChunkSize),
            (int)Math.Floor(y / (float)IChunk.ChunkSize)
        );
        if (!loadedChunks.ContainsKey(ChunkPos))
            return 0;
        Point inChunkPos = new Point(
            x - ChunkPos.X * IChunk.ChunkSize,
            y - ChunkPos.Y * IChunk.ChunkSize
        );

        return loadedChunks[ChunkPos].GetTile(inChunkPos.X, inChunkPos.Y, layer);
    }
}