using ExhaustiveMatching;

namespace Examples
{
    [Closed(typeof(ValuePlace), typeof(DiscardPlace))]
    public abstract class Place
    {
    }

    public class DiscardPlace : Place
    {
    }

    [Closed(typeof(VariablePlace), typeof(FieldPlace))]
    public abstract class ValuePlace : Place
    {
    }

    public class VariablePlace : ValuePlace
    {
    }

    public class FieldPlace : ValuePlace
    {
    }
}
