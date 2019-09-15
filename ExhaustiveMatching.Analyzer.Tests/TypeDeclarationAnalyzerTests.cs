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
        public void SubtypeOfTypeClosedTypeMustBeMember()
        {
            const string test = @"using ExhaustiveMatching;
namespace TestNamespace
{
    [Closed(
        typeof(Square),
        typeof(Circle))]
    public abstract class Shape { }
    public sealed class Square : Shape { }
    public sealed class Circle : Shape { }
    public sealed class Triangle : Shape { }
}";

            var expected = new DiagnosticResult
            {
                Id = "EM011",
                Message =
                    "TestNamespace.Triangle is not a member of its closed supertype: TestNamespace.Shape",
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 10, 25) }
            };

            VerifyCSharpDiagnostic(test, expected);
        }

        [TestMethod]
        public void MemberTypesMustBeDirectSubtype()
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
    public sealed class Square : Shape { }
    public sealed class Circle : Shape { }
}";

            var expected = new DiagnosticResult
            {
                Id = "EM012",
                Message = "Closed type member is not a direct subtype: System.String",
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 9, 16, 6) }
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
    public sealed class Square : Shape { }
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