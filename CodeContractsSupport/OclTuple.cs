using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Exolutio.CodeContracts.Support
{
    public class OclTuple : OclAny, IEquatable<OclTuple>
    {
        private OclTupleType type;
        private OclAny[] parts;

        public struct TuplePart
        {
            internal string name;
            internal OclClassifier type;
            internal OclAny value;
        }

        public static TuplePart Part(string name, OclClassifier type, OclAny value)
        {
            return new TuplePart { name = name, type = type, value = value };
        }

        #region Constructors
        /// <summary>
        /// Create tuple of the specified type and with the specified values.
        /// </summary>
        /// <param name="type">Type of the whole tuple</param>
        /// <param name="parts">Individual values</param>
        public OclTuple(OclTupleType type, params OclAny[] parts)
        {
            this.parts = parts;
            this.type = type;
        }

        public OclTuple(params TuplePart[] parts)
        {
            this.parts = (from x in parts select x.value).ToArray();
            this.type = OclTupleType.Tuple((from x in parts select OclTupleType.Part(x.name, x.type)).ToArray());
        }
        #endregion

        #region Equality
        public override bool Equals(object obj)
        {
            return Equals(obj as OclTuple);
        }
        public bool Equals(OclTuple obj)
        {
            //Compare element names and types
            if (!obj.type.Equals(type))
                return false;
            //Compare elements
            return Enumerable.SequenceEqual(parts, obj.parts);
          
        }
        public override int GetHashCode()
        {
            unchecked
            {
                int code = type.GetHashCode();
                foreach (OclAny a in parts)
                    code = code * 13 + a.GetHashCode();
                return code;
            }
        }

        #endregion
        public T Get<T>(string element) where T : OclAny
        {
            return (T)parts[type.nameToIndex(element)];
        }
        public T Get<T>(int index) where T : OclAny
        {
            return (T)parts[index];
        }

        public override OclClassifier oclType()
        {
            return type;
        }
    }
}
