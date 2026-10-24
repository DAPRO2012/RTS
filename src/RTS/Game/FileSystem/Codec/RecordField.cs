#nullable enable

using System;
using System.Text.Json.Nodes;

public sealed record RecordField<TRecord, TValue>(
    FieldCodec<TValue> Field,
    Func<TRecord, TValue> Getter) : IRecordField<TRecord>
{
    public string Name => Field.Name;

    public DataResult<JsonNode> Encode(TRecord record) =>
        Field.ValueCodec.Encode(Getter(record));

    public DataResult<object?> Decode(JsonNode? input)
    {
        DataResult<TValue> result = Field.ValueCodec.Decode(input);
        if (result is DataResult<TValue>.Success<TValue> success)
        {
            return new DataResult<object?>.Success<object?>(success.Value);
        }

        string message = result is DataResult<TValue>.Error<TValue> error
            ? error.Message
            : "Unknown codec error.";
        return new DataResult<object?>.Error<object?>(message, Optional.Empty<object?>());
    }
}