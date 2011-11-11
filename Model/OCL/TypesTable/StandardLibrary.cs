using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Model.OCL.Types;
using Exolutio.Model.OCL.TypesTable;

namespace Exolutio.Model.OCL.TypesTable {

    

    public partial class Library {

        public StandardTypeName TypeName {
            get;
            set;
        }  

        public Namespace RootNamespace {
            get;
            internal set;
        }

        public TypesTable TypeTable {
            get;
            protected set;
        }

        public Dictionary<Type, Action<Classifier>> LazyOpearation {
            get;
            protected set;
        }

        public Library(TypesTable tt) {
            RootNamespace = new Namespace("");
            this.TypeTable = tt;
            LazyOpearation = new Dictionary<Type, Action<Classifier>>();
            TypeName = new StandardTypeName();
        }



        public CollectionType CreateCollection(CollectionKind kind, Classifier elementType) {
            CollectionType newType ;
            if (kind == CollectionKind.Collection) {
                newType = new CollectionType(TypeTable, elementType, Any);
            }
            else {
                CollectionType inheritFrom = inheritFrom = CreateCollection(CollectionKind.Collection, elementType);
                switch (kind) {
                    case CollectionKind.Bag:
                        inheritFrom = CreateCollection(CollectionKind.Collection, elementType);
                        newType = new BagType(TypeTable, elementType, inheritFrom);
                        break;
                    case CollectionKind.Collection:
                        newType = null;
                        System.Diagnostics.Debug.Fail("Hups.");
                        break;
                    case CollectionKind.OrderedSet:
                        inheritFrom = CreateCollection(CollectionKind.Collection, elementType);
                        newType = new OrderedSetType(TypeTable, elementType, inheritFrom);
                        break;
                    case CollectionKind.Sequence:
                        inheritFrom = CreateCollection(CollectionKind.Collection, elementType);
                        newType = new SequenceType(TypeTable, elementType, inheritFrom);
                        break;
                    case CollectionKind.Set:
                        inheritFrom = CreateCollection(CollectionKind.Collection, elementType);
                        newType = new SetType(TypeTable, elementType, inheritFrom);
                        break;
                    default:
                        newType = null;
                        System.Diagnostics.Debug.Fail("CreateCollectionType( ... ): missing case for CollectionKind.");
                        break;
                }
            }

            //Operations injection
            Type actType = newType.GetType();
            Action<Classifier> lazyOpAction;
            if (LazyOpearation.TryGetValue(actType, out lazyOpAction)) {
                lazyOpAction(newType);
            }

            TypeTable.RegisterType(newType);
            return newType;
        }
    }



   public  class StandardLibraryCreator {
        Library lib;

        public StandardLibraryCreator() {
        }

        protected void InsertClassifier(Classifier c) {
            lib.RootNamespace.NestedClassifier.Add(c);
            lib.TypeTable.RegisterType(c);
        }

        protected void AddOperation(Classifier cl,string name, Classifier returnType, params Classifier [] types) {
            cl.Operations.Add(new Operation(name, true, returnType, types.Select(t => new Parameter("", t))));
        }

