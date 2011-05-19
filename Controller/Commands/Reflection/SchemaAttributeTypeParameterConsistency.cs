using System.Linq;
using System.Reflection;
using EvoX.Model;

namespace EvoX.Controller.Commands.Reflection
{
    public class SchemaAttributeTypeParameterConsistency : ParameterConsistency
    {
        public const string Key = "SchemaAttributeTypeParameterConsistency";

        public override bool VerifyConsistency(object superordinateObject, object candidate)
        {
            Schema schema = (Schema) superordinateObject;
            AttributeType attributeType = (AttributeType)candidate;

            return schema.ProjectVersion.AttributeTypes.Contains(attributeType) &&
                   attributeType.Schema == null || attributeType.Schema == schema;
        }
    }
}