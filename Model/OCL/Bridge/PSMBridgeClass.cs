using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Model.OCL.ConstraintConversion;
using Exolutio.Model.OCL.Types;
using Exolutio.Model.PSM;
using Exolutio.SupportingClasses;

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

        public PSMBridgeClass(TypesTable.TypesTable tt, Namespace ns, PSMClass sourceClass, PSMBridgeClass parent = null, string nameOverride = null)
            : this(tt, ns, nameOverride ?? sourceClass.Name, parent) {
            this.PSMSource = sourceClass;
            this.PSMSourceType = SourceType.PSMClass;

        }

        public PSMBridgeClass(TypesTable.TypesTable tt, Namespace ns, PSMContentModel sourceContentModel, string registerName)
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

        internal void TranslateMembers(PSMBridge bridge, bool translateAsOldVersion = false)
        {
            // property
            if (PSMSource is PSMClass)
            {
                PSMClass sourceClass = (PSMClass)PSMSource;
                foreach (var pr in sourceClass.PSMAttributes)
                {
                    if (pr.AttributeType == null)
                        throw new ExolutioException(string.Format("Type of attribute `{0}` is not specified. ", pr)) { ExceptionTitle = "OCL parsing can not continue"};
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
                CollectionType allInstancesReturn = TypeTable.Library.CreateCollection(CollectionKind.Set, this);
                Operation allInstancesOp = new Operation(@"allInstances", true, allInstancesReturn);
                Operations.Add(allInstancesOp);
            }

            // oclAsType 
            //{
            //    Operation oclAsTypeOp = new Operation(@"oclAsType", true, this.TypeTable.Library.Any,
            //        new Parameter[] { new Parameter("type", this.TypeTable.Library.Type) });
            //    oclAsTypeOp.ReturnTypeDependsOnArguments = true;
            //    Operations.Add(oclAsTypeOp);
            //}

            //skip
            if (PSMSource is PSMClass && PSMSource.Interpretation != null)
            {
                PSMClass sourceClass = (PSMClass)PSMSource;
                IEnumerable<PSMComponent> components = 
                    ((Exolutio.Model.PIM.PIMClass)sourceClass.Interpretation).GetInterpretedComponents().Where(ic => ic != sourceClass 
                        && ic.PSMSchema == sourceClass.PSMSchema); 
                foreach (PSMClass targetClass in components)
                {
                    SkipOperationTag tag = new SkipOperationTag { Source = this, Target = bridge.PSMAssociationMembers[targetClass] }; 
                    Operation skipOp = new Operation(string.Format(@"to{0}", targetClass.Name), true, tag.Target);
                    skipOp.Tag = tag;
                    Operations.Add(skipOp);        
                }
            }

            // J.M.: 20.4.2012 - update, there may be other incoming associations besides parent, 
            // - non tree associations. These are treated the same as parent associations. 
            List<PSMAssociation> incomingAssociations = new List<PSMAssociation>();
            //parent
            if (PSMSource.ParentAssociation != null)
                incomingAssociations.Add(PSMSource.ParentAssociation);
            if (PSMSource is PSMClass)
            {
                incomingAssociations.AddRange(((PSMClass)PSMSource).GetIncomingNonTreeAssociations().Where(a => a.IsNonTreeAssociation));
            }

            foreach (PSMAssociation incomingAssociation in incomingAssociations)
            {
                string parentName = null;
                string defaultName = null;
                List<string> namesInOcl = new List<string>();

                if (incomingAssociation.Parent is PSM.PSMClass)
                {
                    parentName = incomingAssociation.Parent.Name;
                    defaultName = parentName;
                    namesInOcl.Add(parentName);
                }
                else if (incomingAssociation.Parent is PSM.PSMContentModel)
                {
                    parentName = GetContentModelOCLName((PSM.PSMContentModel)incomingAssociation.Parent);
                    defaultName = PSMBridgeAssociation.PARENT_STEP;
                }

                if (incomingAssociation.IsNamed)
                {
                    namesInOcl.Add(incomingAssociation.Name);
                }
                namesInOcl.Add(PSMBridgeAssociation.PARENT_STEP);

                if (parentName != null)
                {
                    Classifier propType = TypeTable.Library.RootNamespace.NestedClassifier[parentName];
                    PSMBridgeAssociation newProp = new PSMBridgeAssociation(defaultName, namesInOcl, incomingAssociation,
                                                                            PSMBridgeAssociation.AssociationDirection.Up,
                                                                            PropertyType.One, propType);
                    //Properties.Add(newProp);
                    namesInOcl.ForEach(name => Properties.Add(newProp, name));
                    //Hack
                    newProp.Tag = incomingAssociation.Parent;

                    this.Parent = newProp;
                }
            }

            int childCount = 0;
            Dictionary<PSM.PSMContentModelType, int> childContentModelCount = new Dictionary<PSM.PSMContentModelType, int>();
            childContentModelCount[PSM.PSMContentModelType.Choice] = 0;
            childContentModelCount[PSM.PSMContentModelType.Sequence] = 0;
            childContentModelCount[PSM.PSMContentModelType.Set] = 0;

            //child 
            foreach (var ass in PSMSource.ChildPSMAssociations)
            {
                string childClassName;
                string defaultName;
                // Association can have more than one name in OCL
                List<string> namesInOcl = new List<string>();

                // Resolve association end type name
                if (ass.Child is PSM.PSMClass)
                {
                    childClassName = ass.Child.Name;
                    if (translateAsOldVersion)
                        childClassName += @"_old";
                    namesInOcl.Add(childClassName);
                    defaultName = childClassName;
                }
                else if (ass.Child is PSM.PSMContentModel)
                {
                    var cM = (PSM.PSMContentModel)ass.Child;
                    childClassName = GetContentModelOCLName((PSM.PSMContentModel)ass.Child);
                    childClassName += @"_old";
                    childContentModelCount[cM.Type] = childContentModelCount[cM.Type] + 1;
                    string cmName = String.Format("{0}_{1}", cM.Type.ToString().ToLower(), childContentModelCount[cM.Type]);
                    namesInOcl.Add(cmName);
                    defaultName = cmName;
                }
                else
                {
                    System.Diagnostics.Debug.Fail("Nepodporovany typ v PSM.");
                    continue;
                }

                if (ass.IsNamed)
                {
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
                foreach (string name in namesInOcl.Distinct())
                {
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

        internal static string GetContentModelOCLName(PSM.PSMContentModel c) {
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

    public abstract class OperationTag
    {

    }

    /// <summary>
    /// Used by "skip" operaions, i.e. toEmployee()
    /// </summary>
    public class SkipOperationTag: OperationTag
    {
        public PSMBridgeClass Source { get; set; }
        public PSMBridgeClass Target { get; set; } 
    }

    /// <summary>
    /// Used to traverse between versions (from target to source version)
    /// </summary>
    public class PrevOperationTag: OperationTag
    {
        public Classifier SourceVersionClassifier { get; set; }
        public Classifier TargetVersionClassifier { get; set; }
    }

    /// <summary>
    /// Used to traverse between versions (from source to target version)
    /// </summary>
    public class NextOperationTag : OperationTag
    {
        public Classifier SourceVersionClassifier { get; set; }
        public Classifier TargetVersionClassifier { get; set; }
    }
}
