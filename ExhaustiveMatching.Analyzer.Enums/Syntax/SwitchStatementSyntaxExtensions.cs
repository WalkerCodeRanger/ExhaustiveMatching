using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ExhaustiveMatching.Analyzer.Enums.Syntax
{
    public static class SwitchStatementSyntaxExtensions
    {
        public static SwitchSectionSyntax DefaultSection(this SwitchStatementSyntax switchStatement)
            => switchStatement.Sections.FirstOrDefault(s => s.Labels.OfType<DefaultSwitchLabelSyntax>().Any());

        public static IEnumerable<SwitchLabelSyntax> Labels(this SwitchStatementSyntax switchStatement)
            => switchStatement.Sections.SelectMany(s => s.Labels);

        public static IEnumerable<CaseSwitchLabelSyntax> CaseSwitchLabels(this SwitchStatementSyntax switchStatement)
            => switchStatement.Labels().OfType<CaseSwitchLabelSyntax>();
    }
}
