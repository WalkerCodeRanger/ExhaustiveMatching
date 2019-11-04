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
        public void ConcreteSubtypeOfClosedTypeMustBeCase()
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
                Id = "EM0011",
                Message = "TestNamespace.Triangle is not a case of its closed supertype: TestNamespace.Shape",
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 10, 18, 8) }
            };

            VerifyCSharpDiagnostic(test, expected);
        }

        [TestMethod]
        public void OpenInterfaceSubtypeOfClosedTypeMustBeCase()
        {
            const string test = @"using ExhaustiveMatching;
namespace TestNamespace
{
    [Closed(
        typeof(ISquare),
        typeof(ICircle))]
    public interface IShape { }
    public interface ISquare : IShape { }
    public interface ICircle : IShape { }
    public interface ITriangle : IShape { }
}";

            var expected = new DiagnosticResult
            {
                Id = "EM0015",
                Message = "Open interface TestNamespace.ITriangle is not a case of its closed supertype: TestNamespace.IShape",
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 10, 22, 9) }
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
                Id = "EM0013",
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
                Id = "EM0012",
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

        [TestMethod, Ignore("Check not yet implemented")]
        public void CaseTypeMustBeUnique()
        {
            const string test = @"using ExhaustiveMatching;
using System;

namespace TestNamespace
{
    [Closed(typeof(Square), typeof(Square))]
    public abstract class Shape { }
    public class Square : Shape { }
}";

            var expected = new DiagnosticResult
            {
                Id = "EM0012",
                Message = "Closed type case is not a direct subtype: TestNamespace.Square",
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 8, 16, 6) }
            };

            VerifyCSharpDiagnostic(test, expected);
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

        [TestMethod]
        public void MultiLevelHierarchy()
        {
            const string test = @"using ExhaustiveMatching;
namespace TestNamespace
{
    [Closed(
        typeof(Square),
        typeof(Circle),
        typeof(Triangle))]
    public abstract class Shape { }
    public class Square : Shape { }
    public class Circle : Shape { }

    [Closed(
        typeof(EquilateralTriangle),
        typeof(IsoscelesTriangle))]
    public abstract class Triangle : Shape { }
    public class EquilateralTriangle : Triangle { }
    public class IsoscelesTriangle : Triangle { }
}";

            VerifyCSharpDiagnostic(test);
        }

        [TestMethod]
        public void MultiLevelHierarchyWithConcreteInteriorTypes()
        {
            const string test = @"using ExhaustiveMatching;
namespace TestNamespace
{
    [Closed(
        typeof(Square),
        typeof(Circle),
        typeof(Triangle))]
    public abstract class Shape { }
    public class Square : Shape { }
    public class Circle : Shape { }

    [Closed(
        typeof(EquilateralTriangle),
        typeof(IsoscelesTriangle))]
    public class Triangle : Shape { } // important part is that triangle is concrete
    public class EquilateralTriangle : Triangle { }
    public class IsoscelesTriangle : Triangle { }
}";

            VerifyCSharpDiagnostic(test);
        }

        [TestMethod]
        public void MirrorHierarchy()
        {
            const string test = @"using ExhaustiveMatching;
namespace TestNamespace
{
    [Closed(
        typeof(ISquare),
        typeof(ICircle))]
    public interface IShape { }
    public interface ISquare : IShape { }
    public interface ICircle : IShape { }

    [Closed(
        typeof(Square),
        typeof(Circle))]
    public abstract class Shape : IShape { }
    public class Square : Shape, ISquare { }
    public class Circle : Shape, ICircle { }
}";

            VerifyCSharpDiagnostic(test);
        }

        [TestMethod]
        public void MirrorHierarchyMustBeCovered()
        {
            const string test = @"using ExhaustiveMatching;
namespace TestNamespace
{
    [Closed(
        typeof(ISquare),
        typeof(ICircle))]
    public interface IShape { }
    public interface ISquare : IShape { }
    public interface ICircle : IShape { }

    [Closed(
        typeof(Square),
        typeof(Circle))]
    public abstract class Shape : IShape { }
    public class Square : Shape, ISquare { }
    public class Circle : Shape { }
}";

            var expected = new DiagnosticResult
            {
                Id = "EM0014",
                Message = "An exhaustive match on TestNamespace.IShape might not cover the type TestNamespace.Circle. It must be a subtype of a leaf type of the case type tree.",
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 16, 18, 6) }
            };

            VerifyCSharpDiagnostic(test, expected);
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new ExhaustiveMatchAnalyzer();
        }
    }
}