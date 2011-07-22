using System;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Exolutio.Model.PIM;
using Exolutio.Model.PSM;
using Exolutio.Model.Serialization;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Exolutio.Model.Versioning;
using Version = Exolutio.Model.Versioning.Version;

namespace Exolutio.Model
{
    public class ProjectVersion : ExolutioObject
    {
        public ProjectVersion(Project p)
            : base(p)
        {
            InitCollections();
        }

        public ProjectVersion(Project p, Guid g) : base(p, g)
        {
            InitCollections();
        }

        private void InitCollections()
        {
            PSMSchemas = new UndirectCollection<PSMSchema>(Project);
            PSMSchemas.MemberAdded += member => member.SetProjectVersion(this);
            PSMSchemas.MemberRemoved += member => member.SetProjectVersion(null);

            PIMDiagrams = new UndirectCollection<PIMDiagram>(Project);
            PIMDiagrams.MemberAdded += member => member.SetProjectVersion(this);
            PIMDiagrams.MemberRemoved += member => member.SetProjectVersion(null);
            PSMDiagrams = new UndirectCollection<PSMDiagram>(Project);
            PSMDiagrams.MemberAdded += member => member.SetProjectVersion(this);
            PSMDiagrams.MemberRemoved += member => member.SetProjectVersion(null);

            AttributeTypes = new UndirectCollection<AttributeType>(Project);
            AttributeTypes.MemberAdded += type => type.SetProjectVersion(this);
            AttributeTypes.MemberRemoved += type => type.SetProjectVersion(null);

            
        }

        

        public UndirectCollection<AttributeType> AttributeTypes { get; private set; }
        
        private Guid pimSchemaGuid;
        public PIMSchema PIMSchema
        {
            get { return Project.TranslateComponent<PIMSchema>(pimSchemaGuid); }
            set { pimSchemaGuid = value; value.SetProjectVersion(this); }
        }

        public UndirectCollection<PSMSchema> PSMSchemas { get; private set; }

        public UndirectCollection<PIMDiagram> PIMDiagrams { get; private set; }

        public UndirectCollection<PSMDiagram> PSMDiagrams { get; private set; }

        public IEnumerable<Diagram> Diagrams
        {
            get { return PIMDiagrams.Cast<Diagram>().Union(PSMDiagrams.Cast<Diagram>()); }
        }

        private Guid versionGuid;
        public Version Version
        {
            get
            {
                return versionGuid != Guid.Empty ? Project.TranslateComponent<Version>(versionGuid) : null;
            }
            set { versionGuid = value != null ? value : Guid.Empty; }
        }

        public void CreateDiagramsForSchemas()
        {
            if (PIMDiagrams.Count == 0)
            {
                PIMDiagram pimDiagram = new PIMDiagram(Project);
                PIMDiagrams.Add(pimDiagram);
                pimDiagram.LoadSchemaToDiagram(PIMSchema);
            }

            if (PSMDiagrams.Count == 0)
            {
                foreach (PSMSchema psmSchema in PSMSchemas)
                {
                    PSMDiagram psmDiagram = new PSMDiagram(Project);
                    PSMDiagrams.Add(psmDiagram);
                    psmDiagram.LoadSchemaToDiagram(psmSchema);
                }
            }
            
            if (Project.UsesVersioning)
            {
                // Create version links between diagrams where schemas are linked
                foreach (ProjectVersion otherVersion in Project.ProjectVersions)
                {
                    if (otherVersion == this)
                        continue;
                    foreach (Diagram diagramFromOtherVersion in otherVersion.Diagrams)
                    {
                        foreach (Diagram diagramFromThisVersion in Diagrams)
                        {
                            if (Project.VersionManager.AreItemsLinked(diagramFromOtherVersion.Schema, diagramFromThisVersion.Schema)
                                && !Project.VersionManager.AreItemsLinked(diagramFromOtherVersion, diagramFromThisVersion))
                            {
                                Project.VersionManager.RegisterVersionLink(this.Version, otherVersion.Version, diagramFromThisVersion, diagramFromOtherVersion);
                            }
                        }
                    }
                }
            }
        }

