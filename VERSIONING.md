# Version Scheme for `ExhaustiveMatching.Analyzer`

## Relevant Features in C# Language Versions

Each version of the C# language adds many new features, but most have no impact
on the `ExhaustiveMatching.Analyzer`. This section lists which new features in
each version in some way impact exhaustive matching and necessitate a newer
version of the package to support.

### C# 6

*NOTE:* C# 6 is the oldest version of the language supported by Roslyn Analyzers and consequently the oldest version supported by `ExhaustiveMatching.Analyzer`. The features listed here may have been introduced before C# 6, but represent the basic level of support.

* `switch` Statements
* `InvalidEnumArgumentException`
* `ArgumentOutOfRangeException`
* `InvalidOperationException`

### C# 7.0

* Pattern Matching
  * [Declaration Patterns](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/operators/patterns#declaration-and-type-patterns)
  * [Constant Patterns](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/operators/patterns#constant-pattern) for `null` and enum values

### C# 8

* `switch` Expressions

### C# 9

* Records
* [Type Patterns](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/operators/patterns#declaration-and-type-patterns)

### C# 10

* Record Structs
