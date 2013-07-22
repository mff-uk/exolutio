using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Exolutio.CodeContracts.Support
{
  
    public class OclObjectType : OclClassifier, IEquatable<OclObjectType>
    {
        private readonly Type type;

        private OclObjectType(Type type)
        {
            this.type = type;
        }

        public static OclObjectType Get<T>()
        {
            return new OclObjectType(typeof(T));//TODO: cache
        }
        public static OclObjectType Get(Type t)
        {
            return new OclObjectType(t);//TODO: cache
        }

        internal override bool ConformsToInternal(OclClassifier cls)
        {
            //Classes and interfaces conform to Any
            if (cls == OclAny.Type)
                return true;
            else if (cls is OclObjectType)
            {
                //Classes and interfaces conform to ancestor classes and interfaces
                OclObjectType tt = (OclObjectType)cls;
                return tt.type.IsAssignableFrom(type);
            }
            else
                return false;
        }
        #region Equality
        public bool Equals(OclObjectType other)
        {
            if (other == null)
                return false;
            return other.type == type;
        }
        public override bool Equals(object obj)
        {
            return Equals(obj as OclObjectType);
        }
        public override int GetHashCode()
        {
            return type.GetHashCode();
        }
        #endregion
    }
}
