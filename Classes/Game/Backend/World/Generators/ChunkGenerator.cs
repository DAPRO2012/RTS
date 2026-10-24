using Microsoft.Xna.Framework;

public class ChunkGenerator : IChunkGenerator
{
    private readonly MaterialGenerator _materialGen = new();
    private readonly int _seed;
    private readonly BiomeGenerator _biomeGen;

    public ChunkGenerator(int seed = 1)
    {
        _seed = seed;
        _biomeGen = new BiomeGenerator(seed);
    }

    public DataResult<Chunk> Generate(Point gridPos)
    {
        int[][][] tiles = new int[Chunk.LayerCount][][];
        int[][] groundTiles = new int[Chunk.ChunkSize][];
        int[][] materialTiles = new int[Chunk.ChunkSize][];
        int[][] structureTiles = new int[Chunk.ChunkSize][];
        for (int arrayIndex = 0; arrayIndex < Chunk.ChunkSize; arrayIndex++)
        {
            groundTiles[arrayIndex] = new int[Chunk.ChunkSize];
            materialTiles[arrayIndex] = new int[Chunk.ChunkSize];
            structureTiles[arrayIndex] = new int[Chunk.ChunkSize];
        }

        tiles[(int)WorldLayer.Ground] = groundTiles;
        tiles[(int)WorldLayer.Material] = materialTiles;
        tiles[(int)WorldLayer.Structure] = structureTiles;

        DataResult<BiomeDefinition[][]> biomeResult = _biomeGen.Generate(gridPos);
        if (!biomeResult.IsSuccess())
        {
            string message = biomeResult is DataResult<BiomeDefinition[][]>.Error<BiomeDefinition[][]> error
                ? error.Message
                : "Biome generation failed.";
            return new DataResult<Chunk>.Error<Chunk>(
                message,
                Optional.Empty<Chunk>());
        }

        BiomeDefinition[][] biomes = biomeResult.Result().Value;
        for (int x = 0; x < Chunk.ChunkSize; x++)
        for (int y = 0; y < Chunk.ChunkSize; y++)
        {
            BiomeDefinition biome = biomes[x][y];
            if (biome == null)
                return new DataResult<Chunk>.Error<Chunk>(
                    "Biome generation produced a null biome.",
                    Optional.Empty<Chunk>());

            int groundId = BuiltInRegistries.GROUNDS.GetNumericId(biome.SurfaceGround);
            if (groundId == 0)
                return new DataResult<Chunk>.Error<Chunk>(
                    $"Biome references an unregistered surface ground: {biome.SurfaceGround}.",
                    Optional.Empty<Chunk>());

            groundTiles[x][y] = groundId;
        }

        DataResult<Material[][]> materialResult = _materialGen.Generate(gridPos, biomes, _seed);
        if (!materialResult.IsSuccess())
        {
            string message = materialResult is DataResult<Material[][]>.Error<Material[][]> error
                ? error.Message
                : "Material generation failed.";
            return new DataResult<Chunk>.Error<Chunk>(
                message,
                Optional.Empty<Chunk>());
        }

        Material[][] materials = materialResult.Result().Value;
        for (int x = 0; x < Chunk.ChunkSize; x++)
        for (int y = 0; y < Chunk.ChunkSize; y++)
        {
            Material material = materials[x][y];
            if (material == null)
                continue;

            int materialId = BuiltInRegistries.MATERIALS.GetNumericId(material);
            if (materialId == 0)
                return new DataResult<Chunk>.Error<Chunk>(
                    "Material generation produced an unregistered tile.",
                    Optional.Empty<Chunk>());

            materialTiles[x][y] = materialId;
        }

        return new DataResult<Chunk>.Success<Chunk>(new Chunk(gridPos, tiles));
    }
}