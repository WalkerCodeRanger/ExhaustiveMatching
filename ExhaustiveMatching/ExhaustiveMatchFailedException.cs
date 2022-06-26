using System;
using System.Linq;
using System.Runtime.Serialization;

namespace ExhaustiveMatching
{
    [Serializable]
    public sealed class ExhaustiveMatchFailedException : Exception
    {
        private const string NoValueMessage = "A match that was supposed to be exhaustive failed to match.";
        private const string EnumValueMessage = "'{0}.{1}' was not matched. Match is supposed to be exhaustive.";
        private const string InvalidEnumMessage = "The value {1} is not valid for enum type '{0}'. Match is supposed to be exhaustive.";
        private const string ObjectValueMessage = "Object of type '{1}' was not matched when matching a '{0}'. Match is supposed to be exhaustive.";
        private const string NullObjectValueMessage = "The value 'null' was not matched when matching a '{0}'. Match is supposed to be exhaustive.";

        /// <summary>
        /// The type of value that was being matched on
        /// </summary>
        public Type MatchingType { get; }

        /// <summary>
        /// The value that failed to match
        /// </summary>
        public object FailedValue { get; }

        internal ExhaustiveMatchFailedException()
            : base(NoValueMessage)
        {
        }

        internal ExhaustiveMatchFailedException(Type matchingType, object failedValue)
            : base(MessageFor(matchingType, failedValue))
        {
            MatchingType = matchingType;
            FailedValue = failedValue;
        }

        private ExhaustiveMatchFailedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        private static string MessageFor(Type matchingType, object failedValue)
        {
            var typeName = GetTypeName(matchingType);
            if (failedValue is Enum enumValue)
            {
                var format = Enum.IsDefined(matchingType, enumValue) ? EnumValueMessage : InvalidEnumMessage;
                return string.Format(format, typeName, failedValue);
            }

            if (failedValue == null)
                return string.Format(NullObjectValueMessage, typeName);

            return string.Format(ObjectValueMessage, typeName, failedValue.GetType());
        }

        private static string GetTypeName(Type type)
        {
            if (type.IsGenericType)
            {
                var openType = type.GetGenericTypeDefinition();
                if (openType == typeof(Nullable<>))
                    return GetTypeName(type.GenericTypeArguments[0])+"?";

                var args = string.Join(", ", type.GenericTypeArguments.Select(GetTypeName));
                var coreTypeName = openType.FullName.Substring(0, openType.FullName.IndexOf('`'));
                return $"{coreTypeName}<{args}>";
            }

            return type.FullName;
        }
    }
}
