using Microsoft.Xna.Framework;

public interface IChunk
{
    public const int ChunkSize = 48;
    public const int TileSize = 64;
    public const int LayerCount = 3;

    protected static int[][][] CreateEmptyTiles()
    {
        int[][][] tiles = new int[IChunk.LayerCount][][];
        for (int l = 0; l < IChunk.LayerCount; l++)
        {
            tiles[l] = new int[IChunk.ChunkSize][];
            for (int x = 0; x < IChunk.ChunkSize; x++)
            {
                tiles[l][x] = new int[IChunk.ChunkSize];
            }
        }
        return tiles;
    }

    public Point GridPosition { get; protected set; }
    public bool IsChanged { get; set; }

    public void SetTile(int x, int y, WorldLayer layer, Identifier tileID);
    public int GetTile(int x, int y, WorldLayer layer);

    public void UpdateWithSnapshot(ChunkSnapshot snapshot);
    public ChunkSnapshot GenerateSnapshot();
}