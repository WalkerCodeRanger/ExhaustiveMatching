using System.Threading.Tasks;
using ExhaustiveMatching.Analyzer.Testing.Helpers;
using ExhaustiveMatching.Analyzer.Testing.Verifiers;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace ExhaustiveMatching.Analyzer.Enums.Tests
{
    public class SwitchStatementAnalyzerSwitchOnNullableEnumTests : DiagnosticVerifier
    {
        [Fact]
        public async Task NotExhaustiveReportsDiagnostic()
        {
            const string args = "CoinFlip? coinFlip";
            const string test = @"
        ◊1⟦switch⟧ (coinFlip)
        {
            default:
                throw new InvalidEnumArgumentException(nameof(coinFlip), (int)coinFlip, typeof(CoinFlip));
            case CoinFlip.Heads:
                Console.WriteLine(""Heads"");
                break;
        }";

            var source = CodeContext.CoinFlip(args, test);
            var expectedNull = DiagnosticResult
                               .Error("EM0002", "'null' value not handled by switch")
                               .AddLocation(source, 1);
            var expectedTails = DiagnosticResult
                                .Error("EM0001", "Enum value not handled by switch 'CoinFlip.Tails'")
                                .AddLocation(source, 1);

            await VerifyCSharpDiagnosticsAsync(source, expectedNull, expectedTails);
        }

        [Fact]
        public async Task ExhaustiveReportsNoDiagnostic()
        {
            const string args = "CoinFlip? coinFlip";
            const string test = @"
        ◊1⟦switch⟧ (coinFlip)
        {
            default:
                throw new InvalidEnumArgumentException(nameof(coinFlip), (int)coinFlip, typeof(CoinFlip));
            case CoinFlip.Heads:
                Console.WriteLine(""Heads"");
                break;
            case CoinFlip.Tails:
                Console.WriteLine(""Tails"");
                break;
            case null:
                    Console.WriteLine(""null"");
                break;
        }";

            await VerifyCSharpDiagnosticsAsync(CodeContext.CoinFlip(args, test));
        }

        [Fact]
        public async Task ExhaustiveWithNamedConstantExpressionReportsNoDiagnostic()
        {
            const string args = "CoinFlip? coinFlip";
            const string test = @"
        const CoinFlip NamedHeads = CoinFlip.Heads;
        const CoinFlip NamedTails = CoinFlip.Tails;

        ◊1⟦switch⟧ (coinFlip)
        {
            default:
                throw new InvalidEnumArgumentException(nameof(coinFlip), (int)coinFlip, typeof(CoinFlip));
            case NamedHeads:
                Console.WriteLine(""Heads"");
                break;
            case NamedTails:
                Console.WriteLine(""Tails"");
                break;
            case null:
                    Console.WriteLine(""null"");
                break;
        }";

            await VerifyCSharpDiagnosticsAsync(CodeContext.CoinFlip(args, test));
        }

        [Fact]
        public async Task ExhaustiveWithCastEnumValuesReportsNoDiagnostic()
        {
            const string args = "CoinFlip? coinFlip";
            const string test = @"
        ◊1⟦switch⟧ (coinFlip)
        {
            default:
                throw new InvalidEnumArgumentException(nameof(coinFlip), (int)coinFlip, typeof(CoinFlip));
            case (CoinFlip?)CoinFlip.Heads:
                Console.WriteLine(""Heads"");
                break;
            case (CoinFlip)CoinFlip.Tails:
                Console.WriteLine(""Tails"");
                break;
            case (CoinFlip?)null:
                    Console.WriteLine(""null"");
                break;
        }";

            await VerifyCSharpDiagnosticsAsync(CodeContext.CoinFlip(args, test));
        }

        [Fact]
        public async Task ExhaustiveWithCastIntValuesReportsNoDiagnostic()
        {
            const string args = "CoinFlip? coinFlip";
            const string test = @"
        ◊1⟦switch⟧ (coinFlip)
        {
            default:
                throw new InvalidEnumArgumentException(nameof(coinFlip), (int)coinFlip, typeof(CoinFlip));
            case (CoinFlip?)1:
                Console.WriteLine(""Heads"");
                break;
            case (CoinFlip)2:
                Console.WriteLine(""Tails"");
                break;
            case (CoinFlip?)null:
                    Console.WriteLine(""null"");
                break;
        }";

            await VerifyCSharpDiagnosticsAsync(CodeContext.CoinFlip(args, test));
        }

        [Fact]
        public async Task ExhaustiveWithIntValuesCastToOtherTypesReportsNoDiagnostic()
        {
            const string args = "CoinFlip? coinFlip";
            const string test = @"
        ◊1⟦switch⟧ (coinFlip)
        {
            default:
                throw new InvalidEnumArgumentException(nameof(coinFlip), (int)coinFlip, typeof(CoinFlip));
            case (CoinFlip?)(byte)1:
                Console.WriteLine(""Heads"");
                break;
            case (CoinFlip)(short)2:
                Console.WriteLine(""Tails"");
                break;
            case (CoinFlip?)null:
                    Console.WriteLine(""null"");
                break;
        }";

            await VerifyCSharpDiagnosticsAsync(CodeContext.CoinFlip(args, test));
        }

        [Fact]
        public async Task ExhaustiveWithArithmeticExpressionsReportsNoDiagnostic()
        {
            const string args = "CoinFlip? coinFlip";
            const string test = @"
        ◊1⟦switch⟧ (coinFlip)
        {
            default:
                throw new InvalidEnumArgumentException(nameof(coinFlip), (int)coinFlip, typeof(CoinFlip));
            case (CoinFlip)1:
                Console.WriteLine(""Heads"");
                break;
            case (CoinFlip)(1+1):
                Console.WriteLine(""Tails"");
                break;
            case null:
                    Console.WriteLine(""null"");
                break;
        }";

            await VerifyCSharpDiagnosticsAsync(CodeContext.CoinFlip(args, test));
        }

        [Fact]
        public async Task ExhaustiveWithExtraValuesReportsNoDiagnostic()
        {
            const string args = "CoinFlip? coinFlip";
            const string test = @"
        ◊1⟦switch⟧ (coinFlip)
        {
            default:
                throw new InvalidEnumArgumentException(nameof(coinFlip), (int)coinFlip, typeof(CoinFlip));
            case CoinFlip.Heads:
            case CoinFlip.Tails:
            case null:
                Console.WriteLine(""Expected"");
                break;
            case (CoinFlip)20:
            case (CoinFlip?)21:
                Console.WriteLine(""Unexpected"");
                break;
        }";

            await VerifyCSharpDiagnosticsAsync(CodeContext.CoinFlip(args, test));
        }

        [Fact]
        public async Task NotExhaustiveWithZeroCaseReportsDiagnostic()
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
            case (byte)0: // Numeric Value of Sunday (zero can be cast, not just literal)
            case DayOfWeek.Saturday:
                Console.WriteLine(""Weekend"");
                break;
            case null:
                Console.WriteLine(""null"");
                break;
        }";

            var source = CodeContext.CoinFlipByte(args, test);
            var expectedFriday = DiagnosticResult
                                 .Error("EM0001", "Enum value not handled by switch 'System.DayOfWeek.Friday'")
                                 .AddLocation(source, 1);

            await VerifyCSharpDiagnosticsAsync(source, expectedFriday);
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
            => new ExhaustiveMatchEnumAnalyzer();
    }
}
