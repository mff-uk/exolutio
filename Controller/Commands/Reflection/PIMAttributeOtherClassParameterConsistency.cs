using System.Reflection;
using EvoX.Model.PIM;

namespace EvoX.Controller.Commands.Reflection
{
    public class PIMAttributeOtherClassParameterConsistency : ParameterConsistency
    {
        public const string Key = "PIMAttributeOtherClassParameterConsistency";

        public override bool VerifyConsistency(object superordinateObject, object candidate)
        {
            PIMAttribute pimAttribute = (PIMAttribute)superordinateObject;
            PIMClass pimClass = (PIMClass)candidate;

            return pimAttribute.PIMClass != pimClass;
        }
    }
}