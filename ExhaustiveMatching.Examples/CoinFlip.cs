using System;
using ExhaustiveMatching;

public enum CoinFlip { Heads, Tails }

class CoinFlipExample
{
    void Example(CoinFlip coinFlip)
    {
        switch (coinFlip)
        {
            case CoinFlip.Heads:
                Console.WriteLine("Heads!");
                break;
            #region Snip
            case CoinFlip.Tails:
                Console.WriteLine("Tails!");
                break;
            #endregion
            default:
                throw ExhaustiveMatch.Failed(coinFlip);
        }
    }
}