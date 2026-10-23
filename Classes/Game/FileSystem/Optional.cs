using System;

public static class Optional
{
    public static Optional<T> Empty<T>() => new Optional<T>();
    public static Optional<T> Of<T>(T value) => new Optional<T>(value);
    public static Optional<T> OfNullable<T>(T value) => value == null ? Empty<T>() : Of(value);
}

public struct Optional<T>
{
    private readonly bool _hasValue;
    private readonly T _value;

    public T Value
    {
        get
        {
            if (IsEmpty()) throw new InvalidOperationException("No value set");
            return _value;
        }
    }

    public bool HasValue() => _hasValue;
    public bool IsEmpty() => !HasValue();

    public T GetValue() => Value;

    public T GetValueOrDefault(T @default = default) => HasValue() ? Value : @default;
    public T GetValueOrElse(Func<T> elseFunc) => HasValue() ? Value : elseFunc();

    public T GetValueOrThrow(Exception err) => HasValue() ? Value : throw err;
    public T GetValueOrThrow() => HasValue() ? Value : throw new InvalidOperationException("No value set");

    internal Optional(T value)
    {
        if (value == null) throw new ArgumentNullException(nameof(value));
        _value = value;
        _hasValue = true;
    }
}