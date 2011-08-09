using System.Linq;
using System.Reflection;
using Exolutio.Model;

namespace Exolutio.Controller.Commands.Reflection
{
    public class SchemaAttributeTypeParameterConsistency : ParameterConsistency
    {
        public const string Key = "SchemaAttributeTypeParameterConsistency";

        public override bool VerifyConsistency(object superordinateObject, object candidate)
        {
            Schema schema = (Schema) superordinateObject;
            AttributeType attributeType = (AttributeType)candidate;

            if (schema is Model.PIM.PIMSchema)
            {
                return schema.ProjectVersion.GetAvailablePIMTypes().Contains(attributeType) &&
                       attributeType.Schema == null || attributeType.Schema == schema;
            }
            else
            {
                return ((Model.PSM.PSMSchema)schema).GetAvailablePSMTypes().Contains(attributeType) &&
                       attributeType.Schema == null || attributeType.Schema == schema;
            }
        }
    }
}