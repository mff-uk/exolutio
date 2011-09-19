using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Exolutio.Model.OCL.Types
{
    /// <summary>
    /// CollectionType describes a list of elements of a particular given type. CollectionType is a concrete metaclass whose
    /// instances are the family of abstract Collection(T) data types.
    /// </summary>
    public class CollectionType : DataType, ICompositeType
    {
        virtual public CollectionKind CollectionKind
        {
            get { return CollectionKind.Collection; }

        }

        public Classifier ElementType
        {
            get;
            protected set;
        }

        public override string Name
        {
            get
            {
                return String.Format("{0}({1})", CollectionKind.ToString(), ElementType.QualifiedName);
            }
            protected set
            {
                // je volano z predka
            }
        }


        public CollectionType(Classifier elemetnType)
            : base("")
        {
            ElementType = elemetnType;
        }

        public CollectionType()
            : base("")
        {
        }

        public override bool ConformsToRegister(Classifier other)
        {
            //resi i potomky (bag,set,sequence,ordetset)
            //if (this.GetType().IsSubclassOf(other.GetType()) || other.GetType() == this.GetType())
            //    return true;
            //else
            return false;
        }

        public override bool Equals(object obj)
        {
            //resi i Equals pro potomky(bag,set,sequence,ordetset)
            CollectionType other = obj as CollectionType;
            if (other == null)
                return false;

            return this.CollectionKind == other.CollectionKind
                && this.ElementType == other.ElementType;
        }

        public override string ToString()
        {
            return Name;
        }

        public override int GetHashCode()
        {
            return CollectionKind.GetHashCode() + this.ElementType.GetHashCode();
        }

        public static bool operator ==(CollectionType left, CollectionType right)
        {
            if (object.ReferenceEquals(left, null) && object.ReferenceEquals(right, null))
                return true;

            if (object.ReferenceEquals(left, null) || object.ReferenceEquals(right, null))
                return false;
            return left.Equals(right);
        }

        public static bool operator !=(CollectionType left, CollectionType right)
        {

            return !(left == right);
        }


        public virtual IteratorOperation LookupIteratorOperation(string name) {
            return CollectionIteratorOperation.Lookup(name);
        }


        #region ICompositeType Members

        static NonCompositeType<CollectionType> simpleRepresentation = NonCompositeType<CollectionType>.Instance;
        public virtual NonCompositeType SimpleRepresentation
        {
            get
            {
                return simpleRepresentation;
            }
        }


        public virtual bool ConformsToSimple(Classifier other)
        {
            return SimpleRepresentation.ConformsTo(other);
        }

        public virtual void RegistredComposite(TypesTable.TypesTable table)
        {
            table.RegisterType(SimpleRepresentation);
            table.RegisterType(ElementType);
        }

        #endregion

        #region IConformsToComposite Members

        public virtual bool ConformsToComposite(Classifier other)
        {
            //resi i potomky (bag,set,sequence,ordetset)
            CollectionType otherColl = other as CollectionType;
            if (otherColl != null && SimpleRepresentation.ConformsTo(otherColl.SimpleRepresentation))
            {
                return ElementType.ConformsTo(otherColl.ElementType);
            }
            else
                return false;
        }
        #endregion

        public override Classifier CommonSuperType(Classifier other)
        {
            Classifier common = CommonSuperType<CollectionType>(other);
            if (common == null)
            {
                if (other is IConformsToComposite && other is ICompositeType == false)
                {
                    return other.CommonSuperType(this);// commonSuperType is symetric
                }
                return base.CommonSuperType(other);
            }
            return common;
        }

        public Classifier CommonSuperType<T>(Classifier other) where T : CollectionType, new()
        {
            if (other is T)
            {
                Classifier commonElementType = this.ElementType.CommonSuperType(((T)other).ElementType);

                if (commonElementType != null)
                {
                    CollectionType commenType = new T();
                    commenType.ElementType = commonElementType;
                    TypeTable.RegisterType(commenType);
                    return commenType;
                }

            }
            return null;
        }

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

            private static void RegistredOperation(string name, Func<int, bool> iteratorCount, Func<CollectionType,Classifier, TypesTable.TypesTable, Classifier> expressionType,
                Func<CollectionType,Classifier, TypesTable.TypesTable, Classifier> bodyType) {
                operation.Add(name,new IteratorOperation(name,iteratorCount, expressionType, bodyType));
            }

            static CollectionIteratorOperation() {
                RegistredOperation("any", c => c == 1, (s,b, t) => s.ElementType, (s, b,t) => t.Boolean);

                RegistredOperation("closure", c => c == 1,
                    (s, b, t) => {
                        if (s.CollectionKind == CollectionKind.Sequence || s.CollectionKind == CollectionKind.OrderedSet) {
                            OrderedSetType ordSet = new OrderedSetType(b);
                            t.RegisterType(ordSet);
                            return ordSet;
                        }
                        else {
                            BagType bag = new BagType(b);
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
                    (s,b, t) => {
                        if (s.CollectionKind == CollectionKind.Sequence || s.CollectionKind == CollectionKind.OrderedSet) {
                            SequenceType seq = new SequenceType(b);
                            t.RegisterType(seq);
                            return seq;
                        }
                        else {
                            BagType bag = new BagType(b);
                            t.RegisterType(bag);
                            return bag;
                        }
                    }
                    , (s,b, t) => b);

                RegistredOperation("collectNested", c => c == 1,
                    (s, b, t) => {
                        return t.Boolean;
                    }
                    , (s, b, t) => b);

                RegistredOperation("exists", c => c == 1,
                    (s, b, t) => {
                        return t.Boolean;
                    }
                    , (s, b, t) => t.Boolean);

                RegistredOperation("forAll", c => c == 1,
                    (s, b, t) => {
                        return t.Boolean;
                    }
                    , (s, b, t) => t.Boolean);

                RegistredOperation("isUnique", c => c == 1,
                    (s, b, t) => {
                        return t.Boolean;
                    }
                    , (s, b, t) => b);

                RegistredOperation("one", c => c == 1,
                    (s, b, t) =>  t.Boolean
                    , (s, b, t) => t.Boolean);

                RegistredOperation("select", c => c == 1,
                    (s, b, t) => s
                    , (s, b, t) => t.Boolean);

                RegistredOperation("reject", c => c == 1,
                    (s, b, t) => s
                    , (s, b, t) => t.Boolean);

                RegistredOperation("sortedBy", c => c == 1,
                    (s, b, t) => {
                        if (s.CollectionKind == CollectionKind.Sequence || s.CollectionKind == CollectionKind.Bag) {
                            SequenceType seq = new SequenceType(b);
                            t.RegisterType(seq);
                            return seq;
                        }
                        else {
                            OrderedSetType ordSet = new OrderedSetType(b);
                            t.RegisterType(ordSet);
                            return ordSet;
                        }
                    }
                    , (s, b, t) => t.Boolean);

            }
        }
    }
}
