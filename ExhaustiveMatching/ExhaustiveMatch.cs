namespace ExhaustiveMatching
{
	public static class ExhaustiveMatch
	{
		public static ExhaustiveMatchFailedException Failed()
		{
			return new ExhaustiveMatchFailedException();
		}

		public static ExhaustiveMatchFailedException Failed<T>(T value)
		{
			return new ExhaustiveMatchFailedException(typeof(T), value);
		}
	}
}
