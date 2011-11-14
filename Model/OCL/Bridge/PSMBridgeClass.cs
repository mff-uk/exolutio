using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Model.OCL.Types;
using Exolutio.Model.PSM;

namespace Exolutio.Model.OCL.Bridge {
    public class PSMBridgeClass : Classifier {

        public PSMAssociationMember PSMSource {
            get;
            private set;
        }

        public SourceType PSMSourceType {
            get;
            private set;
        }

        public PSMBridgeAssociation Parent {
            get;
            private set;
        }

        private Dictionary<PSMAssociation, PSMBridgeAssociation> PSMChildMembers {
            get;
            set;
        }

        private Dictionary<PSMAttribute, PSMBridgeAttribute> PSMPropety {
            get;
            set;
        }

        private PSMBridgeClass(TypesTable.TypesTable tt, string name)
            : base(tt, name, tt.Library.Any) {
                PSMChildMembers = new Dictionary<PSMAssociation, PSMBridgeAssociation>();
            PSMPropety = new Dictionary<PSMAttribute, PSMBridgeAttribute>();
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
        /// Gets the class from type associated with the PIM class.
        /// </summary>
        /// <exception cref="KeyNotFoundException">PSM association not exists in this class.</exception>
        public PSMBridgeAssociation FindChild(PSMAssociation ass) {
            return PSMChildMembers[ass];
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
                    PSMPropety.Add(pr, newProp);
                    //Hack
                    newProp.Tag = pr;
                }
            }

            //parent
            PSM.PSMAssociation parentAss = PSMSource.ParentAssociation;
            if (parentAss != null) {
                string parentName = null;
                if (parentAss.Parent is PSM.PSMClass) {
                    parentName = parentAss.Parent.Name;
                }
                else if (parentAss.Parent is PSM.PSMContentModel) {
                    parentName = GetContentModelOCLName((PSM.PSMContentModel)parentAss.Parent);
                }

                if (parentName != null) {
                    Classifier propType = TypeTable.Library.RootNamespace.NestedClassifier[parentName];
                    PSMBridgeAssociation newProp = new PSMBridgeAssociation("parent",new List<string>(new string[] {"parent"}), parentAss, PSMBridgeAssociation.AssociationDirection.Up, PropertyType.One, propType);
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
                // Association can have more than one name in OCL
                List<string> namesInOcl = new List<string>();

                // Resolve names
                if (ass.Child is PSM.PSMClass) {
                    childClassName = ass.Child.Name;
                }
                else if (ass.Child is PSM.PSMContentModel) {
                    var cM = (PSM.PSMContentModel)ass.Child;
                    childClassName = GetContentModelOCLName((PSM.PSMContentModel)ass.Child);
                    childContentModelCount[cM.Type] = childContentModelCount[cM.Type] + 1;
                    namesInOcl.Add(String.Format("{0}_{1}", cM.Type.ToString().ToLower(), childContentModelCount[cM.Type]));
                }
                else {
                    System.Diagnostics.Debug.Fail("Nepodporovany typ v PSM.");
                    continue;
                }

                string childName = string.Format("child_{0}", ++childCount);
                namesInOcl.Add(childName);

                if (string.IsNullOrEmpty(ass.Name) == false) {
                    namesInOcl.Add(ass.Name);
                }
                else {
                    namesInOcl.Add(String.Format("child_{0}", childClassName));
                }

                Classifier assType = TypeTable.Library.RootNamespace.NestedClassifier[childClassName];
                Classifier propType;
                if (ass.Upper > 1) {
                    propType = TypeTable.Library.CreateCollection(CollectionKind.Set, assType);
                    TypeTable.RegisterType(propType);
                }
                else {
                    propType = assType;
                }

                PSMBridgeAssociation newAss = new PSMBridgeAssociation(childName, namesInOcl,ass, PSMBridgeAssociation.AssociationDirection.Down, PropertyType.One, propType);
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

        public enum SourceType {
            PSMClass,PSMContentModel
        }
    }
}
