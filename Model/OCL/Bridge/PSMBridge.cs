using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Model.OCL.Types;
using Exolutio.Model.PSM;
using Exolutio.Model.OCL.TypesTable;

namespace Exolutio.Model.OCL.Bridge {
    public class PSMBridge:IBridgeToOCL {
         public PSMSchema Schema {
            get;
            private set;
        }

        public TypesTable.TypesTable TypesTable {
            get;
            private set;
        }

        public TypesTable.Library Library {
            get {
                return this.TypesTable.Library;
            }
        }

        private Dictionary<PSM.PSMAssociationMember, Types.Class> PSMAssociationMembers {
            get;
            set;
        }

        private Dictionary<AttributeType, Types.Classifier> PSMAttributeType {
            get;
            set;
        }


        public PSMBridge(PSMSchema schema) {
            PSMAssociationMembers = new Dictionary<PSMAssociationMember, Class>();
            PSMAttributeType = new Dictionary<AttributeType, Classifier>();
            this.Schema = schema;
            CreateTypesTable();
        }

        /// <summary>
        /// Gets the class from type associated with the PIM class.
        /// </summary>
        /// <exception cref="KeyNotFoundException">PIM class does not exist in collection.</exception>
        public Types.Class Find(PSMAssociationMember psmMember) {
            return PSMAssociationMembers[psmMember];
        }

        /// <summary>
        /// Gets the class from type associated with the attribute type.
        /// </summary>
        /// <exception cref="KeyNotFoundException">Attribute type does not exist in collection.</exception>
        public Types.Classifier Find(AttributeType psmAttType) {
            return PSMAttributeType[psmAttType];
        }

        private void CreateTypesTable() {
            Library.StandardTypeName naming = new OCL.TypesTable.Library.StandardTypeName();
            naming.Any = "any";
            naming.Boolean = "boolean";
            naming.Integer = "integer";
            naming.Invalid = "invalid";
            naming.Message = "message";
            naming.Real = "real";
            naming.String = "string";
            naming.Type = "type";
            naming.UnlimitedNatural = "unlimitedNatural";
            naming.Void = "void";

            TypesTable = new TypesTable.TypesTable(naming);
            TypesTable.StandardLibraryCreator sLC = new TypesTable.StandardLibraryCreator();
            sLC.CreateStandardLibrary(TypesTable);

            // Docasna podpora pro typy v Tournaments.eXo
            Class date = new Class(TypesTable, "date");
            date.Operations.Add(new Operation("after", true, TypesTable.Library.Boolean, new Parameter[] { new Parameter("time", date) }));
            date.Operations.Add(new Operation("before", true, TypesTable.Library.Boolean, new Parameter[] { new Parameter("time", date) }));
            date.Operations.Add(new Operation("equals", true, TypesTable.Library.Boolean, new Parameter[] { new Parameter("time", date) }));
            date.Operations.Add(new Operation("<=", true, TypesTable.Library.Boolean, new Parameter[] { new Parameter("time", date) }));
            TypesTable.RegisterType(date);
            TypesTable.Library.RootNamespace.NestedClassifier.Add(date);
            

            Class dateTime = new Class(TypesTable, "dateTime");
            dateTime.Operations.Add(new Operation("after", true, TypesTable.Library.Boolean, new Parameter[] { new Parameter("time", dateTime) }));
            dateTime.Operations.Add(new Operation("before", true, TypesTable.Library.Boolean, new Parameter[] { new Parameter("time", dateTime) }));
            dateTime.Operations.Add(new Operation("equals", true, TypesTable.Library.Boolean, new Parameter[] { new Parameter("time", dateTime) }));
            dateTime.Operations.Add(new Operation("<=", true, TypesTable.Library.Boolean, new Parameter[] { new Parameter("time", dateTime) }));
            TypesTable.RegisterType(dateTime);
            TypesTable.Library.RootNamespace.NestedClassifier.Add(dateTime);
            

            Class matchesStatus = new Class(TypesTable, "MatchStatus");
            TypesTable.Library.RootNamespace.NestedClassifier.Add(matchesStatus);
            TypesTable.RegisterType(matchesStatus);

            Translate(TypesTable);

        }


