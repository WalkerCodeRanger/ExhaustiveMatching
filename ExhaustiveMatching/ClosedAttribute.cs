using System;
using System.Collections.Generic;
using System.Linq;

namespace ExhaustiveMatching
{
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
