namespace ExhaustiveMatching.Analyzer.Tests
{
    public static class CodeContext
    {
        public static string Basic(string args, string body)
        {
            const string context = @"using System;
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
            const string context = @"using System;
using System.ComponentModel; // InvalidEnumArgumentException
using ExhaustiveMatching;

enum CoinFlip {{ Heads = 1, Tails }}

class TestClass
{{
    void TestMethod({0})
    {{{1}
    }}
}}";
            return string.Format(context, args, body);
        }

        public static string Shapes(string args, string body)
        {
            const string context = @"using System;
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

        public static string Result(string args, string body)
        {
            const string context = @"using System;
using System;
using System.Collections.Generic;
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
    public abstract class Result<TSuccess, TError> {{
        private Result() {{ }}

        public sealed class Success : Result<TSuccess, TError> {{
            public TSuccess Value {{ get; }}
            public Success(TSuccess value) {{ Value = value; }}
        }}

        public sealed class Error : Result<TSuccess, TError> {{
            public TError Value {{ get; }}
            public Error(TError value) {{ Value = value; }}
        }}
    }}
}}";
            return string.Format(context, args, body);
        }

        public static string ResultRecord(string args, string body)
        {
            const string context = @"using System;
using System;
using System.Collections.Generic;
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
    public abstract record Result<TSuccess, TError> {{
        private Result() {{ }}

        public sealed record Success(TSuccess Value) : Result<TSuccess, TError>;

        public sealed record Error(TError value) : Result<TSuccess, TError>;
    }}
}}

namespace System.Runtime.CompilerServices {{
    /// <summary>
    /// Reserved to be used by the compiler for tracking metadata.
    /// This class should not be used by developers in source code.
    /// </summary>
    internal static class IsExternalInit {{
    }}
}}";
            return string.Format(context, args, body);
        }
    }
}
