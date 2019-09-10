using System;
using System.Collections.Generic;
using System.Linq;

namespace ExhaustiveMatching
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
    public class ClosedAttribute : Attribute
    {
        public IReadOnlyList<Type> CaseTypes { get; }

        public ClosedAttribute(params Type[] caseTypes)
        {
            CaseTypes = caseTypes.ToList().AsReadOnly();
        }
    }
}
