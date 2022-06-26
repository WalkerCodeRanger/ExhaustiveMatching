using System;

namespace ExhaustiveMatching
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = false)]
    internal class ClosedAttribute : Attribute
    {
    }
}
