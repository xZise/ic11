namespace ic11.ControlFlow.Including;

public interface IIncludeHandler
{
    IncludeResult ReadIncludedFile(string include);    
}