        private void Translate(TypesTable.TypesTable tt) {

            PSM.PSMSchema schema = Schema as PSM.PSMSchema;

            Dictionary<string, string> PSMToOCLMap = new Dictionary<string, string>();
         //   PSMToOCLMap.Add("dateTime", "date");
 
        /*    PSMToOCLMap.Add("nonNegativeInteger", "integer");
           

            PSMToOCLMap.Add("positiveInteger", "integer");*/

            IEnumerable<AttributeType> attTypes = schema.GetAvailablePSMTypes();
            foreach (AttributeType type in attTypes) {
                Classifier existsCl;
                if (Library.RootNamespace.NestedClassifier.TryGetValue(type.Name,out existsCl)) {
                    PSMAttributeType.Add(type, existsCl);
                    continue;
                }
                Classifier parent;
                if (type.BaseType == null) {
                    parent = Library.Any;
                }
                else {
                    if (Library.RootNamespace.NestedClassifier.TryGetValue(type.BaseType.Name, out parent) == false) {
                        parent = Library.Any;
                    }
                }

                Classifier newClassifier = new Classifier(tt, type.Name, parent);
                tt.RegisterType(newClassifier);
                Library.RootNamespace.NestedClassifier.Add(newClassifier);
                PSMAttributeType.Add(type, newClassifier);
            }

            Func<string, string> translateName = (s) => {
                string oclName;
                if (PSMToOCLMap.TryGetValue(s, out oclName)) {
                    return oclName;
                }
                else {
                    return s;
                }
            };

            //preregistrace typu na mala pismena

            List<Tuple<Class, PSM.PSMAssociationMember>> classToProcess = new List<Tuple<Class, PSM.PSMAssociationMember>>();
            foreach (PSM.PSMClass cl in schema.PSMClasses) {
                Class newClass = new Class(tt, cl.Name);
                tt.Library.RootNamespace.NestedClassifier.Add(newClass);
                tt.RegisterType(newClass);
                classToProcess.Add(new Tuple<Class, PSM.PSMAssociationMember>(newClass, cl));
                //Hack
                newClass.Tag = cl;
                //Registred to find
                PSMAssociationMembers.Add(cl, newClass);
            }

            foreach (var cM in schema.PSMContentModels) {
                string cMName = GetContentModelOCLName(cM);
                Class newClass = new Class(tt, cMName);
                tt.Library.RootNamespace.NestedClassifier.Add(newClass);
                tt.RegisterType(newClass);
                classToProcess.Add(new Tuple<Class, PSM.PSMAssociationMember>(newClass, cM));
                newClass.Tag = cM;
                //Registred to find
                PSMAssociationMembers.Add(cM, newClass);
            }

            // Property
            foreach (Tuple<Class, PSM.PSMAssociationMember> item in classToProcess) {
                if (item.Item2 is PSM.PSMClass) {
                    PSM.PSMClass sourceClass = (PSM.PSMClass)item.Item2;
                    foreach (var pr in sourceClass.PSMAttributes) {
                        Classifier propType = tt.Library.RootNamespace.NestedClassifier[translateName(pr.AttributeType.Name)];
                        Property newProp = new Property(pr.Name, PropertyType.One, propType);
                        item.Item1.Properties.Add(newProp);
                        //Hack
                        newProp.Tag = pr;
                    }
                }
            }


            //Associace
            foreach (var item in classToProcess) {
                //parent
                PSM.PSMAssociation parentAss = item.Item2.ParentAssociation;
                if (parentAss != null) {
                    string parentName = null;
                    if (parentAss.Parent is PSM.PSMClass) {
                        parentName = parentAss.Parent.Name;
                    }
                    else if (parentAss.Parent is PSM.PSMContentModel) {
                        parentName = GetContentModelOCLName((PSM.PSMContentModel)parentAss.Parent);
                    }

                    if (parentName != null) {
                        Classifier propType = tt.Library.RootNamespace.NestedClassifier[translateName(parentName)];
                        Property newProp = new Property("parent", PropertyType.One, propType);
                        item.Item1.Properties.Add(newProp);
                        //Hack
                        newProp.Tag = parentAss.Parent;
                    }
                }

                int childCount = 0;
                Dictionary<PSM.PSMContentModelType, int> childContentModelCount = new Dictionary<PSM.PSMContentModelType, int>();
                childContentModelCount[PSM.PSMContentModelType.Choice] = 0;
                childContentModelCount[PSM.PSMContentModelType.Sequence] = 0;
                childContentModelCount[PSM.PSMContentModelType.Set] = 0;

                //child 
                foreach (var ass in item.Item2.ChildPSMAssociations) {
                    string childClassName;
                    List<string> namesInOcl = new List<string>();

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

                    namesInOcl.Add(string.Format("child_{0}", ++childCount));

                    Classifier assType = tt.Library.RootNamespace.NestedClassifier[translateName(childClassName)];

                    if (string.IsNullOrEmpty(ass.Name) == false) {
                        namesInOcl.Add(ass.Name);
                    }
                    else {
                        namesInOcl.Add(String.Format("child_{0}", childClassName));
                    }

                    Classifier propType;
                    if (ass.Upper > 1) {
                        propType = tt.Library.CreateCollection(CollectionKind.Set, assType);
                    }
                    else {
                        propType = assType;
                    }
                    tt.RegisterType(propType);

                    foreach (string name in namesInOcl) {
                        Property newass = new Property(name, PropertyType.One, propType);
                        item.Item1.Properties.Add(newass);
                        //hack
                        newass.Tag = ass;
                    }
                }
            }
        }

        private string GetContentModelOCLName(PSM.PSMContentModel c) {
            PSM.PSMAssociation parentAssocitation = c.ParentAssociation;
            string parentName = parentAssocitation.Parent.Name;
            string name = string.Format("__{0}_{1}_{2}", parentName, c.Type.ToString(), parentAssocitation.Index);
            return name;
        }
    }
}
