using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Exolutio.CodeContracts.Support
{
   
    public class PrimitiveType : OclClassifier
    {
        /// <summary>
        /// The most specific type from the standard library to which this type conforms.
        /// This exists for all primitive types from the standard library.
        /// </summary>
        private OclClassifier parentClassifier;

        private PrimitiveType(OclClassifier parent)
        {
            parentClassifier = parent;
        }

        public static readonly PrimitiveType Boolean = new PrimitiveType(AnyType.OclAny);
        public static readonly PrimitiveType String = new PrimitiveType(AnyType.OclAny);
        public static readonly PrimitiveType Real = new PrimitiveType(AnyType.OclAny);
        public static readonly PrimitiveType Integer = new PrimitiveType(Real);
        public static readonly PrimitiveType UnlimitedNatural = new PrimitiveType(Integer);

        internal override bool ConformsToInternal(OclClassifier cls)
        {
            //Conforms to the same type and transitively to types  which the parent type conforms to
            return cls == this || parentClassifier.ConformsToInternal(cls);
        }
    }

    public class OclVoidType : OclClassifier
    {
        public static readonly OclVoidType OclVoid = new OclVoidType();
        private OclVoidType() { }

        internal override bool ConformsToInternal(OclClassifier cls)
        {
            //OclVoid conforms to everything except OclInvalid
            return cls != InvalidType.OclInvalid;
        }
    }
    public class AnyType : OclClassifier
    {
        public static readonly AnyType OclAny = new AnyType();
        private AnyType() { }

        internal override bool ConformsToInternal(OclClassifier cls)
        {
            //OclAny conforms to OclAny
            return cls == OclAny;
        }
    }
    public class InvalidType : OclClassifier
    {
        public static readonly InvalidType OclInvalid = new InvalidType();
        private InvalidType() { }

        internal override bool ConformsToInternal(OclClassifier cls)
        {
            //OclInvalid conforms to everything
            return true;
        }
    }

}
