﻿using Microsoft.Cci.Extensions;

namespace Microsoft.Cci.Differs.Rules
{
    [ExportDifferenceRule]
    internal class TypesMustExist : DifferenceRule
    {
        public override DifferenceType Diff(IDifferences differences, ITypeDefinition impl, ITypeDefinition contract)
        {
            if (impl == null && contract != null)
            {
                if (!ReportAsMembersMustExist(contract, differences))
                {
                    differences.AddIncompatibleDifference(this,
                        "Type '{0}' does not exist in the implementation but it does exist in the contract.", contract.FullName());
                }

                return DifferenceType.Added;
            }

            return DifferenceType.Unknown;
        }

        // Usability hack: Ordinarily, removing a type does not trigger secondary messages about removing its members. Normally,
        // this is very useful behavior. However, this backfires in the case where a type "vanishes" solely because someone removed 
        // its last [TreatAsPublicSurface] member. [TreatAsPublicSurface] is the only known case where one can "remove" a type merely 
        // by changing something about one of its members - something that has no precedent in the minds of most developers.
        // Reporting the violation as a removal of the type would be quite unhelpful in this case. Thus, if the "removed" type
        // had one or more [TreatAsPublicSurface] members, we counterfeit a MembersMustExist message.
        private bool ReportAsMembersMustExist(ITypeDefinition contract, IDifferences differences)
        {
            bool specialCasedViolation = false;
            //foreach (ITypeDefinitionMember member in contract.Members)
            //{
            //    if (member.MarkedAsPublicSurface())
            //    {
            //        differences.AddIncompatibleDifference(
            //            "MembersMustExist",
            //            "Member '{0}' does not exist in the implementation but it does exist in the contract.", member.FullName());
            //        specialCasedViolation = true;
            //    }
            //}
            return specialCasedViolation;
        }
    }
}

