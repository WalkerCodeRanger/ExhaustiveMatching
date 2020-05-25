using ExhaustiveMatching;

// ReSharper disable All

namespace Examples
{
    public static class CoinFlipSwitchExpressionExample
    {
        #region snippet
        public enum CoinFlip { Heads, Tails }
        #endregion

        public static void Example(CoinFlip coinFlip)
        {
            #region snippet
            // ERROR Enum value not handled by switch: Tails
            var result = coinFlip switch
            {
                CoinFlip.Heads => "Heads!",
                _ => throw ExhaustiveMatch.Failed(coinFlip),
            };
            #endregion

            // Mark as used
            _ = result;
        }
    }
}
