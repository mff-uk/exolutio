using System.Linq;
using System.Reflection;
using EvoX.Model;
using EvoX.Model.PIM;
using EvoX.Model.PSM;

namespace EvoX.Controller.Commands.Reflection
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