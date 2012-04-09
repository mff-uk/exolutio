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

        private PSMBridgeClass(TypesTable.TypesTable tt, Namespace ns, string name, Classifier parent = null)
            : base(tt, ns, name, parent ?? tt.Library.Any) {
            PSMChildMembers = new Dictionary<PSMAssociation, PSMBridgeAssociation>();
            PSMAttribute = new Dictionary<PSMAttribute, PSMBridgeAttribute>();
        }

        public PSMBridgeClass(TypesTable.TypesTable tt, Namespace ns, PSMClass sourceClass, PSMBridgeClass parent = null)
            : this(tt, ns, sourceClass.Name, parent) {
            this.PSMSource = sourceClass;
            this.PSMSourceType = SourceType.PSMClass;

        }

        public PSMBridgeClass(TypesTable.TypesTable tt, Namespace ns, PSMContentModel sourceContentModel)
            : this(tt, ns, GetContentModelOCLName(sourceContentModel)) {
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
                    Classifier baseType = TypeTable.Library.RootNamespace.NestedClassifier[pr.AttributeType.Name];
                    Classifier propType = BridgeHelpers.GetTypeByCardinality(TypeTable, pr, baseType);
                    PSMBridgeAttribute newProp = new PSMBridgeAttribute(pr, PropertyType.One, propType);
                    Properties.Add(newProp);
                    //Registred to FindProperty
                    PSMAttribute.Add(pr, newProp);
                    //Hack
                    newProp.Tag = pr;
                }
            }

            // allInstances
            {
                Operation allInstancesOp = new Operation(@"allInstances", true, this);
                Operations.Add(allInstancesOp);
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
                    defaultName = PSMBridgeAssociation.PARENT_STEP;
                }

                namesInOcl.Add(PSMBridgeAssociation.PARENT_STEP);

                if (parentName != null) {
                    Classifier propType = TypeTable.Library.RootNamespace.NestedClassifier[parentName];
                    PSMBridgeAssociation newProp = new PSMBridgeAssociation(defaultName, namesInOcl, parentAss, PSMBridgeAssociation.AssociationDirection.Up, PropertyType.One, propType);
                    //Properties.Add(newProp);
                    namesInOcl.ForEach(name => Properties.Add(newProp, name));
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
                namesInOcl.Add(String.Format(PSMBridgeAssociation.CHILD_N_STEP, childClassName));
                // child_{order}
                namesInOcl.Add(string.Format(PSMBridgeAssociation.CHILD_N_STEP, ++childCount));

                Classifier assType = TypeTable.Library.RootNamespace.NestedClassifier[childClassName];
                Classifier propType = BridgeHelpers.GetTypeByCardinality(TypeTable, ass, assType);

                PSMBridgeAssociation newAss = new PSMBridgeAssociation(defaultName, namesInOcl, ass, PSMBridgeAssociation.AssociationDirection.Down, PropertyType.One, propType);
                //hack
                newAss.Tag = ass;
                //Registre to find
                PSMChildMembers.Add(ass, newAss);
                //Registred all name in tables
                foreach (string name in namesInOcl) {
                    Properties.Add(newAss, name);
                }
            }
        }

        /// <summary>
        /// Adds members included from structural representatives 
        /// </summary>
        internal void IncludeSRMembers(PSMBridgeClass representedClass)
        {
            foreach (KeyValuePair<PSMAttribute, PSMBridgeAttribute> kvp in representedClass.PSMAttribute)
            {
                PSMBridgeAttribute copy = new PSMBridgeAttribute(kvp.Key, kvp.Value.PropertyType, kvp.Value.Type);
                PSMAttribute.Add(kvp.Key, copy);
                copy.Tag = kvp.Value.Tag;
                Properties.Add(copy);
            }
            foreach (KeyValuePair<PSMAssociation, PSMBridgeAssociation> kvp in PSMChildMembers)
            {
                if (kvp.Value.Direction == PSMBridgeAssociation.AssociationDirection.Down)
                {
                    PSMBridgeAssociation copy = new PSMBridgeAssociation(kvp.Value.DefaultName, kvp.Value.Aliases, kvp.Value.SourceAsscociation, kvp.Value.Direction, kvp.Value.PropertyType, kvp.Value.Type);
                    PSMChildMembers.Add(kvp.Key, copy);
                    copy.Tag = kvp.Value.Tag;
                    Properties.Add(copy, kvp.Value.Name);
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
            PSMClass, PSMContentModel
        }
    }
}
