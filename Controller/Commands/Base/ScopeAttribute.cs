using System;

namespace Exolutio.Controller.Commands
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class, AllowMultiple = false)]
    public class ScopeAttribute: System.Attribute
    {
        [Flags]
        public enum EScope
        {
            None = 0,
            PIMAttribute = 1,
            PIMAssociationEnd = 2,
            PIMAssociation = 4,
            PIMClass = 8,
            PIMDiagram = 16,
            PSMClass = 32,
            PSMAssociation = 64,
            PSMContentModel = 128,
            PSMSchema = 256,
            PSMAttribute = 512,
            PSMSchemaClass = 1024,
            PIMGeneralization = 2048,
            PIM = PIMAttribute | PIMAssociationEnd | PIMAssociation | PIMClass | PIMDiagram | PIMGeneralization,
            PSM = PSMAttribute | PSMAssociation | PSMClass | PSMSchema | PSMSchemaClass | PSMContentModel
        }

        public EScope Scope { get; set; }

        public ScopeAttribute(EScope scope)
        {
            Scope = scope;
        }
    }
}