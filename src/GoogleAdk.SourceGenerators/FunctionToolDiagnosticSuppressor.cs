using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace GoogleAdk.SourceGenerators;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class FunctionToolDiagnosticSuppressor : DiagnosticSuppressor
{
    private static readonly SuppressionDescriptor CS8321Suppression = new SuppressionDescriptor(
        id: "ADKSuppressCS8321",
        suppressedDiagnosticId: "CS8321",
        justification: "Method is used as a marker by the FunctionTool source generator");

    private static readonly SuppressionDescriptor IDE0051Suppression = new SuppressionDescriptor(
        id: "ADKSuppressIDE0051",
        suppressedDiagnosticId: "IDE0051",
        justification: "Method is used as a marker by the FunctionTool source generator");

    private static readonly SuppressionDescriptor CA1811Suppression = new SuppressionDescriptor(
        id: "ADKSuppressCA1811",
        suppressedDiagnosticId: "CA1811",
        justification: "Method is used as a marker by the FunctionTool source generator");

    public override ImmutableArray<SuppressionDescriptor> SupportedSuppressions => 
        ImmutableArray.Create(CS8321Suppression, IDE0051Suppression, CA1811Suppression);

    public override void ReportSuppressions(SuppressionAnalysisContext context)
    {
        foreach (var diagnostic in context.ReportedDiagnostics)
        {
            var location = diagnostic.Location;
            if (location.SourceTree == null)
                continue;

            var root = location.SourceTree.GetRoot(context.CancellationToken);
            var token = root.FindToken(location.SourceSpan.Start);
            var node = token.Parent;

            if (node == null) continue;

            var methodOrLocal = node.FirstAncestorOrSelf<SyntaxNode>(n => 
                n is MethodDeclarationSyntax || n is LocalFunctionStatementSyntax);

            if (methodOrLocal != null)
            {
                SyntaxList<AttributeListSyntax> attributeLists = default;
                if (methodOrLocal is MethodDeclarationSyntax md)
                    attributeLists = md.AttributeLists;
                else if (methodOrLocal is LocalFunctionStatementSyntax lf)
                    attributeLists = lf.AttributeLists;

                bool hasToolAttribute = false;
                foreach (var attrList in attributeLists)
                {
                    foreach (var attr in attrList.Attributes)
                    {
                        var name = attr.Name.ToString();
                        if (name.Contains("FunctionTool"))
                        {
                            hasToolAttribute = true;
                            break;
                        }
                    }
                }

                if (hasToolAttribute)
                {
                    var descriptor = diagnostic.Id switch
                    {
                        "CS8321" => CS8321Suppression,
                        "IDE0051" => IDE0051Suppression,
                        "CA1811" => CA1811Suppression,
                        _ => null
                    };

                    if (descriptor != null)
                    {
                        context.ReportSuppression(Suppression.Create(descriptor, diagnostic));
                    }
                }
            }
        }
    }

    private static bool HasFunctionToolAttribute(SyntaxList<AttributeListSyntax> attributeLists)
    {
        foreach (var attrList in attributeLists)
        {
            foreach (var attr in attrList.Attributes)
            {
                var name = attr.Name.ToString();
                if (name == "FunctionTool" || 
                    name == "FunctionToolAttribute" || 
                    name.EndsWith(".FunctionTool") || 
                    name.EndsWith(".FunctionToolAttribute"))
                {
                    return true;
                }
            }
        }
        return false;
    }
}
