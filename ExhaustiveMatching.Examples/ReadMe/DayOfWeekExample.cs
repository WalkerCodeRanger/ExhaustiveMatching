using System;
using ExhaustiveMatching;

// ReSharper disable All

namespace Examples.ReadMe
{
    public static class DayOfWeekExample
    {
        public static void Example(DayOfWeek dayOfWeek)
        {
            #region snippet
            // EM0001: Switch on Enum Not Exhaustive
            // ERROR Enum value not handled by switch: Sunday
            switch (dayOfWeek)
            {
                default:
                    throw ExhaustiveMatch.Failed(dayOfWeek);
                case DayOfWeek.Monday:
                case DayOfWeek.Tuesday:
                case DayOfWeek.Wednesday:
                case DayOfWeek.Thursday:
                case DayOfWeek.Friday:
                    Console.WriteLine("Weekday");
                    break;
                case DayOfWeek.Saturday:
                    // Omitted Sunday
                    Console.WriteLine("Weekend");
                    break;
            }
            #endregion
        }
    }
}
