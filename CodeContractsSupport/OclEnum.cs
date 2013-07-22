using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Exolutio.CodeContracts.Support
{
    /// <summary>
    /// Values of Enumeration type
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class OclEnum<T> : OclAny, IEquatable<OclEnum<T> > where T:struct
    {
        private readonly T value;
        #region Constructors
        public OclEnum(T v)
        {
            value = v;
        }
        #endregion
        #region Conversions
        public static explicit operator OclEnum<T>(T? e)
        {
            if (e.HasValue)
                return new OclEnum<T>(e.Value);
            else
                return null;
        }

        public static explicit operator T?(OclEnum<T> e)
        {
            if (IsNull(e))
                return null;
            else
                return e.value;
        }
        #endregion
        #region Equality
        public bool Equals(OclEnum<T> other)
        {
            if (IsNull(other))
                return false;
            else
                return value.Equals(other.value);
        }
        public override bool Equals(object obj)
        {
            return Equals(obj as OclEnum<T>);
        }

        public override int GetHashCode()
        {
            return value.GetHashCode();
        }
        #endregion
        #region OCL type
        public new static OclClassifier Type
        {
            get
            {
                return OclEnumType.Enum(typeof(T));
            }
        }
        public override OclClassifier oclType()
        {
            return Type;
        }
        public static OclSet allInstances()
        {
            return new OclSet(Type, from T value in Enum.GetValues(typeof(T)) select new OclEnum<T>(value));
        }
        #endregion
    }
    public static class OclWrapperExtensions
    {
        public static T OclUnwrap<T>(this OclObject o)
            where T : class
        {
            if (OclAny.IsNull(o))
                return null;
            return OclObject.Get<T>(o);
        }
    }
    
}
