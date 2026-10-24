#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;

public sealed class RecordCodecBuilder<TRecord>
{
    private readonly IReadOnlyList<IRecordField<TRecord>> _fields;

    private RecordCodecBuilder(IReadOnlyList<IRecordField<TRecord>> fields)
    {
        _fields = fields;
    }

    public static RecordCodecBuilder<TRecord> Create() =>
        new(Array.Empty<IRecordField<TRecord>>());

    public RecordCodecBuilder<TRecord> Field<TValue>(
        FieldCodec<TValue> field,
        Func<TRecord, TValue> getter)
    {
        ArgumentNullException.ThrowIfNull(field);
        ArgumentNullException.ThrowIfNull(field.ValueCodec);
        ArgumentNullException.ThrowIfNull(getter);

        if (string.IsNullOrWhiteSpace(field.Name))
        {
            throw new ArgumentException("A field name cannot be empty.", nameof(field));
        }

        if (_fields.Any(existing => string.Equals(existing.Name, field.Name, StringComparison.Ordinal)))
        {
            throw new ArgumentException($"Duplicate record field name '{field.Name}'.", nameof(field));
        }

        IRecordField<TRecord>[] fields = new IRecordField<TRecord>[_fields.Count + 1];
        for (int i = 0; i < _fields.Count; i++)
        {
            fields[i] = _fields[i];
        }

        fields[^1] = new RecordField<TRecord, TValue>(field, getter);
        return new RecordCodecBuilder<TRecord>(fields);
    }

    public Codec<TRecord> Build(Func<IReadOnlyList<object?>, TRecord> constructor)
    {
        ArgumentNullException.ThrowIfNull(constructor);
        return new RecordCodec<TRecord>(_fields, constructor);
    }
}

internal interface IRecordField<TRecord>
{
    string Name { get; }
    DataResult<JsonNode> Encode(TRecord record);
    DataResult<object?> Decode(JsonNode? input);
}

internal sealed class RecordCodec<TRecord> : Codec<TRecord>
{
    private readonly IReadOnlyList<IRecordField<TRecord>> _fields;
    private readonly Func<IReadOnlyList<object?>, TRecord> _constructor;

    public RecordCodec(
        IReadOnlyList<IRecordField<TRecord>> fields,
        Func<IReadOnlyList<object?>, TRecord> constructor)
    {
        _fields = fields;
        _constructor = constructor;
    }

    public override DataResult<JsonNode> Encode(TRecord value)
    {
        if (value is null)
        {
            return Error<JsonNode>("Cannot encode a null record.");
        }

        try
        {
            JsonObject result = [];
            foreach (IRecordField<TRecord> field in _fields)
            {
                DataResult<JsonNode> fieldResult = field.Encode(value);
                if (fieldResult is not DataResult<JsonNode>.Success<JsonNode> success)
                {
                    return Error<JsonNode>($"Field '{field.Name}': {GetErrorMessage(fieldResult)}");
                }

                result[field.Name] = success.Value.DeepClone();
            }

            return new DataResult<JsonNode>.Success<JsonNode>(result);
        }
        catch (Exception exception)
        {
            return Error<JsonNode>($"Record encoding failed: {exception.Message}");
        }
    }

    public override DataResult<TRecord> Decode(JsonNode? input)
    {
        if (input is not JsonObject obj)
        {
            return Error<TRecord>("Expected a JSON object.");
        }

        try
        {
            object?[] values = new object?[_fields.Count];
            for (int i = 0; i < _fields.Count; i++)
            {
                IRecordField<TRecord> field = _fields[i];
                DataResult<object?> fieldResult = field.Decode(obj[field.Name]);
                if (fieldResult is not DataResult<object?>.Success<object?> success)
                {
                    return Error<TRecord>($"Field '{field.Name}': {GetErrorMessage(fieldResult)}");
                }

                values[i] = success.Value;
            }

            return new DataResult<TRecord>.Success<TRecord>(
                _constructor(Array.AsReadOnly(values)));
        }
        catch (Exception exception)
        {
            return Error<TRecord>($"Record decoding failed: {exception.Message}");
        }
    }

    private static string GetErrorMessage<T>(DataResult<T> result) =>
        result is DataResult<T>.Error<T> error ? error.Message : "Unknown codec error.";

    private static DataResult<TResult> Error<TResult>(string message) =>
        new DataResult<TResult>.Error<TResult>(message, Optional.Empty<TResult>());
}
