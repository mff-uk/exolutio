﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Linq;
using EvoX.Model;
using EvoX.Model.PSM;
using Component = EvoX.Model.Component;

namespace EvoX.Model.ViewHelper
{
	/// <summary>
	/// ViewHelper class stores view-specific data for an instance of an element in the diagram
	/// (such as its position on the diagram or width and height of the element). These data 
	/// are not part of the UML model itself, but need to be saved and loaded to reconstruct a 
	/// previously saved diagram. 
	/// </summary>
	public class PSMContentModelViewHelper : ViewHelper, IComponentViewHelper
	{
	    public PSMContentModelViewHelper(Diagram diagram)
	        : base(diagram)
	    {
	    }

	    private Guid contentModelGuid;
	    public PSMContentModel ContentModel
	    {
	        get { return Project.TranslateComponent<PSMContentModel>(contentModelGuid); }
	        set { contentModelGuid = value; NotifyPropertyChanged("PSMContentModel"); }
	    }

	    public Component Component
	    {
	        get { return ContentModel; }
	        set { ContentModel = (PSMContentModel) value; }
	    }

        public override Versioning.IEvoXCloneable Clone(ProjectVersion projectVersion, Versioning.ElementCopiesMap createdCopies)
        {
            return new PSMContentModelViewHelper(projectVersion.Project.TranslateComponent<Diagram>(createdCopies.GetGuidForCopyOf(Diagram)));
        }

        public override void FillCopy(Versioning.IEvoXCloneable copyComponent, ProjectVersion projectVersion, Versioning.ElementCopiesMap createdCopies)
        {
            base.FillCopy(copyComponent, projectVersion, createdCopies);
			PSMContentModelViewHelper copyPSMContentModelViewHelper = (PSMContentModelViewHelper) copyComponent;
		}

        public override void Serialize(XElement parentNode, Serialization.SerializationContext context)
        {
            base.Serialize(parentNode, context);
        }

        public override void Deserialize(XElement parentNode, Serialization.SerializationContext context)
        {
            base.Deserialize(parentNode, context);
        }
	}
}
