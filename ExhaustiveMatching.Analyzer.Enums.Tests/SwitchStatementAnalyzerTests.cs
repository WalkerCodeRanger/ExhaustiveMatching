using System.Threading.Tasks;
using ExhaustiveMatching.Analyzer.Testing.Helpers;
using ExhaustiveMatching.Analyzer.Testing.Verifiers;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace ExhaustiveMatching.Analyzer.Enums.Tests
{
    public class SwitchStatementAnalyzerTests : DiagnosticVerifier
    {
        [Fact]
        public async Task SwitchOnEnumIsNotExhaustiveReportsDiagnostic()
        {
            const string args = "DayOfWeek dayOfWeek";
            const string test = @"
        ◊1⟦switch⟧ (dayOfWeek)
        {
            default:
                throw new InvalidEnumArgumentException(nameof(dayOfWeek), (int)dayOfWeek, typeof(DayOfWeek));
            case DayOfWeek.Monday:
            case DayOfWeek.Tuesday:
            case DayOfWeek.Wednesday:
            case DayOfWeek.Thursday:
                // Omitted Friday
                Console.WriteLine(""Weekday"");
                break;
            case DayOfWeek.Saturday:
                // Omitted Sunday
                Console.WriteLine(""Weekend"");
                break;
        }";

            var source = CodeContext.Basic(args, test);
            var expectedFriday = DiagnosticResult.Error("EM0001", "Enum value not handled by switch: System.DayOfWeek.Friday")
                                                 .AddLocation(source, 1);
            var expectedSunday = DiagnosticResult.Error("EM0001", "Enum value not handled by switch: System.DayOfWeek.Sunday")
                                                 .AddLocation(source, 1);

            await VerifyCSharpDiagnosticsAsync(source, expectedFriday, expectedSunday);
        }


        [Fact]
        public async Task SwitchOnNullableEnumIsNotExhaustiveReportsDiagnostic()
        {
            const string args = "DayOfWeek? dayOfWeek";
            const string test = @"
        ◊1⟦switch⟧ (dayOfWeek)
        {
            default:
                throw new InvalidEnumArgumentException(nameof(dayOfWeek), (int)dayOfWeek, typeof(DayOfWeek));
            case DayOfWeek.Monday:
            case DayOfWeek.Tuesday:
            case DayOfWeek.Wednesday:
            case DayOfWeek.Thursday:
                // Omitted Friday
                Console.WriteLine(""Weekday"");
                break;
            case DayOfWeek.Saturday:
                // Omitted Sunday
                Console.WriteLine(""Weekend"");
                break;
        }";

            var source = CodeContext.Basic(args, test);
            var expectedNull = DiagnosticResult.Error("EM0002", "null value not handled by switch")
                                               .AddLocation(source, 1);
            var expectedFriday = DiagnosticResult.Error("EM0001", "Enum value not handled by switch: System.DayOfWeek.Friday")
                                                 .AddLocation(source, 1);
            var expectedSunday = DiagnosticResult.Error("EM0001", "Enum value not handled by switch: System.DayOfWeek.Sunday")
                                                 .AddLocation(source, 1);

            await VerifyCSharpDiagnosticsAsync(CodeContext.Basic(args, test), expectedNull, expectedFriday,
                expectedSunday);
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
            => new ExhaustiveMatchEnumAnalyzer();
    }
}
