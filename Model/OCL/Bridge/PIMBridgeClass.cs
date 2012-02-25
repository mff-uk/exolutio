using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Model.OCL.Types;
using Exolutio.Model.PIM;
using Exolutio.Model.OCL.TypesTable;

namespace Exolutio.Model.OCL.Bridge {
    /// <summary>
    /// Represents class from PIM in OCL type system.
    /// </summary>
    public class PIMBridgeClass : Classifier {
        /// <summary>
        /// Containts source class from PIM.
        /// </summary>
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

        private Dictionary<ModelOperation, Operation> PIMOperations {
            get;
            set;
        }

        /// <summary>
        /// Creates instance which represents <paramref name="sourceClass"/> in OCL type system.
        /// </summary>
        /// <param name="tt">Destination OCL type system.</param>
        /// <param name="sourceClass">Source class</param>
        public PIMBridgeClass(TypesTable.TypesTable tt, Namespace ns, PIMClass sourceClass)
            : base(tt,ns, sourceClass.Name, tt.Library.Any) {
            this.SourceClass = sourceClass;
            PIMAttribute = new Dictionary<PIMAttribute, PIMBridgeAttribute>();
            PIMAssociations = new Dictionary<PIMAssociationEnd, PIMBridgeAssociation>();
            PIMOperations = new Dictionary<ModelOperation, Operation>();
        }

        /// <summary>
        /// Tries find instance of PIMBridgeAttribute associated with <paramref name="att"/> from PIM.
        /// </summary>
        /// <exception cref="KeyNotFoundException"><paramref name="att"/> not exists in this class.</exception>
        public PIMBridgeAttribute FindAttribute(PIMAttribute att) {
            return PIMAttribute[att];
        }

        /// <summary>
        /// Tries find instance of PIMBridgeAssociation associated with <paramref name="ass"/> from PIM.
        /// </summary>
        /// <exception cref="KeyNotFoundException"><paramref name="ass"/> not exists in this class.</exception>
        public PIMBridgeAssociation FindAssociation(PIMAssociationEnd assEnd) {
            return PIMAssociations[assEnd];
        }

        /// <summary>
        /// Tries find instance of Operation associated with <paramref name="op"/> from PIM.
        /// </summary>
        /// <exception cref="KeyNotFoundException"><paramref name="op"/> not exists in this class.</exception>
        public Operation FindOperation(ModelOperation op){
            return PIMOperations[op];
        }


        internal void TranslateMembers() {

            //Attributy
            foreach (var pr in SourceClass.PIMAttributes)
            {
                Classifier propType = pr.AttributeType != null ? TypeTable.Library.RootNamespace.NestedClassifier[pr.AttributeType.Name] : TypeTable.Library.Any;
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
