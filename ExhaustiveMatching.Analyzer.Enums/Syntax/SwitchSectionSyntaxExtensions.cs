using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ExhaustiveMatching.Analyzer.Enums.Syntax
{
    public static class SwitchSectionSyntaxExtensions
    {
        public static ThrowStatementSyntax FirstThrowStatement(this SwitchSectionSyntax switchSection)
            => switchSection.Statements.OfType<ThrowStatementSyntax>().FirstOrDefault();
    }
}
