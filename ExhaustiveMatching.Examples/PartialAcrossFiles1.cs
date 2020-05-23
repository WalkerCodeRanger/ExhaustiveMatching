// This test demonstrates closed types split between partial files

using ExhaustiveMatching;

namespace Examples
{
    [Closed(
        typeof(IPartialChild1),
        typeof(ISameFileInterface))]
    public partial interface IPartialBase { }

    public interface IPartialChild1 : IPartialBase { }

    public partial interface ISameFileInterface : IPartialBase { }

    public class SameFileInterface : ISameFileInterface { }
}