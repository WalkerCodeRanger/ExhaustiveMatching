using System.Threading.Tasks;
using ExhaustiveMatching.Analyzer.Tests.Verifiers;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace ExhaustiveMatching.Analyzer.Tests
{
    public class ExhaustiveMatchAnalyzerTests : CodeFixVerifier
    {
        [Fact]
        public async Task EmptyFileReportsNoDiagnostics()
        {
            const string test = @"";

            await VerifyCSharpDiagnosticsAsync(test);
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new ExhaustiveMatchAnalyzer();
        }
    }
}
