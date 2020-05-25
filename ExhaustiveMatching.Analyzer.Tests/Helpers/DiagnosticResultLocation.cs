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
        private static readonly Regex MarkerRegex = new Regex(@"◊(?<number>\d)+⟦(?<content>[^⟧]*)⟧", RegexOptions.Compiled);
        public static DiagnosticResultLocation FromMarker(string source, int marker, int? length)
        {
            var markerString = marker.ToString();
            var match = MarkerRegex.Matches(source).Single(m => m.Groups["number"].Value == markerString);

            var (line, column) = LineAndColumn(source, match.Index);
            return new DiagnosticResultLocation("Test.cs", line, column, length ?? match.Groups["content"].Length);
        }

        private static (int, int) LineAndColumn(string val, int index)
        {
            var upToIndex = val.Substring(0, index + 1);
            var lines = upToIndex.Split('\n');
            var lineNumber = lines.Count();
            var columnNumber = lines.Last().Length;
            return (lineNumber, columnNumber);
        }

        public static string RemoveMakers(string source)
        {
            return MarkerRegex.Replace(source, m => m.Groups["content"].Value);
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
