using System;
using System.Xml;
using System.Xml.Linq;
using EvoX.Model.Serialization;
using EvoX.SupportingClasses;

namespace EvoX.Model.Versioning
{
    public class Version : EvoXObject
    {
        public Version(Project p) : base(p) { }
        public Version(Project p, Guid g) : base(p, g) { }
        
        private string label;
        public string Label
        {
            get { return label; }
            set { label = value; NotifyPropertyChanged("Label"); }
        }

        private int number;
        public int Number
        {
            get { return number; }
            set
            {
                number = value;
                NotifyPropertyChanged("Number");
                if (string.IsNullOrEmpty(Label))
                {
                    Label = string.Format("v{0}", number);
                }
            }
        }


        private Guid branchedFromGuid;
        public Version BranchedFrom
        {
            get { return branchedFromGuid != Guid.Empty ? Project.TranslateComponent<Version>(branchedFromGuid) : null; }
            set
            {
                branchedFromGuid = value != null ? value : Guid.Empty;
            }
        }

        private readonly EvoXList<IVersionedItem> items = new EvoXList<IVersionedItem>();

        /// <summary>
        /// Contains items in this version.
        /// </summary>
        public EvoXList<IVersionedItem> Items
        {
            get { return items; }
        }

        public override string ToString()
        {
            return Label;
        }

        #region IEvoXSerializable Members

        public override void Serialize(XElement parentNode, SerializationContext context)
        {
            base.Serialize(parentNode, context);

            if (!string.IsNullOrEmpty(Label))
            {
                this.SerializeSimpleValueToAttribute("Label", Label, parentNode, context);
            }

            this.SerializeSimpleValueToAttribute("Number", Number, parentNode, context);
            
            if (BranchedFrom != null)
            {
                this.SerializeIDRef(BranchedFrom, "branchedFromVerID", parentNode, context);
            }
        }

        public override void Deserialize(XElement parentNode, SerializationContext context)
        {
            base.Deserialize(parentNode, context);

            if (parentNode.Attribute("Label") != null)
            {
                Label = SerializationContext.DecodeString(parentNode.Attribute("Label").Value);
            }

            if (parentNode.Attribute("Number") != null)
            {
                Number = SerializationContext.DecodeInt(parentNode.Attribute("Number").Value);
            }

            branchedFromGuid = this.DeserializeIDRef("branchedFromVerID", parentNode, context, true);
        }

        public static Version CreateInstance(Project project)
        {
            return new Version(project, Guid.Empty);
        }

        #endregion

        public void NotifyItemAdded(Schema psmSchema, Component component)
        {
            if (psmSchema.Version != this)
            {
                throw new EvoXModelException();
            }
            NotifyItemAdded(component);
        }

        public void NotifyItemAdded(IVersionedItem item)
        {
            if (item.Version != this)
            {
                throw new EvoXModelException();
            }
            if (Items.Contains(item))
            {
                throw new EvoXModelException();
            }
            Items.Add(item);
            Project.VersionManager.AddVersionedItem(item);
        }

        public void NotifyItemRemoved(Schema psmSchema, Component component)
        {
            if (psmSchema.Version != this)
            {
                throw new EvoXModelException();
            }
            NotifyItemRemoved(component);
        }

        public void NotifyItemRemoved(IVersionedItem item)
        {
            if (item.Version != this)
            {
                throw new EvoXModelException();
            }
            if (!this.Items.Contains(item))
            {
                throw new EvoXModelException();
            }
            Items.Remove(item);
            Project.VersionManager.RemoveVersionedItem(item);
        }
    }
}