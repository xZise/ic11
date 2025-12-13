namespace ic11.ControlFlow.Including;

public class NoIncludeHandler: IIncludeHandler
{
    public static readonly NoIncludeHandler Instance = new();

    public IncludeResult ReadIncludedFile(string include)
    {
        return IncludeResult.NotFound;
    }
}
