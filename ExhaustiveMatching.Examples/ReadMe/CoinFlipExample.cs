using System;

#region snippet
using ExhaustiveMatching;
#endregion

// ReSharper disable All

namespace Examples.ReadMe
{
    public static class CoinFlipExample
    {
        #region snippet
        public enum CoinFlip { Heads, Tails }
        #endregion

        public static void Example(CoinFlip coinFlip)
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

            // ERROR Enum value not handled by switch: Tails
            _ = coinFlip switch
            {
                CoinFlip.Heads => "Heads!",
                _ => throw ExhaustiveMatch.Failed(coinFlip),
            };
            #endregion
        }
    }
}
