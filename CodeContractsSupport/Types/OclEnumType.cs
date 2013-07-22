using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Exolutio.CodeContracts.Support
{
  

    public class OclEnumType : OclClassifier, IEquatable<OclEnumType>
    {
        private readonly Type type;

        public static OclEnumType Enum(Type enumType)
        {
            if (enumType == null)
                throw new ArgumentNullException();
            return new OclEnumType(enumType);
        }



        private OclEnumType(Type enumType)
        {
            type = enumType;
        }

        internal override bool ConformsToInternal(OclClassifier cls)
        {
            //Enums conform only to themselves and OclAny
            return cls.Equals(this) || cls.Equals(OclAny.Type);
        }
        #region Equality
        public bool Equals(OclEnumType other)
        {
            if (other == null)
                return false;
            return other.type == type;
        }
        public override bool Equals(object obj)
        {
            return Equals(obj as OclEnumType);
        }
        public override int GetHashCode()
        {
            return type.GetHashCode();
        }
        #endregion
    }

    
}
