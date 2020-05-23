using System;

#region snippet
using ExhaustiveMatching;
#endregion

// ReSharper disable All

namespace Examples.ReadMe
{
    #region snippet
    enum CoinFlip { Heads, Tails }
    #endregion

    public static class CoinFlipExample
    {
        static void Example(CoinFlip coinFlip)
        {
            #region snippet
            // ERROR Enum value not handled by switch: Tails
            switch (coinFlip)
            {
                default:
                    throw ExhaustiveMatch.Failed(coinFlip);
                case CoinFlip.Heads:
                    Console.WriteLine("Heads!");
                    break;
            }
            #endregion
        }
    }
}
