using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Exolutio.CodeContracts.Support
{
    
    public enum OclCollectionKind
    {
        Collection,Set,OrderedSet,Bag,Sequence
    }
    public class OclCollectionType : OclClassifier, IEquatable<OclCollectionType>
    {
        private OclCollectionKind kind;
        private OclClassifier elementType;

        private OclCollectionType(OclCollectionKind kind, OclClassifier elementType)
        {
            this.kind = kind;
            this.elementType = elementType;
        }

        public static OclClassifier Collection(OclCollectionKind kind, OclClassifier elementType)
        {
            return new OclCollectionType(kind, elementType);//TODO: cache
        }
        public static OclClassifier Collection( OclClassifier elementType)
        {
            return new OclCollectionType(OclCollectionKind.Collection, elementType);//TODO: cache
        }
        public static int Depth(OclClassifier cls)
        {
            int depth = 0;
            while (cls is OclCollectionType)
            {
                ++depth;
                cls = ((OclCollectionType)cls).elementType;
            }
            return depth;
        }
        public static OclClassifier BasicElementType(OclClassifier cls)
        {
            while (cls is OclCollectionType)
                cls = ((OclCollectionType)cls).elementType;
            return cls;
        }

        internal override bool ConformsToInternal(OclClassifier cls)
        {
            //Collection conforms to OclAny
            if (cls == AnyType.OclAny)
                return true;
            else if (cls is OclCollectionType) //Collection conforms to collection of same kind or CollectionKind of conformant element type
            {
                OclCollectionType ct = (OclCollectionType)cls;
                return (ct.kind == OclCollectionKind.Collection || ct.kind==kind) && elementType.ConformsToInternal(ct.elementType);
            }
            else
                return false;
        }
        #region Equality

        public override bool Equals(object obj)
        {
            return Equals(obj as OclCollectionType);
        }
        public bool Equals(OclCollectionType obj)
        {
            if(obj!=null)
                return kind == obj.kind && elementType.Equals(obj.elementType);
            else
                return false;
        }

        public override int GetHashCode()
        {
            return kind.GetHashCode() ^ elementType.GetHashCode();
        }
        #endregion
    }


}
