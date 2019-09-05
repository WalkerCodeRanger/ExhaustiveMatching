using System;
using System.Collections.Generic;
using System.Linq;

namespace ExhaustiveMatching
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
    public class EnumOfTypesAttribute : Attribute
    {
        public IReadOnlyList<Type> CaseTypes { get; }

        public EnumOfTypesAttribute(params Type[] caseTypes)
        {
            CaseTypes = caseTypes.ToList().AsReadOnly();
        }
    }
}
