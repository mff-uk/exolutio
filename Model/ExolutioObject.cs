using System;
using System.ComponentModel;
using System.Xml;
using System.Xml.Linq;
using Exolutio.Model.Serialization;
using Exolutio.Model.Versioning;

namespace Exolutio.Model
{
    public abstract class ExolutioObject : INotifyPropertyChanged, IExolutioSerializable, IExolutioCloneable
    {
        public static implicit operator Guid(ExolutioObject e)
        {
            return e == null ? Guid.Empty : e.ID;
        }

        protected ExolutioObject(Project p)
        {
            Project = p;
            id = Guid.NewGuid();
            Project.mappingDictionary.Add(id, this);
        }

        protected ExolutioObject(Project p, Guid g)
        {
            Project = p;
            if (g != Guid.Empty)
            {
                id = g;
                Project.mappingDictionary.Add(id, this);
            }
        }

        public Project Project { get; private set; }

        private Guid id;

        public Guid ID
        {
            get { return id; }
            set
            {
                if (id != Guid.Empty)
                {
                    throw new ExolutioModelException("Component ID is already defined and cannot be changed. ");
                }
                else
                {
                    id = value;
                    Project.mappingDictionary.Add(id, this);
                }
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;

        [Localizable(false)]
        protected internal void NotifyPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler tmp = PropertyChanged; 
            if (tmp != null)
            {
                tmp(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #region Implementation of IExolutioSerializable
        
        public virtual void Serialize(XElement parentNode, SerializationContext context)
        {
            this.SerializeIDRef(this, "ID", parentNode, context, false);
        }

        public virtual void Deserialize(XElement parentNode, SerializationContext context)
        {
            this.ID = this.DeserializeIDRef("ID", parentNode, context);
        }
        #endregion

        #region Implementation of IExolutioCloneable

        public virtual IExolutioCloneable Clone(ProjectVersion projectVersion, ElementCopiesMap createdCopies)
        {
            throw new NotImplementedException(string.Format("Clone is not implemented for type {0}.", this.GetType().Name));
        }

        public IExolutioCloneable CreateCopy(ProjectVersion projectVersion, ElementCopiesMap createdCopies)
        {
            IExolutioCloneable copyElement = Clone(projectVersion, createdCopies);
            FillCopy(copyElement, projectVersion, createdCopies);
            return copyElement;
        }

        public virtual void FillCopy(IExolutioCloneable copyComponent, ProjectVersion projectVersion, ElementCopiesMap createdCopies)
        {
            if (copyComponent == null)
            {
                throw new ArgumentNullException("copyComponent");
            }
            if (this.GetType() != copyComponent.GetType())
            {
                throw new ExolutioModelException(string.Format("FillCopy called on objects of different type '{0}' and '{1}'.", this.GetType().FullName, copyComponent.GetType().FullName));
            }
        }

        #endregion
    }
}
