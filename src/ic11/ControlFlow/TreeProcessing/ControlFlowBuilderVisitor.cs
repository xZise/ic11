using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using ic11.ControlFlow.Context;
using ic11.ControlFlow.DataHolders;
using ic11.ControlFlow.Messages;
using ic11.ControlFlow.NodeInterfaces;
using ic11.ControlFlow.Nodes;
using System.Globalization;
using static Ic11Parser;

namespace ic11.ControlFlow.TreeProcessing;
public class ControlFlowBuilderVisitor : Ic11BaseVisitor<INodeExpression?>
{
    public readonly FlowContext FlowContext;

    private INode CurrentNode
    {
        get { return FlowContext.CurrentNode; }
        set { FlowContext.CurrentNode = value; }
    }

    private void AddToStatements(IStatement node)
    {
        if (CurrentNode is not IStatementsContainer statementsContainer)
            throw new Exception($"Unexpected statement {node?.GetType().Name}");

        statementsContainer.AddToStatements(node);
    }

    public ControlFlowBuilderVisitor(FlowContext flowContext)
    {
        FlowContext = flowContext;
    }

    public override INodeExpression? Visit(IParseTree tree) => base.Visit(tree);

    public INodeExpression Visit(ExpressionContext context) => base.Visit(context)!;

    public override INodeExpression? VisitDeclaration([NotNull] DeclarationContext context)
    {
        SourceLocation sourceLocation = SourceLocation.FromRuleContext(context);
        if (CurrentNode is not Root root)
            throw new CompilerMessageException("Pin declaration must be top level statement", sourceLocation);

        var newNode = new PinDeclaration(context.IDENTIFIER().GetText(), sourceLocation, context.PINID().GetText());
        root.Statements.Add(newNode);

        return null;
    }

    public override INodeExpression? VisitInclude([NotNull] IncludeContext context)
    {
        var filename = context.HASH_LITERAL().GetText().Trim('"');
        SourceLocation sourceLocation = SourceLocation.FromRuleContext(context);

        if (CurrentNode is not Root)
            throw new CompilerMessageException("Include declaration must be top level statement", sourceLocation);
        
        if (FlowContext.IncludedFiles.TryGetValue(filename, out var location))
            FlowContext.CompilerMessages.Add(new($"Include declaration already in {location}", sourceLocation, Severity.Warning));
        else
            FlowContext.IncludedFiles[filename] = sourceLocation;

        return null;
    }

    public override INodeExpression? VisitFunction([NotNull] FunctionContext context)
    {
        var identifiers = context.IDENTIFIER();
        var name = identifiers[0].GetText();

        var parameters = identifiers.Skip(1)
            .Select(i => new LocatedText(i))
            .ToList();

        var block = context.block();

        var returnType = context.retType.Text switch
        {
            "void" => MethodReturnType.Void,
            "real" => MethodReturnType.Real,
            _ => throw new CompilerMessageException($"Unrecognized method return type {context.retType.Text}. Supported: void, real.", new SourceLocation(context.retType)),
        };

        SourceLocation sourceLocation = SourceLocation.FromRuleContext(context);
        var newNode = new MethodDeclaration(name, sourceLocation, returnType, parameters);

        if (CurrentNode is not Root root)
            throw new CompilerMessageException("Method declaration must be top level statement", sourceLocation);

        root.Statements.Add(newNode);

        CurrentNode = newNode;
        Visit(block);
        CurrentNode = root;

        return null;
    }

    public override INodeExpression? VisitYieldStatement([NotNull] YieldStatementContext context)
    {
        var newNode = new StatementParam0(SourceLocation.FromTerminalNode(context.YIELD()), "yield");
        AddToStatements(newNode);

        return null;
    }

    public override INodeExpression? VisitHcfStatement([NotNull] HcfStatementContext context)
    {
        var newNode = new StatementParam0(SourceLocation.FromTerminalNode(context.HCF()), "hcf");
        AddToStatements(newNode);

        return null;
    }

