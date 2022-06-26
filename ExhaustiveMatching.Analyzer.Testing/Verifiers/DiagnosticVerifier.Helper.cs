using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace ExhaustiveMatching.Analyzer.Tests.Verifiers
{
    /// <summary>
    /// Class for turning strings into documents and getting the diagnostics on them
    /// All methods are static
    /// </summary>
    public abstract partial class DiagnosticVerifier
    {
        private static readonly MetadataReference CoreLibReference = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
        private static readonly MetadataReference SystemCoreReference = MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location);
        private static readonly MetadataReference CSharpSymbolsReference = MetadataReference.CreateFromFile(typeof(CSharpCompilation).Assembly.Location);
        private static readonly MetadataReference CodeAnalysisReference = MetadataReference.CreateFromFile(typeof(Compilation).Assembly.Location);
        private static readonly MetadataReference SystemConsoleReference = MetadataReference.CreateFromFile(typeof(Console).Assembly.Location);
        private static readonly MetadataReference ComponentModelReference = MetadataReference.CreateFromFile(typeof(InvalidEnumArgumentException).Assembly.Location);
        private static readonly MetadataReference ExhaustiveMatchingReference = MetadataReference.CreateFromFile(typeof(ExhaustiveMatch).Assembly.Location);

        internal static string DefaultFilePathPrefix = "Test";
        internal static string CSharpDefaultFileExt = "cs";
        internal static string TestProjectName = "TestProject";

        #region  Get Diagnostics

        /// <summary>
        /// Given classes in the form of strings, their language, and an IDiagnosticAnalyzer to apply to it, return the diagnostics found in the string after converting it to a document.
        /// </summary>
        /// <param name="sources">Classes in the form of strings</param>
        /// <param name="analyzer">The analyzer to be run on the sources</param>
        /// <returns>An IEnumerable of Diagnostics that surfaced in the source code, sorted by Location</returns>
        private static Task<Diagnostic[]> GetSortedDiagnosticsAsync(IReadOnlyCollection<string> sources, DiagnosticAnalyzer analyzer)
        {
            return GetSortedDiagnosticsFromDocumentsAsync(analyzer, GetDocuments(sources));
        }

        /// <summary>
        /// Given an analyzer and a document to apply it to, run the analyzer and gather an array of diagnostics found in it.
        /// The returned diagnostics are then ordered by location in the source document.
        /// </summary>
        /// <param name="analyzer">The analyzer to run on the documents</param>
        /// <param name="documents">The Documents that the analyzer will be run on</param>
        /// <returns>An IEnumerable of Diagnostics that surfaced in the source code, sorted by Location</returns>
        protected static async Task<Diagnostic[]> GetSortedDiagnosticsFromDocumentsAsync(DiagnosticAnalyzer analyzer, Document[] documents)
        {
            var projects = new HashSet<Project>();
            foreach (var document in documents)
                projects.Add(document.Project);

            var diagnostics = new List<Diagnostic>();
            foreach (var project in projects)
            {
                var compilationWithAnalyzers = (await project.GetCompilationAsync().ConfigureAwait(false)).WithAnalyzers(ImmutableArray.Create(analyzer));
                var allDiagnostics = await compilationWithAnalyzers.GetAllDiagnosticsAsync().ConfigureAwait(false);
                var significantDiagnostics = allDiagnostics.Where(diagnostic => !IsIgnoredCompilerDiagnostic(diagnostic));
                foreach (var diagnostic in significantDiagnostics)
                {
                    if (diagnostic.Location == Location.None || diagnostic.Location.IsInMetadata)
                        diagnostics.Add(diagnostic);
                    else
                        foreach (var document in documents)
                        {
                            var tree = await document.GetSyntaxTreeAsync().ConfigureAwait(false);
                            if (tree == diagnostic.Location.SourceTree)
                                diagnostics.Add(diagnostic);
                        }
                }
            }

            var results = SortDiagnostics(diagnostics);
            diagnostics.Clear();
            return results;
        }

        private static bool IsIgnoredCompilerDiagnostic(Diagnostic diagnostic)
        {
            // Skip diagnostics from the compiler that aren't errors
            return diagnostic.Id == "CS5001" // Missing main
                   || (diagnostic.Id.StartsWith("CS") && diagnostic.Severity != DiagnosticSeverity.Error);
        }

        /// <summary>
        /// Sort diagnostics by location in source document
        /// </summary>
        /// <param name="diagnostics">The list of Diagnostics to be sorted</param>
        /// <returns>An IEnumerable containing the Diagnostics in order of Location</returns>
        private static Diagnostic[] SortDiagnostics(IEnumerable<Diagnostic> diagnostics)
        {
            return diagnostics.OrderBy(d => d.Location.SourceSpan.Start).ToArray();
        }

        #endregion

        #region Set up compilation and documents

        /// <summary>
        /// Given an array of strings as sources and a language, turn them into a project and return the documents and spans of it.
        /// </summary>
        /// <param name="sources">Classes in the form of strings</param>
        /// <returns>A Tuple containing the Documents produced from the sources and their TextSpans if relevant</returns>
        private static Document[] GetDocuments(IReadOnlyCollection<string> sources)
        {
            var project = CreateProject(sources);
            var documents = project.Documents.ToArray();

            if (sources.Count != documents.Length)
            {
                throw new InvalidOperationException("Amount of sources did not match amount of Documents created");
            }

            return documents;
        }

        /// <summary>
        /// Create a Document from a string through creating a project that contains it.
        /// </summary>
        /// <param name="source">Classes in the form of a string</param>
        /// <returns>A Document created from the source string</returns>
        protected static Document CreateDocument(string source)
        {
            return CreateProject(new[] { source }).Documents.First();
        }

        /// <summary>
        /// Create a project using the inputted strings as sources.
        /// </summary>
        /// <param name="sources">Classes in the form of strings</param>
        /// <returns>A Project created out of the Documents created from the source strings</returns>
        private static Project CreateProject(IReadOnlyCollection<string> sources)
        {
            string fileNamePrefix = DefaultFilePathPrefix;
            string fileExt = CSharpDefaultFileExt;

            var projectId = ProjectId.CreateNewId(debugName: TestProjectName);

            var assemblyPath = Path.GetDirectoryName(typeof(object).Assembly.Location);
            var systemRuntimePath = MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Runtime.dll"));
            var netstandardPath = MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "netstandard.dll"));

            var solution = new AdhocWorkspace()
                .CurrentSolution
                .AddProject(projectId, TestProjectName, TestProjectName, "C#")
                .AddMetadataReference(projectId, CoreLibReference)
                .AddMetadataReference(projectId, SystemCoreReference)
                .AddMetadataReference(projectId, CSharpSymbolsReference)
                .AddMetadataReference(projectId, CodeAnalysisReference)
                .AddMetadataReference(projectId, SystemConsoleReference)
                .AddMetadataReference(projectId, ComponentModelReference)
                .AddMetadataReference(projectId, systemRuntimePath)
                .AddMetadataReference(projectId, netstandardPath)
                .AddMetadataReference(projectId, ExhaustiveMatchingReference);

            var count = 0;
            foreach (var source in sources)
            {
                var newFileName = fileNamePrefix + count + "." + fileExt;
                var documentId = DocumentId.CreateNewId(projectId, debugName: newFileName);
                solution = solution.AddDocument(documentId, newFileName, SourceText.From(source));
                count++;
            }

            var project = solution.GetProject(projectId);
            project = project?.WithParseOptions(((CSharpParseOptions)project.ParseOptions ?? new CSharpParseOptions()).WithLanguageVersion(LanguageVersion.CSharp9));
            return project;
        }
        #endregion
    }
}

