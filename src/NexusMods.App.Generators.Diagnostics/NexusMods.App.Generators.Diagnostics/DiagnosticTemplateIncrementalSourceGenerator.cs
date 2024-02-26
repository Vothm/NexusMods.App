using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NexusMods.App.Generators.Diagnostics.PostInitializationOutput;

namespace NexusMods.App.Generators.Diagnostics;

[Generator(LanguageNames.CSharp)]
public class DiagnosticTemplateIncrementalSourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext initContext)
    {
        initContext.RegisterPostInitializationOutput(postInitContext =>
        {
            postInitContext.AddSource(DiagnosticTemplateAttribute.HintName, DiagnosticTemplateAttribute.SourceCode);
        });

        var syntaxProvider = initContext.SyntaxProvider.ForAttributeWithMetadataName(
            fullyQualifiedMetadataName: $"{Constants.Namespace}.{DiagnosticTemplateAttribute.Name}",
            predicate: static (syntaxNode, _) => syntaxNode is VariableDeclaratorSyntax,
            transform: Transform
        );

        var compilationProvider = initContext.CompilationProvider;

        var provider = syntaxProvider.Combine(compilationProvider);

        initContext.RegisterSourceOutput(
            provider,
            static (ctx, tuple) => GenerateTarget(ctx, tuple.Left, tuple.Right)
        );
    }

    private static Target Transform(
        GeneratorAttributeSyntaxContext generatorAttributeSyntaxContext,
        CancellationToken cancellationToken)
    {
        var targetSymbol = generatorAttributeSyntaxContext.TargetSymbol;
        if (targetSymbol is not IFieldSymbol fieldSymbol) return new Target();

        var targetNode = generatorAttributeSyntaxContext.TargetNode;
        if (targetNode is not VariableDeclaratorSyntax variableDeclaratorSyntax) return new Target();

        return new Target(fieldSymbol, variableDeclaratorSyntax);
    }

    private static void GenerateTarget(SourceProductionContext context, Target target, Compilation compilation)
    {
        // ReSharper disable ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (target == default(Target) || target.FieldSymbol is null || target.VariableDeclaratorSyntax is null) return;
        // ReSharper restore ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract

        var (templateFieldSymbol, templateVariableDeclaratorSyntax) = target;
        var templateNamespace = templateFieldSymbol.ContainingNamespace.ToDisplayString(CustomSymbolDisplayFormats.NamespaceFormat);

        if (!ParseSyntax(compilation, templateVariableDeclaratorSyntax, out var parsedData)) return;

        var cw = new CodeWriter();

        // header
        cw.AppendLine(Constants.AutoGeneratedHeader);
        cw.AppendLine(Constants.NullableEnable);
        cw.AppendLine();

        // namespace
        cw.AppendLine($"namespace {templateNamespace};");
        cw.AppendLine();

        // containing symbols
        var containingSymbolStack = new Stack<INamedTypeSymbol>();
        var containingSymbol = templateFieldSymbol.ContainingSymbol;
        while (containingSymbol is not INamespaceSymbol && containingSymbol is INamedTypeSymbol containingTypeSymbol)
        {
            containingSymbolStack.Push(containingTypeSymbol);
            containingSymbol = containingSymbol.ContainingSymbol;
        }

        var codeBlockStackForContainingSymbols = new Stack<CodeWriter.CodeBlock>();
        while (containingSymbolStack.Count != 0)
        {
            var containingTypeSymbol = containingSymbolStack.Pop();
            cw.AppendLine($"partial {containingTypeSymbol.ToDisplayString(CustomSymbolDisplayFormats.ContainingSymbolFormat)}");
            codeBlockStackForContainingSymbols.Push(cw.AddBlock());
        }

        var diagnosticName = GetDiagnosticName(target.FieldSymbol);

        cw.Append($"internal static {Constants.DiagnosticsNamespace}.Diagnostic<{diagnosticName}MessageData> Create{diagnosticName}(");
        for (var i = 0; i < parsedData.ParsedMessageDataReferences.Count; i++)
        {
            if (i != 0) cw.Append(", ");
            var dataReference = parsedData.ParsedMessageDataReferences[i];
            cw.Append($"{dataReference.DataReferenceTypeSymbol.ToDisplayString(CustomSymbolDisplayFormats.GlobalFormat)} {dataReference.FieldName}");
        }
        cw.AppendLine(")");
        using (cw.AddBlock())
        {
            cw.Append($"var messageData = new {diagnosticName}MessageData(");
            for (var i = 0; i < parsedData.ParsedMessageDataReferences.Count; i++)
            {
                if (i != 0) cw.Append(", ");
                var dataReference = parsedData.ParsedMessageDataReferences[i];
                cw.Append($"{dataReference.FieldName}");
            }
            cw.AppendLine(");");

            cw.AppendLine();

            cw.AppendLine($"return new {Constants.DiagnosticsNamespace}.Diagnostic<{diagnosticName}MessageData>");
            using (cw.AddBlock())
            {
                cw.Append($"Id = new {Constants.DiagnosticsNamespace}.DiagnosticId(");
                var args = parsedData.IdCreationExpression.ArgumentList!.Arguments;
                if (args[0].Expression is LiteralExpressionSyntax literalSource)
                {
                    cw.Append($"source: \"{literalSource.Token.ValueText}\",");
                }

                if (args[1].Expression is LiteralExpressionSyntax literalNumber)
                {
                    cw.Append($"number: {literalNumber.Token.ValueText}");
                }
                cw.AppendLine("),");

                cw.AppendLine($"Severity = {Constants.DiagnosticsNamespace}.DiagnosticSeverity.{parsedData.SeverityName},");

                cw.Append($"Summary = {Constants.DiagnosticsNamespace}.DiagnosticMessage.From(");
                if (parsedData.SummaryTemplateExpression is LiteralExpressionSyntax literalSummary)
                {
                    cw.Append($"\"{literalSummary.Token.ValueText}\"");
                }
                cw.AppendLine("),");

                cw.Append($"Details = {Constants.DiagnosticsNamespace}.DiagnosticMessage.");
                if (parsedData.DetailsTemplateExpression is null)
                {
                    cw.AppendLine("DefaultValue,");
                } else if (parsedData.DetailsTemplateExpression is LiteralExpressionSyntax literalDetails)
                {
                    cw.AppendLine($"From(\"{literalDetails.Token.ValueText}\"),");
                }

                cw.AppendLine("MessageData = messageData,");
                cw.AppendLine($"DataReferences = new global::System.Collections.Generic.Dictionary<{Constants.DiagnosticsNamespace}.References.DataReferenceDescription, {Constants.DiagnosticsNamespace}.References.IDataReference>");
                using (cw.AddBlock())
                {
                    foreach (var dataReference in parsedData.ParsedMessageDataReferences)
                    {
                        cw.AppendLine($"{{ {Constants.DiagnosticsNamespace}.References.DataReferenceDescription.From(\"{dataReference.FieldName}\"), messageData.{dataReference.FieldName} }},");
                    }
                }

                cw.AppendLine(",");
            }

            cw.AppendLine(";");
        }

        cw.AppendLine($"internal readonly struct {diagnosticName}MessageData");
        using (cw.AddBlock())
        {
            foreach (var dataReference in parsedData.ParsedMessageDataReferences)
            {
                cw.AppendLine($"public readonly {dataReference.DataReferenceTypeSymbol.ToDisplayString(CustomSymbolDisplayFormats.GlobalFormat)} {dataReference.FieldName};");
            }

            cw.AppendLine();

            cw.Append($"public {diagnosticName}MessageData(");
            for (var i = 0; i < parsedData.ParsedMessageDataReferences.Count; i++)
            {
                if (i != 0) cw.Append(", ");
                var dataReference = parsedData.ParsedMessageDataReferences[i];
                cw.Append($"{dataReference.DataReferenceTypeSymbol.ToDisplayString(CustomSymbolDisplayFormats.GlobalFormat)} {dataReference.FieldName}");
            }
            cw.AppendLine(")");
            using (cw.AddBlock())
            {
                foreach (var dataReference in parsedData.ParsedMessageDataReferences)
                {
                    cw.AppendLine($"this.{dataReference.FieldName} = {dataReference.FieldName};");
                }
            }
        }

        while (codeBlockStackForContainingSymbols.Count != 0)
        {
            codeBlockStackForContainingSymbols.Pop().Dispose();
        }

        var hintName = templateFieldSymbol.ToDisplayString();
        context.AddSource($"{hintName}.g.cs", cw.ToString());
    }

    private static string GetDiagnosticName(ISymbol fieldSymbol)
    {
        var name = fieldSymbol.Name;
        if (!name.EndsWith("Template", StringComparison.Ordinal)) return name;
        return name.Remove(startIndex: name.Length - "Template".Length);
    }

    private static bool ParseSyntax(
        Compilation compilation,
        VariableDeclaratorSyntax templateVariableDeclaratorSyntax,
        out ParsedData parsedData)
    {
        const string finish = "Finish";
        const string withMessageData = "WithMessageData";
        const string withDetails = "WithDetails";
        const string withoutDetails = "WithoutDetails";
        const string withSummary = "WithSummary";
        const string withSeverity = "WithSeverity";
        const string withId = "WithId";
        const string start = "Start";

        parsedData = new ParsedData();
        var semanticModel = compilation.GetSemanticModel(templateVariableDeclaratorSyntax.SyntaxTree);

        var initializer = templateVariableDeclaratorSyntax.Initializer;

        // Finish
        if (!IsInvocationWithName(initializer?.Value, finish, out _, out var next)) return false;

        // WithMessageData
        if (!IsInvocationWithName(next, withMessageData, out var withMessageDataArguments, out next)) return false;
        if (withMessageDataArguments.Count != 1) return false;
        if (withMessageDataArguments[0].Expression is not SimpleLambdaExpressionSyntax messageBuilderExpression) return false;
        if (messageBuilderExpression.Body is not InvocationExpressionSyntax messageBuilderExpressionBody) return false;
        if (!ParseMessageBuilder(semanticModel, messageBuilderExpressionBody, out var parsedReferences)) return false;

        // WithDetails
        ExpressionSyntax? detailsTemplateExpression = null;
        if (IsInvocationWithName(next, withDetails, out var withDetailsArguments, out next))
        {
            if (withDetailsArguments.Count != 1) return false;
            detailsTemplateExpression = withDetailsArguments[0].Expression;
        } else if (IsInvocationWithName(next, withoutDetails, out var withoutDetailsArguments, out next))
        {
            if (withoutDetailsArguments.Count != 0) return false;
            detailsTemplateExpression = null;
        }

        // WithSummary
        if (!IsInvocationWithName(next, withSummary, out var withSummaryArguments, out next));
        if (withSummaryArguments.Count != 1) return false;
        var summaryTemplateExpression = withSummaryArguments[0].Expression;

        // WithSeverity
        if (!IsInvocationWithName(next, withSeverity, out var withSeverityArguments, out next)) return false;
        if (withSeverityArguments.Count != 1) return false;
        if (withSeverityArguments[0].Expression is not MemberAccessExpressionSyntax severityMemberAccess) return false;
        var severityName = severityMemberAccess.Name.Identifier.ToString();

        // WithId
        if (!IsInvocationWithName(next, withId, out var withIdArguments, out next)) return false;
        if (withIdArguments.Count != 1) return false;
        if (withIdArguments[0].Expression is not ObjectCreationExpressionSyntax idCreation) return false;
        var idCreationArguments = idCreation.ArgumentList?.Arguments;
        if (idCreationArguments is null || idCreationArguments.Value.Count != 2) return false;

        // Start
        if (!IsInvocationWithName(next, start, out _, out _)) return false;

        parsedData = new ParsedData(
            IdCreationExpression: idCreation,
            SeverityName: severityName,
            SummaryTemplateExpression: summaryTemplateExpression,
            DetailsTemplateExpression: detailsTemplateExpression,
            ParsedMessageDataReferences: parsedReferences
        );

        return true;

        static bool IsInvocationWithName(
            ExpressionSyntax? expression,
            string expectedName,
            out SeparatedSyntaxList<ArgumentSyntax> arguments,
            out ExpressionSyntax next)
        {
            arguments = [];
            next = expression!;

            if (expression is not InvocationExpressionSyntax invocationExpressionSyntax) return false;
            if (invocationExpressionSyntax.Expression is not MemberAccessExpressionSyntax memberAccessExpressionSyntax) return false;
            if (!memberAccessExpressionSyntax.Name.Identifier.ToString().Equals(expectedName)) return false;

            arguments = invocationExpressionSyntax.ArgumentList.Arguments;
            next = memberAccessExpressionSyntax.Expression;
            return true;
        }
    }

    private static bool ParseMessageBuilder(
        SemanticModel semanticModel,
        InvocationExpressionSyntax messageBuilderExpressionBody,
        out List<ParsedMessageBuilderDataReference> res)
    {
        const string addDataReference = "AddDataReference";
        res = [];

        var currentInvocation = messageBuilderExpressionBody;
        while (true)
        {
            if (currentInvocation.ArgumentList.Arguments.Count != 1) return false;
            if (currentInvocation.ArgumentList.Arguments[0].Expression is not LiteralExpressionSyntax literalExpressionSyntax) return false;
            var fieldName = literalExpressionSyntax.Token.ValueText;

            if (currentInvocation.Expression is not MemberAccessExpressionSyntax memberAccessExpression) return false;
            if (memberAccessExpression.Name is not GenericNameSyntax genericName) return false;
            if (!genericName.Identifier.ToString().Equals(addDataReference)) return false;
            if (genericName.TypeArgumentList.Arguments.Count != 1) return false;
            var typeArgument = genericName.TypeArgumentList.Arguments[0];

            var typeSymbol = semanticModel.GetSymbolInfo(typeArgument).Symbol;
            if (typeSymbol is not INamedTypeSymbol namedTypeSymbol) return false;

            res.Add(new ParsedMessageBuilderDataReference(namedTypeSymbol, fieldName));

            if (memberAccessExpression.Expression is InvocationExpressionSyntax tmp)
            {
                currentInvocation = tmp;
            }
            else
            {
                break;
            }
        }

        res.Reverse();
        return true;
    }

    private record struct ParsedMessageBuilderDataReference(INamedTypeSymbol DataReferenceTypeSymbol, string FieldName);

    private record struct ParsedData(
        ObjectCreationExpressionSyntax IdCreationExpression,
        string SeverityName,
        ExpressionSyntax SummaryTemplateExpression,
        ExpressionSyntax? DetailsTemplateExpression,
        List<ParsedMessageBuilderDataReference> ParsedMessageDataReferences
    );

    private record struct Target(IFieldSymbol FieldSymbol, VariableDeclaratorSyntax VariableDeclaratorSyntax);
}
