namespace ExhaustiveMatching.Analyzer
{
	public class SwitchStatementKind
	{
		public readonly bool IsExhaustive;
		public readonly bool DefaultThrowsInvalidEnum;

		public SwitchStatementKind(bool isExhaustive, bool defaultThrowsInvalidEnum)
		{
			IsExhaustive = isExhaustive;
			DefaultThrowsInvalidEnum = defaultThrowsInvalidEnum;
		}
	}
}
