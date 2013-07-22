using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Exolutio.CodeContracts.Support
{
   
    public struct OclTuplePart : IEquatable<OclTuplePart>
    {
        internal readonly string name;
        internal readonly OclClassifier type;

        internal OclTuplePart(string name, OclClassifier type)
        {
            this.name = name;
            this.type = type;
        }

        internal bool conformsTo(OclTuplePart tp)
        {
            return name == tp.name && type.ConformsToInternal(tp.type);
        }

        #region Equality
        public override bool Equals(object obj)
        {
            if (obj is OclTuplePart)
                return Equals((OclTuplePart)obj);
            else
                return false;
        }
        public override int GetHashCode()
        {
            return name.GetHashCode()^type.GetHashCode();
        }
        public bool Equals(OclTuplePart other)
        {
            return name.Equals(other.name) && type.Equals(other.type);
        }
        #endregion
    }

    public sealed class OclTupleType : OclClassifier, IEquatable<OclTupleType>
    {
        #region Static methods
        public static OclTuplePart Part(string name, OclClassifier type){
            return new OclTuplePart(name, type);
        }
        public static OclTupleType Tuple(params OclTuplePart[] parts)
        {
            return new OclTupleType(parts);//TODO: cache
        }
        #endregion
        private readonly OclTuplePart[] parts;

        private OclTupleType(params OclTuplePart[] parts)
        {
            this.parts = parts;
        }

        #region Equality
        public bool Equals(OclTupleType other)
        {
            if (other == null)
                return false;
            return Enumerable.SequenceEqual(parts, other.parts);
        }
        public override bool Equals(object obj)
        {
            return Equals(obj as OclTupleType);
        }
        public override int GetHashCode()
        {
            unchecked{
                int code = 0;
                foreach (OclTuplePart tp in parts)
                    code = code * 17 + tp.GetHashCode();
                return code;
            }
        }
        #endregion

        internal override bool ConformsToInternal(OclClassifier cls)
        {
            //Tuples conform to OclAny
            if (cls.GetType() == typeof(OclAny))
                return true;
            else if (cls.GetType() == typeof(OclTupleType))
            {
                //Tuple conforms to another tuple if they have parts of same names and order and conforming types
                OclTupleType tt = (OclTupleType)cls;
                if (tt.parts.Length != parts.Length)
                    return false;
                else
                {
                    return !tt.parts.Where((t, i) => !parts[i].conformsTo(t)).Any();
                }
            }
            else
                return false;
        }

        /// <summary>
        /// Convert tuple part name to part index
        /// </summary>
        /// <param name="name">Part name. The name must be valid.</param>
        /// <returns>Zero-based index of the tuple part.</returns>
        internal int nameToIndex(string name){
            for (int i = 0; i < parts.Length; ++i)
            {
                if (parts[i].name == name)
                    return i;
            }
            throw new ArgumentException();
        }

        
    }
   
}
