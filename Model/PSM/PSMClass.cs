using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Xml;
using System.Xml.Linq;
using Exolutio.Model.PSM.XPath;
using Exolutio.Model.Serialization;
using Exolutio.Model.Versioning;

namespace Exolutio.Model.PSM
{
    public class PSMClass : PSMAssociationMember, IPSMSemanticComponent
    {
        public PSMClass(Project p) : base(p)
        {
            InitializeCollections(); 
        }

        public PSMClass(Project p, Guid g) : base(p, g)
        {
            InitializeCollections();
        }

        public PSMClass(Project p, PSMSchema schema, bool isRoot) : base(p)
        {
            InitializeCollections();
            if (isRoot)
            {
                schema.Roots.Add(this);
            }
            schema.PSMClasses.Add(this);
        }

        public PSMClass(Project p, PSMSchema schema, int rootIndex = -1)
            : base(p)
        {
            InitializeCollections();
            if (rootIndex == -1)
            {
                schema.Roots.Add(this);
            }
            else
            {
                schema.Roots.Insert(this, rootIndex);
            }
            schema.PSMClasses.Add(this);
        }

        public PSMClass(Project p, Guid g, PSMSchema schema, int rootIndex = -1)
            : base(p, g)
        {
            InitializeCollections();
            if (rootIndex == -1)
            {
                schema.Roots.Add(this);
            }
            else
            {
                schema.Roots.Insert(this, rootIndex);
            }
            schema.PSMClasses.Add(this);
        }

        private void InitializeCollections()
        {
            PSMAttributes =new UndirectCollection<PSMAttribute>(Project);
            GeneralizationsAsGeneral = new UndirectCollection<PSMGeneralization>(Project);
        }

        private Guid representedClassGuid;

        /// <summary>
        /// Gets the PSM Class represented by this structural representative. Returns
        /// <c>null</c> for classes that are not structural representatives.
        /// </summary>
        public PSMClass RepresentedClass
        {
            get
            {
                return representedClassGuid == Guid.Empty ? null : Project.TranslateComponent<PSMClass>(representedClassGuid);
            }
            set 
            {
                // TODO: class must be from the same schema or from a referenced schema
                representedClassGuid = value == null ? Guid.Empty : value; NotifyPropertyChanged("RepresentedClass"); NotifyPropertyChanged("IsStructuralRepresentative");
            }
        }

        /// <summary>
        /// Returns <c>true</c> if the class is a structural representative of another class
        /// </summary>
        /// <seealso cref="RepresentedClass"/>
        /// <seealso cref="HasStructuralRepresentatives"/>
        public bool IsStructuralRepresentative
        {
            get { return RepresentedClass != null; }
        }

        /// <summary>
        /// Returns <c>true</c> if structural representatives of this class exist (may also reside in other 
        /// <see cref="PSMSchema"/>s).
        /// </summary>
        public bool HasStructuralRepresentatives
        {
            get { return Project.mappingDictionary.Values.OfType<PSMClass>().Any(c => c.RepresentedClass == this); }
        }

        public ReadOnlyCollection<PSMClass> Representants
        {
            get
            {
                List<PSMClass> reprs = new List<PSMClass>();

                foreach (PSMSchema s in ProjectVersion.PSMSchemas)
                {
                    foreach (PSMClass c in s.PSMClasses)
                    {
                        if (c.RepresentedClass == this) reprs.Add(c);
                    }
                }
                
                return reprs.AsReadOnly();
            }
        }

        public UndirectCollection<PSMAttribute> PSMAttributes { get; private set; }

        public UndirectCollection<PSMGeneralization> GeneralizationsAsGeneral { get; private set; }

        private Guid generalizationAsSpecificGuid;
        public PSMGeneralization GeneralizationAsSpecific
        {
            get { return generalizationAsSpecificGuid == Guid.Empty ? null : Project.TranslateComponent<PSMGeneralization>(generalizationAsSpecificGuid); }
            set { generalizationAsSpecificGuid = value == null ? Guid.Empty : value; NotifyPropertyChanged("GeneralizationAsSpecific"); }
        }


