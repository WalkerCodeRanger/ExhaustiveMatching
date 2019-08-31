using System;

namespace ExhaustiveMatching.Analyzer.Tests
{
    class TestClass
    {
        void TestMethod(Shape shape)
        {
            switch (shape)
            {
                //case Square square when true:
                //	Console.WriteLine("Square: " + square);
                //	break;
                case Circle circle:
                    Console.WriteLine("Circle: " + circle);
                    break;
                case var x when x is Square:
                    break;
                case Square _:
                    break;
                case null:
                    break;
                default:
                    throw ExhaustiveMatch.Failed(shape);
            }
        }


        void TestMethod123(DayOfWeek dayOfWeek)
        {
            switch (dayOfWeek)
            {
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
                default:
                    throw ExhaustiveMatch.Failed(dayOfWeek);
            }
        }
    }

    [Matchable]
    abstract class Shape { }
    class Square : Shape { }
    class Circle : Shape { }
    class Triangle : Shape { }
}