    public override INodeExpression? VisitDeviceStackClear([NotNull] DeviceStackClearContext context)
    {
        var device = context.identifier.Text;

        if (context.identifier.Type == BASE_DEVICE)
            device = "db";

        var newNode = new StatementParam0(SourceLocation.FromRuleContext(context), $"clr {device}");
        AddToStatements(newNode);

        return null;
    }

    public override INodeExpression? VisitDeviceWithIdStackClear([NotNull] DeviceWithIdStackClearContext context)
    {
        var expression = Visit(context.deviceIdxExpr)!;
        var newNode = new StatementParam1(SourceLocation.FromRuleContext(context), "clrd", expression);
        AddToStatements(newNode);

        return null;
    }

    public override INodeExpression? VisitSleepStatement([NotNull] SleepStatementContext context)
    {
        var expression = Visit(context.expression())!;
        var newNode = new StatementParam1(SourceLocation.FromTerminalNode(context.SLEEP()), "sleep", expression);
        AddToStatements(newNode);

        return null;
    }

    public override INodeExpression? VisitVariableDeclaration([NotNull] VariableDeclarationContext context)
    {
        var variableName = new LocatedText(context.IDENTIFIER());
        var expression = Visit(context.expression())!;
        var newNode = new VariableDeclaration(variableName, SourceLocation.FromRuleContext(context), expression);

        AddToStatements(newNode);

        return null;
    }

    public override INodeExpression? VisitConstantDeclaration([NotNull] ConstantDeclarationContext context)
    {
        var constantName = new LocatedText(context.IDENTIFIER());
        var expression = Visit(context.expression())!;
        var newNode = new ConstantDeclaration(constantName, SourceLocation.FromTerminalNode(context.IDENTIFIER()), expression);

        AddToStatements(newNode);

        return null;
    }

    public override INodeExpression VisitLiteral([NotNull] LiteralContext context)
    {
        var value = context.GetText();
        var sourceLocation = SourceLocation.FromRuleContext(context);

        decimal number;
        try
        {
            number = context.type.Type switch
            {
                STRING_LITERAL => OperationHelper.ToASCII(value.AsSpan(1..^1)),
                HASH_LITERAL => OperationHelper.Hash(value.Trim('"')),
                INTEGER_HEX => OperationHelper.ParseHex(value),
                INTEGER_BINARY => OperationHelper.ParseBinary(value),
                _ when value == "true" => 1m,
                _ when value == "false" => 0m,
                _ => decimal.Parse(value, CultureInfo.InvariantCulture),
            };
        }
        catch (CompilerMessageException ex) when (ex.SourceLocation is null)
        {
            throw ex.WithLocation(sourceLocation);
        }

        return new Literal(sourceLocation, number);
    }

    private static IParseTree GetTreeFromBlockOrStatement(BlockOrStatementContext ctx)
    {
        IParseTree block = ctx.block();
        IParseTree statement = ctx.statement();
        return block ?? statement;
    }

    public override INodeExpression? VisitIfStatement([NotNull] IfStatementContext context)
    {
        var thenPart = GetTreeFromBlockOrStatement(context.thenPart);

        var expression = Visit(context.expression())!;

        var newNode = new If(SourceLocation.FromRuleContext(context), expression);
        AddToStatements(newNode);

        CurrentNode = newNode;
        newNode.CurrentStatementsContainer = IfStatementsContainer.If;
        Visit(thenPart);

        if (context.elsePart != null)
        {
            var elsePart = GetTreeFromBlockOrStatement(context.elsePart);
            newNode.CurrentStatementsContainer = IfStatementsContainer.Else;
            Visit(elsePart);
        }

        CurrentNode = newNode.Parent!;

        return null;
    }

