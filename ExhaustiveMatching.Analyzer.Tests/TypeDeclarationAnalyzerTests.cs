using ExhaustiveMatching.Analyzer.Tests.Helpers;
using ExhaustiveMatching.Analyzer.Tests.Verifiers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ExhaustiveMatching.Analyzer.Tests
{
    [TestClass]
    public class TypeDeclarationAnalyzerTests : CodeFixVerifier
    {
        [TestMethod]
        public void SubtypeOfClosedTypeMustBeCase()
        {
            const string test = @"using ExhaustiveMatching;
namespace TestNamespace
{
    [Closed(
        typeof(Square),
        typeof(Circle))]
    public abstract class Shape { }
    public class Square : Shape { }
    public class Circle : Shape { }
    public class Triangle : Shape { }
}";

            var expected = new DiagnosticResult
            {
                Id = "EM011",
                Message = "TestNamespace.Triangle is not a case of its closed supertype: TestNamespace.Shape",
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 10, 18, 8) }
            };

            VerifyCSharpDiagnostic(test, expected);
        }

        [TestMethod]
        public void CaseTypeMustBeSubtype()
        {
            const string test = @"using ExhaustiveMatching;
using System;

namespace TestNamespace
{
    [Closed(
        typeof(Square),
        typeof(Circle),
        typeof(String))]
    public abstract class Shape { }
    public class Square : Shape { }
    public class Circle : Shape { }
}";

            var expected = new DiagnosticResult
            {
                Id = "EM013",
                Message = "Closed type case is not a subtype: System.String",
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 9, 16, 6) }
            };

            VerifyCSharpDiagnostic(test, expected);
        }

        [TestMethod]
        public void CaseTypeMustBeDirectSubtype()
        {
            const string test = @"using ExhaustiveMatching;
using System;

namespace TestNamespace
{
    [Closed(
        typeof(Rectangle),
        typeof(Square))]
    public abstract class Shape { }
    public class Rectangle : Shape { }
    public class Square : Rectangle { }
}";

            var expected = new DiagnosticResult
            {
                Id = "EM012",
                Message = "Closed type case is not a direct subtype: TestNamespace.Square",
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 8, 16, 6) }
            };

            VerifyCSharpDiagnostic(test, expected);
        }

        [TestMethod]
        public void SingleCaseTypeSupported()
        {
            const string test = @"using ExhaustiveMatching;
using System;

namespace TestNamespace
{
    [Closed(typeof(Square))]
    public abstract class Shape { }
    public class Square : Shape { }
}";

            VerifyCSharpDiagnostic(test);
        }

        /// <summary>
        /// Previous versions of the analyzer would throw an exception when encountering
        /// invalid arguments to <see cref="ClosedAttribute"/>.
        /// </summary>
        [TestMethod]
        public void EmptyTypeofArgumentToClosedAttributeHandled()
        {
            const string test = @"using ExhaustiveMatching;
using System;

namespace TestNamespace
{
    [Closed(typeof())]
    public abstract class Shape { }
}";

            VerifyCSharpDiagnostic(test);
        }

        [TestMethod]
        public void PrimitiveArgumentToClosedAttributeHandled()
        {
            const string test = @"using ExhaustiveMatching;
using System;

namespace TestNamespace
{
    [Closed(5)]
    public abstract class Shape { }
}";

            VerifyCSharpDiagnostic(test);
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new Analyzer.ExhaustiveMatchAnalyzer();
        }
    }
}