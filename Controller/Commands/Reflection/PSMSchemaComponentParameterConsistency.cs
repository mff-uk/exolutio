using System.Linq;
using System.Reflection;
using Exolutio.Model.PSM;

namespace Exolutio.Controller.Commands.Reflection
{
    public class PSMSchemaComponentParameterConsistency : ParameterConsistency
    {
        public const string Key = "PSMSchemaComponentParameterConsistency";

        public override bool VerifyConsistency(object superordinateObject, object candidate)
        {
            PSMSchema psmSchema = (PSMSchema) superordinateObject;
            PSMComponent psmComponent = (PSMComponent)candidate;

            return psmSchema.SchemaComponents.Contains(psmComponent);
        }
    }
}