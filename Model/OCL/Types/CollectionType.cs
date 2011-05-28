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
    }
}