        public IEnumerable<PSMAssociation> GetIncomingNonTreeAssociations()
        {
            return PSMSchema.PSMAssociations.Where(a =>
                a.IsNonTreeAssociation && a.Child == this);
        }

        public IEnumerable<PSMAssociation> GetOutgoingNonTreeAssociations()
        {
            return ChildPSMAssociations.Where(a =>
                a.IsNonTreeAssociation && a.Parent == this);
        }

        private bool final = false;
        public bool Final
        {
            get { return final; }
            set { final = value; NotifyPropertyChanged("Final"); }
        }

        private bool abstr = false;
        public bool Abstract
        {
            get { return abstr; }
            set { abstr = value; NotifyPropertyChanged("Abstract"); }
        }
        
        public override string XPath
        {
            get { return ParentAssociation.XPath; }
        }

        public override Path GetXPathFull(bool followGeneralizations = true)
        {
            UnionPath unionPath = new UnionPath();

            List<Path> nonRecursionPaths = new List<Path>();
            
            #region parent track 
            if (ParentAssociation != null)
            {
                Path parentPath = ParentAssociation.GetXPathFull(followGeneralizations).DeepCopy();
                nonRecursionPaths.Add(parentPath);
                unionPath.ComponentPaths.Add(parentPath);
            }
            #endregion 

            #region generalizations tracks
            if (followGeneralizations)
            {
                foreach (PSMGeneralization generalization in this.GeneralizationsAsGeneral)
                {
                    Path specificClassPath = generalization.Specific.GetXPathFull(followGeneralizations);
                    unionPath.ComponentPaths.Add(specificClassPath);
                    nonRecursionPaths.Add(specificClassPath);
                }
            }

            #endregion 

            #region  first find cycles

            List<PSMComponent> componentsParticipatingInCycles = new List<PSMComponent>();
            Dictionary<PSMAssociation, List<ModelIterator.PSMCycle>> cyclesForAssociations
                = new Dictionary<PSMAssociation, List<ModelIterator.PSMCycle>>();
            foreach (PSMAssociation association in ChildPSMAssociations)
            {
                List<ModelIterator.PSMCycle> cycles = ModelIterator.GetPSMCyclesStartingInAssociation(association,
                                                                                                      followGeneralizationsWhereAsGeneral
                                                                                                          : false,
                                                                                                      followGeneralizationsWhereAsSpecific
                                                                                                          : true);
                cyclesForAssociations.Add(association, cycles);

                foreach (ModelIterator.PSMCycle cycle in cycles)
                {
                    componentsParticipatingInCycles.AddRange(cycle);
                }
            }

            #endregion

            #region process incoming NTAs, which then will be also used as prefixes for cycles

            foreach (PSMAssociation incomingNTA in GetIncomingNonTreeAssociations())
            {
                if (componentsParticipatingInCycles.Contains(incomingNTA))
                    continue;
                Path ntaPath = incomingNTA.GetXPathFull(followGeneralizations);
                unionPath.ComponentPaths.Add(ntaPath);
                nonRecursionPaths.Add(ntaPath);
            }

            #endregion

            #region process cycles
            
            List<string> usedDescendants = new List<string>();
            foreach (KeyValuePair<PSMAssociation, List<ModelIterator.PSMCycle>> kvp in cyclesForAssociations)
            {
                PSMAssociation association = kvp.Key;
                List<ModelIterator.PSMCycle> cycles = kvp.Value;

                foreach (ModelIterator.PSMCycle cycle in cycles)
                {
                    PSMAssociation lastNamedAssociation = cycle.GetLastNamedAssociation();
                    string descendant = lastNamedAssociation != null ? lastNamedAssociation.Name : null;
                    if (descendant != null && !usedDescendants.Contains(descendant))
                    {
                        foreach (Path nonRecursionPath in nonRecursionPaths)
                        {
                            Path path = nonRecursionPath.DeepCopy();
                            SimplePath simplePath = path as SimplePath;
                            bool optimized = false;
                            if (simplePath != null && simplePath.Steps.Any())
                            {
                                Step laststep = simplePath.Steps.Last();
                                if (laststep.NodeTest == descendant && laststep.Axis == Axis.child)
                                {
                                    laststep.Axis = Axis.descendant;
                                    optimized = true; 
                                }
                            }
                            if (!optimized)
                            {
                                Step step = new Step {Axis = Axis.descendant, NodeTest = descendant};
                                path.AddStep(step);
                            }
                            unionPath.ComponentPaths.Add(path);
                        }
                        usedDescendants.Add(descendant);
                    }
                }
            }

            #endregion

            if (unionPath.ComponentPaths.Count == 1)
                return unionPath.ComponentPaths[0];
            else
                return unionPath;
        }