    public override INodeExpression? VisitMemberAssignment([NotNull] MemberAssignmentContext context)
    {
        var valueExpr = Visit(context.valueExpr)!;

        var member = context.member.Text;
        var device = context.identifier.Text;

        if (context.identifier.Type == BASE_DEVICE)
            device = "db";

        var newNode = new MemberAssignment(device, SourceLocation.FromRuleContext(context), member, valueExpr);
        AddToStatements(newNode);

        return null;
    }

    public override INodeExpression? VisitMemberExtendedAssignment([NotNull] MemberExtendedAssignmentContext context)
    {
        var valueExpr = Visit(context.valueExpr)!;
        var targetIdxExpr = Visit(context.targetIdxExpr)!;

        var member = context.member?.Text;
        var device = context.identifier.Text;

        if (context.identifier.Type == BASE_DEVICE)
            device = "db";

        var newNode = new MemberAssignment(device, SourceLocation.FromRuleContext(context), GetDeviceTarget(context.prop.Type), member, targetIdxExpr, valueExpr);
        AddToStatements(newNode);

        return null;
    }

    public override INodeExpression VisitMemberAccess([NotNull] MemberAccessContext context)
    {
        var member = context.member.Text;
        var device = context.identifier.Text;

        if (context.identifier.Type == BASE_DEVICE)
            device = "db";

        var newNode = new MemberAccess(device, SourceLocation.FromRuleContext(context), member);

        return newNode;
    }

    public override INodeExpression? VisitExtendedMemberAccess([NotNull] ExtendedMemberAccessContext context)
    {
        var targetIdxExpr = Visit(context.targetIdxExpr)!;

        var member = context.member?.Text;
        var device = context.identifier.Text;

        if (context.identifier.Type == BASE_DEVICE)
            device = "db";

        var target = GetDeviceTarget(context.prop.Type);

        var newNode = new MemberAccess(device, SourceLocation.FromRuleContext(context), target, targetIdxExpr, member);

        return newNode;
    }

    public override INodeExpression? VisitBatchAccess([NotNull] BatchAccessContext context)
    {
        var typeHash = Visit(context.deviceTypeHashExpr)!;

        var nameHash = context.deviceNameHashExpr is null
            ? null
            : Visit(context.deviceNameHashExpr)!;

        var targetIdx = context.targetIdxExpr is null
            ? null
            : Visit(context.targetIdxExpr)!;

        var deviceProperty = context.member.Text;
        var batchMode = context.batchMode.Text;

        var target = context.prop is null
            ? DeviceTarget.Device
            : GetDeviceTarget(context.prop.Type);

        var newNode = new BatchAccess(SourceLocation.FromRuleContext(context), typeHash, nameHash, targetIdx, target, deviceProperty, batchMode);

        return newNode;
    }

    public override INodeExpression? VisitWhileStatement([NotNull] WhileStatementContext context)
    {
        var innerCode = GetTreeFromBlockOrStatement(context.blockOrStatement());
        var expression = Visit(context.expression())!;

        var newNode = new While(SourceLocation.FromRuleContext(context), expression);
        AddToStatements(newNode);
        CurrentNode = newNode;
        Visit(innerCode);
        CurrentNode = newNode.Parent!;

        return null;
    }

    public override INodeExpression? VisitForStatement([NotNull] ForStatementContext context)
    {
        var newNode = new For(SourceLocation.FromRuleContext(context));
        AddToStatements(newNode);

        CurrentNode = newNode;

        var innerCode = GetTreeFromBlockOrStatement(context.blockOrStatement());

        if (context.statement1 is not null)
        {
            Visit(context.statement1);
            newNode.HasStatement1 = true;
        }

        if (context.expression() is not null)
        {
            var expression = Visit(context.expression())!;
            newNode.Expression = expression;
        }
        else
        {
            newNode.Expression = new Literal(newNode.SourceLocation, 1);
        }

        Visit(innerCode);

        if (context.statement2 is not null)
        {
            Visit(context.statement2);
            newNode.HasStatement2 = true;
        }

        CurrentNode = newNode.Parent!;

        return null;
    }

