using ExhaustiveMatching;

namespace Examples {
    [Closed(typeof(IPartialChild2))]
    public partial interface IPartialBase { }

    public interface IPartialChild2 : IPartialBase { }
}