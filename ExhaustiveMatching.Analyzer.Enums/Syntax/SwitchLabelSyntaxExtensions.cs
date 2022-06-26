using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ExhaustiveMatching.Analyzer.Enums.Syntax
{
    public static class SwitchLabelSyntaxExtensions
    {
        /// <summary>
        /// Whether this switch label is a traditional switch case or default
        /// case. That is, it is not a pattern matching case.
        /// </summary>
        public static bool IsTraditional(this SwitchLabelSyntax switchLabel)
        {
            switch (switchLabel)
            {
                case CaseSwitchLabelSyntax _:
                case DefaultSwitchLabelSyntax _:
                    return true;
                default:
                    return false;
            }
        }
    }
}
