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

    public DataResult<ServerChunk> Generate(Point gridPos)
    {
        int[][][] tiles = new int[IChunk.LayerCount][][];
        int[][] groundTiles = new int[IChunk.ChunkSize][];
        int[][] materialTiles = new int[IChunk.ChunkSize][];
        int[][] structureTiles = new int[IChunk.ChunkSize][];
        for (int arrayIndex = 0; arrayIndex < IChunk.ChunkSize; arrayIndex++)
        {
            groundTiles[arrayIndex] = new int[IChunk.ChunkSize];
            materialTiles[arrayIndex] = new int[IChunk.ChunkSize];
            structureTiles[arrayIndex] = new int[IChunk.ChunkSize];
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
            return new DataResult<ServerChunk>.Error<ServerChunk>(
                message,
                Optional.Empty<ServerChunk>());
        }

        BiomeDefinition[][] biomes = biomeResult.Result().Value;
        for (int x = 0; x < IChunk.ChunkSize; x++)
        for (int y = 0; y < IChunk.ChunkSize; y++)
        {
            BiomeDefinition biome = biomes[x][y];
            if (biome == null)
                return new DataResult<ServerChunk>.Error<ServerChunk>(
                    "Biome generation produced a null biome.",
                    Optional.Empty<ServerChunk>());

            int groundId = BuiltInRegistries.GROUNDS.GetId(biome.SurfaceGround);
            if (groundId == 0)
                return new DataResult<ServerChunk>.Error<ServerChunk>(
                    $"Biome references an unregistered surface ground: {biome.SurfaceGround}.",
                    Optional.Empty<ServerChunk>());

            groundTiles[x][y] = groundId;
        }

        DataResult<Material[][]> materialResult = _materialGen.Generate(gridPos, biomes, _seed);
        if (!materialResult.IsSuccess())
        {
            string message = materialResult is DataResult<Material[][]>.Error<Material[][]> error
                ? error.Message
                : "Material generation failed.";
            return new DataResult<ServerChunk>.Error<ServerChunk>(
                message,
                Optional.Empty<ServerChunk>());
        }

        Material[][] materials = materialResult.Result().Value;
        for (int x = 0; x < IChunk.ChunkSize; x++)
        for (int y = 0; y < IChunk.ChunkSize; y++)
        {
            Material material = materials[x][y];
            if (material == null)
                continue;

            int materialId = BuiltInRegistries.MATERIALS.GetId(material);
            if (materialId == 0)
                return new DataResult<ServerChunk>.Error<ServerChunk>(
                    "Material generation produced an unregistered tile.",
                    Optional.Empty<ServerChunk>());

            materialTiles[x][y] = materialId;
        }

        return new DataResult<ServerChunk>.Success<ServerChunk>(new ServerChunk(gridPos, tiles));
    }
}