using ic11.ControlFlow.Messages;
using ic11.ControlFlow.NodeInterfaces;
using ic11.ControlFlow.Nodes;

namespace ic11.ControlFlow.Context;
public class FlowContext
{
    public readonly Root Root;
    public INode CurrentNode;
    public readonly Dictionary<string, SourceLocation> IncludedFiles = new();
    public readonly Dictionary<string, MethodDeclaration> DeclaredMethods = new();
    public List<IStatement> CurrentStatementList => Root.Statements;
    public readonly List<UserDefinedVariable> AllUserDefinedVariables = new();
    public readonly List<UserDefinedConstant> AllUserDefinedConstants = new();
    public readonly List<CompilerMessage> CompilerMessages = new();

    public FlowContext(string filename)
    {
        var root = new Root(filename);
        CurrentNode = root;
        Root = root;
    }

    public void Include(FlowContext other)
    {
        foreach (IStatement statement in other.CurrentStatementList)
        {
            switch (statement)
            {
                case MethodDeclaration:
                case ConstantDeclaration:
                    CurrentStatementList.Add(statement);
                    break;
                default:
                    throw new Exception($"Invalid statement {statement} in included source");
            }
        }
        CompilerMessages.AddRange(other.CompilerMessages);
    }
}
