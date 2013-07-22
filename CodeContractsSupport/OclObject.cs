using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Exolutio.CodeContracts.Support
{
    /// <summary>
    /// Values of Class type
    /// </summary>
    public class OclObject : OclAny, IEquatable<OclObject>
    {
        /// <summary>
        /// OCL type of the object
        /// </summary>
        internal readonly OclClassifier type;
        /// <summary>
        /// The wrapped object
        /// </summary>
        internal readonly object value;

        internal OclObject(OclClassifier type, object value)
        {
            this.type = type;
            this.value = value;
        }
        #region Conversion
        public static OclObject Wrap(object o)
        {
            if (ReferenceEquals(o, null))
                return null;
            else
                return new OclObject(OclObjectType.Get(o.GetType()), o);
        }
        public static T Get<T>(OclObject o)
        {
            return (T)o.value;
        }
        #endregion
        public override OclClassifier oclType()
        {
            return type;
        }

        #region Equality
        public bool Equals(OclObject other)
        {
            if (IsNull(other))
                return false;
            else
                return value.Equals(other.value);
        }
        public override bool Equals(object obj)
        {
            return Equals(obj as OclObject);
        }

        public override int GetHashCode()
        {
            return value.GetHashCode();
        }
        #endregion

    }
}
