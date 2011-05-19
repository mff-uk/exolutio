using System.Linq;
using System.Reflection;
using EvoX.Model.PIM;

namespace EvoX.Controller.Commands.Reflection
{
    public class PIMSchemaComponentParameterConsistency : ParameterConsistency
    {
        public const string Key = "PIMSchemaComponentParameterConsistency";

        public override bool VerifyConsistency(object superordinateObject, object candidate)
        {
            PIMSchema pimSchema = (PIMSchema) superordinateObject;
            PIMComponent pimComponent = (PIMComponent)candidate;

            return pimSchema.SchemaComponents.Contains(pimComponent);
        }
    }
}