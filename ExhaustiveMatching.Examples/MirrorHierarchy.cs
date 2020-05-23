using System;
using ExhaustiveMatching;

// ReSharper disable All

namespace Examples
{
    public class MirrorExample
    {
        public void Example(IAnimal animal)
        {
            switch (animal)
            {
                default:
                    throw ExhaustiveMatch.Failed(animal);
                case ICat cat:
                    Console.WriteLine("Cat: " + cat);
                    break;
                case IDog dog:
                    Console.WriteLine("Dog: " + dog);
                    break;
            }
        }
    }

    [Closed(typeof(ICat), typeof(IDog))]
    public interface IAnimal { }
    public interface ICat : IAnimal { }
    public interface IDog : IAnimal { }

    public abstract class Animal : IAnimal { }
    public class Cat : Animal, ICat { }
    public class Dog : Animal, IDog { }
}
