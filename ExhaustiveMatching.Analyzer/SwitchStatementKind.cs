namespace ExhaustiveMatching.Analyzer
{
    public readonly struct SwitchStatementKind
    {
        public readonly bool IsExhaustive;
        public readonly bool ThrowsInvalidEnum;

        public SwitchStatementKind(bool isExhaustive, bool throwsInvalidEnum)
        {
            IsExhaustive = isExhaustive;
            ThrowsInvalidEnum = throwsInvalidEnum;
        }
    }
}
