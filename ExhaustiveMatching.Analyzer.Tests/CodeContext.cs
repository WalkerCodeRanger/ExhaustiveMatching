namespace ExhaustiveMatching.Analyzer.Tests
{
    public static class CodeContext
    {
        public static string Basic(string args, string body)
        {
            const string context = @"using System; // DayOfWeek
using System.ComponentModel; // InvalidEnumArgumentException
using ExhaustiveMatching;

class TestClass
{{
    void TestMethod({0})
    {{{1}
    }}
}}";
            return string.Format(context, args, body);
        }

        public static string CoinFlip(string args, string body)
        {
            const string context = @"using System; // DayOfWeek
using System.ComponentModel; // InvalidEnumArgumentException
using ExhaustiveMatching;

class TestClass
{{
    void TestMethod({0})
    {{{1}
    }}
}}

enum CoinFlip {{ Heads, Tails }}";
            return string.Format(context, args, body);
        }

        public static string Shapes(string args, string body)
        {
            const string context = @"using System; // DayOfWeek
using System.ComponentModel; // InvalidEnumArgumentException
using ExhaustiveMatching;
using TestNamespace;
using Nisse;

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
}}

namespace Nisse {{
    public sealed class ValueOutOfRangeException: Exception {{
        public ValueOutOfRangeException(string what, object? value) {{
        }}
    }}
}}
";
            return string.Format(context, args, body);
        }
    }
}
