using EvoX.Model.PIM;

namespace EvoX.Controller.Commands.Reflection
{
    public class PIMAssociationAssociationEndParameterConsistency : ParameterConsistency
    {
        public const string Key = "PIMAssociationAssociationEndParameterConsistency";

        public override bool VerifyConsistency(object superordinateObject, object candidate)
        {
            PIMAssociation pimAssociation = (PIMAssociation)superordinateObject;
            PIMAssociationEnd pimAssociationEnd = (PIMAssociationEnd)candidate;

            return pimAssociation.PIMAssociationEnds.Contains(pimAssociationEnd);
        }
    }
}