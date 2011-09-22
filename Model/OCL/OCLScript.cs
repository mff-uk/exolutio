using System;
using System.Xml.Linq;
using Exolutio.Model.Serialization;
using Exolutio.Model.Versioning;


namespace Exolutio.Model.OCL
{
    public class OCLScript : Component
    {
        #region constructors
     
        public OCLScript(Project p)
            : base(p)
        {
            
        }

        public OCLScript(Project p, Guid g)
            : base(p, g)
        {

        }

        public OCLScript(Project p, Guid g, Schema schema)
            : base(p, g)
        {
            this.Schema = schema;
            schema.OCLScripts.Add(this);
        }

        #endregion

        public string Contents { get; set; }

        public void Compile() {
        }

        #region Implementation of IExolutioSerializable

        public override void Serialize(System.Xml.Linq.XElement parentNode, Serialization.SerializationContext context)
        {
            base.Serialize(parentNode, context);
            this.SerializeSimpleValueToCDATA("Contents", Contents, parentNode, context);    
        }

        public override void Deserialize(System.Xml.Linq.XElement parentNode, Serialization.SerializationContext context)
        {
            base.Deserialize(parentNode, context);
            this.Contents = this.DeserializeSimpleValueFromCDATA("Contents", parentNode, context);
        }

        #endregion 
        
        #region Implementation of IExolutioCloneable

        public override IExolutioCloneable Clone(ProjectVersion projectVersion, ElementCopiesMap createdCopies)
        {
            return new OCLScript(projectVersion.Project, createdCopies.SuggestGuid(this));
        }

        public override void FillCopy(IExolutioCloneable copyComponent, ProjectVersion projectVersion,
                                      ElementCopiesMap createdCopies)
        {
            base.FillCopy(copyComponent, projectVersion, createdCopies);

            OCLScript copyOCLScript = (OCLScript)copyComponent;
            copyOCLScript.Contents = this.Contents;
        }

        #endregion

        public static OCLScript CreateInstance(Project project)
        {
            return new OCLScript(project, Guid.Empty);
        }
    }
}