using ic11.ControlFlow.Context;
using ic11.ControlFlow.NodeInterfaces;
using ic11.ControlFlow.Nodes;

namespace ic11.ControlFlow.TreeProcessing;

public class UnusedVariablesVisitor : ControlFlowContextTreeVisitorBase<object?>
{
    private readonly Dictionary<string, SourceLocation> _declaredDevices = new();
    
    public UnusedVariablesVisitor(FlowContext flowContext) : base(flowContext)
    {
        AllowMethodSkip = true;
        SkippedReturnValue = false;
    }

    protected override Type VisitorType => typeof(UnusedVariablesVisitor);

    protected override object? Visit(Root node)
    {
        base.Visit(node);

        foreach (var device in _declaredDevices)
        {
            _flowContext.CompilerMessages.Add(new($"Unused device {device.Key}", device.Value, Messages.Severity.Warning));
        }

        return null;
    }

    private object? Visit(PinDeclaration node)
    {
        if (!_declaredDevices.ContainsKey(node.Name))
            _declaredDevices.Add(node.Name, node.SourceLocation);
        return null;
    }

    private object? Visit(MemberAssignment node)
    {
        if (node.Device.Name is not null)
            _declaredDevices.Remove(node.Device.Name);
        return null;
    }

    private object? Visit(MemberAccess node)
    {
        if (node.Device.Name is not null)
            _declaredDevices.Remove(node.Device.Name);
        return null;
    }

    protected override object? Visit(MethodDeclaration node)
    {
        foreach ((var variable, var parameter) in node.ParameterVariables.Zip(node.Parameters))
        {
            if (variable.LastReferencedIndex < 0)
            {
                _flowContext.CompilerMessages.Add(new($"Unused parameter {parameter.Text} in {node.Name}", parameter.SourceLocation, Messages.Severity.Warning));
            }
        }

        return base.Visit(node);
    }

    private object? Visit(VariableDeclaration node)
    {
        if (node.Variable?.LastReferencedIndex < 0)
        {
            string containingMethod = "";
            INode? parent = node;
            do
            {
                parent = parent.Parent;
                if (parent is MethodDeclaration md)
                {
                    containingMethod = $" in {md.Name}";
                    break;
                }
            }
            while (parent != null);
            _flowContext.CompilerMessages.Add(new($"Unused variable {node.Name}{containingMethod}", node.Name.SourceLocation, Messages.Severity.Warning));
        }
        return null;
    }
}
