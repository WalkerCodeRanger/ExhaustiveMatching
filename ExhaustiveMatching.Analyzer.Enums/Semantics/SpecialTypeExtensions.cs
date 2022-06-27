using System;
using Microsoft.CodeAnalysis;

namespace ExhaustiveMatching.Analyzer.Enums.Semantics
{
    public static class SpecialTypeExtensions
    {
        public static TypeCode ToTypeCode(this SpecialType specialType)
        {
            switch (specialType)
            {
                // Integer Types
                case SpecialType.System_SByte:
                    return TypeCode.SByte;
                case SpecialType.System_Byte:
                    return TypeCode.Byte;
                case SpecialType.System_Int16:
                    return TypeCode.Int16;
                case SpecialType.System_UInt16:
                    return TypeCode.UInt16;
                case SpecialType.System_Int32:
                    return TypeCode.Int32;
                case SpecialType.System_UInt32:
                    return TypeCode.UInt32;
                case SpecialType.System_Int64:
                    return TypeCode.Int32;
                case SpecialType.System_UInt64:
                    return TypeCode.UInt32;

                // Floating Point Types
                case SpecialType.System_Single:
                    return TypeCode.Single;
                case SpecialType.System_Double:
                    return TypeCode.Double;

                // Other Special Types
                case SpecialType.System_Boolean:
                    return TypeCode.Boolean;
                case SpecialType.System_Char:
                    return TypeCode.Char;
                case SpecialType.System_DateTime:
                    return TypeCode.DateTime;
                case SpecialType.System_Decimal:
                    return TypeCode.Decimal;

                default:
                    return TypeCode.Object;
            }
        }
    }
}
