namespace ExhaustiveMatching.Analyzer.Tests.Fakes
{
    [UnionOfTypes(
        typeof(ExampleCase1),
        typeof(ExampleCase2))]
    public abstract class ExampleBaseClass
    {
        private protected ExampleBaseClass() { }
    }
}
