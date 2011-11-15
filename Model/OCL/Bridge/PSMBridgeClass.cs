using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Model.OCL.Types;
using Exolutio.Model.PSM;

namespace Exolutio.Model.OCL.Bridge {
    /// <summary>
    /// Represents PSMAssociationMember in OCL type system.
    /// </summary>
    public class PSMBridgeClass : Classifier {
        /// <summary>
        /// Containts source PSMAssociationMember from PSM. 
        /// </summary>
        public PSMAssociationMember PSMSource {
            get;
            private set;
        }

        /// <summary>
        /// Type of source.
        /// </summary>
        public SourceType PSMSourceType {
            get;
            private set;
        }

        /// <summary>
        /// Containts association to parent.
        /// </summary>
        public PSMBridgeAssociation Parent {
            get;
            private set;
        }

        private Dictionary<PSMAssociation, PSMBridgeAssociation> PSMChildMembers {
            get;
            set;
        }

        private Dictionary<PSMAttribute, PSMBridgeAttribute> PSMAttribute {
            get;
            set;
        }

        private PSMBridgeClass(TypesTable.TypesTable tt, string name)
            : base(tt, name, tt.Library.Any) {
                PSMChildMembers = new Dictionary<PSMAssociation, PSMBridgeAssociation>();
            PSMAttribute = new Dictionary<PSMAttribute, PSMBridgeAttribute>();
        }

        public PSMBridgeClass(TypesTable.TypesTable tt, PSMClass sourceClass)
            : this(tt, sourceClass.Name) {
            this.PSMSource = sourceClass;
            this.PSMSourceType = SourceType.PSMClass;
            
        }

        public PSMBridgeClass(TypesTable.TypesTable tt, PSMContentModel sourceContentModel)
            : this (tt, GetContentModelOCLName(sourceContentModel)) {
            this.PSMSource = sourceContentModel;
            this.PSMSourceType = SourceType.PSMContentModel;
        }

        /// <summary>
        /// Tries find instance of PSMBridgeAssociation associated with <paramref name="ass"/> from PSM.
        /// </summary>
        /// <exception cref="KeyNotFoundException"><paramref name="ass"/> not exists in this class.</exception>
        public PSMBridgeAssociation FindChild(PSMAssociation ass) {
            return PSMChildMembers[ass];
        }


        /// <summary>
        /// Tries find instance of PSMBridgeAttribute associated with <paramref name="att"/> from PSM.
        /// </summary>
        /// <exception cref="KeyNotFoundException"><paramref name="att"/> not exists in this class.</exception>
        public PSMBridgeAttribute FindAttribute(PSMAttribute att) {
            return PSMAttribute[att];
        }

        internal void TranslateMembers() {
            // property
            if (PSMSource is PSMClass) {
                PSMClass sourceClass = (PSMClass)PSMSource;
                foreach (var pr in sourceClass.PSMAttributes) {
                    Classifier propType = TypeTable.Library.RootNamespace.NestedClassifier[pr.AttributeType.Name];
                    PSMBridgeAttribute newProp = new PSMBridgeAttribute(pr, PropertyType.One, propType);
                    Properties.Add(newProp);
                    //Registred to FindProperty
                    PSMAttribute.Add(pr, newProp);
                    //Hack
                    newProp.Tag = pr;
                }
            }

            //parent
            PSM.PSMAssociation parentAss = PSMSource.ParentAssociation;
            if (parentAss != null) {
                string parentName = null;
                string defaultName = null;
                List<string> namesInOcl = new List<string>();

                if (parentAss.Parent is PSM.PSMClass) {
                    parentName = parentAss.Parent.Name;
                    defaultName = parentName;
                    namesInOcl.Add(parentName);   
                }
                else if (parentAss.Parent is PSM.PSMContentModel) {
                    parentName = GetContentModelOCLName((PSM.PSMContentModel)parentAss.Parent);
                    defaultName = "parent";
                }

                namesInOcl.Add("parent");

                if (parentName != null) {
                    Classifier propType = TypeTable.Library.RootNamespace.NestedClassifier[parentName];
                    PSMBridgeAssociation newProp = new PSMBridgeAssociation(defaultName, namesInOcl, parentAss, PSMBridgeAssociation.AssociationDirection.Up, PropertyType.One, propType);
                    Properties.Add(newProp);
                    //Hack
                    newProp.Tag = parentAss.Parent;

                    this.Parent = newProp;
                }
            }

            int childCount = 0;
            Dictionary<PSM.PSMContentModelType, int> childContentModelCount = new Dictionary<PSM.PSMContentModelType, int>();
            childContentModelCount[PSM.PSMContentModelType.Choice] = 0;
            childContentModelCount[PSM.PSMContentModelType.Sequence] = 0;
            childContentModelCount[PSM.PSMContentModelType.Set] = 0;

            //child 
            foreach (var ass in PSMSource.ChildPSMAssociations) {
                string childClassName;
                string defaultName;
                // Association can have more than one name in OCL
                List<string> namesInOcl = new List<string>();

                // Resolve association end type name
                if (ass.Child is PSM.PSMClass) {
                    childClassName = ass.Child.Name;
                    namesInOcl.Add(childClassName);
                    defaultName = childClassName;
                }
                else if (ass.Child is PSM.PSMContentModel) {
                    var cM = (PSM.PSMContentModel)ass.Child;
                    childClassName = GetContentModelOCLName((PSM.PSMContentModel)ass.Child);
                    childContentModelCount[cM.Type] = childContentModelCount[cM.Type] + 1;
                    string cmName = String.Format("{0}_{1}", cM.Type.ToString().ToLower(), childContentModelCount[cM.Type]);
                    namesInOcl.Add(cmName);
                    defaultName = cmName;
                }
                else {
                    System.Diagnostics.Debug.Fail("Nepodporovany typ v PSM.");
                    continue;
                }
               
                bool hasAssName = !string.IsNullOrEmpty(ass.Name);
                if (hasAssName) {
                    namesInOcl.Add(ass.Name);
                    defaultName = ass.Name;
                }
                // Other naming format 
                // child_{type} 
                namesInOcl.Add(String.Format("child_{0}", childClassName));
                // child_{order}
                namesInOcl.Add(string.Format("child_{0}", ++childCount));

                Classifier assType = TypeTable.Library.RootNamespace.NestedClassifier[childClassName];
                Classifier propType;
                if (ass.Upper > 1) {
                    propType = TypeTable.Library.CreateCollection(CollectionKind.Set, assType);
                    TypeTable.RegisterType(propType);
                }
                else {
                    propType = assType;
                }

                PSMBridgeAssociation newAss = new PSMBridgeAssociation( defaultName, namesInOcl,ass, PSMBridgeAssociation.AssociationDirection.Down, PropertyType.One, propType);
                //hack
                newAss.Tag = ass;
                //Registre to find
                PSMChildMembers.Add(ass, newAss);
                //Registred all name in tables
                foreach (string name in namesInOcl) {                 
                    Properties.Add(newAss,name);
                }  
            }
        }

        private static string GetContentModelOCLName(PSM.PSMContentModel c) {
            PSM.PSMAssociation parentAssocitation = c.ParentAssociation;
            string parentName = parentAssocitation.Parent.Name;
            string name = string.Format("__{0}_{1}_{2}", parentName, c.Type.ToString(), parentAssocitation.Index);
            return name;
        }

        /// <summary>
        /// Source type.
        /// </summary>
        public enum SourceType {
            PSMClass,PSMContentModel
        }
    }
}
