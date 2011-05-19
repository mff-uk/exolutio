using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EvoX.Model.OCL.Types
{
    /// <summary>
    /// TupleType (informally known as record type or struct) combines different types into a single aggregate type. The parts of
    /// a TupleType are described by its attributes, each having a name and a type. There is no restriction on the kind of types that
    /// can be used as part of a tuple. In particular, a TupleType may contain other tuple types and collection types. Each attribute
    /// of a TupleType represents a single feature of a TupleType. Each part is uniquely identified by its name.
    /// </summary>
    public class TupleType : DataType,IEnumerable<Property>,ICompositeType
    {
        public PropertyCollection TupleParts
        {
            get;
            protected set;
        }

        public Property this [string propertyName]
        {
            get
            {
                return TupleParts[propertyName];
            }
        }

        public override string Name
        {
            get
            {

               
                return String.Format("Tuple{0}", TupleParts.ToString());
            }
            
        }

        public TupleType()
            : base("")
        {
            TupleParts = new PropertyCollection(this);
        }


        public override bool ConformsToRegister(Classifier other)
        {
            if ((other.GetType().IsSubclassOf(this.GetType()) || other.GetType() == typeof(TupleType)) == false)
            {
               return base.ConformsToRegister(other); 
            }
            return false;
        }

        #region IEnumerable<VariableDeclaration> Members

        public IEnumerator<Property> GetEnumerator()
        {
            return TupleParts.Values.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return TupleParts.Values.GetEnumerator();
        }

        #endregion

        #region ICompositeType Members

        public bool ConformsToSimple(Classifier other)
        {
            return simpleRepresentation.ConformsTo(other);
        }

        static NonCompositeType<TupleType> simpleRepresentation = NonCompositeType<TupleType>.Instance;
        public virtual NonCompositeType SimpleRepresentation
        {
            get
            {
                return simpleRepresentation;
            }
        }
        

        public void RegistredComposite(TypesTable.TypesTable table)
        {
            table.RegisterType(SimpleRepresentation);
            foreach (Property prop in this)
                table.RegisterType(prop.Type);
        }

        #endregion

        #region IConformsToComposite Members

        public bool ConformsToComposite(Classifier other)
        {
            TupleType otherType = other as TupleType;
            if (otherType == null || SimpleRepresentation.ConformsTo(otherType.SimpleRepresentation)== false)
                return false;

            
            foreach (var otherVar in otherType)
            {
                Property thisVar;
                if (TupleParts.TryGetValue(otherVar.Name, out thisVar) == false)
                    return false;

                if (thisVar.Type.ConformsTo(otherVar.Type) == false)
                    return false;// other neobsahuje propertu nebo neodpovida typ
            }
            return true;
        }

        #endregion

        public override Classifier CommonSuperType(Classifier other)
        {
            TupleType otherTuple = other as TupleType;
            if (otherTuple != null)
            {
                TupleType newTuple = new TupleType();
                foreach (var otherVar in otherTuple)
                {
                    Property thisVar;
                    if (TupleParts.TryGetValue(otherVar.Name, out thisVar) == false)
                        continue;

                    newTuple.TupleParts.Add(new Property(thisVar.Name, (PropertyType)Math.Max((int)thisVar.PropertyType,(int)otherVar.PropertyType), thisVar.Type.CommonSuperType(otherVar.Type)));
                }

                TypeTable.RegisterType(newTuple);
                return newTuple;
            }

            if (other is IConformsToComposite && other is ICompositeType == false)
            {
                return other.CommonSuperType(this);// commonSuperType is symetric
            }
            return base.CommonSuperType(other);
        }
    }
}
