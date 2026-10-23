public interface DataResult<R>
{
    public bool IsSuccess();
    public bool IsError() => !IsSuccess();

    bool HasResultOrPartial();
    Optional<R> ResultOrPartial();
    Optional<R> Result();

    public record Success<T>(T Value) : DataResult<T>
    {
        public Optional<T> Result() => Optional.Of(Value);
        public Optional<Error<T>> GetError() => Optional.Empty<Error<T>>();

        public bool IsSuccess() => true;
        public bool HasResultOrPartial() => Result().HasValue();
        public Optional<T> ResultOrPartial() => Result();
    }

    public record Error<T>(string Message, Optional<T> Partial) : DataResult<T>
    {
        public Optional<T> Result() => Optional.Empty<T>();
        public Optional<Error<T>> GetError() => Optional.Of(this);

        public bool IsSuccess() => false;
        public bool HasResultOrPartial() => Partial.HasValue();
        public Optional<T> ResultOrPartial() => Partial;


    }
}