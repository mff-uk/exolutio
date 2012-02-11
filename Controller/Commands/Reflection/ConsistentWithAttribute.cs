using System;
using System.Collections.Generic;
using Exolutio.Controller.Commands.Atomic.PIM;

namespace Exolutio.Controller.Commands.Reflection
{
    public class ConsistentWithAttribute : Attribute
    {
        public string Consistency { get; set; }
        public string PropertyName { get; set; }

        public ConsistentWithAttribute(string propertyName, string consistency)
        {
            Consistency = consistency;
            PropertyName = propertyName;
        }

        private static readonly Dictionary<string, ParameterConsistency> instances = new Dictionary<string, ParameterConsistency>();

        public ParameterConsistency GetConsistencyInstance()
        {
            return GetConsistencyInstance(Consistency);
        }

        public static ParameterConsistency GetConsistencyInstance(string consistencyKey)
        {
            if (consistencyKey == PIMSchemaComponentParameterConsistency.Key)
            {
                if (!instances.ContainsKey(PIMSchemaComponentParameterConsistency.Key))
                {
                    instances[PIMSchemaComponentParameterConsistency.Key] = new PIMSchemaComponentParameterConsistency();
                }
                return instances[PIMSchemaComponentParameterConsistency.Key];
            }

            if (consistencyKey == PIMClassAttributeParameterConsistency.Key)
            {
                if (!instances.ContainsKey(PIMClassAttributeParameterConsistency.Key))
                {
                    instances[PIMClassAttributeParameterConsistency.Key] = new PIMClassAttributeParameterConsistency();
                }
                return instances[PIMClassAttributeParameterConsistency.Key];
            }

            if (consistencyKey == PSMClassAttributeParameterConsistency.Key)
            {
                if (!instances.ContainsKey(PSMClassAttributeParameterConsistency.Key))
                {
                    instances[PSMClassAttributeParameterConsistency.Key] = new PSMClassAttributeParameterConsistency();
                }
                return instances[PSMClassAttributeParameterConsistency.Key];
            }

            if (consistencyKey == PSMSchemaComponentParameterConsistency.Key)
            {
                if (!instances.ContainsKey(PSMSchemaComponentParameterConsistency.Key))
                {
                    instances[PSMSchemaComponentParameterConsistency.Key] = new PSMSchemaComponentParameterConsistency();
                }
                return instances[PSMSchemaComponentParameterConsistency.Key];
            }

            if (consistencyKey == PIMAssociationAssociationEndParameterConsistency.Key)
            {
                if (!instances.ContainsKey(PIMAssociationAssociationEndParameterConsistency.Key))
                {
                    instances[PIMAssociationAssociationEndParameterConsistency.Key] = new PIMAssociationAssociationEndParameterConsistency();
                }
                return instances[PIMAssociationAssociationEndParameterConsistency.Key];
            }

            if (consistencyKey == SchemaComponentParameterConsistency.Key)
            {
                if (!instances.ContainsKey(SchemaComponentParameterConsistency.Key))
                {
                    instances[SchemaComponentParameterConsistency.Key] = new SchemaComponentParameterConsistency();
                }
                return instances[SchemaComponentParameterConsistency.Key];
            }

            if (consistencyKey == SchemaAttributeTypeParameterConsistency.Key)
            {
                if (!instances.ContainsKey(SchemaAttributeTypeParameterConsistency.Key))
                {
                    instances[SchemaAttributeTypeParameterConsistency.Key] = new SchemaAttributeTypeParameterConsistency();
                }
                return instances[SchemaAttributeTypeParameterConsistency.Key];
            }

            if (consistencyKey == InterpretationConsistency.Key)
            {
                if (!instances.ContainsKey(InterpretationConsistency.Key))
                {
                    instances[InterpretationConsistency.Key] = new InterpretationConsistency();
                }
                return instances[InterpretationConsistency.Key];
            }

            if (consistencyKey == PIMAttributeOtherClassParameterConsistency.Key)
            {
                if (!instances.ContainsKey(PIMAttributeOtherClassParameterConsistency.Key))
                {
                    instances[PIMAttributeOtherClassParameterConsistency.Key] = new PIMAttributeOtherClassParameterConsistency();
                }
                return instances[PIMAttributeOtherClassParameterConsistency.Key];
            }

            if (consistencyKey == PIMAttributeNeighboringClassParameterConsistency.Key)
            {
                if (!instances.ContainsKey(PIMAttributeNeighboringClassParameterConsistency.Key))
                {
                    instances[PIMAttributeNeighboringClassParameterConsistency.Key] = new PIMAttributeNeighboringClassParameterConsistency();
                }
                return instances[PIMAttributeNeighboringClassParameterConsistency.Key];
            }

            if (consistencyKey == PSMAssociationMemberParameterConsistency.Key)
            {
                if (!instances.ContainsKey(PSMAssociationMemberParameterConsistency.Key))
                {
                    instances[PSMAssociationMemberParameterConsistency.Key] = new PSMAssociationMemberParameterConsistency();
                }
                return instances[PSMAssociationMemberParameterConsistency.Key];
            }

            if (consistencyKey == PSMClassInterpretedEndParameterConsistency.Key)
            {
                if (!instances.ContainsKey(PSMClassInterpretedEndParameterConsistency.Key))
                {
                    instances[PSMClassInterpretedEndParameterConsistency.Key] = new PSMClassInterpretedEndParameterConsistency();
                }
                return instances[PSMClassInterpretedEndParameterConsistency.Key];
            }
            throw new ArgumentException(string.Format("Unknown consistency type: {0}.", consistencyKey), "consistencyKey");
        }
    }
}