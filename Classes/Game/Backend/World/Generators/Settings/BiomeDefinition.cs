public sealed record BiomeDefinition(
    Identifier SurfaceGround,
    float Temperature,
    float Humidity,
    MaterialFeature[] Features);

public sealed record MaterialFeature(Identifier Material, float Chance, int Seed);
