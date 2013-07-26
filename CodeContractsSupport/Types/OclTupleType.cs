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
            return new OclTupleType(parts);
        }
        public static OclTupleType Tuple(IEnumerable<OclTuplePart> parts)
        {
            return new OclTupleType(parts);
        }
        #endregion
        private readonly Dictionary<string, OclClassifier> parts = new Dictionary<string, OclClassifier>();

        private OclTupleType(IEnumerable<OclTuplePart> parts)
        {
            foreach (OclTuplePart part in parts)
                this.parts[part.name] = part.type;
        }

        #region Equality
        public bool Equals(OclTupleType other)
        {
            if (other == null)
                return false;
            //The type must have same number of parts
            if (other.parts.Count != parts.Count)
                return false;
            //Every part from this type must be in the other type and have the same type
            foreach (var part in parts)
            {
                OclClassifier partType;
                if (!other.parts.TryGetValue(part.Key, out partType))
                    return false;
                if (!part.Value.Equals(partType))
                    return false;
            }
            return true;
        }
        public override bool Equals(object obj)
        {
            return Equals(obj as OclTupleType);
        }
        public override int GetHashCode()
        {
            unchecked{
                int code = 0;
                //Combine hash codes of part names and types
                foreach (KeyValuePair<string, OclClassifier> tp in parts)
                    code += tp.Key.GetHashCode() + 11*tp.Value.GetHashCode();
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
                if (tt.parts.Count != parts.Count)
                    return false;
                else
                {
                    //Each tuple part typ must conform to the corresponding part type
                    foreach (var part in parts)
                    {
                        OclClassifier partType;
                        if (!tt.parts.TryGetValue(part.Key, out partType))
                            return false;
                        if (!part.Value.ConformsToInternal(partType))
                            return false;
                    }
                    return true;
                }
            }
            else
                return false;
        }
        
    }
   
}
