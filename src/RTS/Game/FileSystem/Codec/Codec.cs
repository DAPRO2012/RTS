#nullable enable

using System;
using System.Text.Json.Nodes;

public abstract class Codec<T>
{
    public abstract DataResult<JsonNode> Encode(T value);
    public abstract DataResult<T> Decode(JsonNode? input);

    public Codec<T[]> ArrayOf() => new ArrayCodec<T>(this);
    public Codec<T[,,]> Array3Of() => new RectangularArray3Codec<T>(this);

    public FieldCodec<T> FieldOf(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("A field name cannot be empty.", nameof(name));
        }

        return new FieldCodec<T>(name, this);
    }
}

public sealed record FieldCodec<T>(string Name, Codec<T> ValueCodec);

internal sealed class ArrayCodec<T> : Codec<T[]>
{
    private readonly Codec<T> _elementCodec;

    public ArrayCodec(Codec<T> elementCodec)
    {
        _elementCodec = elementCodec;
    }

    public override DataResult<JsonNode> Encode(T[] value)
    {
        if (value is null)
        {
            return Error<JsonNode>("Cannot encode a null array.");
        }

        JsonArray result = [];
        for (int i = 0; i < value.Length; i++)
        {
            DataResult<JsonNode> elementResult = _elementCodec.Encode(value[i]);
            if (elementResult is not DataResult<JsonNode>.Success<JsonNode> success)
            {
                return Error<JsonNode>(
                    $"Element at index {i}: {GetErrorMessage(elementResult)}");
            }

            result.Add(success.Value.DeepClone());
        }

        return new DataResult<JsonNode>.Success<JsonNode>(result);
    }

    public override DataResult<T[]> Decode(JsonNode? input)
    {
        if (input is not JsonArray array)
        {
            return Error<T[]>("Expected a JSON array.");
        }

        T[] result = new T[array.Count];
        for (int i = 0; i < array.Count; i++)
        {
            DataResult<T> elementResult = _elementCodec.Decode(array[i]);
            if (elementResult is not DataResult<T>.Success<T> success)
            {
                return Error<T[]>(
                    $"Element at index {i}: {GetErrorMessage(elementResult)}");
            }

            result[i] = success.Value;
        }

        return new DataResult<T[]>.Success<T[]>(result);
    }

    private static string GetErrorMessage<TResult>(DataResult<TResult> result) =>
        result is DataResult<TResult>.Error<TResult> error
            ? error.Message
            : "Unknown codec error.";

    private static DataResult<TResult> Error<TResult>(string message) =>
        new DataResult<TResult>.Error<TResult>(message, Optional.Empty<TResult>());
}

internal sealed class RectangularArray3Codec<T> : Codec<T[,,]>
{
    private readonly Codec<T> _elementCodec;

    public RectangularArray3Codec(Codec<T> elementCodec)
    {
        _elementCodec = elementCodec;
    }

    public override DataResult<JsonNode> Encode(T[,,] value)
    {
        if (value is null)
        {
            return Error<JsonNode>("Cannot encode a null three-dimensional array.");
        }

        int length0 = value.GetLength(0);
        int length1 = value.GetLength(1);
        int length2 = value.GetLength(2);
        JsonArray result = [];

        for (int i = 0; i < length0; i++)
        {
            JsonArray dimension1 = [];
            result.Add(dimension1);

            for (int j = 0; j < length1; j++)
            {
                JsonArray dimension2 = [];
                dimension1.Add(dimension2);

                for (int k = 0; k < length2; k++)
                {
                    DataResult<JsonNode> elementResult = _elementCodec.Encode(value[i, j, k]);
                    if (elementResult is not DataResult<JsonNode>.Success<JsonNode> success)
                    {
                        return Error<JsonNode>(
                            $"Element at [{i}, {j}, {k}]: {GetErrorMessage(elementResult)}");
                    }

                    dimension2.Add(success.Value.DeepClone());
                }
            }
        }

        return new DataResult<JsonNode>.Success<JsonNode>(result);
    }

