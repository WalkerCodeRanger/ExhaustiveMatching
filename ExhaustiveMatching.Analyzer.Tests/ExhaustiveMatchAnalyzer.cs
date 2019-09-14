using ExhaustiveMatching.Analyzer.Tests.Verifiers;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ExhaustiveMatching.Analyzer.Tests
{
    public class ExhaustiveMatchAnalyzer : CodeFixVerifier
    {
        [TestMethod]
        public void EmptyFileReportsNoDiagnostics()
        {
            const string test = @"";

            VerifyCSharpDiagnostic(test);
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new Analyzer.ExhaustiveMatchAnalyzer();
        }
    }
}