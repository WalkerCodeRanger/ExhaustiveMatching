using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ExhaustiveMatching.Analyzer.Enums.Syntax
{
    public static class SwitchStatementSyntaxExtensions
    {
        public static SwitchSectionSyntax DefaultSection(this SwitchStatementSyntax switchStatement)
            => switchStatement.Sections.FirstOrDefault(s => s.Labels.OfType<DefaultSwitchLabelSyntax>().Any());
    }
}
