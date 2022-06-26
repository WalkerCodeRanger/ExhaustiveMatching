using System.Threading.Tasks;
using ExhaustiveMatching.Analyzer.Tests.Verifiers;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace ExhaustiveMatching.Analyzer.Enums.Tests
{
    public class ExhaustiveMatchEnumAnalyzerTests : DiagnosticVerifier
    {
        [Fact]
        public async Task EmptyFileReportsNoDiagnostics()
        {
            const string test = @"";

            await VerifyCSharpDiagnosticsAsync(test);
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new ExhaustiveMatchEnumAnalyzer();
        }
    }
}
