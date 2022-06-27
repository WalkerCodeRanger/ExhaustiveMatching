using System.Threading.Tasks;
using ExhaustiveMatching.Analyzer.Testing.Helpers;
using ExhaustiveMatching.Analyzer.Testing.Verifiers;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace ExhaustiveMatching.Analyzer.Enums.Tests
{
    public class SwitchStatementAnalyzerSwitchOnEnumTests : DiagnosticVerifier
    {
        [Fact]
        public async Task NotExhaustiveReportsDiagnostic()
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
            var expectedFriday = DiagnosticResult
                                 .Error("EM0001", "Enum value not handled by switch 'System.DayOfWeek.Friday'")
                                 .AddLocation(source, 1);
            var expectedSunday = DiagnosticResult
                                 .Error("EM0001", "Enum value not handled by switch 'System.DayOfWeek.Sunday'")
                                 .AddLocation(source, 1);

            await VerifyCSharpDiagnosticsAsync(source, expectedFriday, expectedSunday);
        }

        [Fact]
        public async Task NotExhaustiveWithPatternCaseReportsDiagnostic()
        {
            const string args = "CoinFlip coinFlip";
            const string test = @"
        ◊1⟦switch⟧ (coinFlip)
        {
            default:
                throw new InvalidEnumArgumentException(nameof(coinFlip), (int)coinFlip, typeof(CoinFlip));
            case CoinFlip.Heads:
                Console.WriteLine(""Heads"");
                break;
            ◊2⟦case CoinFlip flip:⟧
                Console.WriteLine(flip);
                break;
        }";

            var source = CodeContext.CoinFlip(args, test);
            var expectedTails = DiagnosticResult
                                .Error("EM0001", "Enum value not handled by switch 'CoinFlip.Tails'")
                                .AddLocation(source, 1);
            var casePattern = DiagnosticResult
                              .Error("EM0101", "Case pattern not supported in exhaustive switch on enum type 'case CoinFlip flip:'")
                              .AddLocation(source, 2);

            await VerifyCSharpDiagnosticsAsync(source, expectedTails, casePattern);
        }

        [Fact]
        public async Task NotExhaustiveWithCastNumericCaseReportsDiagnostic()
        {
            const string args = "CoinFlip coinFlip";
            const string test = @"
        ◊1⟦switch⟧ (coinFlip)
        {
            default:
                throw new InvalidEnumArgumentException(nameof(coinFlip), (int)coinFlip, typeof(CoinFlip));
            case (CoinFlip)1:
                Console.WriteLine(""Heads"");
                break;
        }";

            var source = CodeContext.CoinFlipByte(args, test);
            var expectedTails = DiagnosticResult.Error("EM0001", "Enum value not handled by switch 'CoinFlip.Tails'")
                                                .AddLocation(source, 1);

            await VerifyCSharpDiagnosticsAsync(source, expectedTails);
        }

        [Fact]
        public async Task NotExhaustiveWithCastCaseReportsDiagnostic()
        {
            const string args = "CoinFlip coinFlip";
            const string test = @"
        ◊1⟦switch⟧ (coinFlip)
        {
            default:
                throw new InvalidEnumArgumentException(nameof(coinFlip), (int)coinFlip, typeof(CoinFlip));
            case (CoinFlip)(int)CoinFlip.Heads:
                Console.WriteLine(""Heads"");
                break;
        }";

            var source = CodeContext.CoinFlipByte(args, test);
            var expectedTails = DiagnosticResult.Error("EM0001", "Enum value not handled by switch 'CoinFlip.Tails'")
                                                .AddLocation(source, 1);

            await VerifyCSharpDiagnosticsAsync(source, expectedTails);
        }

        [Fact]
        public async Task NotExhaustiveWithZeroCaseReportsDiagnostic()
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
            case (byte)0: // Numeric Value of Sunday (zero can be cast, not just literal)
            case DayOfWeek.Saturday:
                Console.WriteLine(""Weekend"");
                break;
        }";

            var source = CodeContext.CoinFlipByte(args, test);
            var expectedFriday = DiagnosticResult
                                 .Error("EM0001", "Enum value not handled by switch 'System.DayOfWeek.Friday'")
                                 .AddLocation(source, 1);

            await VerifyCSharpDiagnosticsAsync(source, expectedFriday);
        }

        [Fact]
        public async Task ExhaustiveWithStringCaseReportsNoDiagnostic()
        {
            const string args = "CoinFlip coinFlip";
            const string test = @"
        switch (coinFlip)
        {
            default:
                throw new InvalidEnumArgumentException(nameof(coinFlip), (int)coinFlip, typeof(CoinFlip));
            case CoinFlip.Heads:
            case CoinFlip.Tails:
                Console.WriteLine(""Expected"");
                break;
            case ◊1⟦""Testing""⟧:
                Console.WriteLine(""Unexpected"");
                break;
        }";

            var source = CodeContext.CoinFlip(args, test);
            var cannotConvert = DiagnosticResult.Error("CS0029", "Cannot implicitly convert type 'string' to 'CoinFlip'")
                                                .AddLocation(source, 1);

            await VerifyCSharpDiagnosticsAsync(source, cannotConvert);
        }

        [Fact]
        public async Task ExhaustiveWithOutOfRangeCaseReportsNoDiagnostic()
        {
            const string args = "CoinFlip coinFlip";
            const string test = @"
        switch (coinFlip)
        {
            default:
                throw new InvalidEnumArgumentException(nameof(coinFlip), (int)coinFlip, typeof(CoinFlip));
            case CoinFlip.Heads:
            case CoinFlip.Tails:
                Console.WriteLine(""Expected"");
                break;
            case ◊1⟦1_000⟧:
                Console.WriteLine(""Unexpected"");
                break;
        }";

            var source = CodeContext.CoinFlipByte(args, test);
            var cannotConvert = DiagnosticResult.Error("CS0266", "Cannot implicitly convert type 'int' to 'CoinFlip'. An explicit conversion exists (are you missing a cast?)")
                                                .AddLocation(source, 1);

            await VerifyCSharpDiagnosticsAsync(source, cannotConvert);
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
            => new ExhaustiveMatchEnumAnalyzer();
    }
}
