using System.Threading.Tasks;
using ExhaustiveMatching.Analyzer.Tests.Helpers;
using ExhaustiveMatching.Analyzer.Tests.Verifiers;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace ExhaustiveMatching.Analyzer.Tests
{
    public class SwitchStatementAnalyzerTests : CodeFixVerifier
    {
        [Fact]
        public async Task SwitchOnEnumThrowingInvalidEnumIsNotExhaustiveReportsDiagnostic()
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
                                 .Error("EM0001", "Enum value not handled by switch: Friday")
                                 .AddLocation(source, 1);
            var expectedSunday = DiagnosticResult
                                 .Error("EM0001", "Enum value not handled by switch: Sunday")
                                 .AddLocation(source, 1);

            await VerifyCSharpDiagnosticsAsync(source, expectedFriday, expectedSunday);
        }

        [Fact]
        public async Task SwitchOnEnumThrowingExhaustiveMatchFailedIsNotExhaustiveReportsDiagnostic()
        {
            const string args = "DayOfWeek dayOfWeek";
            const string test = @"
        ◊1⟦switch⟧ (dayOfWeek)
        {
            default:
                throw ExhaustiveMatch.Failed(dayOfWeek);
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
                                 .Error("EM0001", "Enum value not handled by switch: Friday")
                                 .AddLocation(source, 1);
            var expectedSunday = DiagnosticResult
                                 .Error("EM0001", "Enum value not handled by switch: Sunday")
                                 .AddLocation(source, 1);

            await VerifyCSharpDiagnosticsAsync(source, expectedFriday, expectedSunday);
        }


        [Fact]
        public async Task SwitchOnNullableEnum()
        {
            const string args = "DayOfWeek? dayOfWeek";
            const string test = @"
        ◊1⟦switch⟧ (dayOfWeek)
        {
            default:
                throw ExhaustiveMatch.Failed(dayOfWeek);
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
            var expectedNull = DiagnosticResult
                               .Error("EM0002", "null value not handled by switch")
                               .AddLocation(source, 1);
            var expectedFriday = DiagnosticResult
                                 .Error("EM0001", "Enum value not handled by switch: Friday")
                                 .AddLocation(source, 1);
            var expectedSunday = DiagnosticResult
                                 .Error("EM0001", "Enum value not handled by switch: Sunday")
                                 .AddLocation(source, 1);

            await VerifyCSharpDiagnosticsAsync(CodeContext.Basic(args, test), expectedNull, expectedFriday, expectedSunday);
        }


        [Fact]
        public async Task SwitchOnClosedThrowingExhaustiveMatchFailedIsNotExhaustiveReportsDiagnostic()
        {
            const string args = "Shape shape";
            const string test = @"
        ◊1⟦switch⟧ (shape)
        {
            case Square square:
                Console.WriteLine(""Square: "" + square);
                break;
            default:
                throw ExhaustiveMatch.Failed(shape);
        }";

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
        switch (shape)
        {
            case Square square:
                Console.WriteLine(""Square: "" + square);
                break;
            case Circle circle:
                Console.WriteLine(""Circle: "" + circle);
                break;
            case Triangle triangle:
                Console.WriteLine(""Triangle: "" + triangle);
                break;
            case null: // checking this is allowed
                Console.WriteLine(""null"");
                break;
            default:
                throw ExhaustiveMatch.Failed(shape);
        }";

            await VerifyCSharpDiagnosticsAsync(CodeContext.Shapes(args, test));
        }

        [Fact]
        public async Task UnsupportedCaseClauses()
        {
            const string args = "Shape shape";
            const string test = @"
        switch (shape)
        {
            case Square square:
                Console.WriteLine(""Square: "" + square);
                break;
            case Circle circle:
                Console.WriteLine(""Circle: "" + circle);
                break;
            case Triangle triangle ◊1⟦when true⟧:
                Console.WriteLine(""Triangle: "" + triangle);
                break;
            case ◊2⟦12⟧:
                break;
            default:
                throw ExhaustiveMatch.Failed(shape);
        }";

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
        switch (◊1⟦o⟧)
        {
            case string s:
                Console.WriteLine(""string: "" + s);
                break;
            case Triangle triangle ◊2⟦when true⟧:
                Console.WriteLine(""Triangle: "" + triangle);
                break;
            case ◊3⟦12⟧:
                break;
            default:
                throw ExhaustiveMatch.Failed(o);
        }";

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
        switch (shape)
        {
            case Square square:
                Console.WriteLine(""Square: "" + square);
                break;
            case Circle circle:
                Console.WriteLine(""Circle: "" + circle);
                break;
            case ◊1⟦EquilateralTriangle equilateralTriangle⟧:
                Console.WriteLine(""EquilateralTriangle: "" + equilateralTriangle);
                break;
            case Triangle triangle:
                Console.WriteLine(""Triangle: "" + triangle);
                break;
            case ◊2⟦string s⟧:
                Console.WriteLine(""string: "" + s);
                break;
            default:
                throw ExhaustiveMatch.Failed(shape);
        }";

            var source = CodeContext.Shapes(args, test);
            var expected1 = DiagnosticResult
                            .Error("EM0103", "TestNamespace.EquilateralTriangle is not a case type inheriting from type being matched: TestNamespace.Shape")
                            .AddLocation(source, 1);
            var compileError = DiagnosticResult
                               .Error("CS8121", "An expression of type 'Shape' cannot be handled by a pattern of type 'string'.")
                               .AddLocation(source, 2, length: 6);
            var expected2 = DiagnosticResult
                            .Error("EM0103", "System.String is not a case type inheriting from type being matched: TestNamespace.Shape")
                            .AddLocation(source, 2);

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
    [Closed(typeof(Triangle))]
    public abstract class Shape { }
    public class Square : Shape { }
    public class Circle : Shape { }
    public class Triangle : Shape { }

    class TestClass
    {
        void TestMethod(Shape shape)
        {
            switch (shape)
            {
                default:
                    throw ExhaustiveMatch.Failed(shape);
                case Square square:
                    Console.WriteLine(""Square: "" + square);
                    break;
                case Circle circle:
                    Console.WriteLine(""Circle: "" + circle);
                    break;
                case Triangle triangle:
                    Console.WriteLine(""Triangle: "" + triangle);
                    break;
            }
        }
    }
}";

            await VerifyCSharpDiagnosticsAsync(source);
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
            switch (shape)
            {
                case Square square:
                    Console.WriteLine(""Square: "" + square);
                    break;
                case Circle circle:
                    Console.WriteLine(""Circle: "" + circle);
                    break;
                default:
                    throw ExhaustiveMatch.Failed(shape);
            }
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
            switch (token)
            {
                default:
                    throw ExhaustiveMatch.Failed(token);
                case IKeywordToken _:
                    Console.WriteLine(""foreach"");
                    break;
            }
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
            const string source = @"using System;
using ExhaustiveMatching;

namespace TestNamespace
{
    [Closed(typeof(◊1⟦IToken⟧), typeof(IKeywordToken))]
    public interface IToken { }
    public interface IKeywordToken : IToken { }

    class TestClass
    {
        void TestMethod(IToken token)
        {
            switch (token)
            {
                case IKeywordToken _:
                    Console.WriteLine(""foreach"");
                    break;
                default:
                    throw ExhaustiveMatch.Failed(token);
            }
        }
    }
}";
            var expected = DiagnosticResult
                           .Error("EM0013", "Closed type case is not a subtype: TestNamespace.IToken")
                           .AddLocation(source, 1);

            await VerifyCSharpDiagnosticsAsync(source, expected);
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new ExhaustiveMatchAnalyzer();
        }
    }
}
