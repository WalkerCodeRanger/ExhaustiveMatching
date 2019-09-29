using System;
using ExhaustiveMatching;
using TestNamespace;

class MirrorExample
{
    void Example(IAnimal animal)
    {
        switch (animal)
        {
            case ICat cat:
                Console.WriteLine("Cat: " + cat);
                break;
            case IDog dog:
                Console.WriteLine("Dog: " + dog);
                break;
            default:
                throw ExhaustiveMatch.Failed(animal);
        }
    }
}

namespace TestNamespace
{
    [Closed(typeof(ICat), typeof(IDog))]
    public interface IAnimal { }
    public interface ICat : IAnimal { }
    public interface IDog : IAnimal { }

    public abstract class Animal : IAnimal { }
    public class Cat : Animal, ICat { }
    public class Dog : Animal, IDog { }
}
