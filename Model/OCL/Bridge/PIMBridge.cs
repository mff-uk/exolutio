using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Model.OCL.Types;
using Exolutio.Model.PIM;
using Exolutio.Model.OCL.TypesTable;
using Exolutio.SupportingClasses;

namespace Exolutio.Model.OCL.Bridge {
    /// <summary>
    /// Represent connection between PIM scheme and OCL types system.
    /// </summary>
    public class PIMBridge:IBridgeToOCL {
        public PIM.PIMSchema Schema {
            get;
            private set;
        }

        Schema IBridgeToOCL.Schema { get { return Schema; } }

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

        public Classifier Find(Component component)
        {
            if (component is PIMClass)
            {
                return Find((PIMClass) component);
            }
            else 
                throw new ExolutioModelException(string.Format("PIMBridge can locate only components of type `PIMClass`. Type of component `{0}` is `{1}`.", component, component.GetType().Name));
        }

        private void CreateTypesTable() {
            TypesTable = new TypesTable.TypesTable();
            TypesTable.StandardLibraryCreator sLC = new TypesTable.StandardLibraryCreator();
            sLC.CreateStandardLibrary(TypesTable);

            // Docasna podpora pro typy v Tournaments.eXo
            Class date = new Class(TypesTable,TypesTable.Library.RootNamespace, "Date");
            date.Operations.Add(new Operation("after", true, TypesTable.Library.Boolean, new Parameter[] { new Parameter("time", date) }));
            date.Operations.Add(new Operation("before", true, TypesTable.Library.Boolean, new Parameter[] { new Parameter("time", date) }));
            date.Operations.Add(new Operation("equals", true, TypesTable.Library.Boolean, new Parameter[] { new Parameter("time", date) }));
            date.Operations.Add(new Operation("trunc", true, date, new Parameter[] { }));
            date.Operations.Add(new Operation("=", true, TypesTable.Library.Boolean, new Parameter[] { new Parameter("other", date) }));
            date.Operations.Add(new Operation("<=", true, TypesTable.Library.Boolean, new Parameter[] { new Parameter("other", date) }));
            date.Operations.Add(new Operation("<", true, TypesTable.Library.Boolean, new Parameter[] { new Parameter("other", date) }));
            date.Operations.Add(new Operation(">", true, TypesTable.Library.Boolean, new Parameter[] { new Parameter("other", date) }));
            date.Operations.Add(new Operation(">=", true, TypesTable.Library.Boolean, new Parameter[] { new Parameter("other", date) }));

        //    TypesTable.Library.RootNamespace.NestedClassifier.Add(date);
            TypesTable.RegisterType(date);

            Class matchesStatus = new Class(TypesTable,TypesTable.Library.RootNamespace, "MatchStatus");
        //    TypesTable.Library.RootNamespace.NestedClassifier.Add(matchesStatus);
            TypesTable.RegisterType(matchesStatus);

            Translate(TypesTable);

        }


        private void Translate(TypesTable.TypesTable tt) {

            List<PIMBridgeClass> classToProcess = new List<PIMBridgeClass>();
            //vytvoreni prazdnych trid
            //musi predchazet propertam a associacim, aby se neodkazovalo na neexistujici typy

            // JM: usporadani trid tak, aby predkove byli zalozeni pred potomky
            List<PIMClass> pimClassesHierarchy = ModelIterator.GetPIMClassesInheritanceBFS(Schema).ToList();

            foreach (PIM.PIMClass pimClass in pimClassesHierarchy)
            {
                // JM: parent - predek v PIM modelu
                PIMBridgeClass parent = pimClass.GeneralizationAsSpecific != null ? PIMClasses[pimClass.GeneralizationAsSpecific.General] : null;
                PIMBridgeClass newClass = new PIMBridgeClass(tt,tt.Library.RootNamespace, pimClass, parent);
                //  tt.Library.RootNamespace.NestedClassifier.Add(newClass);
                tt.RegisterType(newClass);
                classToProcess.Add(newClass);
                //Hack
                newClass.Tag = pimClass;
                //Registred to find
                PIMClasses.Add(pimClass, newClass);
            }

            //Translates classes members
            classToProcess.ForEach(cl => cl.TranslateMembers());
        }
    }
}
