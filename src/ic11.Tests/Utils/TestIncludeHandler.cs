namespace ic11.Tests.Utils;

using ic11.ControlFlow.Including;

public class TestIncludeHandler : IIncludeHandler
{
    public readonly Dictionary<string, string> AvailableIncludes = new();
    private readonly HashSet<string> _includedFiles = new();

    public IncludeResult ReadIncludedFile(string include)
    {
        if (_includedFiles.Contains(include))
        {
            return IncludeResult.AlreadyIncluded;
        }
        if (AvailableIncludes.TryGetValue(include, out var contents))
        {
            _includedFiles.Add(include);
            return new IncludeResult(contents, this);
        }
        return IncludeResult.NotFound;
    }
}