using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml;
using System.Xml.Linq;
using Exolutio.Model;
using Exolutio.Model.Serialization;
using Exolutio.Model.Versioning;
using Component = Exolutio.Model.Component;

namespace Exolutio.Model.ViewHelper
{
	/// <summary>
	/// Stores visualization information of an element on a diagram
	/// </summary>
	public abstract class ViewHelper : INotifyPropertyChanged, IExolutioSerializable, IExolutioCloneable
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

            //if (this is IFoldableComponentViewHelper)
            //{
            //    ((IFoldableComponentViewHelper) copyComponent).IsFolded = ((IFoldableComponentViewHelper) this).IsFolded;
            //}
        }

        #region Implementation of IExolutioSerializable

	    public Project Project
	    {
	        get { return Diagram.Project; }
	    }

        public virtual void Serialize(XElement parentNode, SerializationContext context)
        {
            //if (this is IFoldableComponentViewHelper)
            //{
            //    this.SerializeSimpleValueToElement("IsFolded", ((IFoldableComponentViewHelper)this).IsFolded, parentNode, context);
            //}
        }

        public virtual void Deserialize(XElement parentNode, SerializationContext context)
        {
            //if (this is IFoldableComponentViewHelper)
            //{
            //    string isFoldedStr = this.DeserializeSimpleValueFromElement("IsFolded", parentNode, context, true);
            //    ((IFoldableComponentViewHelper) this).IsFolded = !string.IsNullOrEmpty(isFoldedStr) && bool.Parse(isFoldedStr);
            //}
        }

	    #endregion

	    
	    public Guid ID
	    {
	        get { throw new NotImplementedException(); }
	    }
	}

    public delegate ViewHelper ViewHelperFactoryMethodDelegate();
}