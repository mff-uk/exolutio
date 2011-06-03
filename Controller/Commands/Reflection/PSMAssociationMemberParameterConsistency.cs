using System.Reflection;
using Exolutio.Model.PSM;

namespace Exolutio.Controller.Commands.Reflection
{
    public class PSMAssociationMemberParameterConsistency : ParameterConsistency
    {
        public const string Key = "PSMAssociationMemberParameterConsistency";

        public override bool VerifyConsistency(object superordinateObject, object candidate)
        {
            PSMAssociationMember PSMAssociationMember = (PSMAssociationMember)superordinateObject;
            PSMAssociation PSMAssociation = (PSMAssociation)candidate;

            return PSMAssociationMember.ChildPSMAssociations.Contains(PSMAssociation);
        }
    }
}