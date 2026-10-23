using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

public class ChunkGenerator : IChunkGenerator
{
    //private readonly LayerGenerator<Material> _materialGen = new();
    private readonly GroundGenerator _groundGen = new();

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

        DataResult<Ground[][]> groundResult =  _groundGen.Generate(gridPos);
        //DataResult<Material[][]> materialResult =  _materialGen.Generate(gridPos);

        Ground[][] groundPartial = groundResult.ResultOrPartial().GetValueOrDefault();
        //Material[][] materialPartial = materialResult.ResultOrPartial().GetValueOrDefault();
        for (int x = 0; x < Chunk.ChunkSize; x++)
        for (int y = 0; y < Chunk.ChunkSize; y++)
        {
            groundTiles[x][y] = BuiltInRegistries.GROUNDS.GetNumericId(groundPartial[x][y]);
            //materialTiles[x][y] = BuiltInRegistries.MATERIALS.GetNumericId(materialPartial[x][y]);
        }


        tiles[0] = groundTiles;
        tiles[1] = materialTiles;
        tiles[2] = structureTiles;
        Chunk chunk = new Chunk(gridPos, tiles);

        if (groundResult.IsSuccess() /*&& materialResult.IsSuccess()*/)
            return new DataResult<Chunk>.Success<Chunk>(chunk);
        return new DataResult<Chunk>.Error<Chunk>("Chunk generation failed.", Optional.Of(chunk));
    }
}