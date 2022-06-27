using System;
using System.Collections.Generic;
using System.Linq;
using ExhaustiveMatching.Analyzer.Enums.Semantics;
using ExhaustiveMatching.Analyzer.Enums.Utility;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ExhaustiveMatching.Analyzer.Enums.Analysis
{
    public static class SwitchOnEnumAnalyzer
    {
        /// <summary>
        /// Figure out which enum values are unused.
        /// </summary>
        /// <remarks>Cases in a switch on an enum type can be actual enum values, but they can also
        /// be integer values etc. To handle that, checking for unused values must be done on the
        /// numeric value of the enum values.</remarks>
        public static IEnumerable<ISymbol> UnusedEnumValues(
            SyntaxNodeAnalysisContext context,
            INamedTypeSymbol enumType,
            IEnumerable<ExpressionSyntax> caseExpressions)
        {
            // For performance and storage space, a sorted array is used instead of a HashSet or
            // SortedSet. Both of those use more memory and have more overhead. Hash of primitive
            // types is not normally well distributed. It is expected that the values used will
            // rarely contain duplicates.
            var valuesUsed = caseExpressions.Select(e => GetEnumCaseValue(context, e, enumType))
                                            .WhereNotNull().ToArray();
            Array.Sort(valuesUsed);

            var allSymbols = enumType.GetMembers().OfType<IFieldSymbol>();

            // Use where instead of Except because we have a set
            return allSymbols.Where(s => !SortedArrayContains(valuesUsed, s.ConstantValue));
        }

        /// <summary>
        /// Get the numeric value of a case or <see langword="null"/> if it cannot be gotten.
        /// </summary>
        /// <remarks>Case expressions can contain errors. They can also be various forms of literal
        /// zero where the type won't match the underlying type of the enum. This deals with all
        /// of that.</remarks>
        private static object GetEnumCaseValue(
            SyntaxNodeAnalysisContext context,
            ExpressionSyntax expression,
            INamedTypeSymbol enumType)
        {
            var underlyingType = enumType.EnumUnderlyingType.SpecialType;
            return GetEnumCaseValue(context.SemanticModel, expression, underlyingType.ToTypeCode());
        }

        private static object GetEnumCaseValue(
            SemanticModel semanticModel,
            ExpressionSyntax expression,
            TypeCode typeCode)
        {
            var optional = semanticModel.GetConstantValue(expression);
            if (optional.HasValue)
            {
                if (optional.Value is null) return null;

                // Make sure it is converted to the right type
                return TryChangeType(optional.Value, typeCode, out var converted)
                    ? converted : null;
            }

            if (expression is CastExpressionSyntax castExpression)
                return GetEnumCaseValue(semanticModel, castExpression.Expression, typeCode);

            return null;
        }

        /// <summary>
        /// Try a conversion
        /// </summary>
        /// <remarks>There seems to be no built in way to try a conversion. Without writing
        /// custom converter code for every pair of types, the only option is to catch the exception
        /// from <see cref="Convert.ChangeType(object,Type)"/></remarks>
        private static bool TryChangeType(object value, TypeCode typeCode, out object converted)
        {
            try
            {
                converted = Convert.ChangeType(value, typeCode);
                return true;
            }
            catch
            {
                converted = null;
                return false;
            }
        }

        private static bool SortedArrayContains(Array array, object value)
            => Array.BinarySearch(array, value) >= 0;
    }
}
