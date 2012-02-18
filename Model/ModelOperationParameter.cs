using System;
using System.Xml.Linq;
using Exolutio.Model.Serialization;

namespace Exolutio.Model
{
    public class ModelOperationParameter: ExolutioObject, IExolutioSerializable
    {
        public ModelOperationParameter(Project p) : base(p)
        {
            MakeUpIdIfNotDeserialized = true; 
        }

        public ModelOperationParameter(Project p, Guid g) : base(p, g)
        {
            MakeUpIdIfNotDeserialized = true; 
        }

        public ModelOperation ModelOperation { get; set; }

        private Guid typeGuid;

        public AttributeType Type
        {
            get
            {
                return typeGuid != Guid.Empty
                           ? ModelOperation.Project.TranslateComponent<AttributeType>(typeGuid)
                           : null;
            }
            set
            {
                if (value != null)
                {
                    typeGuid = value;
                }
                else
                {
                    typeGuid = Guid.Empty;
                }
                if (ModelOperation != null)
                {
                    ModelOperation.NotifyPropertyChanged("Parameters");
                }
            }
        }

        private string name;
        public string Name
        {
            get { return name; }
            set
            {
                name = value;
                if (ModelOperation != null)
                {
                    ModelOperation.NotifyPropertyChanged("Parameters");
                }
            }
        }

        public override void Deserialize(XElement parentNode, SerializationContext context)
        {
            base.Deserialize(parentNode, context);
            this.Name = this.DeserializeSimpleValueFromAttribute("Name", parentNode, context);
            this.typeGuid = this.DeserializeAttributeType(parentNode, context, optional:true);
        }

        public override void Serialize(XElement parentNode, SerializationContext context)
        {
            base.Serialize(parentNode, context);
            this.SerializeSimpleValueToAttribute("Name", Name, parentNode, context);
            if (Type != null)
            {
                this.SerializeAttributeType(Type, parentNode, context);
            }
        }

        public static ModelOperationParameter CreateInstance(Project project)
        {
            return new ModelOperationParameter(project, Guid.Empty);
        }


        public new Project Project
        {
            get { return ModelOperation.Project; }
        }
    }
}