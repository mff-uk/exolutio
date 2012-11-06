using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Model.OCL.Types;
using Exolutio.Model.OCL.TypesTable;
using Exolutio.SupportingClasses;

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

        public Dictionary<Type, Action<Classifier>> LazyOperation {
            get;
            protected set;
        }

        public Action<Classifier> DefaultLazyOperation {
            get;
            set;
        }

        public const string OclAsSet = @"oclAsSet";


        public Library(TypesTable tt) {
            RootNamespace = new Namespace("");
            this.TypeTable = tt;
            LazyOperation = new Dictionary<Type, Action<Classifier>>();
            TypeName = new StandardTypeName();
        }

        public Library(TypesTable tt, StandardTypeName naming) {
            RootNamespace = new Namespace("");
            this.TypeTable = tt;
            LazyOperation = new Dictionary<Type, Action<Classifier>>();
            TypeName = naming;
        }

        private Dictionary<Tuple<Classifier, CollectionKind>, CollectionType> collectionTypeCache
            = new Dictionary<Tuple<Classifier, CollectionKind>, CollectionType>(new ColComp()); 

        private class ColComp:IEqualityComparer<Tuple<Classifier, CollectionKind>>
        {
            public bool Equals(Tuple<Classifier, CollectionKind> x, Tuple<Classifier, CollectionKind> y)
            {
                return x.Item1 == y.Item1 && x.Item2 == y.Item2;
            }

            public int GetHashCode(Tuple<Classifier, CollectionKind> obj)
            {
                return obj.Item1.GetHashCode() ^ obj.Item2.GetHashCode();
            }
        }

		

        public CollectionType CreateCollection(CollectionKind kind, Classifier elementType, 
			bool deferFullInitialization = false, List<CollectionType> deferredCollectionList = null) {
            
			CollectionType newType;
            var tupleCacheKey = new Tuple<Classifier, CollectionKind>(elementType, kind);
            if (collectionTypeCache.ContainsKey(tupleCacheKey))
                return collectionTypeCache[tupleCacheKey];

            if (kind == CollectionKind.Collection) {
                newType = new CollectionType(TypeTable, elementType, Any);
            }
            else {
				CollectionType inheritFrom;// = CreateCollection(CollectionKind.Collection, elementType, deferFullInitialization, deferredCollectionList);
                switch (kind) {
                    case CollectionKind.Bag:
						inheritFrom = CreateCollection(CollectionKind.Collection, elementType, deferFullInitialization, deferredCollectionList);
                        newType = new BagType(TypeTable, elementType, inheritFrom);
                        break;
                    case CollectionKind.Collection:
                        newType = null;
                        System.Diagnostics.Debug.Fail("Hups.");
                        break;
                    case CollectionKind.OrderedSet:
						inheritFrom = CreateCollection(CollectionKind.Collection, elementType, deferFullInitialization, deferredCollectionList);
                        newType = new OrderedSetType(TypeTable, elementType, inheritFrom);
                        break;
                    case CollectionKind.Sequence:
						inheritFrom = CreateCollection(CollectionKind.Collection, elementType, deferFullInitialization, deferredCollectionList);
                        newType = new SequenceType(TypeTable, elementType, inheritFrom);
                        break;
                    case CollectionKind.Set:
						inheritFrom = CreateCollection(CollectionKind.Collection, elementType, deferFullInitialization, deferredCollectionList);
                        newType = new SetType(TypeTable, elementType, inheritFrom);
                        break;
                    default:
                        newType = null;
                        System.Diagnostics.Debug.Fail("CreateCollectionType( ... ): missing case for CollectionKind.");
                        break;
                }
            }

	        newType.DeferredFullInitialization = deferFullInitialization;

			if (!deferFullInitialization)
			{
				PerformDeferredInitialization(newType);
			}
			else
			{
				deferredCollectionList.Add(newType);
			}

	        TypeTable.RegisterType(newType);
            collectionTypeCache[tupleCacheKey] = newType;
            return newType;
        }

	    internal void PerformDeferredInitialization(CollectionType collectionType)
	    {
			// Operations injection
		    Type actType = collectionType.GetType();
		    Action<Classifier> lazyOpAction;
		    if (LazyOperation.TryGetValue(actType, out lazyOpAction))
		    {
			    lazyOpAction(collectionType);
		    }
		    collectionType.DeferredFullInitialization = false;
	    }
    }

	

    public class StandardLibraryCreator {
        Library lib;

        public StandardLibraryCreator() {
        }

        protected void InsertClassifier(Classifier c) {
            //  lib.RootNamespace.NestedClassifier.Add(c);
            lib.TypeTable.RegisterType(c);
        }

        protected void AddOperation(Classifier cl, string name, Classifier returnType, params Classifier[] types) {
            cl.Operations.Add(new Operation(name, true, returnType, types.Select(t => new Parameter("", t))));
        }


        public void CreateStandardLibrary(TypesTable tt) {
            lib = tt.Library;
            Namespace ns = lib.RootNamespace;

			List<CollectionType> defferedCollectionList = new List<CollectionType>();

            // add default template / generic operation 
            lib.DefaultLazyOperation = (c) => {
                // oclAsSet is problem on Set type (infinity recursion)
                if (c is CollectionType == false) {
                    CollectionType set = lib.CreateCollection(CollectionKind.Set, c, true, defferedCollectionList);
                    AddOperation(c, Library.OclAsSet, set);
                }
            };

            Classifier oclAny = new Classifier(tt, ns, lib.TypeName.Any);
            InsertClassifier(oclAny);
            Classifier real = new Classifier(tt, ns, lib.TypeName.Real, oclAny);
            InsertClassifier(real);
            Classifier integer = new Classifier(tt, ns, lib.TypeName.Integer, real);
            InsertClassifier(integer);
            Classifier unlimited = new Classifier(tt, ns, lib.TypeName.UnlimitedNatural, integer);
            InsertClassifier(unlimited);
            Classifier str = new Classifier(tt, ns, lib.TypeName.String, oclAny);
            InsertClassifier(str);
            Classifier boolean = new Classifier(tt, ns, lib.TypeName.Boolean, oclAny);
            InsertClassifier(boolean);
            Classifier message = new Classifier(tt, ns, lib.TypeName.Message, oclAny);
            InsertClassifier(message);
            Classifier type = new Classifier(tt, ns, lib.TypeName.Type, oclAny);
            InsertClassifier(type);

            Classifier voidT = new VoidType(tt, lib.TypeName.Void);
            InsertClassifier(voidT);
            Classifier invalid = new InvalidType(tt, lib.TypeName.Invalid);
            InsertClassifier(invalid);

            //OCLAny
            AddOperation(oclAny, "=", boolean, oclAny);
            AddOperation(oclAny, "<>", boolean, oclAny);
            AddOperation(oclAny, "oclIsUndefined", boolean);
            AddOperation(oclAny, "oclIsInvalid", boolean);
            //AddOperation(oclAny, "oclAsType", boolean);
            {
                Operation oclAsTypeOp = new Operation(@"oclAsType", true, tt.Library.Any,
                    new Parameter[] { new Parameter("type", tt.Library.Type) });
                oclAsTypeOp.ReturnTypeDependsOnArguments = true;
                oclAny.Operations.Add(oclAsTypeOp);
            }
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
            AddOperation(real, ">", boolean, real);
            AddOperation(real, "<=", boolean, real);
            AddOperation(real, ">=", boolean, real);
            AddOperation(real, "toString", str);

            //integer
            AddOperation(integer, "+", integer, integer);
            AddOperation(integer, "-", integer, integer);
            AddOperation(integer, "*", integer, integer);
            AddOperation(integer, "/", integer, real);
            AddOperation(integer, "-", integer);
            AddOperation(integer, "abs", integer);
            AddOperation(integer, "div", integer, integer);
            AddOperation(integer, "mod", integer, integer);
            AddOperation(integer, "min", integer, integer);
            AddOperation(integer, "max", integer, integer);
            AddOperation(integer, "toString", str);

            //string
            AddOperation(str, "+", str, str);
            AddOperation(str, "size", integer);
            AddOperation(str, "concat", str, str);
            AddOperation(str, "substring", str, integer, integer);
            AddOperation(str, "toInteger", integer);
            AddOperation(str, "toReal", real);
            AddOperation(str, "toBoolean", boolean);
            AddOperation(str, "toUpperCase", str);
            AddOperation(str, "toLowerCase", str);
            AddOperation(str, "indexOf", integer, str);
            AddOperation(str, "equalsIgnoreCase", str, boolean);
            AddOperation(str, "at", str, integer);
            AddOperation(str, "<", str, boolean);
            AddOperation(str, ">", str, boolean);
            AddOperation(str, "<=", str, boolean);
            AddOperation(str, ">=", str, boolean);

            // string - non standard (known in XPath)
            AddOperation(str, "substring-before", str, str);
            AddOperation(str, "substring-after", str, str);
            AddOperation(str, "matches", str, str);
            AddOperation(str, "starts-with", str, str);
            AddOperation(str, "ends-with", str, str);


            //boolean
            AddOperation(boolean, "or", boolean, boolean);
            AddOperation(boolean, "xor", boolean, boolean);
            AddOperation(boolean, "and", boolean, boolean);
            AddOperation(boolean, "not", boolean);
            AddOperation(boolean, "implies", boolean, boolean);
            AddOperation(boolean, "toString", str);

            #region lazy operations
            lib.LazyOperation.Add(typeof(Classifier), (c) => {
                                                       
            });

            lib.LazyOperation.Add(typeof(CollectionType), (c) => {
                CollectionType coll = c as CollectionType;
                Classifier t = coll.ElementType;

                AddOperation(coll, "=", boolean, coll);
                AddOperation(coll, "<>", boolean, coll);
                AddOperation(coll, "size", integer);
                AddOperation(coll, "includes", boolean, coll.ElementType);
                AddOperation(coll, "excludes", boolean, coll.ElementType);
                AddOperation(coll, "count", integer, coll.ElementType);
                AddOperation(coll, "includesAll", boolean, coll);
                AddOperation(coll, "excludesAll", boolean, coll);
                AddOperation(coll, "isEmpty", boolean);
                AddOperation(coll, "notEmpty", boolean);

				/* TODO: musi splnovat nektere podminky (zatim nekontrolujeme) */
                AddOperation(coll, "sum", t);
                AddOperation(coll, "max", t);
                AddOperation(coll, "min", t);

                //Missing: product, asSet, asOrdredSet,as  sequence,asBag, flatten


            });


            //Added by J.M. 2.3.2012, should not be under CollectionType 
            //AddOperation(coll, "at", coll.ElementType, integer);
            //AddOperation(coll, "first", coll.ElementType);
            //AddOperation(coll, "last", coll.ElementType);
            lib.LazyOperation.Add(typeof(SetType), (c) => {
                CollectionType coll = c as CollectionType;
                Classifier t = coll.ElementType;

                AddOperation(coll, "union", coll, coll);
                //union with bag missing
                AddOperation(coll, "=", boolean, coll);
                AddOperation(coll, "intersection", coll, coll);
                //intersection with bag missing
                AddOperation(coll, "-", coll, coll);
                AddOperation(coll, "including", coll, t);
                AddOperation(coll, "excluding", coll, t);
                AddOperation(coll, "symmetricDifference", coll, coll);
                AddOperation(coll, "count", integer, t);
                //flatten missing
                AddOperation(coll, "asSet", coll);
                //asOrderedSet(),asSequence(),asBag() missing
            });

            lib.LazyOperation.Add(typeof(SequenceType), (c) => {
                CollectionType coll = c as CollectionType;
                Classifier t = coll.ElementType;

                AddOperation(coll, "count", integer, t);
                AddOperation(coll, "=", boolean, coll);
				SetType set = (SetType)lib.CreateCollection(CollectionKind.Set, t, true, defferedCollectionList);

                AddOperation(coll, "union", coll, coll);
                //flatten missing
                AddOperation(coll, "append", coll, t);
                AddOperation(coll, "prepend", coll, t);
                AddOperation(coll, "insertAt", coll, integer, t);
                AddOperation(coll, "subSequence", coll, integer, integer);
                AddOperation(coll, "at", coll.ElementType, integer);
                AddOperation(coll, "indexOf", integer, t);
                AddOperation(coll, "first", coll.ElementType);
                AddOperation(coll, "last", coll.ElementType);
                AddOperation(coll, "including", coll, t);
                AddOperation(coll, "excluding", coll, t);
                AddOperation(coll, "reverse", coll, t);
                AddOperation(coll, "asSequence", coll, t);
                AddOperation(coll, "asSet", set);
                //asOrderedSet(),asBag(),asSet() missing
            });

            lib.LazyOperation.Add(typeof(BagType), (c) =>
            {
                CollectionType coll = c as CollectionType;
                Classifier t = coll.ElementType;
				SetType set = (SetType)lib.CreateCollection(CollectionKind.Set, t, true, defferedCollectionList);
                AddOperation(coll, "asSet", set);
                //asOrderedSet(),asBag(),asSet() missing
            });

            #endregion 

            // after lazy operations are defined can we use collection types
			CollectionType strSeq = lib.CreateCollection(CollectionKind.Sequence, str, true, defferedCollectionList);
            tt.RegisterType(strSeq);
            AddOperation(str, "characters", strSeq);
            AddOperation(str, "tokenize", strSeq, str);

	        foreach (CollectionType collectionType in defferedCollectionList)
	        {
		        if (collectionType.DeferredFullInitialization)
		        {
					lib.PerformDeferredInitialization(collectionType);
		        }
	        }
        }
    }
}