    public override INodeExpression? VisitAssignment([NotNull] AssignmentContext context)
    {
        var expression = Visit(context.expression())!;
        var variableName = context.IDENTIFIER().GetText();

        var newNode = new VariableAssignment(variableName, SourceLocation.FromTerminalNode(context.IDENTIFIER()), expression);
        AddToStatements(newNode);

        return null;
    }

    public override INodeExpression VisitNullaryOp([NotNull] NullaryOpContext context)
    {
        return new NullaryOperation(SourceLocation.FromRuleContext(context), context.op.Text);
    }

    public override INodeExpression VisitUnaryOp([NotNull] UnaryOpContext context)
    {
        var operand = Visit(context.operand)!;

        var newNode = new UnaryOperation(SourceLocation.FromRuleContext(context), operand, context.op.Text);
        return newNode;
    }

    public override INodeExpression VisitBinaryOp([NotNull] BinaryOpContext context)
    {
        var operand1 = Visit(context.left)!;
        var operand2 = Visit(context.right)!;

        var newNode = new BinaryOperation(SourceLocation.FromRuleContext(context), operand1, operand2, context.op.Text);

        return newNode;
    }

    public override INodeExpression VisitTernaryOp([NotNull] TernaryOpContext context)
    {
        var operandA = Visit(context.a)!;
        var operandB = Visit(context.b)!;
        var operandC = Visit(context.c)!;

        var newNode = new TernaryOperation(SourceLocation.FromRuleContext(context), operandA, operandB, operandC, context.op.Text);

        return newNode;
    }

    public override INodeExpression VisitIdentifier([NotNull] IdentifierContext context)
    {
        var name = context.IDENTIFIER().GetText();

        var newNode = new UserDefinedValueAccess(name, SourceLocation.FromTerminalNode(context.IDENTIFIER()));

        return newNode;
    }

    public override INodeExpression? VisitDeviceWithIdAssignment([NotNull] DeviceWithIdAssignmentContext context)
    {
        var deviceIdxExpr = Visit(context.deviceIdxExpr)!;
        var value = Visit(context.valueExpr)!;

        var deviceProperty = context.member.Text;

        var newNode = new DeviceWithIndexAssignment(SourceLocation.FromRuleContext(context), deviceIdxExpr, DeviceIndexType.Id, value, deviceProperty);
        AddToStatements(newNode);

        return null;
    }

    public override INodeExpression? VisitDeviceWithIdExtendedAssignment([NotNull] DeviceWithIdExtendedAssignmentContext context)
    {
        var deviceIdxExpr = Visit(context.deviceIdxExpr)!;
        var targetIdxExpr = Visit(context.targetIdxExpr)!;
        var value = Visit(context.valueExpr)!;

        var deviceProperty = context.member?.Text;

        var newNode = new DeviceWithIndexAssignment(SourceLocation.FromRuleContext(context), deviceIdxExpr, DeviceIndexType.Id, targetIdxExpr, value,
            GetDeviceTarget(context.prop.Type), deviceProperty);

        AddToStatements(newNode);

        return null;
    }

    public override INodeExpression? VisitBatchAssignment([NotNull] BatchAssignmentContext context)
    {
        var deviceTypeHash = Visit(context.deviceTypeHashExpr)!;

        var deviceNameHash = context.deviceNameHashExpr is null
            ? null
            : Visit(context.deviceNameHashExpr)!;

        var targetIdx = context.targetIdxExpr is null
            ? null
            : Visit(context.targetIdxExpr)!;

        var value = Visit(context.valueExpr)!;

        var deviceProperty = context.member.Text;

        var target = context.prop is null
            ? DeviceTarget.Device
            : GetDeviceTarget(context.prop.Type);
        SourceLocation.FromRuleContext(context);

        var newNode = new BatchAssignment(SourceLocation.FromRuleContext(context), deviceTypeHash, deviceNameHash, targetIdx, value, deviceProperty, target);

        AddToStatements(newNode);

        return null;
    }