        #region Implementation of IExolutioSerializable

        public override void Serialize(XElement parentNode, SerializationContext context)
        {
            base.Serialize(parentNode, context);
            if (IsStructuralRepresentative)
            {
                XElement representsElement = new XElement(context.ExolutioNS + "RepresentedClass");
                this.SerializeIDRef(RepresentedClass, "representedPSMClassId", representsElement, context);
                parentNode.Add(representsElement);
            }
            this.WrapAndSerializeIDRefCollection("GeneralizationsAsGeneral", "PSMGeneralization", "psmGeneralizationsAsGeneralID", GeneralizationsAsGeneral,
                                                 parentNode, context);
            if (GeneralizationAsSpecific != null) this.SerializeIDRef(GeneralizationAsSpecific, "psmGeneralizationAsSpecificID", parentNode, context);

            this.WrapAndSerializeCollection("PSMAttributes", "PSMAttribute", PSMAttributes, parentNode, context);
            this.SerializeSimpleValueToAttribute("abstract", Abstract, parentNode, context);
            this.SerializeSimpleValueToAttribute("final", Final, parentNode, context);
        }

        public override void Deserialize(XElement parentNode, SerializationContext context)
        {
            base.Deserialize(parentNode, context);
            XElement representedClassElement = parentNode.Element(context.ExolutioNS + "RepresentedClass");
            if (representedClassElement != null)
            {
                representedClassGuid = this.DeserializeIDRef("representedPSMClassId", representedClassElement, context, true);
            }
            
            this.DeserializeWrappedIDRefCollection("GeneralizationsAsGeneral", "psmGeneralizationsAsGeneralID", GeneralizationsAsGeneral, parentNode, context);
            generalizationAsSpecificGuid = this.DeserializeIDRef("psmGeneralizationAsSpecificID", parentNode, context, true);
            this.DeserializeWrappedCollection("PSMAttributes", PSMAttributes, PSMAttribute.CreateInstance, parentNode, context);

            bool succeeded, result;
            result = Boolean.TryParse(this.DeserializeSimpleValueFromAttribute("abstract", parentNode, context, true), out succeeded);
            Abstract = succeeded ? result : false;
            result = Boolean.TryParse(this.DeserializeSimpleValueFromAttribute("final", parentNode, context, true), out succeeded);
            Final = succeeded ? result : false;
        }
        public static PSMClass CreateInstance(Project project)
        {
            return new PSMClass(project, Guid.Empty);
        }

        #endregion

        public override string ToString()
        {
            return "PSMClass: \"" + Name + "\"";
        }

        #region Implementation of IExolutioCloneable

        public override IExolutioCloneable Clone(ProjectVersion projectVersion, ElementCopiesMap createdCopies)
        {
            return new PSMClass(projectVersion.Project, createdCopies.SuggestGuid(this));
        }

        public override void FillCopy(IExolutioCloneable copyComponent, ProjectVersion projectVersion,
                                      ElementCopiesMap createdCopies)
        {
            base.FillCopy(copyComponent, projectVersion, createdCopies);

            PSMClass copyPSMClass = (PSMClass) copyComponent;
            if (RepresentedClass != null)
            {
                copyPSMClass.representedClassGuid = createdCopies.GetGuidForCopyOf(RepresentedClass);
            }
            this.CopyCollection(PSMAttributes, copyPSMClass.PSMAttributes, projectVersion, createdCopies);
            copyPSMClass.Abstract = this.Abstract;
            copyPSMClass.Final = this.Final;

        }

        #endregion
    }
}