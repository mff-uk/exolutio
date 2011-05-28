using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;
using Exolutio.Model.Serialization;
using System.Collections.ObjectModel;
using Exolutio.Model.Versioning;
using Exolutio.Model.PSM;
using System.Text;

namespace Exolutio.Model.PIM
{
    public class PIMAssociation : PIMComponent
    {
        #region Constructors

        public void InitializeCollections()
        {
            PIMAssociationEnds = new UndirectCollection<PIMAssociationEnd>(Project);
            PIMAssociationEnds.MemberAdded += delegate
                                                  {
                                                      NotifyPropertyChanged("PIMAssociationEnds");
                                                      NotifyPropertyChanged("PIMClasses");
                                                  };
            PIMAssociationEnds.MemberRemoved += delegate
                                                    {
                                                        NotifyPropertyChanged("PIMAssociationEnds");
                                                        NotifyPropertyChanged("PIMClasses");
                                                    };
        }

        public PIMAssociation(Project p) : base(p) { InitializeCollections(); }
        public PIMAssociation(Project p, Guid g) : base(p, g) { InitializeCollections(); }
        public PIMAssociation(Project p, PIMSchema schema)
            : this(p, schema, new PIMAssociationEnd[0])
        {

        }
        public PIMAssociation(Project p, PIMSchema schema, params PIMAssociationEnd[] ends)
            : base(p)
        {
            InitializeCollections();

            schema.PIMAssociations.Add(this);
            foreach (PIMAssociationEnd e in ends)
            {
                e.PIMAssociation = this;
                PIMAssociationEnds.Add(e);
            }
        }
        public PIMAssociation(Project p, Guid g, PIMSchema schema, params PIMAssociationEnd[] ends)
            : base(p, g)
        {
            InitializeCollections();

            schema.PIMAssociations.Add(this);
            foreach (PIMAssociationEnd e in ends)
            {
                e.PIMAssociation = this;
                PIMAssociationEnds.Add(e);
            }
        }
        public PIMAssociation(Project p, PIMSchema schema, params PIMClass[] classes)
            : base(p)
        {
            InitializeCollections();

            schema.PIMAssociations.Add(this);
            foreach (PIMClass c in classes)
            {
                PIMAssociationEnd e = new PIMAssociationEnd(Project, c, this, schema);
            }
        }
        public PIMAssociation(Project p, Guid g, PIMSchema schema, params PIMClass[] classes)
            : base(p, g)
        {
            InitializeCollections();

            schema.PIMAssociations.Add(this);
            foreach (PIMClass c in classes)
            {
                PIMAssociationEnd e = new PIMAssociationEnd(Project, c, this, schema);
            }
        }
        public PIMAssociation(Project p, PIMSchema schema, params KeyValuePair<PIMClass, Guid>[] classesAndEnds)
            : base(p)
        {
            InitializeCollections();

            schema.PIMAssociations.Add(this);
            foreach (KeyValuePair<PIMClass, Guid> pair in classesAndEnds)
            {
                PIMAssociationEnd e = new PIMAssociationEnd(Project, pair.Value, pair.Key, this, schema);
            }
        }
        public PIMAssociation(Project p, Guid g, PIMSchema schema, params KeyValuePair<PIMClass, Guid>[] classesAndEnds)
            : base(p, g)
        {
            InitializeCollections();

            schema.PIMAssociations.Add(this);
            foreach (KeyValuePair<PIMClass, Guid> pair in classesAndEnds)
            {
                PIMAssociationEnd e = new PIMAssociationEnd(Project, pair.Value, pair.Key, this, schema);
            }
        }
        #endregion

        public UndirectCollection<PIMAssociationEnd> PIMAssociationEnds { get; private set; }

        public ComponentList<PIMClass> PIMClasses
        {
            get
            {
                ComponentList<PIMClass> List = new ComponentList<PIMClass>();

                foreach (PIMAssociationEnd pimAssociationEnd in PIMAssociationEnds)
                {
                    PIMClass participant = pimAssociationEnd.PIMClass;
                    if (participant != null) List.Add(participant);
                }

                return List;
            }
        }

        public override ComponentList<PSMComponent> GetInterpretedComponents()
        {
            ComponentList<PSMComponent> list = new ComponentList<PSMComponent>();
            foreach (PSMSchema schema in Project.LatestVersion.PSMSchemas)
            {
                foreach (PSMAssociation a in schema.PSMAssociations)
                {
                    if (a.Interpretation == this) list.Add(a);
                }
            }
            return list;
        }

        #region Implementation of IExolutioSerializable


        public override void Serialize(XElement parentNode, Serialization.SerializationContext context)
        {
            base.Serialize(parentNode, context);
            this.WrapAndSerializeCollection("PIMAssociationEnds", "PIMAssociationEnd", PIMAssociationEnds, parentNode, context);
        }

        public override void Deserialize(XElement parentNode, SerializationContext context)
        {
            base.Deserialize(parentNode, context);
            this.DeserializeWrappedCollection("PIMAssociationEnds", PIMAssociationEnds, PIMAssociationEnd.CreateInstance, parentNode, context);
        }

        public static PIMAssociation CreateInstance(Project project)
        {
            return new PIMAssociation(project, Guid.Empty);
        }

        #endregion

        public override string ToString()
        {
            StringBuilder s = new StringBuilder("PIMAssociation");
            s.Append(" \"" + Name + "\"");
            s.Append(':');
            foreach (PIMClass c in PIMClasses)
            {
                s.Append(' ');
                s.Append(c.ToString());
                s.Append(',');
                /*if (c.Name != null) s.Append(" " + c.Name + " ");
                else s.Append("NONAMEPIMCLASS ");*/
            }
            s.Remove(s.Length - 1, 1);
            return s.ToString();
        }

        #region Implementation of IExolutioCloneable

        public override IExolutioCloneable Clone(ProjectVersion projectVersion, ElementCopiesMap createdCopies)
        {
            return new PIMAssociation(projectVersion.Project, createdCopies.SuggestGuid(this));
        }

        public override void FillCopy(IExolutioCloneable copyComponent, ProjectVersion projectVersion,
                                      ElementCopiesMap createdCopies)
        {
            base.FillCopy(copyComponent, projectVersion, createdCopies);
            PIMAssociation copyPIMAssociation = (PIMAssociation)copyComponent;

            this.CopyCollection(PIMAssociationEnds, copyPIMAssociation.PIMAssociationEnds, projectVersion, createdCopies);
        }

        #endregion
    }
}
