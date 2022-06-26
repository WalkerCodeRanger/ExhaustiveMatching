using System;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;

namespace ExhaustiveMatching
{
    public static class ExhaustiveMatch
    {
        private const string NoValueMessage = "A match that was supposed to be exhaustive failed to match.";
        private const string EnumValueMessage = "'{0}.{1}' was not matched. Match is supposed to be exhaustive.";
        private const string InvalidEnumMessage = "The value {1} is not valid for enum type '{0}'. Match is supposed to be exhaustive.";
        private const string ObjectValueMessage = "Object of type '{1}' was not matched when matching a '{0}'. Match is supposed to be exhaustive.";
        private const string NullObjectValueMessage = "The value 'null' was not matched when matching a '{0}'. Match is supposed to be exhaustive.";

        [Pure]
        public static Exception Failed<T>(string paramName, T value)
        {
            if (paramName is null) throw new ArgumentNullException(nameof(paramName));

            if (value == null)
                return new ArgumentNullException(paramName);

            if (value is Enum enumValue)
            {
                var enumType = enumValue.GetType();
                if (!enumType.IsEnumDefined(enumValue))
                {
                    switch (enumValue.GetTypeCode())
                    {
                        case TypeCode.Byte:
                        case TypeCode.SByte:
                        case TypeCode.Int16:
                        case TypeCode.UInt16:
                        case TypeCode.Int32:
                            // Guaranteed to be convertible to Int32
                            break;
                        case TypeCode.UInt32:
                        case TypeCode.UInt64:
                            // Check if too large for Int32 and manually format if needed
                            if (enumValue.CompareTo(Enum.ToObject(enumType, int.MaxValue)) > 0)
                                return CreateInvalidEnumArgumentException<T>(paramName, enumValue, enumType);
                            break;
                        case TypeCode.Int64:
                            // Check if too large or small for Int32 and manually format if needed
                            if (enumValue.CompareTo(Enum.ToObject(enumType, int.MinValue)) < 0
                                || enumValue.CompareTo(Enum.ToObject(enumType, int.MaxValue)) > 0)
                                return CreateInvalidEnumArgumentException<T>(paramName, enumValue, enumType);
                            break;
                        default:
                            throw new NotSupportedException(
                                $"Enum with type code {enumValue.GetTypeCode()} not supported.");
                    }

                    return new InvalidEnumArgumentException(paramName, Convert.ToInt32(enumValue), enumType);
                }
            }

            return new InvalidOperationException(paramName);
        }

        private static InvalidEnumArgumentException CreateInvalidEnumArgumentException<T>(string paramName, Enum enumValue, Type enumType)
            => new InvalidEnumArgumentException(
                $"The value of argument '{paramName}' ({enumValue:D}) is invalid for Enum type '{enumType.Name}'.");

        private static string MessageFor(Type matchingType, object failedValue)
        {
            var typeName = GetTypeName(matchingType);
            if (failedValue is Enum enumValue)
            {
                var format = Enum.IsDefined(matchingType, enumValue) ? EnumValueMessage : InvalidEnumMessage;
                return string.Format(format, typeName, failedValue);
            }

            if (failedValue == null) return string.Format(NullObjectValueMessage, typeName);

            return string.Format(ObjectValueMessage, typeName, failedValue.GetType());
        }

        private static string GetTypeName(Type type)
        {
            if (type.IsGenericType)
            {
                var openType = type.GetGenericTypeDefinition();
                if (openType == typeof(Nullable<>)) return GetTypeName(type.GenericTypeArguments[0]) + "?";

                var args = string.Join(", ", type.GenericTypeArguments.Select(GetTypeName));
                var coreTypeName = openType.FullName.Substring(0, openType.FullName.IndexOf('`'));
                return $"{coreTypeName}<{args}>";
            }

            return type.FullName;
        }

        // ExhaustiveEnumMatch.FailedOnArgument
        // ExhaustiveEnumMatch.FailedOn
        // ExhaustiveEnumMatch.OnArgumentFailed(

        // FailedOnArgument
        // Returns:
        //   InvalidEnumArgumentException if it is an invalid enum
        //   ArgumentNullException if it is null
        //   ArgumentOutOfRangeException
        //
        // Look for throw ExhaustiveMatching.ExhaustiveMatch.Failed....(...)

        //public static ExhaustiveMatchFailedException Failed()
        //{
        //    return new ExhaustiveMatchFailedException();
        //}

        //public static ExhaustiveMatchFailedException Failed<T>(T value)
        //{
        //    return new ExhaustiveMatchFailedException(typeof(T), value);
        //}

        public static InvalidEnumArgumentException InvalidEnumArgument<TEnum>(TEnum value, string paramName)
            where TEnum : struct, Enum, IConvertible
        {
            return new InvalidEnumArgumentException(paramName, value.ToInt32(CultureInfo.InvariantCulture), typeof(TEnum));
        }
    }
}
