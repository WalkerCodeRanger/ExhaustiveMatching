using System;
using ExhaustiveMatching.Tests.Fakes;
using Xunit;

namespace ExhaustiveMatching.Tests
{
    public class ExhaustiveMatchFailedExceptionTests
    {
        [Fact]
        public void WithoutValueGivesGenericMessage()
        {
            const string expectedMessage = "A match that was supposed to be exhaustive failed to match.";

            var ex = ExhaustiveMatch.Failed();

            Assert.Equal(expectedMessage, ex.Message);
            Assert.Null(ex.FailedValue);
            Assert.Null(ex.MatchingType);
        }

        [Fact]
        public void EnumValueGivesEnumMessage()
        {
            const string expectedMessage = "'ExhaustiveMatching.Tests.Fakes.FakeIntEnum.Yes' was not matched. Match is supposed to be exhaustive.";

            var ex = ExhaustiveMatch.Failed(FakeIntEnum.Yes);

            Assert.Equal(expectedMessage, ex.Message);
            Assert.Equal(FakeIntEnum.Yes, ex.FailedValue);
            Assert.Equal(typeof(FakeIntEnum), ex.MatchingType);
        }

        [Fact]
        public void InvalidEnumValueGivesInvalidEnumMessage()
        {
            const FakeIntEnum invalidValue = (FakeIntEnum)123;
            const string expectedMessage = "The value 123 is not valid for enum type 'ExhaustiveMatching.Tests.Fakes.FakeIntEnum'. Match is supposed to be exhaustive.";

            var ex = ExhaustiveMatch.Failed(invalidValue);

            Assert.Equal(expectedMessage, ex.Message);
            Assert.Equal(invalidValue, ex.FailedValue);
            Assert.Equal(typeof(FakeIntEnum), ex.MatchingType);
        }

        [Fact]
        public void NullValueForNullableEnumGivesNullObjectMessage()
        {
            const string expectedMessage = "The value 'null' was not matched when matching a 'ExhaustiveMatching.Tests.Fakes.FakeIntEnum?'. Match is supposed to be exhaustive.";

            var ex = ExhaustiveMatch.Failed((FakeIntEnum?)null);

            Assert.Equal(expectedMessage, ex.Message);
            Assert.Null(ex.FailedValue);
            Assert.Equal(typeof(FakeIntEnum?), ex.MatchingType);
        }

        [Fact]
        public void NullValueForEnumTypeGivesNullObjectMessage()
        {
            const string expectedMessage = "The value 'null' was not matched when matching a 'System.Enum'. Match is supposed to be exhaustive.";

            var ex = ExhaustiveMatch.Failed<Enum>(null);

            Assert.Equal(expectedMessage, ex.Message);
            Assert.Null(ex.FailedValue);
            Assert.Equal(typeof(Enum), ex.MatchingType);
        }

        [Fact]
        public void ObjectValueGivesTypeMessage()
        {
            const string expectedMessage = "Object of type 'System.String' was not matched when matching a 'System.ICloneable'. Match is supposed to be exhaustive.";
            var value = (ICloneable)"TestingValue";

            var ex = ExhaustiveMatch.Failed(value);

            Assert.Equal(expectedMessage, ex.Message);
            Assert.Equal(value, ex.FailedValue);
            Assert.Equal(typeof(ICloneable), ex.MatchingType);
        }

        [Fact]
        public void NullObjectGivesNullObjectMessage()
        {
            const string expectedMessage = "The value 'null' was not matched when matching a 'System.ICloneable'. Match is supposed to be exhaustive.";
            ICloneable value = default;

            var ex = ExhaustiveMatch.Failed(value);

            Assert.Equal(expectedMessage, ex.Message);
            Assert.Null(ex.FailedValue);
            Assert.Equal(typeof(ICloneable), ex.MatchingType);
        }
    }
}
