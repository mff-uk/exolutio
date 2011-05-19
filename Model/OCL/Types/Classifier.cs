using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EvoX.Model.OCL.Types
{
    /// <summary>
    /// A classifier is a classification of instances, it describes a set of instances that have features in common.
    /// Implement UML.Classes.Kernel.Classifier and partially UML.Classes.Kernel.Class from UML SuperStructure
    /// </summary>
    public abstract class Classifier : ModelElement, IHasOwner<ModelElement>
    {
        /// <summary>
        /// The attributes (i.e., the properties) owned by the class. The association is ordered.
        /// </summary> 
        public virtual PropertyCollection Properties
        {
            get;
            protected set;
        }

        /// <summary>
        /// The operations owned by the class. The association is ordered.
        /// </summary>
        public virtual OperationCollection Operations
        {
            get;
            protected set;
        }

        public virtual bool IsAbstract
        {
            get
            {
                return false;
            }
        }

        internal virtual TypesTable.TypesTable TypeTable
        {
            get;
            set;
        }

        public override string QualifiedName
        {
            get
            {
                //pridat konstantu na ::
                if (Owner == null)
                    return Name;

                return String.Concat(Owner.QualifiedName, "::", Name);
            }
        }

        public NestedElemetCollection<Classifier, ModelElement> NestedClassifier
        {
            get;
            private set;
        }

        public Classifier(string name)
            : base(name)
        {
            Properties = new PropertyCollection(this);
            Operations = new OperationCollection(this);
            NestedClassifier = new NestedElemetCollection<Classifier, ModelElement>(this);
        }

        public virtual bool ConformsTo(Classifier other)
        {
            return TypeTable.ConformsTo(this,other);
        }

        public virtual bool ConformsToRegister(Classifier other)
        {
            //OCL: conformsTo = (self=other) or (self.allParents()->includes(other))
            Type thisType = this.GetType();
            Type otherType = other.GetType();

            if (otherType == typeof(AnyType))
                return true;

            return (thisType == otherType || thisType.IsSubclassOf(otherType));

        }

        public virtual Classifier CommonSuperType(Classifier other)
        {
            Classifier realThis = this;

            if(this is ICompositeType)
                realThis = ((ICompositeType)this).SimpleRepresentation;

            if (other is ICompositeType)
                other = ((ICompositeType)other).SimpleRepresentation;

            return TypeTable.CommonSuperType(realThis, other);
        }


        //Ignore: isFinalSpecialization, much associations, much operations

        #region IHasOwner<ModelElement> Members

        public virtual ModelElement Owner
        {
            get;
            set;
        }

        #endregion
    }
}
