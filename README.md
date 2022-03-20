# ExhaustiveMatching.Analyzer

`ExhaustiveMatching.Analyzer` adds exhaustive matching to C# switch statements
and expressions.

*Get compiler errors for missing cases in a switch statement or expression.*
Mark which switches should have exhaustiveness checking by throwing an exception
in the default case. Exhaustiveness checking works not just for enums, but for
classes and interfaces. Turn them into [discriminated unions (aka sum
types)](https://en.wikipedia.org/wiki/Tagged_union) by marking them with the
`Closed` attribute and listing the cases. `ExhaustiveMatching.Analyzer` goes
beyond what other languages support by handling full inheritance hierarchies.

## Quickstart Guide

Mark a switch statement or expression as exhaustive and get errors for missing
cases.

```csharp
using ExhaustiveMatching;

public enum CoinFlip { Heads, Tails }

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
```

Create [discriminated unions (aka sum types)](https://en.wikipedia.org/wiki/Tagged_union)
and get errors for missing switch cases.

```csharp
[Closed(typeof(IPv4Address), typeof(IPv6Address))]
public abstract class IPAddress { … }

public class IPv4Address : IPAddress { … }
public class IPv6Address : IPAddress { … }

// ERROR Subtype not handled by switch: IPv6Address
switch (ipAddress)
{
    default:
        throw ExhaustiveMatch.Failed(ipAddress);
    case IPv4Address ipv4Address:
        return ipv4Address.MapToIPv6();
}
```

## Packages and Downloading

All packages are available on [NuGet.org](https://www.nuget.org). There are
three packages available for different situations.

* [ExhaustiveMatching.Analyzer](https://www.nuget.org/packages/ExhaustiveMatching.Analyzer/):
  The standard package that includes all analyzers. Adds a dependency on
  `ExhaustiveMatching.Analyzer` to your project.
* ExhaustiveMatching.Analyzer.Enums *(Not Yet Implemented)*: Only includes the
  exhaustive matching analyzer for enums using `InvalidEnumArgumentException`.
  Avoids adding any dependencies or additional code to your project.
* ExhaustiveMatching.Analyzer.Source *(Not Yet Implemented)*: For advanced
  scenarios, avoids adding dependencies to your project while supporting most
  analyzers by injecting code into your project. See [Dependency Free
  Usage](#dependency-free-usage) for details.

## Versioning and Compatibility

*This package does not use [semantic versioning](https://semver.org).* Instead,
the first two version numbers match the major and minor version of the
`Microsoft.CodeAnalysis` package referenced by the analyzer. This determines the
version of Visual Studio, MSBuild, etc. the analyzer is compatible with. The
next two version numbers are the major and minor versions of this package. At
any time, multiple versions of Visual Studio may be actively supported. You
should use the most recent version compatible with the environment you are
using. If an older version is used, functionality will be missing.

Your project must target a framework [compatible with .NET Standard 2.0](https://docs.microsoft.com/en-us/dotnet/standard/net-standard?tabs=net-standard-2-0#select-net-standard-version).
In addition, a minimum version of Visual studio or .NET Core is required
depending on the version of the package you are using. See the table below.

| Package Version      | Minimum Visual Studio Version                  | C# Language |
| -------------------- | ---------------------------------------------- | ----------- |
| 3.3.*major*.*minor*  | Visual Studio 2019 version 16.3, .NET Core 3.0 | C# 8        |
| 2.10.*major*.*minor* | Visual Studio 2017 version 15.9                | C# 7.3      |
| 1.3.*major*.*minor*  | Visual Studio 2015 Update 3                    | C# 6.0      |
| 0.*x*                | Visual Studio 2019 version 16.3, .NET Core 3.0 | C# 8        |

## Usage

Install the `ExhaustiveMatching.Analyzer` package into each project that will
contain exhaustive switch statements, switch expressions, or the classes and
interfaces that will be switched on. Additionally, *install the package in any
project that will reference a project containing types marked with the `Closed`
attribute*. This is important because the analyzer enforces rules about
inheriting from and implementing closed types. If the analyzer isn't in a
project then those rules may be violated without an error being reported. Most
of the time, the `ExhaustiveMatching.Analyzer` can be added to every project in
a solution.

As soon as you add the NuGet package to your project, the IDE (Microsoft Visual
Studio, JetBrains Rider and possibly others) should automatically enable the
analyzer and start marking non-exhaustive switches in your code. Analogously,
MSBuild and dotnet CLI should report the errors with no additional set up. If
this is not working, you may not be using a compatible version. See [Versioning
and Compatibility](#versioning-and-compatibility) for more information.

### Exhaustive Switch on Enum Values

To enable exhaustiveness checking for a switch on an enum, throw an
`ExhaustiveMatchFailedException` from the default case. That exception is
constructed using the `ExhaustiveMatch.Failed(…)` factory method which should be
passed the value being switched on. For switches with exhaustiveness checking,
the analyzer will report an error for any missing enum cases.

```csharp
// ERROR Enum value not handled by switch: Sunday
switch(dayOfWeek)
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
```

Exhaustiveness checking is also applied to switches that throw
`InvalidEnumArgumentException`. This exception indicates that the value doesn't
match any of the defined enum values. Thus, if the code throws it from the
default case, the developer is expecting that all defined enum cases will be
handled by the switch. Using this exception, the throw statement in the above
example would be `throw new InvalidEnumArgumentException(nameof(dayOfWeek),
(int)dayOfWeek, typeof(DayOfWeek));`. Since this is longer and less readable,
its use is discouraged.

### Exhaustive Switch on Type

C# 7.0 added pattern matching including the ability to switch on the type of a
value. To ensure any possible value will be handled, all subtypes must be
matched by some case. That is what exhaustiveness checking ensures.

To enable exhaustiveness checking for a switch on type, two things must be done.
The default case must throw an `ExhaustiveMatchFailedException` (using the
`ExhaustiveMatch.Failed(…)` factory method) and the type being switched on must
be marked with the `Closed` attribute. The closed attribute makes a type similar
to an enum by giving it a defined set of possible cases. However, instead of a
fixed set of values like an enum, a closed type has a fixed set of direct
subtypes.

**CAUTION:** subtyping rules for types with the `[Closed(...)]` attribute are
enforced by the analyzer, not the language. They can be circumvented. To prevent
this include the exhaustive matching analyzer in all projects in your solution,
do not expose a closed type to external code that may not be using exhaustive
matching, and do not use dynamic code generation to create subtypes. If
unexpected subtypes are created, an `ExhaustiveMatchFailedException` exception
could be generated at runtime.

This example shows how to declare a closed class `Shape` that can be either a
circle or a square.

```csharp
[Closed(typeof(Circle), typeof(Square))]
public abstract class Shape { … }

public class Circle : Shape { … }
public class Square : Shape { … }
```

A switch on the type of a shape can then be checked for exhaustiveness.

```csharp
switch(shape)
{
    case Circle _:
        Console.WriteLine("Circle");
        break;
    case Square _:
        Console.WriteLine("Square");
        break;
    default:
        throw ExhaustiveMatch.Failed(shape);
}
```

### Handling Null

Since C# reference types are always nullable, but may be intended to never be
null, exhaustiveness checking does not require a case for null. If a null value
is expected it can be handled by a `case null:`. The analyzer will ignore this
case for its analysis.

For nullable enum types, the analyzer requires that there be a `case null:` to
handle the null value.

### Type Hierarchies

While a given closed type can only have its direct subtypes as cases, some of
those subtypes may themselves be closed types. This allows for flexible
switching on multiple levels of a type hierarchy. The exhaustiveness check
ensures that every possible value is handled by some case. However, a single
case high up in the hierarchy can handle many types.

In the example below, an expression tree is being evaluated. The switch is able
to match against multiple levels of the hierarchy while exhaustiveness checking
ensures no cases are missing. Notice how the `Addition` and `Subtraction` cases
are indirect subtypes of `Expression`, and the `Value` case handles both
`Constant` and `Variable`. This kind of sophisticated multi-level switching is
not supported in most languages that include exhaustive matching.

```csharp
[Closed(typeof(BinaryOperator), typeof())]
public abstract class Expression { … }

[Closed(typeof(Addition), typeof(Subtraction))]
public abstract class BinaryOperator { … }

public class Addition { … }
public class Subtraction { … }

public abstract class Value { … }

public class Constant { … }
public class Variable { … }

public int Evaluate(Expression expression)
{
    switch(expression)
    {
        case Addition a:
            return Evaluate(a.Left) + Evaluate(a.Right);
        case Subtraction s:
            return Evaluate(s.Left) - Evaluate(s.Right);
        case Value v: // handles both Constant and Variable
            return v.GetValue();
        default:
            throw ExhaustiveMatch.Failed(expression);
    }
}
```

## Analyzer Errors

The analyzer reports various errors for incorrect code. The table below gives a
complete list of them along with a description.

<table>
    <tr>
        <th>Number</th>
        <th>Description</th>
    </tr>
    <tr>
        <th>EM0001</th>
        <td>A switch on an enum is missing a case</td>
    </tr>
    <tr>
        <th>EM0002</th>
        <td>A switch on a nullable enum is missing a null case</td>
    </tr>
    <tr>
        <th>EM0003</th>
        <td>A switch on type is missing a case</td>
    </tr>
    <tr>
        <th>EM0011</th>
        <td>A concrete type is not listed as a case in a closed type it is a direct subtype of</td>
    </tr>
    <tr>
        <th>EM0012</th>
        <td>A case type listed in the closed attribute is not a direct subtype of the closed type (though it is a subtype)</td>
    </tr>
    <tr>
        <th>EM0013</th>
        <td>A case type listed in the closed attribute is not a subtype of the closed type</td>
    </tr>
    <tr>
        <th>EM0014</th>
        <td>A concrete subtype of a closed type is not covered by some case</td>
    </tr>
    <tr>
        <th>EM0015</th>
        <td>An open interface is not listed as a case in a closed type it is a direct subtype of</td>
    </tr>
    <tr>
        <th>EM0100</th>
        <td>An exhaustive switch can't contain when guards</td>
    </tr>
    <tr>
        <th>EM0101</th>
        <td>Case pattern is not supported</td>
    </tr>
    <tr>
        <th>EM0102</th>
        <td>Can't do exhaustiveness checking for switch on a type that is not an enum and not closed</td>
    </tr>
    <tr>
        <th>EM0103</th>
        <td>Case is for a type that is not in the closed type hierarchy</td>
    </tr>
    <tr>
        <th>EM0104</th>
        <td>Duplicate 'Closed' attribute on type</td>
    </tr>
    <tr>
        <th>EM0105</th>
        <td>Duplicate case type</td>
    </tr>
</table>

## Dependency Free Usage

*(Not Yet Implemented)*

In some situations, it isn't desirable to add a dependency to your project. For
example, a published NuGet package may want to use exhaustive matching without
adding a dependency on `ExhaustiveMatching.Analyzer` to their project. The
`ExhaustiveMatching.Analyzer.Private` package supports these cases. It does so
by injecting the source for the `ExhaustiveMatch` and `InternalClosedAttribute`
directly into the project. This classes are marked `internal` and will not be
visible from outside of the project.

This package does not support the `ClosedAttribute` because it would not be safe
to do so. If a closed class were publicly exposed from