        public void CreateStandardLibrary(TypesTable tt) {
            lib = tt.Library;
            
            Classifier oclAny = new Classifier(tt,lib.TypeName.Any);
            InsertClassifier(oclAny);
            Classifier real = new Classifier(tt,lib.TypeName.Real,oclAny);
            InsertClassifier(real);
            Classifier integer = new Classifier(tt, lib.TypeName.Integer,real);
            InsertClassifier(integer);
            Classifier unlimited = new Classifier(tt, lib.TypeName.UnlimitedNatural, integer);
            InsertClassifier(unlimited);
            Classifier str = new Classifier(tt,lib.TypeName.String,oclAny);
            InsertClassifier(str);
            Classifier boolean = new Classifier(tt,lib.TypeName.Boolean,oclAny);
            InsertClassifier(boolean);
            Classifier message = new Classifier(tt,lib.TypeName.Message,oclAny);
            InsertClassifier(message);  
            Classifier type = new Classifier(tt,lib.TypeName.Type,oclAny);
            InsertClassifier(type);

            Classifier voidT = new VoidType(tt);
            InsertClassifier(voidT);
            Classifier invalid = new InvalidType(tt);
            InsertClassifier(invalid);

            //OCLAny
            AddOperation(oclAny, "=", boolean,  oclAny);
            AddOperation(oclAny, "<>", boolean,  oclAny);
            AddOperation(oclAny, "oclIsUndefined", boolean);
            AddOperation(oclAny, "oclIsInvalid", boolean);
            AddOperation(oclAny, "oclAsType", boolean);
            AddOperation(oclAny, "oclIsNew", boolean);
            //par dalsi operaci chybi

            //Real
            AddOperation(real, "+", real, real);
            AddOperation(real, "-", real, real);
            AddOperation(real, "*", real, real);
            AddOperation(real, "/", real, real);
            AddOperation(real, "-", real);
            AddOperation(real, "abs", real);
            AddOperation(real, "floor", real);
            AddOperation(real, "round", real);
            AddOperation(real, "max", real, real);
            AddOperation(real, "min", real, real);
            AddOperation(real, "<", boolean, real);
            AddOperation(real, ">",  boolean, real);
            AddOperation(real, "<=",  boolean, real);
            AddOperation(real, ">=",  boolean, real);
            AddOperation(real, "toString", str);

            //integer
            AddOperation(integer, "+", integer, integer);
            AddOperation(integer, "-", integer, integer);
            AddOperation(integer, "*", integer, integer);
            AddOperation(integer, "/", integer, real);
            AddOperation(integer, "-", integer);
            AddOperation(integer, "abs", integer);
            AddOperation(integer, "div", integer,integer);
            AddOperation(integer, "mod", integer,integer);
            AddOperation(integer, "min", integer, integer);
            AddOperation(integer, "max", integer, integer);
            AddOperation(integer, "toString", str);

            CollectionType strSeq = lib.CreateCollection(CollectionKind.Sequence, str);
            tt.RegisterType(strSeq);
            //string
            AddOperation(str, "+", str, str);
            AddOperation(str, "size", integer);
            AddOperation(str, "concat", str, str);
            AddOperation(str, "substring", str, integer,integer);
            AddOperation(str, "toInteger", integer);
            AddOperation(str, "toReal", real);
            AddOperation(str, "toBoolean", boolean);
            AddOperation(str, "toUpperCase", str);
            AddOperation(str, "toLowerCase", str);
            AddOperation(str, "indexOf", integer,str);
            AddOperation(str, "equalsIgnoreCase", str, boolean);
            AddOperation(str, "at", str, integer);
            AddOperation(str, "characters", strSeq);
            AddOperation(str, "<", str, boolean);
            AddOperation(str, ">", str, boolean);
            AddOperation(str, "<=", str, boolean);
            AddOperation(str, ">=", str, boolean);

            //boolean
            AddOperation(boolean, "or", boolean, boolean);
            AddOperation(boolean, "xor", boolean, boolean);
            AddOperation(boolean, "and", boolean, boolean);
            AddOperation(boolean, "not", boolean);
            AddOperation(boolean, "implies", boolean, boolean);
            AddOperation(boolean, "toString", str);

            lib.LazyOpearation.Add(typeof(CollectionType), (c) => {
                CollectionType coll = c as CollectionType;
                AddOperation(coll, "=", boolean, coll);
                AddOperation(coll, "<>", boolean, coll);
                AddOperation(coll, "size", integer);
                AddOperation(coll, "includes", boolean,coll.ElementType);
                AddOperation(coll, "excludes", boolean,coll.ElementType);
                AddOperation(coll, "count", coll.ElementType,coll.ElementType);
                AddOperation(coll, "includesAll", boolean,coll);
                AddOperation(coll, "excludesAll", boolean, coll);
                AddOperation(coll, "isEmpty", boolean);
                AddOperation(coll, "notEmpty", boolean);
                //max,min,sum,product
                

            });

        }
        
    }
}
