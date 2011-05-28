using System.Linq;
using System.Reflection;
using Exolutio.Model;
using Exolutio.Model.PIM;
using Exolutio.Model.PSM;

namespace Exolutio.Controller.Commands.Reflection
{
    public class InterpretationConsistency : ParameterConsistency
    {
        public const string Key = "InterpretationConsistency";

        public override bool VerifyConsistency(object superordinateObject, object candidate)
        {
            PSMComponent psmComponent = (PSMComponent)superordinateObject;
            PIMComponent component = (PIMComponent)candidate;

            if (psmComponent is PSMAssociation && component is PIMAssociation)
            {
                return true;
            }

            if (psmComponent is PSMAttribute && component is PIMAttribute)
            {
                return true;
            }

            if (psmComponent is PSMClass && component is PIMClass)
            {
                return true;
            }

            return false;
        }
    }
}