using System;
using ExhaustiveMatching;
using TestNamespace;

class ShapeExample
{
    void Example(Shape shape)
    {
        switch (shape)
        {
            case Square square:
                Console.WriteLine("Square: " + square);
                break;
            case Circle circle:
                Console.WriteLine("Circle: " + circle);
                break;
            //case EquilateralTriangle equilateralTriangle:
            //    Console.WriteLine("EquilateralTriangle: " + equilateralTriangle);
            //    break;
            case Triangle triangle:
                Console.WriteLine("Triangle: " + triangle);
                break;
            //case string s:
            //    Console.WriteLine("string: " + s);
            //    break;
            default:
                throw ExhaustiveMatch.Failed(shape);
        }
    }
}

namespace TestNamespace
{
    [Closed(typeof(Square), typeof(Circle), typeof(Triangle))]
    public abstract class Shape { }

    public class Square : Shape { }

    public class Circle : Shape { }

    // abstract to show abstract leaf types are checked
    public abstract class Triangle : Shape { }

    public class EquilateralTriangle : Triangle { }

    public class IsoscelesTriangle : Triangle { }
}
