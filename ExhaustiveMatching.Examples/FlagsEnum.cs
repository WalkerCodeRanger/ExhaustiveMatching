using System;
using ExhaustiveMatching;

namespace Examples
{
    [Flags]
    public enum FlagsEnum
    {
        Flag1 = 1,
        Flag2 = 2,
    }

    public static class FlagsEnumExample
    {
        public static void Example(FlagsEnum value)
        {
            switch (value)
            {
                default:
                    throw ExhaustiveMatch.Failed(value);
                case FlagsEnum.Flag1:
                    Console.WriteLine("Flag1");
                    break;
                case FlagsEnum.Flag2:
                    Console.WriteLine("Flag2");
                    break;
            }
        }
    }
}
