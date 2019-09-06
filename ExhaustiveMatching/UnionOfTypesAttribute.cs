using System;
using System.Collections.Generic;
using System.Linq;

namespace ExhaustiveMatching
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
    public class UnionOfTypesAttribute : Attribute
    {
        public IReadOnlyList<Type> CaseTypes { get; }

        public UnionOfTypesAttribute(params Type[] caseTypes)
        {
            CaseTypes = caseTypes.ToList().AsReadOnly();
        }
    }
}
