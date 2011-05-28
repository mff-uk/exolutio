using System;
using Exolutio.Model.PIM;
using Exolutio.Model.PSM;

namespace Exolutio.Model
{
    public abstract class ModelWorker<TResult>
    {
        public virtual TResult DefaultResult
        {
            get { return default(TResult); }
        }

        public TResult ProcessComponent(Component component)
        {
            if (component is PIMComponent)
            {
                if (component is PIMClass)
                {
                    return ProcessPIMClass((PIMClass) component);
                }
                if (component is PIMAssociation)
                {
                    return ProcessPIMAssociation((PIMAssociation) component);
                }
                if (component is PIMAttribute)
                {
                    return ProcessPIMAttribute((PIMAttribute) component);
                }
                if (component is PIMAssociationEnd)
                {
                    return ProcessPIMAssociationEnd((PIMAssociationEnd) component);
                }
            }
            if (component is PSMComponent)
            {
                if (component is PSMAssociation)
                {
                    return ProcessPSMAssociation((PSMAssociation) component);
                }
                if (component is PSMAttribute)
                {
                    return ProcessPSMAttribute((PSMAttribute) component);
                }
                if (component is PSMClass)
                {
                    return ProcessPSMClass((PSMClass) component);
                }
                if (component is PSMContentModel)
                {
                    return ProcessPSMContentModel((PSMContentModel) component);
                }
                if (component is PSMSchemaClass)
                {
                    return ProcessPSMSchemaClass((PSMSchemaClass) component);
                }
            }
            throw new ArgumentOutOfRangeException("component", "Unknown component type.");
        }

        public virtual TResult ProcessPSMSchemaClass(PSMSchemaClass psmSchemaClass)
        {
            return DefaultResult;
        }

        public virtual TResult ProcessPSMContentModel(PSMContentModel psmContentModel)
        {
            return DefaultResult;
        }

        public virtual TResult ProcessPSMClass(PSMClass psmClass)
        {
            return DefaultResult;
        }

        public virtual TResult ProcessPSMAttribute(PSMAttribute psmAttribute)
        {
            return DefaultResult;
        }

        public TResult ProcessPSMAssociationMember(PSMAssociationMember psmAssociationMember)
        {
            if (psmAssociationMember is PSMClass)
            {
                return ProcessPSMClass((PSMClass) psmAssociationMember);
            }
            if (psmAssociationMember is PSMContentModel)
            {
                return ProcessPSMContentModel((PSMContentModel) psmAssociationMember);
            }
            if (psmAssociationMember is PSMSchemaClass)
            {
                return ProcessPSMSchemaClass((PSMSchemaClass) psmAssociationMember);
            }
            throw new ArgumentOutOfRangeException("psmAssociationMember", "Unknown component type.");
        }

        public virtual TResult ProcessPSMAssociation(PSMAssociation psmAssociation)
        {
            return DefaultResult;
        }

        public virtual TResult ProcessPIMAssociationEnd(PIMAssociationEnd pimAssociationEnd)
        {
            return DefaultResult;
        }

        public virtual TResult ProcessPIMAttribute(PIMAttribute pimAttribute)
        {
            return DefaultResult;
        }

        public virtual TResult ProcessPIMAssociation(PIMAssociation pimAssociation)
        {
            return DefaultResult;
        }

        public virtual TResult ProcessPIMClass(PIMClass pimClass)
        {
            return DefaultResult;
        }

        public TResult ProcessComponentOrSchema(ExolutioObject @object)
        {
            if (@object is Component)
            {
                return ProcessComponent((Component) @object);
            }
            if (@object is Schema)
            {
                return ProcessSchema((Schema) @object);
            }
            throw new ArgumentOutOfRangeException("object", "Unknown component type.");
        }

        public TResult ProcessSchema(Schema schema)
        {
            if (schema is PIMSchema)
            {
                return ProcessPIMSchema((PIMSchema)schema);
            }
            if (schema is PSMSchema)
            {
                return ProcessPSMSchema((PSMSchema)schema);
            }
            throw new ArgumentOutOfRangeException("schema", "Unknown component type.");
        }

        public virtual TResult ProcessPSMSchema(PSMSchema psmSchema)
        {
            return DefaultResult;
        }

        public virtual TResult ProcessPIMSchema(PIMSchema pimSchema)
        {
            return DefaultResult;
        }
    }
}