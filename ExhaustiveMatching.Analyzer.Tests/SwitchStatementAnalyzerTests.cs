using ExhaustiveMatching.Analyzer.Tests.Helpers;
using ExhaustiveMatching.Analyzer.Tests.Verifiers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ExhaustiveMatching.Analyzer.Tests
{
    [TestClass]
    public class SwitchStatementAnalyzerTests : CodeFixVerifier
    {
        [TestMethod]
        public void SwitchOnEnumThrowingInvalidEnumIsNotExhaustiveReportsDiagnostic()
        {
            const string args = "DayOfWeek dayOfWeek";
            const string test = @"
        switch(dayOfWeek)
        {
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
            default:
                throw new InvalidEnumArgumentException(nameof(dayOfWeek), (int)dayOfWeek, typeof(DayOfWeek));
        }";

            var expectedFriday = new DiagnosticResult
            {
                Id = "EM0001",
                Message = "Enum value not handled by switch: Friday",
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 10, 9, 6) }
            };

            var expectedSunday = new DiagnosticResult
            {
                Id = "EM0001",
                Message = "Enum value not handled by switch: Sunday",
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 10, 9, 6) }
            };

            VerifyCSharpDiagnostic(CodeContext(args, test), expectedFriday, expectedSunday);
        }

        [TestMethod]
        public void SwitchOnEnumThrowingExhaustiveMatchFailedIsNotExhaustiveReportsDiagnostic()
        {
            const string args = "DayOfWeek dayOfWeek";
            const string test = @"
        switch(dayOfWeek)
        {
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
            default:
                throw ExhaustiveMatch.Failed(dayOfWeek);
        }";

            var expectedFriday = new DiagnosticResult
            {
                Id = "EM0001",
                Message = "Enum value not handled by switch: Friday",
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 10, 9, 6) }
            };

            var expectedSunday = new DiagnosticResult
            {
                Id = "EM0001",
                Message = "Enum value not handled by switch: Sunday",
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 10, 9, 6) }
            };

            VerifyCSharpDiagnostic(CodeContext(args, test), expectedFriday, expectedSunday);
        }

        [TestMethod]
        public void SwitchOnClosedThrowingExhaustiveMatchFailedIsNotExhaustiveReportsDiagnostic()
        {
            const string args = "Shape shape";
            const string test = @"
        switch (shape)
        {
            case Square square:
                Console.WriteLine(""Square: "" + square);
                break;
            default:
                throw ExhaustiveMatch.Failed(shape);
        }";

            var expectedCircle = new DiagnosticResult
            {
                Id = "EM0002",
                Message = "Subtype not handled by switch: TestNamespace.Circle",
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 10, 9, 6) }
            };
            var expectedTriangle = new DiagnosticResult
            {
                Id = "EM0002",
                Message = "Subtype not handled by switch: TestNamespace.Triangle",
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 10, 9, 6) }
            };

            VerifyCSharpDiagnostic(CodeContext(args, test), expectedCircle, expectedTriangle);
        }

        [TestMethod]
        public void ExhaustiveObjectSwitchAllowsNull()
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

            VerifyCSharpDiagnostic(CodeContext(args, test));
        }

        [TestMethod]
        public void UnsupportedCaseClauses()
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
            case Triangle triangle when true:
                Console.WriteLine(""Triangle: "" + triangle);
                break;
            case 12:
                break;
            default:
                throw ExhaustiveMatch.Failed(shape);
        }";

            var expected1 = new DiagnosticResult
            {
                Id = "EM0100",
                Message = "When guard is not supported in an exhaustive switch",
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 18, 36, 9) }
            };

            var expected2 = new DiagnosticResult
            {
                Id = "EM0101",
                Message = "Case clause type not supported in exhaustive switch: 12",
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 21, 18, 2) }
            };

            VerifyCSharpDiagnostic(CodeContext(args, test), expected1, expected2);
        }

        [TestMethod]
        public void SwitchOnNonClosedType()
        {
            const string args = "object o";
            const string test = @"
        switch (o)
        {
            case string s:
                Console.WriteLine(""string: "" + s);
                break;
            case Triangle triangle when true:
                Console.WriteLine(""Triangle: "" + triangle);
                break;
            case 12:
                break;
            default:
                throw ExhaustiveMatch.Failed(shape);
        }";

            var expected1 = new DiagnosticResult
            {
                Id = "EM0102",
                Message = "Exhaustive switch must be on enum or closed type, was on: System.Object",
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 10, 17, 1) }
            };

            // Still reports these errors
            var expected2 = new DiagnosticResult
            {
                Id = "EM0100",
                Message = "When guard is not supported in an exhaustive switch",
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 15, 36, 9) }
            };

            var expected3 = new DiagnosticResult
            {
                Id = "EM0101",
                Message = "Case clause type not supported in exhaustive switch: 12",
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 18, 18, 2) }
            };

            VerifyCSharpDiagnostic(CodeContext(args, test), expected1, expected2, expected3);
        }

        [TestMethod]
        public void ErrorForMatchOnTypesOutsideOfHierarchy()
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
            case EquilateralTriangle equilateralTriangle:
                Console.WriteLine(""EquilateralTriangle: "" + equilateralTriangle);
                break;
            case Triangle triangle:
                Console.WriteLine(""Triangle: "" + triangle);
                break;
            case string s:
                Console.WriteLine(""string: "" + s);
                break;
            default:
                throw ExhaustiveMatch.Failed(shape);
        }";

            var expected1 = new DiagnosticResult
            {
                Id = "EM0103",
                Message = "TestNamespace.EquilateralTriangle is not a case type inheriting from type being matched: TestNamespace.Shape",
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 18, 18, 39) }
            };

            var expected2 = new DiagnosticResult
            {
                Id = "EM0103",
                Message = "System.String is not a case type inheriting from type being matched: TestNamespace.Shape",
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 24, 18, 8) }
            };

            VerifyCSharpDiagnostic(CodeContext(args, test), expected1, expected2);
        }


        [TestMethod]
        public void HandlesMultipleClosedAttributes()
        {
            const string test = @"using ExhaustiveMatching;
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
                case Square square:
                    Console.WriteLine(""Square: "" + square);
                    break;
                case Circle circle:
                    Console.WriteLine(""Circle: "" + circle);
                    break;
                case Triangle triangle:
                    Console.WriteLine(""Triangle: "" + triangle);
                    break;
                default:
                    throw ExhaustiveMatch.Failed(shape);
            }
        }
    }
}";

            VerifyCSharpDiagnostic(test);
        }

        /// <summary>
        /// Regression test for an issue where `typeof()` as a case type would
        /// cause all switches on that type to report a missing case with no
        /// type listed. (It was the error type.)
        /// </summary>
        [TestMethod]
        public void EmptyTypeofDoesNotCauseMissingCase()
        {
            const string test = @"using ExhaustiveMatching;
namespace TestNamespace
{
    [Closed(
        typeof(Square),
        typeof(),
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

            VerifyCSharpDiagnostic(test);
        }

        /// <summary>
        /// Regression test for an issue where using a closed interface type in
        /// the middle of a case hierarchy gave EM013 that the type was not a
        /// case type.
        /// </summary>
        [TestMethod]
        public void MultipleInterfaceLevels()
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
                case IKeywordToken _:
                    Console.WriteLine(""foreach"");
                    break;
                default:
                    throw ExhaustiveMatch.Failed(token);
            }
        }
    }
}";

            VerifyCSharpDiagnostic(test);
        }

        /// <summary>
        /// Regression test for infinite loop when a type listed itself as one
        /// of its case types.
        /// </summary>
        [TestMethod]
        public void SelfTypeAsCaseType()
        {
            const string test = @"using System;
using ExhaustiveMatching;
namespace TestNamespace
{
    [Closed(typeof(IToken), typeof(IKeywordToken)]
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
            var expected = new DiagnosticResult
            {
                Id = "EM0013",
                Message = "Closed type case is not a subtype: TestNamespace.IToken",
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 5, 20, 6) }
            };

            VerifyCSharpDiagnostic(test, expected);
        }

        private static string CodeContext(string args, string body)
        {
            const string context = @"using System; // DayOfWeek
using System.ComponentModel; // InvalidEnumArgumentException
using ExhaustiveMatching;
using TestNamespace;

class TestClass
{{
    void TestMethod({0})
    {{{1}
    }}
}}

namespace TestNamespace
{{
    [Closed(
        typeof(Square),
        typeof(Circle),
        typeof(Triangle))]
    public abstract class Shape {{ }}
    public class Square : Shape {{ }}
    public class Circle : Shape {{ }}
    public abstract class Triangle : Shape {{ }} // abstract to show abstract leaf types are checked
    public class EquilateralTriangle : Triangle {{ }}
    public class IsoscelesTriangle : Triangle {{ }}
}}";
            return string.Format(context, args, body);
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new ExhaustiveMatchAnalyzer();
        }
    }
}