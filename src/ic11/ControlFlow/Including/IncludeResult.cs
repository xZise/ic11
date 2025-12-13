namespace ic11.ControlFlow.Including;

public readonly struct IncludeResult
{
    public enum Results
    {
        NotFound,
        Found,
        AlreadyIncluded
    }

    public static readonly IncludeResult NotFound = new(Results.NotFound, null, null);
    public static readonly IncludeResult AlreadyIncluded = new(Results.AlreadyIncluded, null, null);

    public readonly Results Result;
    public readonly string? Contents;
    public readonly IIncludeHandler? Handler;

    public IncludeResult(string contents, IIncludeHandler handler): this(Results.Found, contents, handler)
    {}

    private IncludeResult(Results result, string? contents, IIncludeHandler? handler)
    {
        Result = result;
        Contents = contents;
        Handler = handler;
    }

    public void Found(Action<string, IIncludeHandler> callback)
    {
        if (Result == Results.Found)
        {
            callback(Contents!, Handler!);
        }
    }
}
