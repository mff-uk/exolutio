using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Exolutio.Model.Serialization;
using Exolutio.Model.Versioning;
using Exolutio.SupportingClasses;

namespace Exolutio.Model
{
	public class Stereotype: ExolutioObject
	{
		public Stereotype(Project p) : base(p)
		{
		}

		public Stereotype(Project p, Guid g) : base(p, g)
		{
		
		}

		private string name;
		public string Name
		{
			get { return name; }
			set
			{
				name = value;
				NotifyPropertyChanged("Name");
			}
		}

		public override string ToString()
		{
			return this.Name;
		}
	}

	public class StereotypeInstance : ExolutioVersionedObject
	{
		public StereotypeInstance(Project p)
			: base(p)
		{
		}

		public StereotypeInstance(Project p, Guid g)
			: base(p, g)
		{
		}

		public StereotypeInstance(Project p, Guid g, Stereotype stereotype)
			: base(p, g)
		{
			Stereotype = stereotype;
		}

		private Guid stereotypeGuid;

		public Stereotype Stereotype
		{
			get { return Project.TranslateComponent<Stereotype>(stereotypeGuid); }
			set
			{
				stereotypeGuid = value;
				NotifyPropertyChanged("Stereotype");
			}
		}

		private Guid componentGuid;

		public Component Component
		{
			get { return Project.TranslateComponent<Component>(componentGuid); }
			set
			{
				componentGuid = value;
				NotifyPropertyChanged("Component");
			}
		}

		public override ProjectVersion ProjectVersion
		{
			get { return Component.ProjectVersion; }
		}

		internal static StereotypeInstance CreateInstance(Project project)
		{
			return new StereotypeInstance(project, Guid.Empty);
		}

		public override void Deserialize(XElement parentNode, SerializationContext context)
		{
			base.Deserialize(parentNode, context);

			this.stereotypeGuid = this.DeserializeIDRef("StereotypeID", parentNode, context);
			this.componentGuid = context.TagGuid.Value;
		}

		public override void Serialize(XElement parentNode, SerializationContext context)
		{
			base.Serialize(parentNode, context);

			this.SerializeIDRef(Stereotype, "StereotypeID", parentNode, context);
		}

		public override void FillCopy(IExolutioCloneable copyComponent, ProjectVersion projectVersion,
		                              ElementCopiesMap createdCopies)
		{
			base.FillCopy(copyComponent, projectVersion, createdCopies);

			StereotypeInstance copyStereotypeInstance = (StereotypeInstance)copyComponent;
			copyStereotypeInstance.componentGuid = createdCopies.GetGuidForCopyOf(Component);
			copyStereotypeInstance.stereotypeGuid = this.stereotypeGuid;
		}

		public override IExolutioCloneable Clone(ProjectVersion projectVersion, ElementCopiesMap createdCopies)
		{
			return new StereotypeInstance(projectVersion.Project, createdCopies.SuggestGuid(this));
		}

		public override string ToString()
		{
			return (this.Stereotype != null ? this.Stereotype.Name : "Stereotype") + "Instance";
		}
	}

	public static class StereotypeExt
	{
		public static string GetStereotypesString(this IEnumerable<StereotypeInstance> stereotypes, string separator = ",")
		{
			return stereotypes.ConcatWithSeparator(aps => "<<" + aps.Stereotype.Name + ">>", separator);
		}
	}

}