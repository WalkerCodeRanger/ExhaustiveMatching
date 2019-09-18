# ExhaustiveMatching.Analyzer

ExhaustiveMatching.Analyzer adds exhaustive matching to C# switch statements.

*Get compiler errors when you leave out a case in a switch statement.* Mark which switch statements should have exhaustive matching by throwing an exception in the default case. Exhaustive matching works not just for enums, but for classes and interfaces. Turn them into [discriminated unions (aka sum types)](https://en.wikipedia.org/wiki/Tagged_union) by marking them with the `Closed` attribute and listing the cases. ExhaustiveMatching.Analyzer goes beyond what other languages support by handling full inheritance hierarchies.

## Exhaustive Matching for C\#

Mark a switch statement as exhaustive and get errors for missing cases.

```csharp
using ExhaustiveMatching;

public enum CoinFlip { Heads, Tails }

// ERROR Enum value not processed by switch: Tails
switch (coinFlip)
{
    case CoinFlip.Heads:
        Console.WriteLine("Heads!");
        break;
    default:
        throw ExhaustiveMatch.Failed(coinFlip);
}
```

Create [discriminated unions (aka sum types)](https://en.wikipedia.org/wiki/Tagged_union) and get errors for missing cases.

```csharp
[Closed(typeof(IPv4Address), typeof(IPv6Address))]
abstract class IPAddress { … }

class IPv4Address : IPAddress { … }
class IPv6Address : IPAddress { … }

// ERROR Subtype not processed by switch: IPv6Address
switch (ipAddress)
{
    case IPv4Address ipv4Address:
        return ipv4Address.MapToIPv6();
    default:
        throw ExhaustiveMatch.Failed(ipAddress);
}
```

## Why\?

## Download

The latest stable release of ExhaustiveMatching.Analyzer is [available on NuGet](https://www.nuget.org/packages/ExhaustiveMatching.Analyzer/).

## Features

A full explanation of features

### Exhaustive Matching on Enum Values

Add the `ExhaustiveMatching` NuGet package to a project. To enable exhaustiveness checking for a switch on an enum, throw an `InvalidEnumArgumentException` from the default case. This exception is used to indicate that the value in an enum variable doesn't match any of the defined enum values. Thus, if the code throws it from the default case, the developer is expecting that all defined enum cases will be handled by the switch statement.

```csharp
switch(dayOfWeek)
{
    case DayOfWeek.Monday:
    case DayOfWeek.Tuesday:
    case DayOfWeek.Wednesday:
    case DayOfWeek.Thursday:
    case DayOfWeek.Friday:
        Console.WriteLine(""Weekday"");
        break;
    case DayOfWeek.Saturday:
        // Omitted Sunday
        Console.WriteLine(""Weekend"");
        break;
    default:
        throw new InvalidEnumArgumentException(nameof(dayOfWeek), (int)dayOfWeek, typeof(DayOfWeek));
}
```

Easier yet, use the convenient `ExhaustiveMatch` class in the `ExhaustiveMatching` package. The throw statement above can be replaced by `throw ExhaustiveMatch.Failed(dayOfWeek)`. This will throw an `ExhaustiveMatchFailedException` that provides both the enum type and value that wasn't matched.

### Exhaustive Matching on Type

C# 7.0 added pattern matching including the ability to switch on the type of a value. When switching on the type of a value, any value must have a subclass type that is a subclass. To be sure any value will be matched by some case clause, all these types must be matched by some case. This provides powerful case matching possibilities. The Scala language uses a similar idea in the form of "case classes" for pattern matching.

To enable exhaustiveness checking on a type match, two things must be done. The default case must throw an `ExhaustiveMatchFailedException` (using `ExhaustiveMatch.Failed(...)`) and the type being matched up must be marked with the `Matchable` attribute. Classes marked with the matchable attributes have a number of limitations imposed on them which combine to ensure the analyzer can determine the complete set of possible subclasses.

This example shows how to declare a class `Answer` that is matchable.

```csharp
[Matchable]
public abstract class Answer
{
    private protected Answer () {}
}

public sealed class Yes : Answer {}
public sealed class No : Answer {}
```

Values of type `Answer` can then be exhaustively matched against.

```csharp
switch(answer)
{
    case Yes yes:
        Console.WriteLine("The answer is Yes!");
        break;
    case No no:
        Console.WriteLine("The answer is no.");
        break;
    default:
        throw ExhaustiveMatch.Failed(answer);
}
```

### Rules for Matchable Types

1. Each class must be either:
   * `sealed`
   * Have all constructors `private`, `internal`, or `private protected` (added in C# 7.2)

**TODO:** Write out the rest of the rules

### Null Values and Exhaustiveness

**TODO**

### Rules for Matching on Type

**TODO**

## Analyzer Errors and Warnings

**TODO**

Number | Meaning
---|---
EM001 | A switch statement marked for exhaustiveness checking is not an exhaustive match.
