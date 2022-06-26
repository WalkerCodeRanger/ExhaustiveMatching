using System.ComponentModel;
using ExhaustiveMatching.Internal.Tests.Fakes;
using Xunit;

namespace ExhaustiveMatching.Internal.Tests
{
    public class ExhaustiveMatchTests
    {
        [Fact]
        public void InvalidByteEnumArgument()
        {
            var ex = ExhaustiveMatch.Failed("paramName", (FakeByteEnum)255);

            var typedEx = Assert.IsType<InvalidEnumArgumentException>(ex);
            Assert.StartsWith("The value of argument 'paramName' (255) is invalid for Enum type 'FakeByteEnum'.",
                typedEx.Message);
            Assert.Equal("paramName", typedEx.ParamName);
        }

        [Theory]
        [InlineData(int.MinValue)]
        [InlineData(int.MaxValue)]
        public void InvalidIntEnumArgument(int value)
        {
            var ex = ExhaustiveMatch.Failed("paramName", (FakeIntEnum)value);

            var typedEx = Assert.IsType<InvalidEnumArgumentException>(ex);
            Assert.StartsWith($"The value of argument 'paramName' ({value}) is invalid for Enum type 'FakeIntEnum'.",
                typedEx.Message);
            Assert.Equal("paramName", typedEx.ParamName);
        }

        [Theory]
        [InlineData(int.MinValue)]
        [InlineData(int.MaxValue)]
        [InlineData((long)int.MinValue - 1)]
        [InlineData((long)int.MaxValue + 1)]
        public void InvalidLongEnumArgument(long value)
        {
            var ex = ExhaustiveMatch.Failed("paramName", (FakeLongEnum)value);

            var typedEx = Assert.IsType<InvalidEnumArgumentException>(ex);
            Assert.StartsWith($"The value of argument 'paramName' ({value}) is invalid for Enum type 'FakeLongEnum'.",
                typedEx.Message);
            if (value < int.MinValue || value > int.MaxValue)
                Assert.Null(typedEx.ParamName);
            else
                Assert.Equal("paramName", typedEx.ParamName);
        }
    }
}