    public override DataResult<T[,,]> Decode(JsonNode? input)
    {
        if (input is not JsonArray dimension0)
        {
            return Error<T[,,]>("Expected a JSON array for dimension 0.");
        }

        int? length1 = null;
        int? length2 = null;

        for (int i = 0; i < dimension0.Count; i++)
        {
            if (dimension0[i] is not JsonArray dimension1)
            {
                return Error<T[,,]>($"Expected a JSON array at [{i}].");
            }

            if (length1 is null)
            {
                length1 = dimension1.Count;
            }
            else if (dimension1.Count != length1)
            {
                return Error<T[,,]>($"Inconsistent dimension 1 length at [{i}].");
            }

            for (int j = 0; j < dimension1.Count; j++)
            {
                if (dimension1[j] is not JsonArray dimension2)
                {
                    return Error<T[,,]>($"Expected a JSON array at [{i}, {j}].");
                }

                if (length2 is null)
                {
                    length2 = dimension2.Count;
                }
                else if (dimension2.Count != length2)
                {
                    return Error<T[,,]>($"Inconsistent dimension 2 length at [{i}, {j}].");
                }
            }
        }

        int dimension1Length = length1 ?? 0;
        int dimension2Length = length2 ?? 0;
        T[,,] result = new T[dimension0.Count, dimension1Length, dimension2Length];

        for (int i = 0; i < dimension0.Count; i++)
        {
            JsonArray dimension1 = (JsonArray)dimension0[i]!;
            for (int j = 0; j < dimension1Length; j++)
            {
                JsonArray dimension2 = (JsonArray)dimension1[j]!;
                for (int k = 0; k < dimension2Length; k++)
                {
                    DataResult<T> elementResult = _elementCodec.Decode(dimension2[k]);
                    if (elementResult is not DataResult<T>.Success<T> success)
                    {
                        return Error<T[,,]>(
                            $"Element at [{i}, {j}, {k}]: {GetErrorMessage(elementResult)}");
                    }

                    result[i, j, k] = success.Value;
                }
            }
        }

        return new DataResult<T[,,]>.Success<T[,,]>(result);
    }

    private static string GetErrorMessage<TResult>(DataResult<TResult> result) =>
        result is DataResult<TResult>.Error<TResult> error
            ? error.Message
            : "Unknown codec error.";

    private static DataResult<TResult> Error<TResult>(string message) =>
        new DataResult<TResult>.Error<TResult>(message, Optional.Empty<TResult>());
}

public static class Codec
{
    public static Codec<string> STRING { get; } = new StringCodec();
    public static Codec<int> INT { get; } = new IntCodec();

    private sealed class StringCodec : Codec<string>
    {
        public override DataResult<JsonNode> Encode(string value)
        {
            if (value is null)
            {
                return Error<JsonNode>("Cannot encode a null string.");
            }

            return new DataResult<JsonNode>.Success<JsonNode>(JsonValue.Create(value)!);
        }

        public override DataResult<string> Decode(JsonNode? input)
        {
            if (input is JsonValue value && value.TryGetValue<string>(out string? result)
                && result is not null)
            {
                return new DataResult<string>.Success<string>(result);
            }

            return Error<string>("Expected a string.");
        }
    }

    private sealed class IntCodec : Codec<int>
    {
        public override DataResult<JsonNode> Encode(int value) =>
            new DataResult<JsonNode>.Success<JsonNode>(JsonValue.Create(value)!);

        public override DataResult<int> Decode(JsonNode? input)
        {
            if (input is JsonValue value && value.TryGetValue<int>(out int result))
            {
                return new DataResult<int>.Success<int>(result);
            }

            return Error<int>("Expected an integer.");
        }
    }

    private static DataResult<T> Error<T>(string message) =>
        new DataResult<T>.Error<T>(message, Optional.Empty<T>());
}