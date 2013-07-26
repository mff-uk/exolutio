using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Exolutio.CodeContracts.Support
{
    public class OclTuple : OclAny, IEquatable<OclTuple>
    {
        private readonly OclTupleType type;
        private readonly Dictionary<string, OclAny> parts = new Dictionary<string, OclAny>();

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
        public static TuplePart Part(string name, OclAny value)
        {
            return new TuplePart { name = name, type = null, value = value };
        }


        #region Constructors
        
        public OclTuple(params TuplePart[] parts)
        {
            if(parts == null)
                throw new ArgumentNullException();
            foreach (var part in parts)
                this.parts[part.name] = part.value;
            this.type = OclTupleType.Tuple(from x in parts select OclTupleType.Part(x.name, x.type));
        }
        public OclTuple(OclTupleType type, params TuplePart[] parts)
        {
            if (type == null || parts == null)
                throw new ArgumentNullException();
            foreach (var part in parts)
                this.parts[part.name] = part.value;
            this.type = type;
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
            return parts.All(part => obj.parts[part.Key].Equals(part.Value));

        }
        public override int GetHashCode()
        {
            unchecked
            {
                int code = type.GetHashCode();
                foreach (KeyValuePair<string, OclAny> part in parts)
                    code = part.Value.GetHashCode();
                return code;
            }
        }

        #endregion
        public T Get<T>(string element) where T : OclAny
        {
            return (T)parts[element];
        }

        public override OclClassifier oclType()
        {
            return type;
        }
    }
}
