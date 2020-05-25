using System.Threading.Tasks;
using ExhaustiveMatching.Analyzer.Tests.Helpers;
using ExhaustiveMatching.Analyzer.Tests.Verifiers;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace ExhaustiveMatching.Analyzer.Tests
{
    public class TypeDeclarationAnalyzerTests : CodeFixVerifier
    {
        [Fact]
        public async Task ConcreteSubtypeOfClosedTypeMustBeCase()
        {
            const string source = @"using ExhaustiveMatching;
namespace TestNamespace
{
    [Closed(
        typeof(Square),
        typeof(Circle))]
    public abstract class Shape { }
    public class Square : Shape { }
    public class Circle : Shape { }
    public class ◊1⟦Triangle⟧ : Shape { }
}";

            var expected = DiagnosticResult
                           .Error("EM0011", "TestNamespace.Triangle is not a case of its closed supertype: TestNamespace.Shape")
                           .AddLocation(source, 1);

            await VerifyCSharpDiagnosticsAsync(source, expected);
        }

        [Fact]
        public async Task OpenInterfaceSubtypeOfClosedTypeMustBeCase()
        {
            const string source = @"using ExhaustiveMatching;
namespace TestNamespace
{
    [Closed(
        typeof(ISquare),
        typeof(ICircle))]
    public interface IShape { }
    public interface ISquare : IShape { }
    public interface ICircle : IShape { }
    public interface ◊1⟦ITriangle⟧ : IShape { }
}";

            var expected = DiagnosticResult
                           .Error("EM0015", "Open interface TestNamespace.ITriangle is not a case of its closed supertype: TestNamespace.IShape")
                           .AddLocation(source, 1);

            await VerifyCSharpDiagnosticsAsync(source, expected);
        }

        [Fact]
        public async Task CaseTypeMustBeSubtype()
        {
            const string source = @"using ExhaustiveMatching;
using System;

namespace TestNamespace
{
    [Closed(
        typeof(Square),
        typeof(Circle),
        typeof(◊1⟦String⟧))]
    public abstract class Shape { }
    public class Square : Shape { }
    public class Circle : Shape { }
}";

            var expected = DiagnosticResult
                           .Error("EM0013", "Closed type case is not a subtype: System.String")
                           .AddLocation(source, 1);

            await VerifyCSharpDiagnosticsAsync(source, expected);
        }

        [Fact]
        public async Task CaseTypeMustBeDirectSubtype()
        {
            const string source = @"using ExhaustiveMatching;
using System;

namespace TestNamespace
{
    [Closed(
        typeof(Rectangle),
        typeof(◊1⟦Square⟧))]
    public abstract class Shape { }
    public class Rectangle : Shape { }
    public class Square : Rectangle { }
}";

            var expected = DiagnosticResult
                           .Error("EM0012", "Closed type case is not a direct subtype: TestNamespace.Square")
                           .AddLocation(source, 1);

            await VerifyCSharpDiagnosticsAsync(source, expected);
        }

        [Fact]
        public async Task SingleCaseTypeSupported()
        {
            const string test = @"using System;
using ExhaustiveMatching;

namespace TestNamespace
{
    [Closed(typeof(Square))]
    public abstract class Shape { }
    public class Square : Shape { }
}";

            await VerifyCSharpDiagnosticsAsync(test);
        }

        [Fact(Skip = "Check not yet implemented")]
        public async Task CaseTypeMustBeUnique()
        {
            const string source = @"using ExhaustiveMatching;
using System;

namespace TestNamespace
{
    [Closed(typeof(Square), typeof(◊1⟦Square⟧))]
    public abstract class Shape { }
    public class Square : Shape { }
}";

            var expected = DiagnosticResult
                .Error("EM0012", "Case type must be unique")
                .AddLocation(source, 1);

            await VerifyCSharpDiagnosticsAsync(source, expected);
        }

        /// <summary>
        /// Previous versions of the analyzer would throw an exception when encountering
        /// invalid arguments to <see cref="ClosedAttribute"/>.
        /// </summary>
        [Fact]
        public async Task EmptyTypeofArgumentToClosedAttributeHandled()
        {
            const string source = @"using ExhaustiveMatching;
using System;

namespace TestNamespace
{
    [Closed(typeof(◊1⟦⟧))]
    public abstract class Shape { }
}";

            var compileError = DiagnosticResult
                               .Error("CS1031", "Type expected")
                               .AddLocation(source, 1);

            await VerifyCSharpDiagnosticsAsync(source, compileError);
        }

        [Fact]
        public async Task PrimitiveArgumentToClosedAttributeHandled()
        {
            const string source = @"using ExhaustiveMatching;
using System;

namespace TestNamespace
{
    [Closed(◊1⟦5⟧)]
    public abstract class Shape { }
}";
            var compileError = DiagnosticResult
                               .Error("CS1503", "Argument 1: cannot convert from 'int' to 'System.Type'")
                               .AddLocation(source, 1);

            await VerifyCSharpDiagnosticsAsync(source, compileError);
        }

        [Fact]
        public async Task MultiLevelHierarchy()
        {
            const string source = @"using ExhaustiveMatching;
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

            await VerifyCSharpDiagnosticsAsync(source);
        }

        [Fact]
        public async Task MultiLevelHierarchyWithConcreteInteriorTypes()
        {
            const string source = @"using ExhaustiveMatching;
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

            await VerifyCSharpDiagnosticsAsync(source);
        }

        [Fact]
        public async Task MirrorHierarchy()
        {
            const string source = @"using ExhaustiveMatching;
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

            await VerifyCSharpDiagnosticsAsync(source);
        }

        [Fact]
        public async Task MirrorHierarchyMustBeCovered()
        {
            const string source = @"using ExhaustiveMatching;
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
    public class ◊1⟦Circle⟧ : Shape { }
}";

            var expected = DiagnosticResult
                           .Error("EM0014", "An exhaustive match on TestNamespace.IShape might not cover the type TestNamespace.Circle. It must be a subtype of a leaf type of the case type tree.")
                           .AddLocation(source, 1);

            await VerifyCSharpDiagnosticsAsync(source, expected);
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new ExhaustiveMatchAnalyzer();
        }
    }
}
