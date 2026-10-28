using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework;

public interface ILevel
{
    public ReadOnlyDictionary<Point, IChunk> GetChunks();

    public IChunk GetChunk(Point position);

    public int GetTile(Point position, WorldLayer layer);
    public int GetTile(int x, int y, WorldLayer layer);
}