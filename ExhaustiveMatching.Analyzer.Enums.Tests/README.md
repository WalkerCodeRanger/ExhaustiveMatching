# ExhaustiveMatching.Analyzer.Enums.Tests

The tests in this project use a special syntax for marking the expected location of errors. Trying to correctly enter the line, column, and length of each error and then updated them as need was challenging. Instead, the span of an error is surrounded in `◊N⟦…⟧` where `N` is an integer that is used to refer to that marker. These markers are then used to compute locations for the errors and then automatically removed before compiling the code.