        #region IExolutioSerializable Members

        public override void Serialize(XElement parentNode, SerializationContext context)
        {
            base.Serialize(parentNode, context);
            if (Project.UsesVersioning)
            {
                this.SerializeIDRef(Version, "versionID", parentNode, context);
            }

            this.WrapAndSerializeCollection("AttributeTypes", "AttributeType", AttributeTypes, parentNode, context);

            this.SerializeToChildElement("PIMSchema", PIMSchema, parentNode, context);

            this.WrapAndSerializeCollection("PSMSchemas", "PSMSchema", PSMSchemas, parentNode, context);

            this.WrapAndSerializeCollection("PIMDiagrams", "PIMDiagram", PIMDiagrams, parentNode, context);

            this.WrapAndSerializeCollection("PSMDiagrams", "PSMDiagram", PSMDiagrams, parentNode, context);
        }

        public override void Deserialize(XElement parentNode, SerializationContext context)
        {
            base.Deserialize(parentNode, context);
            context.CurrentProjectVersion = this;

            if (Project.UsesVersioning)
            {
                if (parentNode.Attribute("versionID") == null)
                {
                    context.Log.AddErrorFormat("Missing attribute 'versionID' in node {0}", parentNode);
                }
                versionGuid = this.DeserializeIDRef("versionID", parentNode, context);
            }
            else
            {
                if (parentNode.Attribute("versionID") != null)
                {
                    context.Log.AddErrorFormat("Attribute 'versionID' should not be present in node {0} because the project does not use versioning.", parentNode);
                }
            }

            this.DeserializeWrappedCollection("AttributeTypes", AttributeTypes, AttributeType.CreateInstance, parentNode, context);

            PIMSchema objPIMSchema = new PIMSchema(Project, Guid.Empty);
            objPIMSchema.DeserializeFromChildElement("PIMSchema", parentNode, context);
            PIMSchema = objPIMSchema;

            this.DeserializeWrappedCollection("PSMSchemas", PSMSchemas, PSMSchema.CreateInstance, parentNode, context);

            this.DeserializeWrappedCollection("PIMDiagrams", PIMDiagrams, PIMDiagram.CreateInstance, parentNode, context);

            this.DeserializeWrappedCollection("PSMDiagrams", PSMDiagrams, PSMDiagram.CreateInstance, parentNode, context);

            context.CurrentProjectVersion = null;
        }
       
        public static ProjectVersion CreateInstance(Project project)
        {
            return new ProjectVersion(project, Guid.Empty);
        }

        #endregion

        #region Implementation of IExolutioCloneable

        public override IExolutioCloneable Clone(ProjectVersion projectVersion, ElementCopiesMap createdCopies)
        {
            return new ProjectVersion(projectVersion.Project, createdCopies.SuggestGuid(this));
        }

        public override void FillCopy(IExolutioCloneable copyComponent, ProjectVersion projectVersion,
                                      ElementCopiesMap createdCopies)
        {
            base.FillCopy(copyComponent, projectVersion, createdCopies);

            ProjectVersion copyProjectVersion = (ProjectVersion)copyComponent;
            copyProjectVersion.versionGuid = projectVersion.versionGuid;

            this.CopyCollection(AttributeTypes, copyProjectVersion.AttributeTypes, projectVersion, createdCopies);
            copyProjectVersion.PIMSchema = PIMSchema.CreateTypedCopy(projectVersion, createdCopies);
            this.CopyCollection(PSMSchemas, copyProjectVersion.PSMSchemas, projectVersion, createdCopies);
            this.CopyCollection(PIMDiagrams, copyProjectVersion.PIMDiagrams, projectVersion, createdCopies);
            this.CopyCollection(PSMDiagrams, copyProjectVersion.PSMDiagrams, projectVersion, createdCopies);
        }

        #endregion
    }
}
