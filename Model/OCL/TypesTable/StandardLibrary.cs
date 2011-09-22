using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Model.OCL.Types;
using Exolutio.Model.OCL.TypesTable;

namespace Exolutio.Model.OCL.TypesTable {

    class StandardLibraryType {
        public const string Integer = "Integer";
        public const string Real = "Real";
        public const string Any = "Any";
        public const string UnlimitedNatural = "UnlimitedNatural";
        public const string String = "String";
        public const string Boolean = "Boolean";
        public const string Invalid = "Invalid";
        public const string Message = "Message";
        public const string Void = "Void";
        public const string Type = "Type";
    }

    public class Library {
        public Classifier Integer {
            get {
                return RootNamespace.NestedClassifier[StandardLibraryType.Integer];
            }
        }

        public Classifier Real {
            get {
                return RootNamespace.NestedClassifier[StandardLibraryType.Real];
            }
        }

        public Classifier UnlimitedNatural {
            get {
                return RootNamespace.NestedClassifier[StandardLibraryType.UnlimitedNatural];
            }
        }

        public Classifier String {
            get {
                return RootNamespace.NestedClassifier[StandardLibraryType.String];
            }
        }


        public Classifier Boolean {
            get {
                return RootNamespace.NestedClassifier[StandardLibraryType.Boolean];
            }
        }

        public Classifier Invalid {
            get {
                return RootNamespace.NestedClassifier[StandardLibraryType.Integer];
            }
        }

        public Classifier Any {
            get {
                return RootNamespace.NestedClassifier[StandardLibraryType.Any];
            }
        }

        public Classifier Message {
            get {
                return RootNamespace.NestedClassifier[StandardLibraryType.Message];
            }
        }

        public Classifier Void {
            get {
                return RootNamespace.NestedClassifier[StandardLibraryType.Void];
            }
        }

        public Classifier Type {
            get {
                return RootNamespace.NestedClassifier[StandardLibraryType.Type];
            }
        }

        public Library(TypesTable tt) {
            RootNamespace = new Namespace("");
            this.TypeTable = tt;
            LazyOpearation = new Dictionary<Type, Action<Classifier>>();
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
            
            Classifier oclAny = new Classifier(tt,StandardLibraryType.Any);
            InsertClassifier(oclAny);
            Classifier real = new Classifier(tt,StandardLibraryType.Real,oclAny);
            InsertClassifier(real);
            Classifier integer = new Classifier(tt, StandardLibraryType.Integer,real);
            InsertClassifier(integer);
            Classifier unlimited = new Classifier(tt, StandardLibraryType.UnlimitedNatural, integer);
            InsertClassifier(unlimited);
            Classifier str = new Classifier(tt,StandardLibraryType.String,oclAny);
            InsertClassifier(str);
            Classifier boolean = new Classifier(tt,StandardLibraryType.Boolean,oclAny);
            InsertClassifier(boolean);
            Classifier message = new Classifier(tt,StandardLibraryType.Message,oclAny);
            InsertClassifier(message);  
            Classifier type = new Classifier(tt,StandardLibraryType.Type,oclAny);
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

            SequenceType strSeq = new SequenceType(tt,str);
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
