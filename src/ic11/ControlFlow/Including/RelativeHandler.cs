namespace ic11.ControlFlow.Including;

public class RelativeHandler: IIncludeHandler
{
    private readonly string _sourceFilePath;
    private readonly HashSet<string> _includedPaths;
    private readonly RelativeHandler? _parent;

    private RelativeHandler(string sourceFilePath, RelativeHandler parent)
    {
        _sourceFilePath = sourceFilePath;
        _includedPaths = parent._includedPaths;
        _parent = parent;
    }

    private RelativeHandler(string sourceFilePath)
    {
        _sourceFilePath = sourceFilePath;
        _includedPaths = new();
    }

    public static RelativeHandler Create(string sourceFilePath)
    {
        var libPath = Path.Combine(AppContext.BaseDirectory, "lib");
        if (Directory.Exists(libPath))
        {
            RelativeHandler lib = new(libPath);
            return new RelativeHandler(sourceFilePath, lib);
        }
        return new(sourceFilePath);
    }

    public IncludeResult ReadIncludedFile(string include)
    {
        string includedFileName = Path.GetFullPath(include, _sourceFilePath);
        if (_includedPaths.Contains(includedFileName))
        {
            return IncludeResult.AlreadyIncluded;
        }

        string contents;
        try
        {
            contents = File.ReadAllText(includedFileName);
        }
        catch (FileNotFoundException)
        {
            if (_parent != null)
            {
                return _parent.ReadIncludedFile(include);
            }
            return IncludeResult.NotFound;
        }
        _includedPaths.Add(includedFileName);
        // the file name is always inside another directory
        string includesFilePath = Path.GetDirectoryName(includedFileName)!;
        RelativeHandler handler = includesFilePath == _sourceFilePath ? this : new RelativeHandler(includesFilePath, this);
        return new(contents, handler);
    }
}