    public override INodeExpression? VisitDeviceWithIndexAssignment([NotNull] DeviceWithIndexAssignmentContext context)
    {
        var deviceIdxExpr = Visit(context.deviceIdxExpr)!;
        var value = Visit(context.valueExpr)!;

        var deviceProperty = context.member.Text;

        var newNode = new DeviceWithIndexAssignment(SourceLocation.FromRuleContext(context), deviceIdxExpr, DeviceIndexType.Pin, value, deviceProperty);
        AddToStatements(newNode);

        return null;
    }

    public override INodeExpression? VisitDeviceWithIndexExtendedAssignment([NotNull] DeviceWithIndexExtendedAssignmentContext context)
    {
        var deviceIdxExpr = Visit(context.deviceIdxExpr)!;
        var targetIdxExpr = Visit(context.targetIdxExpr)!;
        var valueExpr = Visit(context.valueExpr)!;

        var deviceProperty = context.member?.Text;

        var newNode = new DeviceWithIndexAssignment(SourceLocation.FromRuleContext(context), deviceIdxExpr, DeviceIndexType.Pin, targetIdxExpr, valueExpr, GetDeviceTarget(context.prop.Type), deviceProperty);
        AddToStatements(newNode);

        return null;
    }

    public override INodeExpression? VisitDeviceIndexAccess([NotNull] DeviceIndexAccessContext context)
    {
        var member = context.member.Text;
        var deviceIdxExpr = Visit(context.deviceIdxExpr)!;

        var newNode = new DeviceWithIndexAccess(SourceLocation.FromRuleContext(context), deviceIdxExpr, DeviceIndexType.Pin, member);

        return newNode;
    }

    public override INodeExpression? VisitExtendedDeviceIndexAccess([NotNull] ExtendedDeviceIndexAccessContext context)
    {
        var member = context.member?.Text;
        var deviceIdxExpr = Visit(context.deviceIdxExpr)!;
        var targetIdxExpr = Visit(context.targetIdxExpr)!;

        var newNode = new DeviceWithIndexAccess(SourceLocation.FromRuleContext(context), deviceIdxExpr, DeviceIndexType.Pin, targetIdxExpr, GetDeviceTarget(context.prop.Type), member);

        return newNode;
    }

    public override INodeExpression VisitDeviceIdAccess([NotNull] DeviceIdAccessContext context)
    {
        var member = context.member.Text;

        var deviceIdExpr = Visit(context.expression())!;

        var newNode = new DeviceWithIndexAccess(SourceLocation.FromRuleContext(context), deviceIdExpr, DeviceIndexType.Id, member);

        return newNode;
    }

    public override INodeExpression VisitExtendedDeviceIdAccess([NotNull] ExtendedDeviceIdAccessContext context)
    {
        var member = context.member?.Text;
        var deviceIdxExpr = Visit(context.deviceIdxExpr)!;
        var targetIdxExpr = Visit(context.targetIdxExpr)!;

        var newNode = new DeviceWithIndexAccess(SourceLocation.FromRuleContext(context), deviceIdxExpr, DeviceIndexType.Id, targetIdxExpr, GetDeviceTarget(context.prop.Type), member);

        return newNode;
    }

    public override INodeExpression? VisitContinueStatement([NotNull] ContinueStatementContext context)
    {
        var newNode = new Continue(SourceLocation.FromRuleContext(context));
        AddToStatements(newNode);

        return null;
    }

    public override INodeExpression? VisitBreakStatement([NotNull] BreakStatementContext context)
    {
        var newNode = new Break(SourceLocation.FromRuleContext(context));
        AddToStatements(newNode);

        return null;
    }

    public override INodeExpression VisitFunctionCall([NotNull] FunctionCallContext context)
    {
        var name = context.IDENTIFIER().GetText();

        var paramExpressions = context.expression()
            .Select(e => Visit(e)!)
            .ToList();

        var newNode = new MethodCall(name, SourceLocation.FromTerminalNode(context.IDENTIFIER()), paramExpressions);

        return newNode;
    }

