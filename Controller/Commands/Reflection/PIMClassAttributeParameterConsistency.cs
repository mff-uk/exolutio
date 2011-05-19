using System.Reflection;
using EvoX.Model.PIM;

namespace EvoX.Controller.Commands.Reflection
{
    public class PIMClassAttributeParameterConsistency : ParameterConsistency
    {
        public const string Key = "PIMClassAttributeParameterConsistency";

        public override bool VerifyConsistency(object superordinateObject, object candidate)
        {
            PIMClass pimClass = (PIMClass)superordinateObject;
            PIMAttribute pimAttribute = (PIMAttribute)candidate;

            return pimClass.PIMAttributes.Contains(pimAttribute);
        }
    }
}