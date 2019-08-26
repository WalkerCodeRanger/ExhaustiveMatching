using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ExhaustiveMatch.Analyzer
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class ExhaustiveMatchAnalyzer : DiagnosticAnalyzer
	{
		public const string DiagnosticId = "ExhaustiveMatchAnalyzer";

		// You can change these strings in the Resources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
		// See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/Localizing%20Analyzers.md for more on localization
		private static readonly LocalizableString EM001Title = LoadString(nameof(Resources.EM001Title));
		private static readonly LocalizableString EM001Message = LoadString(nameof(Resources.EM001Message));
		private static readonly LocalizableString EM001Description = LoadString(Resources.EM001Description);

		private const string Category = "Logic";

		private static readonly DiagnosticDescriptor NotExhaustiveEnumSwitchRule =
			new DiagnosticDescriptor("EM001", EM001Title,
				EM001Message, Category, DiagnosticSeverity.Error, isEnabledByDefault: true,
				EM001Description);

		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
			ImmutableArray.Create(NotExhaustiveEnumSwitchRule);

		public override void Initialize(AnalysisContext context)
		{
			context.RegisterSyntaxNodeAction(AnalyzeSwitchStatement, SyntaxKind.SwitchStatement);
		}

		private static void AnalyzeSwitchStatement(SyntaxNodeAnalysisContext context)
		{
			if (!(context.Node is SwitchStatementSyntax switchStatement))
				throw new InvalidOperationException(
					"Switch AnalyzeSwitchStatement called with a non-switch statement context");

			if (!IsExhaustive(context, switchStatement))
				return;

			var expressionTypeInfo = context.SemanticModel
				.GetTypeInfo(switchStatement.Expression, context.CancellationToken);

			if (expressionTypeInfo.Type?.TypeKind == TypeKind.Enum)
				AnalyzeEnumSwitchStatement(context, switchStatement, expressionTypeInfo.Type);
		}

		private static void AnalyzeEnumSwitchStatement(
			SyntaxNodeAnalysisContext context,
			SwitchStatementSyntax switchStatement,
			ITypeSymbol type)
		{
			var symbolsUsed = switchStatement
				.Sections
				.SelectMany(s => s.Labels)
				.OfType<CaseSwitchLabelSyntax>()
				.Select(l => context.SemanticModel.GetSymbolInfo(l.Value, context.CancellationToken).Symbol)
				.ToImmutableHashSet();

			var unusedSymbols = type.GetMembers()
					.Where(m => m.Kind == SymbolKind.Field && !symbolsUsed.Contains(m))
					.Select(m => type.Name + "." + m.Name)
					.ToArray();

			if (unusedSymbols.Any())
			{
				var diagnostic = Diagnostic.Create(NotExhaustiveEnumSwitchRule,
					switchStatement.GetLocation(),
					string.Join("\n", unusedSymbols));
				context.ReportDiagnostic(diagnostic);
			}

			//throw new NotImplementedException();
		}

		private static bool IsExhaustive(SyntaxNodeAnalysisContext context, SwitchStatementSyntax switchStatement)
		{
			var defaultSection = switchStatement.Sections.FirstOrDefault(s =>
				s.Labels.OfType<DefaultSwitchLabelSyntax>().Any());

			var throwStatement = defaultSection?.Statements.OfType<ThrowStatementSyntax>().FirstOrDefault();

			// If there is no default section or it doesn't throw, we assume the
			// dev doesn't want an exhaustive match
			if (throwStatement == null) return false;

			// If it isn't `throw new...` ignore it
			if (!(throwStatement.Expression is ObjectCreationExpressionSyntax objectCreation))
				return false;

			var exceptionType = context.SemanticModel
								.GetTypeInfo(objectCreation, context.CancellationToken).Type;
			var invalidEnumArgumentExceptionType = context.Compilation.GetTypeByMetadataName("System.ComponentModel.InvalidEnumArgumentException");

			return exceptionType != null && exceptionType.Equals(invalidEnumArgumentExceptionType);
		}
		private static LocalizableResourceString LoadString(string name)
		{
			return new LocalizableResourceString(name,
				Resources.ResourceManager, typeof(Resources));
		}
	}
}
