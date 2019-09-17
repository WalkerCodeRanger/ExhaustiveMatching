using ExhaustiveMatching.Analyzer.Tests.Verifiers;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ExhaustiveMatching.Analyzer.Tests
{
    [TestClass]
    public class MultipleAttributesTests : CodeFixVerifier
    {
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

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new Analyzer.ExhaustiveMatchAnalyzer();
        }
    }
}
