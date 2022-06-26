using System;
using System.Collections.Generic;
using System.Linq;

namespace ExhaustiveMatching
{
    // Allow multiple because we allow one in each part of a partial class
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true)]
    public class ClosedAttribute : Attribute
    {
        public IReadOnlyList<Type> Cases { get; }

        public ClosedAttribute(params Type[] cases)
        {
            Cases = cases.ToList().AsReadOnly();
        }
    }
}
