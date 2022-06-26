using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace ExhaustiveMatching.Analyzer.Tests.Helpers
{
    /// <summary>
    /// Location where the diagnostic appears, as determined by path, line number, and column number.
    /// </summary>
    public readonly struct DiagnosticResultLocation
    {
        /// <summary>
        /// This uses the special balanced matching feature to allow markers to be nested. It
        /// also uses positive lookahead so that the nested marker won't be consumed and can
        /// then be matched.
        /// </summary>
        private static readonly Regex NestedMarkerRegex
            = new Regex(@"◊(?<number>\d)+(?=⟦(?<content>([^⟦⟧]|(?<open>⟦)|(?<-open>⟧))*(?(open)(?!)))⟧)",
                RegexOptions.Compiled|RegexOptions.ExplicitCapture);

        /// <summary>
        /// Marker regex without nesting. Needed for find replace to work correctly
        /// </summary>
        private static readonly Regex MarkerRegex
            = new Regex(@"◊(?<number>\d)+⟦(?<content>([^⟦⟧]|(?<open>⟦)|(?<-open>⟧))*(?(open)(?!)))⟧",
                RegexOptions.Compiled | RegexOptions.ExplicitCapture);

        /// <summary>
        /// Just the first part of the marker for use when removing for line and column
        /// </summary>
        private static readonly Regex MarkerStart
            = new Regex(@"◊(?<number>\d)+⟦", RegexOptions.Compiled | RegexOptions.ExplicitCapture);

        public static DiagnosticResultLocation FromMarker(string source, int marker)
        {
            var markerString = marker.ToString();
            var match = NestedMarkerRegex.Matches(source).Single(m => m.Groups["number"].Value == markerString);
            var (line, column) = LineAndColumn(source, match.Index);
            // Get the length, requires we remove any nested markers
            var length = RemoveMakers(match.Groups["content"].Value).Length;
            return new DiagnosticResultLocation("Test.cs", line, column, length);
        }

        private static (int, int) LineAndColumn(string val, int index)
        {
            var upToIndex = val.Substring(0, index + 1);
            var lines = upToIndex.Split('\n');
            var lineNumber = lines.Length;
            var lastLine = lines.Last();
            // Take out any parts of previous markers so column is correct
            lastLine = MarkerStart.Replace(lastLine, "").Replace("⟧", "");
            var columnNumber = lastLine.Length;
            return (lineNumber, columnNumber);
        }

        public static string RemoveMakers(string source)
        {
            // Have to recursively remove markers because of nesting
            return MarkerRegex.Replace(source, m => RemoveMakers(m.Groups["content"].Value));
        }

        private DiagnosticResultLocation(string path, int line, int column, int length = -1)
        {
            if (line < -1)
                throw new ArgumentOutOfRangeException(nameof(line), @"line must be >= -1");

            if (column < -1)
                throw new ArgumentOutOfRangeException(nameof(column), @"column must be >= -1");

            if (length < -1)
                throw new ArgumentOutOfRangeException(nameof(length), @"length must be >= -1");

            Path = path;
            Line = line;
            Column = column;
            Length = length;
        }

        public string Path { get; }
        public int Line { get; }
        public int Column { get; }
        public int Length { get; }
    }
}
