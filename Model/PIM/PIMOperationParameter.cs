using System;
using System.Xml.Linq;
using Exolutio.Model.Serialization;

namespace Exolutio.Model.PIM
{
    public class PIMOperationParameter: IExolutioSerializable
    {
        public PIMOperation PIMOperation { get; set; }

        private Guid typeGuid;

        public AttributeType Type
        {
            get
            {
                return typeGuid != Guid.Empty
                           ? PIMOperation.Project.TranslateComponent<AttributeType>(typeGuid)
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
                if (PIMOperation != null)
                {
                    PIMOperation.NotifyPropertyChanged("Parameters");
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
                if (PIMOperation != null)
                {
                    PIMOperation.NotifyPropertyChanged("Parameters");
                }
            }
        }

        public void Deserialize(XElement parentNode, SerializationContext context)
        {
            this.Name = this.DeserializeSimpleValueFromAttribute("Name", parentNode, context);
            this.typeGuid = this.DeserializeAttributeType(parentNode, context, optional:true);
        }

        public void Serialize(XElement parentNode, SerializationContext context)
        {
            this.SerializeSimpleValueToAttribute("Name", Name, parentNode, context);
            if (Type != null)
            {
                this.SerializeAttributeType(Type, parentNode, context);
            }
        }

        public static PIMOperationParameter CreateInstance(Project project)
        {
            return new PIMOperationParameter();
        }


        public Project Project
        {
            get { return PIMOperation.Project; }
        }
    }
}