using System.Reflection;
using Exolutio.Model.PSM;

namespace Exolutio.Controller.Commands.Reflection
{
    public class PSMClassAttributeParameterConsistency : ParameterConsistency
    {
        public const string Key = "PSMClassAttributeParameterConsistency";

        public override bool VerifyConsistency(object superordinateObject, object candidate)
        {
            PSMClass PSMClass = (PSMClass)superordinateObject;
            PSMAttribute PSMAttribute = (PSMAttribute)candidate;

            return PSMClass.PSMAttributes.Contains(PSMAttribute);
        }
    }
}