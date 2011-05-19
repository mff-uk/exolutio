using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml;
using System.Xml.Linq;
using EvoX.Model;
using EvoX.Model.Serialization;
using EvoX.Model.Versioning;
using Component = EvoX.Model.Component;

namespace EvoX.Model.ViewHelper
{
	/// <summary>
	/// Stores visualization information of an element on a diagram
	/// </summary>
	public abstract class ViewHelper : INotifyPropertyChanged, IEvoXSerializable, IEvoXCloneable
	{
        private Diagram diagram;

		public Diagram Diagram
		{
			get
			{
				return diagram;
			}
			private set
			{
				diagram = value;
				NotifyPropertyChanged("Diagram");
			}
		}

		protected ViewHelper()
		{
		}

		protected ViewHelper(Diagram diagram)
		{
			Diagram = diagram;
		}

		public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propertyName)
		{
			NotifyPropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}

        public void NotifyPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(sender, e);
			}
		}

	    public virtual IEvoXCloneable Clone(ProjectVersion projectVersion, ElementCopiesMap createdCopies)
	    {
	        throw new NotImplementedException(string.Format("Clone is not implemented for type {0}.", this.GetType().Name));
	    }

	    public IEvoXCloneable CreateCopy(ProjectVersion projectVersion, ElementCopiesMap createdCopies)
        {
            IEvoXCloneable copyElement = Clone(projectVersion, createdCopies);
            FillCopy(copyElement, projectVersion, createdCopies);
            return copyElement;
        }

	    public virtual void FillCopy(IEvoXCloneable copyComponent, ProjectVersion projectVersion, ElementCopiesMap createdCopies)
        {
            if (copyComponent == null)
            {
                throw new ArgumentNullException("copyComponent");
            }
            if (this.GetType() != copyComponent.GetType())
            {
                throw new EvoXModelException(string.Format("FillCopy called on objects of different type '{0}' and '{1}'.", this.GetType().FullName, copyComponent.GetType().FullName));
            }
        }

        #region Implementation of IEvoXSerializable

	    public Project Project
	    {
	        get { return Diagram.Project; }
	    }

        public virtual void Serialize(XElement parentNode, SerializationContext context)
        {

        }

        public virtual void Deserialize(XElement parentNode, SerializationContext context)
        {
            
        }

	    #endregion

	    
	    public Guid ID
	    {
	        get { throw new NotImplementedException(); }
	    }
	}

    public delegate ViewHelper ViewHelperFactoryMethodDelegate();
}