    public override INodeExpression? VisitFunctionCallStatement([NotNull] FunctionCallStatementContext context)
    {
        var name = context.IDENTIFIER().GetText();

        var paramExpressions = context.expression()
            .Select(e => Visit(e)!)
            .ToList();

        var newNode = new MethodCall(name, SourceLocation.FromTerminalNode(context.IDENTIFIER()), paramExpressions);
        AddToStatements(newNode);

        return null;
    }

    public override INodeExpression? VisitReturnStatement([NotNull] ReturnStatementContext context)
    {
        var newNode = new Return(SourceLocation.FromRuleContext(context));
        AddToStatements(newNode);

        return null;
    }

    public override INodeExpression? VisitReturnValueStatement([NotNull] ReturnValueStatementContext context)
    {
        var expression = Visit(context.expression())!;

        var newNode = new Return(SourceLocation.FromRuleContext(context), expression);
        AddToStatements(newNode);

        return null;
    }

    public override INodeExpression? VisitArraySizeDeclaration([NotNull] ArraySizeDeclarationContext context)
    {
        var sizeExpression = Visit(context.sizeExpr)!;

        var newNode = new ArrayDeclaration(context.IDENTIFIER().GetText(), SourceLocation.FromRuleContext(context), sizeExpression);
        AddToStatements(newNode);

        return null;
    }

    public override INodeExpression? VisitArrayListDeclaration([NotNull] ArrayListDeclarationContext context)
    {
        var elementExpressions = context.expression()
            .Select(ec => Visit(ec)!)
            .ToList();

        var newNode = new ArrayDeclaration(context.IDENTIFIER().GetText(), SourceLocation.FromRuleContext(context), elementExpressions);
        AddToStatements(newNode);

        return null;
    }

    public override INodeExpression? VisitArrayAssignment([NotNull] ArrayAssignmentContext context)
    {
        var indexExpr = Visit(context.indexExpr)!;
        var valueExpr = Visit(context.valueExpr)!;

        var newNode = new ArrayAssignment(context.IDENTIFIER().GetText(), SourceLocation.FromRuleContext(context), indexExpr, valueExpr);
        AddToStatements(newNode);

        return null;
    }

    public override INodeExpression? VisitArrayElementAccess([NotNull] ArrayElementAccessContext context)
    {
        var indexExpr = Visit(context.indexExpr)!;
        var newNode = new ArrayAccess(context.IDENTIFIER().GetText(), SourceLocation.FromRuleContext(context), indexExpr);

        return newNode;
    }

    public override INodeExpression VisitParenthesis([NotNull] ParenthesisContext context) =>
        Visit(context.expression())!;
    public override INodeExpression? VisitChildren(IRuleNode node) => base.VisitChildren(node);
    public override INodeExpression? VisitDelimitedStatement([NotNull] DelimitedStatementContext context) => base.VisitDelimitedStatement(context);
    public override INodeExpression? VisitErrorNode(IErrorNode node) => base.VisitErrorNode(node);
    public override INodeExpression? VisitStatement([NotNull] StatementContext context) => base.VisitStatement(context);
    public override INodeExpression? VisitTerminal(ITerminalNode node) => base.VisitTerminal(node);
    public override INodeExpression? VisitUndelimitedStatement([NotNull] UndelimitedStatementContext context) => base.VisitUndelimitedStatement(context);
    public override INodeExpression? VisitBlock([NotNull] BlockContext context) => base.VisitBlock(context);
    protected override INodeExpression? AggregateResult(INodeExpression? aggregate, INodeExpression? nextResult) => base.AggregateResult(aggregate, nextResult);

    private static DeviceTarget GetDeviceTarget(int propType) =>
        propType switch
        {
            SLOTS => DeviceTarget.Slots,
            REAGENTS => DeviceTarget.Reagents,
            STACK => DeviceTarget.Stack,
            _ => throw new Exception("Unecpected device target"),
        };
}
