using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Xml;
using System.Xml.Linq;
using Exolutio.Model.Serialization;
using Exolutio.Model.Versioning;
using Exolutio.Model.ViewHelper;
using Exolutio.SupportingClasses.XML;
using System.ComponentModel;

namespace Exolutio.Model
{
    public abstract class Diagram : ExolutioVersionedObjectNotAPartOfSchema
    {
        protected Diagram(Project p) : base(p)
        {
            AddFactoryMethods();
            InitializeCollections();
        }

        protected Diagram(Project p, Guid g) : base(p, g)
        {
            AddFactoryMethods();
            InitializeCollections();
        }

        private void InitializeCollections()
        {
            Components = new UndirectCollection<Component>(Project);
        }

        public abstract void AddFactoryMethods();

        private string caption;
        public string Caption
        {
            get { return caption; }
            set { caption = value; NotifyPropertyChanged("Caption"); }
        }

        public UndirectCollection<Component> Components { get; private set; }

        public readonly Dictionary<Component, ViewHelper.ViewHelper> viewHelpers = new Dictionary<Component, ViewHelper.ViewHelper>();

        public Dictionary<Component, ViewHelper.ViewHelper> ViewHelpers
        {
            get
            {
                return viewHelpers;
            }
        }

        protected Dictionary<Type, ViewHelperFactoryMethodDelegate> viewHelperFactoryMethods = new Dictionary<Type, ViewHelperFactoryMethodDelegate>();

        public override string ToString()
        {
            return Caption;
        }

        #region Overrides of ExolutioVersionedObject

        private Guid schemaGuid = Guid.Empty;

        public Schema Schema
        {
            get { return schemaGuid != Guid.Empty ? Project.TranslateComponent<Schema>(schemaGuid) : null; }
            set { schemaGuid = value != null ? value.ID : Guid.Empty; }
        }

        #endregion

        #region Implementation of IExolutioCloneable

        public override void FillCopy(IExolutioCloneable copyComponent, ProjectVersion projectVersion,
                                      ElementCopiesMap createdCopies)
        {
            base.FillCopy(copyComponent, projectVersion, createdCopies);

            Diagram copyDiagram = (Diagram)copyComponent;

            copyDiagram.SetProjectVersion(projectVersion);
            copyDiagram.Caption = this.Caption;
            copyDiagram.schemaGuid = createdCopies.GetGuidForCopyOf(this.Schema);
            this.CopyRefCollection(Components, copyDiagram.Components, projectVersion, createdCopies);
            this.CopyDictionary(ViewHelpers, copyDiagram.ViewHelpers, projectVersion, createdCopies);
        }

        #endregion

        #region Implementation of IExolutioSerializable

        public override void Serialize(XElement parentNode, SerializationContext context)
        {
            base.Serialize(parentNode, context);

            this.SerializeIDRef(Schema, "schemaID", parentNode, context);

            XAttribute captionAttribute = new XAttribute("Caption", Caption);
            parentNode.Add(captionAttribute);

            XElement componentsElement = new XElement(context.ExolutioNS + "Components");
            parentNode.Add(componentsElement);
            foreach (KeyValuePair<Component, ViewHelper.ViewHelper> kvp in ViewHelpers)
            {
                XElement componentElement = new XElement(context.ExolutioNS + "Component");
                componentsElement.Add(componentElement);

                XAttribute typeAttribue = new XAttribute("Type", kvp.Key.GetType().Name);
                componentElement.Add(typeAttribue);

                if (kvp.Key.Schema != Guid.Empty)
                {
                    kvp.Key.SerializeIDRef(kvp.Key, "componentID", componentElement, context);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Without schema {0}", kvp.Key.GetType());
                }

                XElement viewHelperNode = new XElement(context.ExolutioNS + "ViewHelper");
                componentElement.Add(viewHelperNode);
                kvp.Value.Serialize(viewHelperNode, context);
            }
        }

        public override void Deserialize(XElement parentNode, SerializationContext context)
        {
            base.Deserialize(parentNode, context);

            schemaGuid = this.DeserializeIDRef("schemaID", parentNode, context);

            if (parentNode.Attribute("Caption") != null)
            {
                Caption = SerializationContext.DecodeString(parentNode.Attribute("Caption").Value);
            }

            foreach (XElement element in parentNode.Element(context.ExolutioNS + "Components").Elements())
            {
                Guid componentGuid = this.DeserializeIDRef("componentID", element, context);

                Component component;
                try
                {
                    
                    component = (Component)Project.TranslateComponent(componentGuid);
                }
                catch (Exception)
                {
                    continue;
                }

                ViewHelper.ViewHelper viewHelper = viewHelperFactoryMethods[Project.TranslateComponent(componentGuid).GetType()]();
                ((IComponentViewHelper)viewHelper).Component = component;
                if (element.Element(context.ExolutioNS + "ViewHelper") != null)
                {
                    viewHelper.Deserialize(element.Element(context.ExolutioNS + "ViewHelper"), context);
                    ViewHelpers[component] = viewHelper;
                }
                Components.Add(component);
            }

            SetProjectVersion(context.CurrentProjectVersion);
        }
        #endregion

        public virtual void LoadSchemaToDiagram(Schema schema, bool bindingOnly = false)
        {
            Schema = schema;
        }

        public virtual void UnloadSchemaFromDiagram()
        {
            
        }
    }

    public class DiagramEventArgs
    {
        public Diagram Diagram { get; set; }
    }
}