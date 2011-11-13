using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Model.OCL.Types;
using Exolutio.Model.PIM;
using Exolutio.Model.OCL.TypesTable;

namespace Exolutio.Model.OCL.Bridge {
    public class PIMBridgeClass : Classifier {
        public PIMClass SourceClass {
            get;
            private set;
        }

        private Dictionary<PIMAttribute, PIMBridgeAttribute> PIMAttribute {
            get;
            set;
        }

        private Dictionary<PIMAssociationEnd, PIMBridgeAssociation> PIMAssociations {
            get;
            set;
        }

        private Dictionary<PIMOperation, Operation> PIMOperations {
            get;
            set;
        }

        public PIMBridgeClass(TypesTable.TypesTable tt, PIMClass sourceClass)
            : base(tt, sourceClass.Name, tt.Library.Any) {
            this.SourceClass = sourceClass;
            PIMAttribute = new Dictionary<PIMAttribute, PIMBridgeAttribute>();
            PIMAssociations = new Dictionary<PIMAssociationEnd, PIMBridgeAssociation>();
            PIMOperations = new Dictionary<PIMOperation, Operation>();
        }

        public Property FindAttribute(PIMAttribute att) {
            return PIMAttribute[att];
        }

        public Property FindAssociation(PIMAssociationEnd assEnd) {
            return PIMAssociations[assEnd];
        }

        public Operation FindOperation(PIMOperation op){
            return PIMOperations[op];
        }


        internal void TranslateMembers() {

            //Attributy
            foreach (var pr in SourceClass.PIMAttributes) {
                Classifier propType = TypeTable.Library.RootNamespace.NestedClassifier[pr.AttributeType.Name];
                PIMBridgeAttribute newProp = new PIMBridgeAttribute(pr, PropertyType.One, propType);
                Properties.Add(newProp);
                //Hack
                newProp.Tag = pr;
                //Registration to find
                PIMAttribute.Add(pr, newProp);
            }


            //Associace

            foreach (var ass in SourceClass.PIMAssociationEnds) {
                var end = ass.PIMAssociation.PIMAssociationEnds.Where(a => a.ID != ass.ID).First();
                Classifier assType = TypeTable.Library.RootNamespace.NestedClassifier[end.PIMClass.Name];
                string name;
                if (string.IsNullOrEmpty(end.Name)) {
                    name = assType.Name;
                }
                else {
                    name = end.Name;
                }
                Classifier propType;
                if (end.Upper > 1) {
                    propType = TypeTable.Library.CreateCollection(CollectionKind.Set, assType);
                }
                else {
                    propType = assType;
                }
                TypeTable.RegisterType(propType);
                PIMBridgeAssociation newass = new PIMBridgeAssociation(name,ass.PIMAssociation,end, PropertyType.One, propType);
                Properties.Add(newass);

                //hack
                newass.Tag = end;
                //Registration to find
                PIMAssociations.Add(end, newass);
            }


            //Operation

            foreach (var op in SourceClass.PIMOperations) {
                Operation newOp = new Operation(op.Name, true,
                    op.ResultType != null ? TypeTable.Library.RootNamespace.NestedClassifier[op.ResultType.Name] : TypeTable.Library.Void,
                    op.Parameters.Select(p => new Parameter(p.Name, TypeTable.Library.RootNamespace.NestedClassifier[p.Type.Name])));
                Operations.Add(newOp);

                //hack
                newOp.Tag = op;
                // Registration to find
                PIMOperations.Add(op, newOp);
            }

        }

    }

}
