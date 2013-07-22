using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;


namespace Exolutio.CodeContracts.Support
{
    public class DuplicateValueException : Exception
    {
        public DuplicateValueException() :
            base("Cannot insert duplicate values into OrderedSet")
        {
        }
    }

    /// <summary>
    /// Implemetation of OCL OrderedSet
    /// </summary>
    public class OclOrderedSet : OclCollection
    {
        internal readonly List<OclAny> list;

        #region Constructors
        public OclOrderedSet(OclClassifier elementType):
            base(elementType)
        {
            list = new List<OclAny>();
        }
        public OclOrderedSet(OclClassifier elementType, IEnumerable<OclAny> items) :
            base(elementType)
        {
            list = new List<OclAny>(items.Distinct());
        }
        public OclOrderedSet(OclClassifier elementType, params OclAny[] items) :
            this(elementType, (IEnumerable<OclAny>)items)
        {   
        }

        public OclOrderedSet(OclClassifier elementType, params OclCollectionLiteralPart[] items) :
            base(elementType)
        {
            this.list = new List<OclAny>();
            foreach(OclCollectionLiteralPart item in items)
                list.AddRange(item);
        }

        #endregion
        #region Equality
        public override bool Equals(object obj)
        {
            if(!(obj is OclOrderedSet))
                return false;
            return ((OclOrderedSet)obj).list.SequenceEqual(list);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 41;
                foreach (OclAny item in list)
                {
                    hash = hash * 37 + (IsNull(item) ? 0 : item.GetHashCode());
                }
                return hash;
            }
        }
        #endregion 
        #region IEnumerable
        [Pure]
        public override IEnumerator<OclAny> GetEnumerator()
        {
            return list.GetEnumerator();
        }
       
        #endregion
        #region OCL operations from Collection
        public override OclInteger size()
        {
            return (OclInteger)list.Count;
        }

        public override OclBoolean isEmpty()
        {
            return (OclBoolean)(list.Count == 0);
        }

        public override OclBoolean notEmpty()
        {
            return (OclBoolean)(list.Count != 0);
        }
        public override OclOrderedSet asOrderedSet()
        {
            return this;
        }
        public override OclCollection flatten()
        {
            return flattenToOrderedSet();
        }
        #endregion
        #region OCL operations
        [Pure]
        public OclOrderedSet append<T>(OclClassifier newElementType, T item) where T : OclAny
        {
            OclOrderedSet set = new OclOrderedSet(newElementType, list);
            set.list.Add(item);
            return set;
        }
        [Pure]
        public OclOrderedSet prepend<T>(OclClassifier newElementType, T item) where T : OclAny
        {
            OclOrderedSet set = new OclOrderedSet(newElementType, list);
            set.list.Insert(0, item);
            return set;
        }
        [Pure]
        public OclOrderedSet insertAt<T>(OclClassifier newElementType, OclInteger index, T obj) where T : OclAny
        {
            OclOrderedSet o = new OclOrderedSet(newElementType, list);
            int indexI = (int) index;
            o.list.Insert(indexI - 1, obj);
            return o;
        }
        [Pure]
        public OclOrderedSet subOrderedSet(OclInteger first, OclInteger last)
        {
            int firstI = (int)first;
            int lastI = (int)last;
            return new OclOrderedSet(elementType, list.Skip(firstI - 1).Take(lastI - firstI + 1));
        }
        [Pure]
        public T at<T>(OclInteger i) where T : OclAny
        {
            if (IsNull(i))
                throw new ArgumentNullException();
            return (T)list[(int)i - 1];
        }
        [Pure]
        public  OclInteger indexOf<T>(T obj)
            where T : OclAny
        {
            int index = list.IndexOf(obj);
            if (index < 0)  //Element not found
                throw new ArgumentException();
            return (OclInteger)(index + 1);
        }
        [Pure]
        public   T   first<T>()
            where T : OclAny
        {
            return (T)list[0];
        }
        [Pure]
        public T last<T>() 
            where T : OclAny
        {
            return (T)list[list.Count - 1];
        }

        [Pure]
        public OclOrderedSet reverse()
        {
            OclOrderedSet l = new OclOrderedSet(elementType, list);
            l.list.Reverse();
            return l;
        }
        [Pure]
        public OclOrderedSet flattenToOrderedSet()
        {
            List<OclAny> list = new List<OclAny>();
            FlattenToList(list, OclCollectionType.Depth(elementType));
            return new OclOrderedSet(OclCollectionType.BasicElementType(elementType), list);
        }
      
        #endregion
        #region OCL Iterations from Collection
        public override OclCollection closure<T>(OclClassifier newElementType, Func<T, OclAny> body)
        {
            return closureToOrderedSet(newElementType,body);
        }
        [Pure]
        public OclOrderedSet closureToOrderedSet<T>(OclClassifier newElementType, Func<T, OclAny> body) where T : OclAny
        {
            return ClosureToOrderedSet(newElementType,body);
        }
        public override OclCollection collect<T>(OclClassifier newElementType, Func<T, OclAny> f)
        {
            return collectToSequence<T>(newElementType, f);
        }

        [Pure]
        public OclSequence collectToSequence<T>(OclClassifier newElementType, Func<T, OclAny> f)
            where T : OclAny
        {
            return collectNested(newElementType, f).flattenToSequence();
        }
        #endregion
        #region OCL iterations

        public OclOrderedSet sortedBy<T, K>(Func<T, K> f)
            where T : OclAny
            where K : OclAny
        {
            return new OclOrderedSet(elementType, list.OrderBy(x => f((T)x)));
        }

        public OclSequence collectNested<T, K>(OclClassifier newElementType, Func<T, K> f)
            where T : OclAny
            where K : OclAny
        {
            return new OclSequence(newElementType, list.Select(x => f((T)x)));
        }

        public OclOrderedSet select<T>(Func<T, OclBoolean> predicate) where T : OclAny
        {
            return new OclOrderedSet(elementType, list.Where(x => (bool)predicate((T)x)));
        }

        public OclOrderedSet reject<T>(Func<T, OclBoolean> f) where T : OclAny
        {
            return new OclOrderedSet(elementType, list.Where(item => !(bool)f((T)item)));
        }

        #endregion
        #region OCL Type
        public override OclClassifier oclType()
        {
            return OclCollectionType.Collection(OclCollectionKind.OrderedSet, elementType);
        }
        #endregion
    }

}
