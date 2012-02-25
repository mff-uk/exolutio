using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Exolutio.Model.OCL.Types
{
    /// <summary>
    /// A classifier is a classification of instances, it describes a set of instances that have features in common.
    /// Implement UML.Classes.Kernel.Classifier and partially UML.Classes.Kernel.Class from UML SuperStructure
    /// </summary>
    public class Classifier : IModelElement, IHasOwner<IModelElement>
    {
        /// <summary>
        /// This gives the superclasses of a class.
        /// </summary>
        public virtual Classifier SuperClassifier {
            get;
            protected set;
        }

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

        internal  TypesTable.TypesTable TypeTable
        {
            get;
            set;
        }



        #region IModelElement Members
        Lazy<string> _QualifiedName;
        public string QualifiedName {
            get {
                return _QualifiedName.Value;
            }

        }

        public string Name {
            get;
            private set;
        }

        public object Tag {
            get;
            set;
        }

        #endregion


        public NestedElemetCollection<Classifier, IModelElement> NestedClassifier
        {
            get;
            private set;
        }

        public Classifier(TypesTable.TypesTable tt, Namespace ns, string name)
        {
            this.TypeTable = tt;
           
            Properties = new PropertyCollection(this);
            Operations = new OperationCollection(this);
            NestedClassifier = new NestedElemetCollection<Classifier, IModelElement>(this);

            this.Name = name;
            ns.NestedClassifier.Add(this);
            _QualifiedName = new Lazy<string>(() => GetQualifiedName(),true);
          
        }

        public Classifier(TypesTable.TypesTable tt, Namespace ns, string name, Classifier superClassifier)
            :this(tt,ns,name)
        {
            this.SuperClassifier = superClassifier;
        }

        private string GetQualifiedName() {
            //pridat konstantu na ::
            if (Owner == null)
                return Name;
            if (Owner is Namespace) {
                if (((Namespace)Owner).Owner == null && string.IsNullOrWhiteSpace(Owner.Name)) {
                    return Name;
                }
            }
            return String.Concat(Owner.QualifiedName, "::", Name);
        }

       /* public override int GetHashCode() {
            return QualifiedName.GetHashCode();
        }

        public override bool Equals(object obj) {
            if (obj is Classifier != true) {
                return false;
            }
            return QualifiedName == ((Classifier)obj).QualifiedName;
        }*/

        public virtual bool ConformsTo(Classifier other)
        {
            return TypeTable.ConformsTo(this,other);
        }

        public virtual bool ConformsToRegister(Classifier other)
        {
            //OCL: conformsTo = (self=other) or (self.allParents()->includes(other))
            if (ReferenceEquals(this, other)) {
                return true;
            }

            return SuperClassifier != null && other == this.SuperClassifier;
            
        }

        #region Opreration from spec chap.: 8.8.8
        public virtual Classifier CommonSuperType(Classifier other)
        {
            return TypeTable.CommonSuperType(this, other);
        }

        public virtual Property LookupProperty(string name) {
            Property property;
            if(Properties.TryGetValue(name,out property)){
                return property;
            }
            if (SuperClassifier != null) {
                return SuperClassifier.LookupProperty(name);
            }
            return null;
        }

        public virtual Operation LookupOperation(string name,IEnumerable<Classifier> parameterTypes) {
            OperationList ops;
            if (Operations.TryGetValue(name,out ops)) {
                return ops.LookupOperation(parameterTypes);
            }
            if (SuperClassifier != null) {
                return SuperClassifier.LookupOperation(name, parameterTypes);
            }
            return null;
        }


        #endregion


        //Ignore: isFinalSpecialization, much associations, much operations

        #region IHasOwner<IModelElement> Members
        public virtual IModelElement Owner
        {
            get;
            set;
        }

        #endregion

        public override string ToString() {
            return QualifiedName;
        }

    }
}
