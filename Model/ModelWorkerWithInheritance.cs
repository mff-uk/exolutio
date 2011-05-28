using System;
using Exolutio.Model.PIM;
using Exolutio.Model.PSM;

namespace Exolutio.Model
{
    public abstract class ModelWorkerWithInheritance<TResult, TContext>
    {
        protected abstract TResult GetResult(TContext context);

        protected abstract TContext CreateStartingContext();

        public TResult Process(object @object)
        {
            TContext context = CreateStartingContext();
            if (@object is PIMComponent)
            {
                if (@object is PIMClass)
                {
                    ProcessPIMClass((PIMClass) @object, ref context);
                }
                if (@object is PIMAssociation)
                {
                    ProcessPIMAssociation((PIMAssociation)@object, ref context);
                }
                if (@object is PIMAttribute)
                {
                    ProcessPIMAttribute((PIMAttribute)@object, ref context);
                }
                if (@object is PIMAssociationEnd)
                {
                    ProcessPIMAssociationEnd((PIMAssociationEnd)@object, ref context);
                }
            }
            else if (@object is PSMComponent)
            {
                if (@object is PSMAssociation)
                {
                    ProcessPSMAssociation((PSMAssociation)@object, ref context);
                }
                if (@object is PSMAttribute)
                {
                    ProcessPSMAttribute((PSMAttribute)@object, ref context);
                }
                if (@object is PSMClass)
                {
                    ProcessPSMClass((PSMClass)@object, ref context);
                }
                if (@object is PSMContentModel)
                {
                    ProcessPSMContentModel((PSMContentModel)@object, ref context);
                }
                if (@object is PSMSchemaClass)
                {
                    ProcessPSMSchemaClass((PSMSchemaClass)@object, ref context);
                }
            }
            else if (@object is PSMSchema)
            {
                ProcessPSMSchema((PSMSchema)@object, ref context);
            } 
            else if (@object is PIMSchema)
            {
                ProcessPIMSchema((PIMSchema)@object, ref context);
            }
            else 
                throw new ArgumentOutOfRangeException("object", "Unknown component type.");

            return GetResult(context);
        }

        public virtual void ProcessPSMSchemaClass(PSMSchemaClass psmSchemaClass, ref TContext context)
        {
            ProcessPSMAssociationMember(psmSchemaClass, ref context);
        }

        public virtual void ProcessPSMContentModel(PSMContentModel psmContentModel, ref TContext context)
        {
            ProcessPSMAssociationMember(psmContentModel, ref context);
        }

        public virtual void ProcessPSMClass(PSMClass psmClass, ref TContext context)
        {
            ProcessPSMAssociationMember(psmClass, ref context);
        }

        public virtual void ProcessPSMAttribute(PSMAttribute psmAttribute, ref TContext context)
        {
            ProcessPSMComponent(psmAttribute, ref context);
        }

        public virtual void ProcessPSMComponent(PSMComponent psmComponent, ref TContext context)
        {
            ProcessComponent(psmComponent, ref context);
        }

        public virtual void ProcessComponent(Component component, ref TContext context)
        {
            ProcessObject(component, ref context);
        }

        public virtual void ProcessObject(object @object, ref TContext context)
        {
            
        }

        public virtual void ProcessPSMAssociationMember(PSMAssociationMember psmAssociationMember, ref TContext context)
        {
            ProcessPSMComponent(psmAssociationMember, ref context);
        }

        public virtual void ProcessPSMAssociation(PSMAssociation psmAssociation, ref TContext context)
        {
            ProcessPSMComponent(psmAssociation, ref context);
        }

        public virtual void ProcessPIMAssociationEnd(PIMAssociationEnd pimAssociationEnd, ref TContext context)
        {
            ProcessPIMComponent(pimAssociationEnd, ref context);
        }

        private void ProcessPIMComponent(PIMComponent pimComponent, ref TContext context)
        {
            ProcessComponent(pimComponent, ref context);
        }

        public virtual void ProcessPIMAttribute(PIMAttribute pimAttribute, ref TContext context)
        {
            ProcessPIMComponent(pimAttribute, ref context);
        }

        public virtual void ProcessPIMAssociation(PIMAssociation pimAssociation, ref TContext context)
        {
            ProcessPIMComponent(pimAssociation, ref context);
        }

        public virtual void ProcessPIMClass(PIMClass pimClass, ref TContext context)
        {
            ProcessPIMComponent(pimClass, ref context);
        }

        public virtual void ProcessSchema(Schema schema, ref TContext context)
        {
            ProcessObject(schema, ref context);
        }

        public virtual void ProcessPSMSchema(PSMSchema psmSchema, ref TContext context)
        {
            ProcessSchema(psmSchema, ref context);
        }

        public virtual void ProcessPIMSchema(PIMSchema pimSchema, ref TContext context)
        {
            ProcessSchema(pimSchema, ref context);
        }
    }
}