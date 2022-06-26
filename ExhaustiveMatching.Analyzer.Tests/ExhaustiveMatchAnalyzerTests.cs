using System.Threading.Tasks;
using ExhaustiveMatching.Analyzer.Testing.Verifiers;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace ExhaustiveMatching.Analyzer.Tests
{
    public class ExhaustiveMatchAnalyzerTests : DiagnosticVerifier
    {
        [Fact]
        public async Task EmptyFileReportsNoDiagnostics()
        {
            const string test = @"";

            await VerifyCSharpDiagnosticsAsync(test);
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
            => new ExhaustiveMatchAnalyzer();
    }
}
