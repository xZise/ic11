using ic11.ControlFlow.Context;
using ic11.ControlFlow.Messages;
using ic11.ControlFlow.NodeInterfaces;
using System.Reflection;

namespace ic11.ControlFlow.TreeProcessing;

public abstract class ControlFlowContextTreeVisitorBase<TResult>: ControlFlowTreeVisitorBase<TResult>
{
    protected FlowContext _flowContext;

    public ControlFlowContextTreeVisitorBase(FlowContext flowContext): base()
    {
        _flowContext = flowContext;
    }

    public override TResult Visit(INode node)
    {
        try
        {
            return base.Visit(node);
        }
        catch (TargetInvocationException ex) when (ex.InnerException is CompilerMessageException innerException)
        {
            _flowContext.CompilerMessages.Add(innerException.Error(node.SourceLocation));
            return default!;
        }
    }
}
