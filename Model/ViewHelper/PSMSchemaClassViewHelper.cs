using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Linq;
using Exolutio.Model;
using Exolutio.Model.PSM;
using Component = Exolutio.Model.Component;

namespace Exolutio.Model.ViewHelper
{
	/// <summary>
	/// ViewHelper class stores view-specific data for an instance of an element in the diagram
	/// (such as its position on the diagram or width and height of the element). These data 
	/// are not part of the UML model itself, but need to be saved and loaded to reconstruct a 
	/// previously saved diagram. 
	/// </summary>
    public class PSMSchemaClassViewHelper : ViewHelper, IComponentViewHelper, IFoldableComponentViewHelper
	{
	    public PSMSchemaClassViewHelper(Diagram diagram)
	        : base(diagram)
	    {
	    }

	    private Guid classGuid;
	    public PSMSchemaClass Class
	    {
	        get { return Project.TranslateComponent<PSMSchemaClass>(classGuid); }
	        set { classGuid = value; NotifyPropertyChanged("PSMSchemaClass"); }
	    }

	    public Component Component
	    {
	        get { return Class; }
	        set { Class = (PSMSchemaClass) value; }
	    }

        public override Versioning.IExolutioCloneable Clone(ProjectVersion projectVersion, Versioning.ElementCopiesMap createdCopies)
        {
            return new PSMSchemaClassViewHelper(projectVersion.Project.TranslateComponent<Diagram>(createdCopies.GetGuidForCopyOf(Diagram)));
		}

        public override void FillCopy(Versioning.IExolutioCloneable copyComponent, ProjectVersion projectVersion, Versioning.ElementCopiesMap createdCopies)
        {
            base.FillCopy(copyComponent, projectVersion, createdCopies);
            PSMSchemaClassViewHelper copyPSMSchemaClassViewHelper = (PSMSchemaClassViewHelper) copyComponent;
		}

        public override void Serialize(XElement parentNode, Serialization.SerializationContext context)
        {
            base.Serialize(parentNode, context);
        }

        public override void Deserialize(XElement parentNode, Serialization.SerializationContext context)
        {
            base.Deserialize(parentNode, context);
        }

        public bool CanFold()
        {
            return !IsFolded;
        }

        private bool isFolded;
        public bool IsFolded
        {
            get { return isFolded; }
            set { isFolded = value; NotifyPropertyChanged("IsFolded"); }
        }
	}
}
