using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Exolutio.Model.OCL.Types {
    /// <summary>
    /// CollectionType describes a list of elements of a particular given type. CollectionType is a concrete metaclass whose
    /// instances are the family of abstract Collection(T) data types.
    /// </summary>
    public class CollectionType : DataType, ICompositeType {

        public CollectionKind CollectionKind {
            private set;
            get;
        }

        public Classifier ElementType {
            get;
            protected set;
        }

        public CollectionType(TypesTable.TypesTable tt, Classifier elemetnType, Classifier superClassifier)
            : this(tt, CollectionKind.Collection, elemetnType, superClassifier) {
        }

        protected CollectionType(TypesTable.TypesTable tt, CollectionKind collectionKind, Classifier elemetnType, Classifier superClassifier)
            : base(tt,new Namespace("isolatedNS"), GetName(collectionKind, elemetnType), superClassifier) {
            this.CollectionKind = collectionKind;
            this.ElementType = elemetnType;
        }

        private static string GetName(CollectionKind kind, Classifier elemetnType) {
            return String.Format("{0}({1})", kind.ToString(), elemetnType.QualifiedName);
        }



        public override bool ConformsToRegister(Classifier other) {
            return (other == TypeTable.Library.Any);
        }

        public override bool Equals(object obj) {
            //resi i Equals pro potomky(bag,set,sequence,ordetset)
            CollectionType other = obj as CollectionType;
            if (other == null)
                return false;

            return this.CollectionKind == other.CollectionKind
                && this.ElementType == other.ElementType;
        }

        public override string ToString() {
            return Name;
        }

        public override int GetHashCode() {
            return CollectionKind.GetHashCode() + this.ElementType.GetHashCode();
        }

        public static bool operator ==(CollectionType left, CollectionType right) {
            if (object.ReferenceEquals(left, null) && object.ReferenceEquals(right, null))
                return true;

            if (object.ReferenceEquals(left, null) || object.ReferenceEquals(right, null))
                return false;
            return left.Equals(right);
        }

        public static bool operator !=(CollectionType left, CollectionType right) {

            return !(left == right);
        }


        public virtual IteratorOperation LookupIteratorOperation(string name) {
            return CollectionIteratorOperation.Lookup(name);
        }




        #region IConformsToComposite Members

        public virtual bool ConformsToComposite(Classifier other) {
            //resi i potomky (bag,set,sequence,ordetset)
            CollectionType otherColl = other as CollectionType;
            if (otherColl != null && (otherColl.GetType().IsSubclassOf(this.GetType()) || otherColl.GetType() == typeof(CollectionType) || otherColl.GetType() == this.GetType())) {
                return ElementType.ConformsTo(otherColl.ElementType);
            }
            else
                return false;
        }
        #endregion

        public override Classifier CommonSuperType(Classifier other) {
            Classifier common = CommonSuperType<CollectionType>((tt, el) => tt.Library.CreateCollection(CollectionKind.Collection, el), other);
            if (common == null) {
                if (other is IConformsToComposite && other is ICompositeType == false) {
                    return other.CommonSuperType(this);// commonSuperType is symetric
                }
                return base.CommonSuperType(other);
            }
            return common;
        }

        public Classifier CommonSuperType<T>(Func<TypesTable.TypesTable, Classifier, T> creator, Classifier other) where T : CollectionType {
            if (other is T) {
                Classifier commonElementType = this.ElementType.CommonSuperType(((T)other).ElementType);

                if (commonElementType != null) {
                    CollectionType commenType = creator(TypeTable, commonElementType);
                    TypeTable.RegisterType(commenType);
                    return commenType;
                }

            }
            return null;
        }

        #region ICompositeType Members

        public bool ConformsToSimple(Classifier other) {
            return TypeTable.ConformsTo(this, other, true);
        }

        #endregion

        /// <summary>
        /// 
        /// </summary>
        static class CollectionIteratorOperation {
            static Dictionary<string, IteratorOperation> operation = new Dictionary<string, IteratorOperation>();
            public static IteratorOperation Lookup(string name) {
                IteratorOperation op;
                if (operation.TryGetValue(name, out op)) {
                    return op;
                }
                else {
                    return null;
                }
            }

            private static void RegistredOperation(string name, Func<int, bool> iteratorCount, Func<CollectionType, Classifier, TypesTable.TypesTable, Classifier> expressionType,
                Func<CollectionType, Classifier, TypesTable.TypesTable, Classifier> bodyType) {
                operation.Add(name, new IteratorOperation(name, iteratorCount, expressionType, bodyType));
            }

            static CollectionIteratorOperation() {
                RegistredOperation("any", c => c == 1, (s, b, t) => s.ElementType, (s, b, t) => t.Library.Boolean);

                RegistredOperation("closure", c => c == 1,
                    (s, b, t) => {
                        if (s.CollectionKind == CollectionKind.Sequence || s.CollectionKind == CollectionKind.OrderedSet) {
                            OrderedSetType ordSet = (OrderedSetType)t.Library.CreateCollection(CollectionKind.OrderedSet, b);
                            return ordSet;
                        }
                        else {
                            BagType bag = (BagType)t.Library.CreateCollection(CollectionKind.Bag, b);
                            t.RegisterType(bag);
                            return bag;
                        }
                    }
                    , (s, b, t) => {
                        if (b is CollectionType) {
                            return ((CollectionType)b).ElementType;
                        }
                        else {
                            return b;
                        }
                    });

                RegistredOperation("collect", c => c == 1,
                    (s, b, t) => {
                        if (s.CollectionKind == CollectionKind.Sequence || s.CollectionKind == CollectionKind.OrderedSet) {
                            SequenceType seq = (SequenceType)t.Library.CreateCollection(CollectionKind.Sequence, b);
                            t.RegisterType(seq);
                            return seq;
                        }
                        else {
                            BagType bag = (BagType)t.Library.CreateCollection(CollectionKind.Bag, b);
                            t.RegisterType(bag);
                            return bag;
                        }
                    }
                    , (s, b, t) => b);

                RegistredOperation("collectNested", c => c == 1,
                    (s, b, t) => {
                        return t.Library.Boolean;
                    }
                    , (s, b, t) => b);

                RegistredOperation("exists", c => c == 1,
                    (s, b, t) => {
                        return t.Library.Boolean;
                    }
                    , (s, b, t) => t.Library.Boolean);

                RegistredOperation("forAll", c => c == 1,
                    (s, b, t) => {
                        return t.Library.Boolean;
                    }
                    , (s, b, t) => t.Library.Boolean);

                RegistredOperation("isUnique", c => c == 1,
                    (s, b, t) => {
                        return t.Library.Boolean;
                    }
                    , (s, b, t) => b);

                RegistredOperation("one", c => c == 1,
                    (s, b, t) => t.Library.Boolean
                    , (s, b, t) => t.Library.Boolean);

                RegistredOperation("select", c => c == 1,
                    (s, b, t) => s
                    , (s, b, t) => t.Library.Boolean);

                RegistredOperation("reject", c => c == 1,
                    (s, b, t) => s
                    , (s, b, t) => t.Library.Boolean);

                RegistredOperation("sortedBy", c => c == 1,
                    (s, b, t) => {
                        if (s.CollectionKind == CollectionKind.Sequence || s.CollectionKind == CollectionKind.Bag) {
                            SequenceType seq = (SequenceType)t.Library.CreateCollection(CollectionKind.Sequence, b); ;
                            t.RegisterType(seq);
                            return seq;
                        }
                        else {
                            OrderedSetType ordSet = (OrderedSetType)t.Library.CreateCollection(CollectionKind.OrderedSet, b); ;
                            t.RegisterType(ordSet);
                            return ordSet;
                        }
                    }
                    , (s, b, t) => t.Library.Boolean);

            }
        }


    }
}
