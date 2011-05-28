using System.Linq;
using System.Reflection;
using Exolutio.Model;

namespace Exolutio.Controller.Commands.Reflection
{
    public class SchemaComponentParameterConsistency : ParameterConsistency
    {
        public const string Key = "SchemaComponentParameterConsistency";

        public override bool VerifyConsistency(object superordinateObject, object candidate)
        {
            Schema schema = (Schema) superordinateObject;
            Component component = (Component)candidate;

            return schema.SchemaComponents.Contains(component);
        }
    }
}