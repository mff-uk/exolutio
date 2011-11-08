using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Model.OCL.Types;
using Exolutio.Model.PIM;
using Exolutio.Model.OCL.TypesTable;

namespace Exolutio.Model.OCL.Bridge {
    /// <summary>
    /// Represent connection between PIM scheme and OCL types system.
    /// </summary>
    public class PIMBridge:IBridgeToOCL {
        public PIM.PIMSchema Schema {
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

        private Dictionary<PIM.PIMClass, Types.Class> PIMClasses {
            get;
            set;
        }

        public PIMBridge(PIM.PIMSchema schema) {
            PIMClasses = new Dictionary<PIMClass, Class>();
            this.Schema = schema;
            CreateTypesTable();
        }

        /// <summary>
        /// Gets the class from type associated with the PIM class.
        /// </summary>
        /// <exception cref="KeyNotFoundException">PIM class does not exist in collection.</exception>
        public Types.Class Find(PIM.PIMClass pimClass) {
            return PIMClasses[pimClass];
        }

        private void CreateTypesTable() {
            TypesTable = new TypesTable.TypesTable();
            TypesTable.StandardLibraryCreator sLC = new TypesTable.StandardLibraryCreator();
            sLC.CreateStandardLibrary(TypesTable);

            // Docasna podpora pro typy v Tournaments.eXo
            Class date = new Class(TypesTable, "Date");
            date.Operations.Add(new Operation("after", true, TypesTable.Library.Boolean, new Parameter[] { new Parameter("time", date) }));
            date.Operations.Add(new Operation("before", true, TypesTable.Library.Boolean, new Parameter[] { new Parameter("time", date) }));
            date.Operations.Add(new Operation("equals", true, TypesTable.Library.Boolean, new Parameter[] { new Parameter("time", date) }));
            date.Operations.Add(new Operation("<=", true, TypesTable.Library.Boolean, new Parameter[] { new Parameter("time", date) }));

            TypesTable.Library.RootNamespace.NestedClassifier.Add(date);
            TypesTable.RegisterType(date);

            Class matchesStatus = new Class(TypesTable, "MatchStatus");
            TypesTable.Library.RootNamespace.NestedClassifier.Add(matchesStatus);
            TypesTable.RegisterType(matchesStatus);

            Translate(TypesTable);

        }


        private void Translate(TypesTable.TypesTable tt) {

            PIM.PIMSchema schema = Schema;
            //obsahuje v Tuplu tridu z OCL a k ni korespondujici tridu z PIM
            List<Tuple<Class, PIM.PIMClass>> classToProcess = new List<Tuple<Class, PIM.PIMClass>>();
            //vytvoreni prazdnych trid
            //musi predchazet propertam a associacim, aby se neodkazovalo na neexistujici typy
            foreach (PIM.PIMClass cl in schema.PIMClasses) {
                Class newClass = new Class(tt, cl.Name);
                tt.Library.RootNamespace.NestedClassifier.Add(newClass);
                tt.RegisterType(newClass);
                classToProcess.Add(new Tuple<Class, PIM.PIMClass>(newClass, cl));
                //Hack
                newClass.Tag = cl;
                //Registred to find
                PIMClasses.Add(cl, newClass);
            }

            // Property
            foreach (Tuple<Class, PIM.PIMClass> item in classToProcess) {
                foreach (var pr in item.Item2.PIMAttributes) {
                    Classifier propType = tt.Library.RootNamespace.NestedClassifier[pr.AttributeType.Name];
                    Property newProp = new Property(pr.Name, PropertyType.One, propType);
                    item.Item1.Properties.Add(newProp);
                    //Hack
                    newProp.Tag = pr;
                }
            }

            //Associace
            foreach (Tuple<Class, PIM.PIMClass> item in classToProcess) {
                foreach (var ass in item.Item2.PIMAssociationEnds) {
                    var end = ass.PIMAssociation.PIMAssociationEnds.Where(a => a.ID != ass.ID).First();
                    Classifier assType = tt.Library.RootNamespace.NestedClassifier[end.PIMClass.Name];
                    string name;
                    if (string.IsNullOrEmpty(end.Name)) {
                        name = assType.Name;
                    }
                    else {
                        name = end.Name;
                    }
                    Classifier propType;
                    if (end.Upper > 1) {
                        propType = tt.Library.CreateCollection(CollectionKind.Set, assType);
                    }
                    else {
                        propType = assType;
                    }
                    tt.RegisterType(propType);
                    Property newass = new Property(name, PropertyType.One, propType);
                    item.Item1.Properties.Add(newass);

                    //hack
                    newass.Tag = end;
                }
            }

            //Operation
            foreach (Tuple<Class, PIM.PIMClass> item in classToProcess) {
                foreach (var op in item.Item2.PIMOperations) {
                    Operation newOp = new Operation(op.Name, true, op.ResultType != null ? tt.Library.RootNamespace.NestedClassifier[op.ResultType.Name] : tt.Library.Void,
                    op.Parameters.Select(p => new Parameter(p.Name, tt.Library.RootNamespace.NestedClassifier[p.Type.Name])));
                    item.Item1.Operations.Add(newOp);

                    //hack
                    newOp.Tag = op;
                }
            }
        }
    }
}
