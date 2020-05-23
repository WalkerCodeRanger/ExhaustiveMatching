using ExhaustiveMatching.Analyzer.Tests.Verifiers;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace ExhaustiveMatching.Analyzer.Tests
{
    public class ExhaustiveMatchAnalyzerTests : CodeFixVerifier
    {
        [Fact]
        public void EmptyFileReportsNoDiagnostics()
        {
            const string test = @"";

            VerifyCSharpDiagnostic(test);
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new ExhaustiveMatchAnalyzer();
        }
    }
}
