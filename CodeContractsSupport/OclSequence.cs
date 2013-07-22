using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace Exolutio.CodeContracts.Support
{
    /// <summary>
    /// OCL Sequence implementation
    /// </summary>
    public sealed class OclSequence : OclCollection
    {
        internal readonly List<OclAny> list;
        #region Constructors
        /// <summary>
        /// Create an empty sequence of the specified element type.
        /// </summary>
        /// <param name="elementType">Element type.</param>
        public OclSequence(OclClassifier elementType):
            base(elementType)
        {
            list = new List<OclAny>();
        }

        public OclSequence(OclClassifier elementType, IEnumerable<OclAny> items) :
            base(elementType)
        {
            this.list = new List<OclAny>(items);
        }

        public OclSequence(OclClassifier elementType, params OclAny[] items):
            this(elementType, (IEnumerable<OclAny>)items)
        {
        }
        public OclSequence(OclClassifier elementType, params OclCollectionLiteralPart[] items) :
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
            if (!(obj is OclSequence))
                return false;
            return list.SequenceEqual(((OclSequence)obj).list);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 19;
                foreach (OclAny item in list)
                {
                    hash = hash * 31 + (IsNull(item) ? 0 : item.GetHashCode());
                }
                return hash;
            }
        }

        #endregion

        #region OCL Operations from Collection
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

        public override OclSequence asSequence()
        {
            return this;
        }

        public override OclCollection flatten()
        {
            return flattenToSequence();
        }
        #endregion
        #region OCL Operations
        [Pure]
        public OclSequence union(OclClassifier newElementType, OclSequence s)
        {
            OclSequence newList = new OclSequence(newElementType, list);
            newList.list.AddRange(s);
            return newList;
        }

        [Pure]
        public OclSequence flattenToSequence()
        {
            OclSequence seq = new OclSequence(OclCollectionType.BasicElementType(elementType));
            FlattenToList(seq.list, OclCollectionType.Depth(elementType));
            return seq;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <returns></returns>
        [Pure]
        public OclSequence append<T>(OclClassifier newElementType, T item) where T : OclAny
        {
            OclSequence newList = new OclSequence(newElementType, list);
            newList.list.Add(item);
            return newList;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <returns></returns>
        [Pure]
        public OclSequence prepend<T>(OclClassifier newElementType, T item) where T : OclAny
        {
            OclSequence newList = new OclSequence(newElementType, list);
            newList.list.Insert(0, item);
            return newList;
        }
      
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="index"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        [Pure]
        public OclSequence insertAt<T>(OclClassifier newElementType, OclInteger index, T item) where T : OclAny
        {
            OclSequence newList = new OclSequence(newElementType, list);
            int intIndex = (int)index;
            newList.list.Insert(intIndex - 1, item);
            return newList;
        }
        [Pure]
        public OclSequence subSequence(OclInteger start, OclInteger end)
        {
            int intStart = (int)start, intEnd = (int) end;
            
            if (intEnd < intStart || intStart < 1 || intEnd > list.Count)
                throw new IndexOutOfRangeException();

            OclSequence newList = new OclSequence(elementType);
            for (int i = intStart - 1; i < intEnd; ++i)
            {
                newList.list.Add(list[i]);
            }
            return newList;
        }
        [Pure]
        public T at<T>(OclInteger index) where T : OclAny
        {
            if (IsNull(index))
                throw new ArgumentNullException();
            return (T)list[(int)index - 1];
        }
        [Pure]
        public OclInteger indexOf<T>(T item) where T : OclAny{
            int index = list.IndexOf(item);
            if (index < 0)  //Element not found
                throw new ArgumentException();
            return (OclInteger)(index + 1);
        }

        [Pure]
        public T first<T>() where T : OclAny{
            return (T)list[0];
        }
        [Pure]
        public T last<T>() where T : OclAny{
            return (T)list[list.Count - 1];
        }
        [Pure]
        public OclSequence including<T>(OclClassifier newElementType, T item) where T : OclAny
        {
            return append<T>(newElementType, item);
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <returns></returns>
        [Pure]
        public OclSequence excluding<T>(T item) where T : OclAny
        {
            OclSequence newList = new OclSequence(elementType);
            foreach (OclAny i in list)
            {
                if (!i.Equals(item))
                    newList.list.Add(i);
            }
            return newList;
        }
        
        [Pure]
        public OclSequence reverse()
        {
            OclSequence newList = new OclSequence(elementType, list);
            newList.list.Reverse();
            return newList;
        }
        
        
        #endregion
        #region OCL Iterations from Collection
        public override OclCollection closure<T>(OclClassifier newElementType, Func<T, OclAny> body)
        {
            return closureToOrderedSet(newElementType, body);
        }
        [Pure]
        public OclOrderedSet closureToOrderedSet<T>(OclClassifier newElementType, Func<T, OclAny> body) where T : OclAny
        {
            return ClosureToOrderedSet(newElementType, body);
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
        #region OCL Iterations
        [Pure]
        public OclSequence sortedBy<T,K>(Func<T, K> f) where T : OclAny
        {
            return new OclSequence(elementType, list.OrderBy(x => f((T)x)));
        }
        [Pure]
        public OclSequence collectNested<T,K>(OclClassifier newElementType, Func<T, K> f) where T: OclAny where K : OclAny
        {
            return new OclSequence(newElementType, list.Select(x => f((T)x)));
        }
        [Pure]
        public OclSequence select<T>(Func<T, OclBoolean> predicate) where T : OclAny
        {
            return new OclSequence(elementType, list.Where(x => (bool)predicate((T)x)));
        }
        [Pure]
        public OclSequence reject<T>(Func<T, OclBoolean> f) where T : OclAny
        {
            return new OclSequence(elementType, list.Where(item => !(bool)f((T)item)));
        }
        [Pure]
        public OclOrderedSet closureOrderedSet<T>(Func<T, OclAny> f) where T : OclAny
        {
            OclOrderedSet os = new OclOrderedSet(elementType);//TODO:!!!!
            return os;
        }
        #endregion
        #region OCL Type
        public override OclClassifier oclType()
        {
            return OclCollectionType.Collection(OclCollectionKind.Sequence, elementType);
        }
        #endregion
        #region IEnumerable implementation
        public override IEnumerator<OclAny> GetEnumerator()
        {
            return list.GetEnumerator();
        }

        #endregion
    }
}
