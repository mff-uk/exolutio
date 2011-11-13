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

        private Dictionary<PIM.PIMClass, PIMBridgeClass> PIMClasses {
            get;
            set;
        }

        public PIMBridge(PIM.PIMSchema schema) {
            PIMClasses = new Dictionary<PIMClass, PIMBridgeClass>();
            this.Schema = schema;
            CreateTypesTable();
        }

        /// <summary>
        /// Gets the class from type associated with the PIM class.
        /// </summary>
        /// <exception cref="KeyNotFoundException">PIM class does not exist in collection.</exception>
        public PIMBridgeClass Find(PIM.PIMClass pimClass) {
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
            List<PIMBridgeClass> classToProcess = new List<PIMBridgeClass>();
            //vytvoreni prazdnych trid
            //musi predchazet propertam a associacim, aby se neodkazovalo na neexistujici typy
            foreach (PIM.PIMClass cl in schema.PIMClasses) {
                PIMBridgeClass newClass = new PIMBridgeClass(tt, cl);
                tt.Library.RootNamespace.NestedClassifier.Add(newClass);
                tt.RegisterType(newClass);
                classToProcess.Add(newClass);
                //Hack
                newClass.Tag = cl;
                //Registred to find
                PIMClasses.Add(cl, newClass);
            }

            //Translates classes members
            classToProcess.ForEach(cl => cl.TranslateMembers());
        }
    }
}
