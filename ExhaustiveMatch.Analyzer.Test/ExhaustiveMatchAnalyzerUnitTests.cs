using System;
using ExhaustiveMatching.Analyzer.Tests.Helpers;
using ExhaustiveMatching.Analyzer.Tests.Verifiers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ExhaustiveMatching.Analyzer.Tests
{
    [TestClass]
    public class UnitTest : CodeFixVerifier
    {
        [TestMethod]
        public void EmptyFileReportsNoDiagnostics()
        {
            const string test = @"";

            VerifyCSharpDiagnostic(test);
        }

        //Diagnostic and CodeFix both triggered and checked for
        //[TestMethod]
        public void TestMethod2()
        {
            const string test = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        class TypeName
        {   
        }
    }";
            var expected = new DiagnosticResult
            {
                Id = "ExhaustiveMatchAnalyzer",
                Message = String.Format("Type name '{0}' contains lowercase letters", "TypeName"),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 11, 15)
                        }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        class TYPENAME
        {   
        }
    }";
            VerifyCSharpFix(test, fixtest);
        }

        [TestMethod]
        public void EnumSwitchThrowInvalidEnumIsNotExhaustiveReportsDiagnostic()
        {
            const string args = "DayOfWeek dayOfWeek";
            const string test = @"
		switch(dayOfWeek)
		{
			case DayOfWeek.Monday:
			case DayOfWeek.Tuesday:
			case DayOfWeek.Wednesday:
			case DayOfWeek.Thursday:
			case DayOfWeek.Friday:
				Console.WriteLine(""Weekday"");
			break;
			case DayOfWeek.Saturday:
				// Omitted Sunday
				Console.WriteLine(""Weekend"");
				break;
			default:
				throw new InvalidEnumArgumentException(nameof(dayOfWeek), (int)dayOfWeek, typeof(DayOfWeek));
		}";

            var expected = new DiagnosticResult
            {
                Id = "EM001",
                Message = "Missing cases:\nDayOfWeek.Sunday",
                Severity = DiagnosticSeverity.Error,
                Locations =
                    new[] {
                        new DiagnosticResultLocation("Test0.cs", 10, 3)
                    }
            };

            VerifyCSharpDiagnostic(CodeContext(args, test), expected);
        }

        [TestMethod]
        public void EnumSwitchThrowExhaustiveMatchFailedIsNotExhaustiveReportsDiagnostic()
        {
            const string args = "DayOfWeek dayOfWeek";
            const string test = @"
		switch(dayOfWeek)
		{
			case DayOfWeek.Monday:
			case DayOfWeek.Tuesday:
			case DayOfWeek.Wednesday:
			case DayOfWeek.Thursday:
			case DayOfWeek.Friday:
				Console.WriteLine(""Weekday"");
			break;
			case DayOfWeek.Saturday:
				// Omitted Sunday
				Console.WriteLine(""Weekend"");
				break;
			default:
				throw ExhaustiveMatch.Failed(dayOfWeek);
		}";

            var expected = new DiagnosticResult
            {
                Id = "EM001",
                Message = "Missing cases:\nDayOfWeek.Sunday",
                Severity = DiagnosticSeverity.Error,
                Locations =
                    new[] {
                        new DiagnosticResultLocation("Test0.cs", 10, 3)
                    }
            };

            VerifyCSharpDiagnostic(CodeContext(args, test), expected);
        }

        [TestMethod]
        public void ObjectSwitchThrowExhaustiveMatchFailedIsNotExhaustiveReportsDiagnostic()
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
			default:
				throw ExhaustiveMatch.Failed(shape);
		}";

            var expected = new DiagnosticResult
            {
                Id = "EM001",
                Message = "Missing cases:\nDayOfWeek.Sunday",
                Severity = DiagnosticSeverity.Error,
                Locations =
                    new[] {
                        new DiagnosticResultLocation("Test0.cs", 9, 3)
                    }
            };

            VerifyCSharpDiagnostic(CodeContext(args, test), expected);
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
	abstract class Shape {{ }}
	class Square : Shape {{ }}
	class Circle : Shape {{ }}
	class Triangle : Shape {{ }}
}}";
            return string.Format(context, args, body);
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new ExhaustiveMatchAnalyzerCodeFixProvider();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new ExhaustiveMatchAnalyzer();
        }
    }
}