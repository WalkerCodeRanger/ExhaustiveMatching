using System;
using System.Runtime.Serialization;

namespace ExhaustiveMatch
{
	[Serializable]
	public sealed class ExhaustiveMatchFailedException : Exception
	{
		/// <summary>
		/// The type of value that was being matched on,
		/// </summary>
		public Type MatchingType { get; }

		/// <summary>
		/// The value that 
		/// </summary>
		public object FailedValue { get; }

		internal ExhaustiveMatchFailedException()
		: this(null, null)      // TODO calculate a message
		{
		}

		internal ExhaustiveMatchFailedException(Type matchingType, object failedValue)

		{
			MatchingType = matchingType;
			FailedValue = failedValue;
			// TODO calculate a message
		}

		private ExhaustiveMatchFailedException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}
}
