using System.Reflection;
using Exolutio.Model.PIM;
using System.Linq;
using Exolutio.Model.PSM;

namespace Exolutio.Controller.Commands.Reflection
{
    public class PSMClassInterpretedEndParameterConsistency : ParameterConsistency
    {
        public const string Key = "PSMClassInterpretedEndParameterConsistency";

        public override bool VerifyConsistency(object superordinateObject, object candidate)
        {
            PSMClass psmClass = (PSMClass) superordinateObject;
            if (psmClass.Interpretation == null)
            {
                psmClass = Model.ModelIterator.NearestInterpretedParentClass(psmClass);
            }
            PIMAssociationEnd pimAssociationEnd = (PIMAssociationEnd) candidate;

            if (psmClass.Interpretation != null)
            {
                PIMClass pimClass = (PIMClass) psmClass.Interpretation;
                foreach (PIMAssociationEnd pimAE in pimClass.PIMAssociationEnds)
                {
                    if (pimAE != candidate && pimAE.PIMAssociation.PIMAssociationEnds.Contains(candidate))
                        return true;
                }
            }
            return false;
        }
    }
}