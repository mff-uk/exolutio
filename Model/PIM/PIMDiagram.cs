using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Exolutio.Model.Serialization;
using Exolutio.Model.Versioning;
using Exolutio.Model.ViewHelper;

namespace Exolutio.Model.PIM
{
    public class PIMDiagram: Diagram
    {
        public PIMSchema PIMSchema
        {
            get { return (PIMSchema) Schema; }
            set { Schema = value; }
        }

        public PIMDiagram(Project p) : base(p)
        {
            
        }

        public PIMDiagram(Project p, Guid g) : base(p, g)
        {
            
        }

        public override void AddFactoryMethods()
        {
            viewHelperFactoryMethods[typeof(PIMClass)] = delegate { return new PIMClassViewHelper(this); };
            viewHelperFactoryMethods[typeof(PIMAssociation)] = delegate { return new PIMAssociationViewHelper(this); };
        }

        public IEnumerable<PIMComponent> PIMComponents
        {
            get { return Components.Cast<PIMComponent>(); }
        }

        public override void LoadSchemaToDiagram(Schema schema, bool bindingOnly = false)
        {
            base.LoadSchemaToDiagram(schema, bindingOnly);
            PIMSchema pimSchema = (PIMSchema)schema;

            if (!bindingOnly)
            {
                Caption = pimSchema.Caption;

                foreach (PIMComponent pimComponent in ModelIterator.GetPIMComponents(pimSchema))
                {
                    if (pimComponent.IsOfType(typeof (PIMClass), typeof (PIMAssociation)))
                    {
                        Components.Add(pimComponent);
                    }
                }
            }

            PIMSchema.ComponentRemoved += Components_ComponentRemoved;
        }

        private void Components_ComponentRemoved(Schema psmschema, Component component)
        {
            if (viewHelperFactoryMethods.ContainsKey(component.GetType()))
            {
                Components.Remove(component);
                ViewHelpers.Remove(component);
            }
        }

        #region Implementation of IExolutioCloneable

        public override IExolutioCloneable Clone(ProjectVersion projectVersion, ElementCopiesMap createdCopies)
        {
            return new PIMDiagram(projectVersion.Project, createdCopies.SuggestGuid(this));
        }

        public override void FillCopy(IExolutioCloneable copyComponent, ProjectVersion projectVersion,
                                      ElementCopiesMap createdCopies)
        {
            base.FillCopy(copyComponent, projectVersion, createdCopies);

            PIMDiagram copyPIMDiagram = (PIMDiagram)copyComponent;
            copyPIMDiagram.SetProjectVersion(projectVersion);
        }

        #endregion

        public static PIMDiagram CreateInstance(Project project)
        {
            return new PIMDiagram(project, Guid.Empty);
        }

        public override void Serialize(XElement parentNode, SerializationContext context)
        {
            base.Serialize(parentNode, context);
        }

        public override void Deserialize(XElement parentNode, SerializationContext context)
        {
            base.Deserialize(parentNode, context);
            LoadSchemaToDiagram(PIMSchema, true);
        }
    }
}