using System.Threading.Tasks;
using ExhaustiveMatching.Analyzer.Testing.Helpers;
using ExhaustiveMatching.Analyzer.Testing.Verifiers;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace ExhaustiveMatching.Analyzer.Tests
{
    public class SwitchExpressionAnalyzerTests : DiagnosticVerifier
    {
        [Fact]
        public async Task SwitchOnEnumThrowingInvalidEnumIsNotExhaustiveReportsDiagnostic()
        {
            const string args = "CoinFlip coinFlip";
            const string test = @"
        var result = coinFlip ◊1⟦switch⟧
        {
            CoinFlip.Heads => ""Heads!"",
            _ => throw new InvalidEnumArgumentException(nameof(coinFlip), (int)coinFlip, typeof(CoinFlip)),
        };";

            var source = CodeContext.CoinFlip(args, test);
            var expectedTails = DiagnosticResult.Error("EM0001", "Enum value not handled by switch: CoinFlip.Tails")
                                                .AddLocation(source, 1);

            await VerifyCSharpDiagnosticsAsync(source, expectedTails);
        }

        [Fact]
        public async Task SwitchOnEnumThrowingExhaustiveMatchFailedIsNotExhaustiveReportsDiagnostic()
        {
            const string args = "CoinFlip coinFlip";
            const string test = @"
        var result = coinFlip ◊1⟦switch⟧
        {
            CoinFlip.Heads => ""Heads!"",
            _ => throw ExhaustiveMatch.Failed(coinFlip),
        };";

            var source = CodeContext.CoinFlip(args, test);
            var expectedTails = DiagnosticResult.Error("EM0001", "Enum value not handled by switch: CoinFlip.Tails")
                                                .AddLocation(source, 1);

            await VerifyCSharpDiagnosticsAsync(source, expectedTails);
        }


        [Fact]
        public async Task SwitchOnNullableEnum()
        {
            const string args = "CoinFlip? coinFlip";
            const string test = @"
        var result = coinFlip ◊1⟦switch⟧
        {
            CoinFlip.Heads => ""Heads!"",
            _ => throw ExhaustiveMatch.Failed(coinFlip),
        };";

            var source = CodeContext.CoinFlip(args, test);
            var expectedNull = DiagnosticResult.Error("EM0002", "null value not handled by switch")
                                               .AddLocation(source, 1);
            var expectedTails = DiagnosticResult.Error("EM0001", "Enum value not handled by switch: CoinFlip.Tails")
                                                .AddLocation(source, 1);

            await VerifyCSharpDiagnosticsAsync(source, expectedNull, expectedTails);
        }


        [Fact]
        public async Task SwitchOnClosedThrowingExhaustiveMatchFailedIsNotExhaustiveReportsDiagnostic()
        {
            const string args = "Shape shape";
            const string test = @"
        var result = shape ◊1⟦switch⟧
        {
            Square square => ""Square: "" + square,
            _ => throw ExhaustiveMatch.Failed(shape)
        };";

            var source = CodeContext.Shapes(args, test);
            var expectedCircle = DiagnosticResult
                                 .Error("EM0003", "Subtype not handled by switch: TestNamespace.Circle")
                                 .AddLocation(source, 1);
            var expectedTriangle = DiagnosticResult
                                   .Error("EM0003", "Subtype not handled by switch: TestNamespace.Triangle")
                                   .AddLocation(source, 1);

            await VerifyCSharpDiagnosticsAsync(source, expectedCircle, expectedTriangle);
        }

        [Fact]
        public async Task ExhaustiveObjectSwitchAllowsNull()
        {
            const string args = "Shape shape";
            const string test = @"
        var result = shape switch
        {
            Square square => ""Square: "" + square,
            Circle circle => ""Circle: "" + circle,
            Triangle triangle => ""Triangle: "" + triangle,
            null => ""null"",
            _ => throw ExhaustiveMatch.Failed(shape),
        };";

            await VerifyCSharpDiagnosticsAsync(CodeContext.Shapes(args, test));
        }

        [Fact]
        public async Task ExhaustiveObjectSwitchAllowsLiteralTypeExpressionSyntax()
        {
            const string args = "Shape shape";
            const string test = @"
        var result = shape switch
        {
            Square square => ""Square: "" + square,
            Circle circle => ""Circle: "" + circle,
            Triangle => ""Triangle!"", // type name
            _ => throw ExhaustiveMatch.Failed(shape),
        };";

            await VerifyCSharpDiagnosticsAsync(CodeContext.Shapes(args, test));
        }

        [Fact]
        public async Task UnsupportedCaseClauses()
        {
            const string args = "Shape shape";
            const string test = @"
        var result = shape switch
        {
            Square square => ""Square: "" + square,
            Circle circle => ""Circle: "" + circle,
            Triangle triangle ◊1⟦when true⟧ => ""Triangle: "" + triangle,
            ◊2⟦12⟧ => null,
            _  => throw ExhaustiveMatch.Failed(shape),
        };";

            var source = CodeContext.Shapes(args, test);
            var expected1 = DiagnosticResult
                            .Error("EM0100", "When guard is not supported in an exhaustive switch")
                            .AddLocation(source, 1);
            var compileError = DiagnosticResult
                               .Error("CS0029", "Cannot implicitly convert type 'int' to 'TestNamespace.Shape'")
                               .AddLocation(source, 2);
            var expected2 = DiagnosticResult
                            .Error("EM0101", "Case pattern not supported in exhaustive switch: 12")
                            .AddLocation(source, 2);

            await VerifyCSharpDiagnosticsAsync(source, expected1, compileError, expected2);
        }

        [Fact]
        public async Task SwitchOnNonClosedType()
        {
            const string args = "object o";
            const string test = @"
        var result = ◊1⟦o⟧ switch
        {
            string s => ""string: "" + s,
            Triangle triangle ◊2⟦when true⟧ => ""Triangle: "" + triangle,
            ◊3⟦12⟧ => null,
            _ => throw ExhaustiveMatch.Failed(o),
        };";

            var source = CodeContext.Shapes(args, test);
            var expected1 = DiagnosticResult
                            .Error("EM0102", "Exhaustive switch must be on enum or closed type, was on: System.Object")
                            .AddLocation(source, 1);

            // Still reports these errors
            var expected2 = DiagnosticResult
                            .Error("EM0100", "When guard is not supported in an exhaustive switch")
                            .AddLocation(source, 2);
            var expected3 = DiagnosticResult
                            .Error("EM0101", "Case pattern not supported in exhaustive switch: 12")
                            .AddLocation(source, 3);

            await VerifyCSharpDiagnosticsAsync(source, expected1, expected2, expected3);
        }

        [Fact]
        public async Task ErrorForMatchOnTypesOutsideOfHierarchy()
        {
            const string args = "Shape shape";
            const string test = @"
        var result = shape switch
        {
            Square square => ""Square: "" + square,
            Circle circle => ""Circle: "" + circle,
            ◊1⟦EquilateralTriangle equilateralTriangle⟧ => ""EquilateralTriangle: "" + equilateralTriangle,
            Triangle triangle => ""Triangle: "" + triangle,
            ◊3⟦◊2⟦string⟧ s⟧ => ""string: "" + s,
            _ => throw ExhaustiveMatch.Failed(shape),
        };";

            var source = CodeContext.Shapes(args, test);
            var expected1 = DiagnosticResult
                            .Error("EM0103", "TestNamespace.EquilateralTriangle is not a case type inheriting from type being matched: TestNamespace.Shape")
                            .AddLocation(source, 1);
            var compileError = DiagnosticResult
                               .Error("CS8121", "An expression of type 'Shape' cannot be handled by a pattern of type 'string'.")
                               .AddLocation(source, 2);
            var expected2 = DiagnosticResult
                            .Error("EM0103", "System.String is not a case type inheriting from type being matched: TestNamespace.Shape")
                            .AddLocation(source, 3);

            await VerifyCSharpDiagnosticsAsync(source, expected1, compileError, expected2);
        }


        [Fact]
        public async Task HandlesMultipleClosedAttributes()
        {
            const string source = @"using System;
using ExhaustiveMatching;

namespace TestNamespace
{
    [Closed(
        typeof(Square),
        typeof(Circle))]
    [◊1⟦Closed(typeof(Triangle))⟧]
    public abstract class Shape { }
    public class Square : Shape { }
    public class Circle : Shape { }
    public class Triangle : Shape { }

    class TestClass
    {
        void TestMethod(Shape shape)
        {
            var result = shape switch
            {
                Square square => ""Square: "" + square,
                Circle circle => ""Circle: "" + circle,
                Triangle triangle => ""Triangle: "" + triangle,
                _ => throw ExhaustiveMatch.Failed(shape),
            };
        }
    }
}";
            var expected1 = DiagnosticResult
                            .Error("EM0104", "Duplicate 'Closed' attribute")
                            .AddLocation(source, 1);

            await VerifyCSharpDiagnosticsAsync(source, expected1);
        }

        /// <summary>
        /// Regression test for an issue where `typeof()` as a case type would
        /// cause all switches on that type to report a missing case with no
        /// type listed. (It was the error type.)
        /// </summary>
        [Fact]
        public async Task EmptyTypeofDoesNotCauseMissingCase()
        {
            const string source = @"using System;
using ExhaustiveMatching;

namespace TestNamespace
{
    [Closed(
        typeof(Square),
        typeof(◊1⟦⟧),
        typeof(Circle))]
    public abstract class Shape { }
    public class Square : Shape { }
    public class Circle : Shape { }

    class TestClass
    {
        void TestMethod(Shape shape)
        {
            _ = shape switch
            {
                Square square => ""Square: "" + square,
                Circle circle => ""Circle: "" + circle,
                _ => throw ExhaustiveMatch.Failed(shape),
            };
        }
    }
}";

            var compileError = DiagnosticResult
                               .Error("CS1031", "Type expected")
                               .AddLocation(source, 1);

            await VerifyCSharpDiagnosticsAsync(source, compileError);
        }

        /// <summary>
        /// Regression test for an issue where using a closed interface type in
        /// the middle of a case hierarchy gave EM0013 that the type was not a
        /// case type.
        /// </summary>
        [Fact]
        public async Task MultipleInterfaceLevels()
        {
            const string test = @"using System;
using ExhaustiveMatching;
namespace TestNamespace
{
    [Closed(typeof(IKeywordToken))]
    public interface IToken { }
    [Closed(typeof(IForeachKeyword))]
    public interface IKeywordToken : IToken { }
    public interface IForeachKeyword : IKeywordToken { }

    class TestClass
    {
        void TestMethod(IToken token)
        {
            _ = token switch
            {
                IKeywordToken _ => ""foreach"",
                _ => throw ExhaustiveMatch.Failed(token),
            };
        }
    }
}";

            await VerifyCSharpDiagnosticsAsync(test);
        }

        /// <summary>
        /// Regression test for infinite loop when a type listed itself as one
        /// of its case types and was used in a switch.
        /// </summary>
        [Fact]
        public async Task SelfTypeAsCaseType()
        {
            const string source = @"using ExhaustiveMatching;

namespace TestNamespace
{
    [Closed(typeof(◊1⟦IToken⟧), typeof(IKeywordToken))]
    public interface IToken { }
    public interface IKeywordToken : IToken { }


    class TestClass
    {
        void TestMethod(IToken token)
        {
            _ = token switch
            {
                IKeywordToken _ => ""foreach"",
                _ => throw ExhaustiveMatch.Failed(token),
            };
        }
    }
}";
            var expected = DiagnosticResult
                           .Error("EM0013", "Closed type case is not a subtype: TestNamespace.IToken")
                           .AddLocation(source, 1);

            await VerifyCSharpDiagnosticsAsync(source, expected);
        }

        [Fact]
        public async Task SwitchOnStructurallyClosedThrowingExhaustiveMatchFailedIsNotExhaustiveReportsDiagnostic()
        {
            const string args = "Result<string, string> result";
            const string test = @"
        var x = result ◊1⟦switch⟧
        {
            Result<string, string>.Error error => ""Error: "" + error,
            _ => throw ExhaustiveMatch.Failed(result),
        };";

            var source = CodeContext.Result(args, test);
            var expectedSuccess = DiagnosticResult
                .Error("EM0003", "Subtype not handled by switch: TestNamespace.Success")
                .AddLocation(source, 1);

            await VerifyCSharpDiagnosticsAsync(source, expectedSuccess);
        }

        [Fact]
        public async Task SwitchOnStructurallyClosedThrowingExhaustiveMatchDoesNotReportsDiagnostic()
        {
            const string args = "Result<string, string> result";
            const string test = @"
        var x = result ◊1⟦switch⟧
        {
            Result<string, string>.Error error => ""Error: "" + error,
            Result<string, string>.Success success => ""Success: "" + success,
            _ => throw ExhaustiveMatch.Failed(result),
        };";

            var source = CodeContext.Result(args, test);

            await VerifyCSharpDiagnosticsAsync(source);
        }

        [Fact]
        public async Task SwitchOnStructurallyClosedWithLabelThrowingExhaustiveMatchDoesNotReportsDiagnostic()
        {
            const string args = "Result<string, string> result";
            const string test = @"
        var x = result ◊1⟦switch⟧
        {
            Result<string, string>.Error error => ""Error: "" + error,
            Result<string, string>.Success => ""Success!"",
            _ => throw ExhaustiveMatch.Failed(result),
        };";

            var source = CodeContext.Result(args, test);

            await VerifyCSharpDiagnosticsAsync(source);
        }

        [Fact]
        public async Task SwitchOnStructurallyClosedThrowingExhaustiveMatchAllowNull()
        {
            const string args = "Result<string, string> result";
            const string test = @"
        var x = result ◊1⟦switch⟧
        {
            Result<string, string>.Error error => ""Error: "" + error,
            Result<string, string>.Success success => ""Success: "" + success,
            null => ""null"",
            _ => throw ExhaustiveMatch.Failed(result),
        };";

            var source = CodeContext.Result(args, test);

            await VerifyCSharpDiagnosticsAsync(source);
        }

        [Fact]
        public async Task SwitchOnStructurallyClosedRecordThrowingExhaustiveMatchFailedIsNotExhaustiveReportsDiagnostic()
        {
            const string args = "Result<string, string> result";
            const string test = @"
        var x = result ◊1⟦switch⟧
        {
            Result<string, string>.Error error => ""Error: "" + error,
            _ => throw ExhaustiveMatch.Failed(result),
        };";

            var source = CodeContext.ResultRecord(args, test);
            var expectedSuccess = DiagnosticResult
                .Error("EM0003", "Subtype not handled by switch: TestNamespace.Success")
                .AddLocation(source, 1);

            await VerifyCSharpDiagnosticsAsync(source, expectedSuccess);
        }

        [Fact]
        public async Task SwitchOnStructurallyClosedRecordThrowingExhaustiveMatchDoesNotReportsDiagnostic()
        {
            const string args = "Result<string, string> result";
            const string test = @"
        var x = result ◊1⟦switch⟧
        {
            Result<string, string>.Error error => ""Error: "" + error,
            Result<string, string>.Success success => ""Success: "" + success,
            _ => throw ExhaustiveMatch.Failed(result),
        };";

            var source = CodeContext.ResultRecord(args, test);

            await VerifyCSharpDiagnosticsAsync(source);
        }

        [Fact]
        public async Task SwitchOnStructurallyClosedRecordThrowingExhaustiveMatchAllowNull()
        {
            const string args = "Result<string, string> result";
            const string test = @"
        var x = result ◊1⟦switch⟧
        {
            Result<string, string>.Error error => ""Error: "" + error,
            Result<string, string>.Success success => ""Success: "" + success,
            null => ""null"",
            _ => throw ExhaustiveMatch.Failed(result),
        };";

            var source = CodeContext.ResultRecord(args, test);

            await VerifyCSharpDiagnosticsAsync(source);
        }

        [Fact]
        public async Task SwitchOnStruct()
        {
            const string args = "HashCode hashCode";
            const string test = @"
        _ = ◊1⟦hashCode⟧ switch
        {
            HashCode code => ""Hashcode: "" + code,
            ◊2⟦_⟧ => throw ExhaustiveMatch.Failed(hashCode),
        };";

            var source = CodeContext.Basic(args, test);
            var expected = DiagnosticResult
                           .Error("EM0102", "Exhaustive switch must be on enum or closed type, was on: System.HashCode")
                           .AddLocation(source, 1);
            var compileError = DiagnosticResult
                               .Error("CS8510", "The pattern is unreachable. It has already been handled by a previous arm of the switch expression or it is impossible to match.")
                               .AddLocation(source, 2);

            await VerifyCSharpDiagnosticsAsync(source, expected, compileError);
        }

        [Fact]
        public async Task SwitchOnNullableStruct()
        {
            const string args = "HashCode? hashCode";
            const string test = @"
        _ = ◊1⟦hashCode⟧ switch
        {
            null => ""null"",
            _ => throw ExhaustiveMatch.Failed(hashCode),
        };";

            var source = CodeContext.Basic(args, test);
            var expected = DiagnosticResult
                           .Error("EM0102", "Exhaustive switch must be on enum or closed type, was on: System.Nullable")
                           .AddLocation(source, 1);

            await VerifyCSharpDiagnosticsAsync(source, expected);
        }

        [Fact]
        public async Task SwitchOnTuple()
        {
            const string args = "Shape shape1, Shape shape2";
            const string test = @"
        _ = ◊1⟦(shape1, shape2)⟧ switch
        {
            ◊2⟦(Square square1, Square square2)⟧ => ""squares"",
            _ => throw ExhaustiveMatch.Failed((shape1, shape2)),
        };";

            var source = CodeContext.Shapes(args, test);
            var expected1 = DiagnosticResult
                            .Error("EM0102", "Exhaustive switch must be on enum or closed type, was on: System.ValueTuple")
                            .AddLocation(source, 1);
            var expected2 = DiagnosticResult
                            .Error("EM0101",
                                "Case pattern not supported in exhaustive switch: (Square square1, Square square2)")
                            .AddLocation(source, 2);

            await VerifyCSharpDiagnosticsAsync(source, expected1, expected2);
        }

        [Fact]
        public async Task SwitchOnNullableTuple()
        {
            const string args = "Shape shape1, Shape shape2";
            const string test = @"
        (Shape, Shape)? value = (shape1, shape2);
        _ = ◊1⟦value⟧ switch
        {
            ◊2⟦(Square square1, Square square2)⟧ => ""squares"",
            _ => throw ExhaustiveMatch.Failed(value),
        };";

            var source = CodeContext.Shapes(args, test);
            // TODO type name is bad
            var expected1 = DiagnosticResult
                            .Error("EM0102", "Exhaustive switch must be on enum or closed type, was on: System.Nullable")
                            .AddLocation(source, 1);
            var expected2 = DiagnosticResult
                            .Error("EM0101",
                                "Case pattern not supported in exhaustive switch: (Square square1, Square square2)")
                            .AddLocation(source, 2);

            await VerifyCSharpDiagnosticsAsync(source, expected1, expected2);
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
            => new ExhaustiveMatchAnalyzer();
    }
}
