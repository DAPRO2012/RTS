using System;

public record Weighted<T>
{
    private static readonly Logger LOGGER = LogUtils.GetLogger();

    public T Value { get; init; }
    public int Weight { get; init; }

    public Weighted(T value, int weight)
    {
        if (weight < 0) {
            throw new ArgumentException("Weight should be >= 0");
        }
        if (weight == 0) {
            LOGGER.Warn("Found 0 weight, make sure this is intentional!");
        }

        Value = value;
        Weight = weight;
    }
}