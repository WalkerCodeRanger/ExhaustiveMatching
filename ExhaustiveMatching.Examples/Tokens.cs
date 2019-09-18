﻿// This test demonstrates both partial types and complex hierarchies. It
// is based on an example from the source of a compiler.

using System;
using ExhaustiveMatching;

// This section represents code that would be auto generated
#region autogenerated

[Closed(
    typeof(IPlusToken),
    typeof(IMinusToken))]
public partial interface IToken { }

public partial interface IPlusToken : IToken { }
public partial interface IMinusToken : IToken { }

[Closed(
    typeof(ITrueKeywordToken),
    typeof(IFalseKeywordToken))]
public partial interface IKeywordToken { }

public partial interface ITrueKeywordToken : IKeywordToken { }
public partial interface IFalseKeywordToken : IKeywordToken { }
#endregion

#region handwritten

[Closed(
    typeof(IOperatorToken),
    typeof(IKeywordToken))]
public partial interface IToken { }

[Closed(
    typeof(IOperatorToken),
    typeof(IBooleanLiteralToken)
    )]
public partial interface IKeywordToken : IToken { }

[Closed(
    typeof(ITrueKeywordToken),
    typeof(IFalseKeywordToken))]
public interface IBooleanLiteralToken : IKeywordToken { }

public partial interface ITrueKeywordToken : IBooleanLiteralToken { }

public partial interface IFalseKeywordToken : IBooleanLiteralToken { }

[Closed(
    typeof(IPlusToken),
    typeof(IMinusToken))]
public partial interface IOperatorToken : IToken { }

public partial interface IPlusToken : IOperatorToken { }
public partial interface IMinusToken : IOperatorToken { }
#endregion

class TokensExample
{
    void Example(IToken token)
    {
        switch (token)
        {
            case IKeywordToken _:
                Console.WriteLine("boolean literal");
                break;
            case IPlusToken _:
                Console.WriteLine("+");
                break;
            case IMinusToken _:
                Console.WriteLine("+");
                break;
            default:
                throw ExhaustiveMatch.Failed(token);
        }
    }
}