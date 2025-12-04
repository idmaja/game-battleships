public readonly struct Result<T>
{
    public bool Success { get; }
    public string Error { get; }
    public T Value { get; }

    private Result(bool success, T value, string error)
    {
        Success = success;
        Value = value;
        Error = error;
    }

    public static Result<T> Ok(T value)
    {
        return new Result<T>(true, value, string.Empty);
    }

    public static Result<T> Failed(string error)
    {
        return new Result<T>(false, default!, error);
    }
}
