using System.Text;
using Antlr4.Runtime;
using ic11.ControlFlow.Context;
using ic11.ControlFlow.InstructionsProcessing;
using ic11.ControlFlow.Messages;
using ic11.ControlFlow.Nodes;
using ic11.ControlFlow.TreeProcessing;

namespace ic11;

public class Program
{
    static void Main(string[] args)
    {
        if (args.Length != 1 && args.Length != 2)
        {
            Console.WriteLine("Usage: ic11 path [-w]");
            return;
        }

        var argsPath = args[0];
        var filePaths = new List<string>();
        var pathType = PathType.Nonexistant;
        var shouldSave = args.Contains("-w");

        if (File.Exists(argsPath))
            pathType = PathType.File;

        if (Directory.Exists(argsPath))
            pathType = PathType.Directory;

        if (pathType == PathType.Nonexistant)
        {
            Console.WriteLine("File or directory does not exist");
            return;
        }

        if (pathType == PathType.File)
        {
            CompileFile(argsPath, shouldSave);
            return;
        }

        if (pathType == PathType.Directory)
        {
            Console.WriteLine($"Compiling every *.ic11 file in directory {argsPath}");

            var ic11Files = Directory.EnumerateFiles(argsPath)
                .Where(p => Path.GetExtension(p).Equals(".ic11", StringComparison.InvariantCultureIgnoreCase))
                .Select(Path.GetFileName);

            foreach (var file in ic11Files)
            {
                Console.WriteLine($"\n\n{file}\n");
                CompileFile(Path.Combine(argsPath, file!), shouldSave);
            }
        }

        //Console.WriteLine(new ControlFlowTreeVisualizer().Visualize(flowContext.Root));
    }

    private static void CompileFile(string path, bool shouldSave)
    {
        var input = File.ReadAllText(path);
        var compilationResult = CompileText(input, path);
        bool hasErrors = false;
        foreach (var error in compilationResult.CompilerMessages.OrderBy(m => m.Severity).ThenBy(m => (m.SourceLocation.LineNumber, m.SourceLocation.Column)))
        {
            hasErrors |= error.Severity == Severity.Error;
            Console.Error.WriteLine($"{error.Severity} in {error.SourceLocation}:");
            Console.Error.WriteLine(error.Message);
            string? errorCode = error.SourceLocation.ShowErrorCode();
            if (errorCode != null)
            {
                Console.Error.WriteLine(errorCode);
            }
        }
        if (string.IsNullOrEmpty(compilationResult.Instructions) || hasErrors)
        {
            return;
        }

        string output = compilationResult.Instructions;
        Console.WriteLine(output);

        if (shouldSave)
        {
            var directoryPath = Path.GetDirectoryName(path);
            var fileName = Path.Combine(directoryPath!, Path.GetFileNameWithoutExtension(path) + ".ic10");
            File.WriteAllText(fileName, output);
        }
    }

    public static (string Instructions, List<CompilerMessage> CompilerMessages) CompileText(string input, string filename)
    {
        Ic11InputStream inputStream = new Ic11InputStream(input, filename);
        Ic11Lexer lexer = new Ic11Lexer(inputStream);
        CommonTokenStream commonTokenStream = new CommonTokenStream(lexer);
        Ic11Parser parser = new Ic11Parser(commonTokenStream);

        var tree = parser.program(); // Assuming 'program' is the entry point of your grammar

        var flowContext = new FlowContext(filename);
        var flowAnalyzer = new ControlFlowBuilderVisitor(flowContext);
        flowAnalyzer.Visit(tree);

        new RootStatementsSorter().SortStatements(flowContext.Root);
        new MethodsVisitor(flowContext).Visit(flowContext.Root);
        new MethodCallsVisitor(flowContext).VisitRoot(flowContext.Root);
        new ScopeVisitor(flowContext).Visit(flowContext.Root);
        new VariableVisitor(flowContext).Visit(flowContext.Root);
        new VariableCyclesAdjVisitor().VisitRoot(flowContext.Root);
        new MethodCallGraphVisitor(flowContext).VisitRoot(flowContext.Root);
        new RegisterVisitor(flowContext).DoWork();
        new MethodsRegisterRangesDistributor(flowContext).DoWork();
        var instructions = new Ic10CommandGenerator(flowContext).Visit(flowContext.Root);
        if (flowContext.CompilerMessages.Any(m => m.Severity == Severity.Error))
        {
            return ("", flowContext.CompilerMessages);
        }

        UselessInstructionRemover.Remove(instructions);
        LabelsRemoval.RemoveLabels(instructions);

        var output = new StringBuilder();

        foreach (var item in instructions)
            output.AppendLine(item.Render());

        return (output.ToString(), flowContext.CompilerMessages);
    }

    private enum PathType
    {
        Nonexistant,
        File,
        Directory,
    }
}