using System;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace ExhaustiveMatching.Analyzer.Testing.Helpers
{
    /// <summary>
    /// Struct that stores information about a Diagnostic appearing in a source
    /// </summary>
    public struct DiagnosticResult
    {
        public static DiagnosticResult Error(string id, string message)
        {
            return new DiagnosticResult(DiagnosticSeverity.Error, id, message);
        }

        private DiagnosticResult(DiagnosticSeverity severity, string id, string message)
        {
            Severity = severity;
            Id = id;
            Message = message;
            Locations = Array.Empty<DiagnosticResultLocation>();
        }

        public DiagnosticResultLocation[] Locations { get; private set; }

        public DiagnosticSeverity Severity { get; set; }

        public string Id { get; }

        public string Message { get; }

        public string Path => Locations.Length > 0 ? Locations[0].Path : "";

        public int Line => Locations.Length > 0 ? Locations[0].Line : -1;

        public int Column => Locations.Length > 0 ? Locations[0].Column : -1;

        public DiagnosticResult AddLocation(string source, int marker)
        {
            var newLocation = DiagnosticResultLocation.FromMarker(source, marker);
            Locations = Locations.Append(newLocation).ToArray();
            return this;
        }
    }
}
