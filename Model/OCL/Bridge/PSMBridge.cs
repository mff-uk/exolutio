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

        private Dictionary<PSM.PSMAssociationMember, PSMBridgeClass> PSMAssociationMembers {
            get;
            set;
        }

        private Dictionary<AttributeType, Types.Classifier> PSMAttributeType {
            get;
            set;
        }


        public PSMBridge(PSMSchema schema) {
            PSMAssociationMembers = new Dictionary<PSMAssociationMember, PSMBridgeClass>();
            PSMAttributeType = new Dictionary<AttributeType, Classifier>();
            this.Schema = schema;
            CreateTypesTable();
        }

        /// <summary>
        /// Gets the class from type associated with the PSM class.
        /// </summary>
        /// <exception cref="KeyNotFoundException">PIM class does not exist in collection.</exception>
        public PSMBridgeClass Find(PSMAssociationMember psmMember) {
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
            Class date = new Class(TypesTable,TypesTable.Library.RootNamespace, "date");
            date.Operations.Add(new Operation("after", true, TypesTable.Library.Boolean, new Parameter[] { new Parameter("time", date) }));
            date.Operations.Add(new Operation("before", true, TypesTable.Library.Boolean, new Parameter[] { new Parameter("time", date) }));
            date.Operations.Add(new Operation("equals", true, TypesTable.Library.Boolean, new Parameter[] { new Parameter("time", date) }));
            date.Operations.Add(new Operation("<=", true, TypesTable.Library.Boolean, new Parameter[] { new Parameter("time", date) }));
            TypesTable.RegisterType(date);
          //  TypesTable.Library.RootNamespace.NestedClassifier.Add(date);


            Class dateTime = new Class(TypesTable, TypesTable.Library.RootNamespace, "dateTime");
            dateTime.Operations.Add(new Operation("after", true, TypesTable.Library.Boolean, new Parameter[] { new Parameter("time", dateTime) }));
            dateTime.Operations.Add(new Operation("before", true, TypesTable.Library.Boolean, new Parameter[] { new Parameter("time", dateTime) }));
            dateTime.Operations.Add(new Operation("equals", true, TypesTable.Library.Boolean, new Parameter[] { new Parameter("time", dateTime) }));
            dateTime.Operations.Add(new Operation("<=", true, TypesTable.Library.Boolean, new Parameter[] { new Parameter("time", dateTime) }));
            TypesTable.RegisterType(dateTime);
          //  TypesTable.Library.RootNamespace.NestedClassifier.Add(dateTime);


            Class matchesStatus = new Class(TypesTable, TypesTable.Library.RootNamespace, "MatchStatus");
           // TypesTable.Library.RootNamespace.NestedClassifier.Add(matchesStatus);
            TypesTable.RegisterType(matchesStatus);

            Translate(TypesTable);

        }


        private void Translate(TypesTable.TypesTable tt) {

            PSM.PSMSchema schema = Schema as PSM.PSMSchema;
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
                Classifier newClassifier = new Classifier(tt,tt.Library.RootNamespace, type.Name, parent);
                tt.RegisterType(newClassifier);
            //    Library.RootNamespace.NestedClassifier.Add(newClassifier);
                PSMAttributeType.Add(type, newClassifier);
            }

            List<PSMBridgeClass> classToProcess = new List<PSMBridgeClass>();
            foreach (PSM.PSMClass cl in schema.PSMClasses) {
                PSMBridgeClass newClass = new PSMBridgeClass(tt,tt.Library.RootNamespace, cl);
              //  tt.Library.RootNamespace.NestedClassifier.Add(newClass);
                tt.RegisterType(newClass);
                classToProcess.Add(newClass);
                //Hack
                newClass.Tag = cl;
                //Registred to find
                PSMAssociationMembers.Add(cl, newClass);
            }

            foreach (var cM in schema.PSMContentModels) {
                string cMName = GetContentModelOCLName(cM);
                PSMBridgeClass newClass = new PSMBridgeClass(tt,tt.Library.RootNamespace, cM);
               // tt.Library.RootNamespace.NestedClassifier.Add(newClass);
                tt.RegisterType(newClass);
                classToProcess.Add(newClass);
                newClass.Tag = cM;
                //Registred to find
                PSMAssociationMembers.Add(cM, newClass);
            }

            classToProcess.ForEach(cl => cl.TranslateMembers());
        }

        private string GetContentModelOCLName(PSM.PSMContentModel c) {
            PSM.PSMAssociation parentAssocitation = c.ParentAssociation;
            string parentName = parentAssocitation.Parent.Name;
            string name = string.Format("__{0}_{1}_{2}", parentName, c.Type.ToString(), parentAssocitation.Index);
            return name;
        }
    }
}
