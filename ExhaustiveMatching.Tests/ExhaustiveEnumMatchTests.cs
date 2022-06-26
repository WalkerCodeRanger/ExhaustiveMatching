using ExhaustiveMatching.Tests.Fakes;
using Xunit;

namespace ExhaustiveMatching.Tests
{
    public class ExhaustiveEnumMatchTests
    {
        //[Fact]
        //public void ResolvesNullableOverloads()
        //{
        //    ExhaustiveEnumMatch.FailedOnArgument("Foo", ExampleEnum.Yes);
        //    ExhaustiveEnumMatch.FailedOnArgument("Foo", default(ExampleEnum?));
        //}

        [Fact]
        public void EnumSomething()
        {
            //var paramMessage = new ArgumentException("", "{0}").Message;
            //var ex1 = new InvalidEnumArgumentException("{0}", 42, typeof(ExampleEnum));
            //var formattedMessage = ex1.Message;
            //if (formattedMessage.EndsWith(paramMessage))
            //    formattedMessage = formattedMessage[..^paramMessage.Length];

            //formattedMessage = formattedMessage.Replace("42", "{1}");

            ////var resources = typeof(InvalidEnumArgumentException).Assembly.GetManifestResourceNames();
            //var isDefined = Enum.IsDefined(typeof(ExampleEnum), ExampleEnum.Yes);
            //var ex = ExhaustiveEnumMatch.FailedOnArgument("Foo", ExampleEnum.Yes);
        }

        [Fact]
        public void EnumOutOfIntRange()
        {
            //var ex = ExhaustiveEnumMatch.FailedOnArgument("Foo", (LongEnum)long.MaxValue);
        }
    }
}
