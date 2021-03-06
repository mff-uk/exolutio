﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace Exolutio.CodeContracts.Support
{
    /// <summary>
    /// Non- generic OCL Set collection
    /// </summary>
    public class OclSet : OclCollection, IEquatable<OclSet>
    {
        internal readonly HashSet<OclAny> set;

        #region Constructors
        /// <summary>
        /// Create empty set with the specified element type
        /// </summary>
        /// <param name="elementType">Element type</param>
        public OclSet(OclClassifier elementType) :
            base(elementType)
        {
            set = new HashSet<OclAny>();
        }

        /// <summary>
        /// Create set with the specified element type filled with elements from a HashSet
        /// </summary>
        /// <param name="elementType">Element type</param>
        /// <param name="set">Set to be copied</param>
        public OclSet(OclClassifier elementType, HashSet<OclAny> set) :
            base(elementType)
        {
            this.set = new HashSet<OclAny>(set);
        }

        /// <summary>
        /// Create set with the specified element type and fill it with elements
        /// </summary>
        /// <param name="elementType">Element type</param>
        /// <param name="items">Collection of elements</param>
        public OclSet(OclClassifier elementType, IEnumerable<OclAny> items) :
            base(elementType)
        {
            set = new HashSet<OclAny>(items);
        }

        /// <summary>
        /// Create set with the specified element type and fill it with elements
        /// </summary>
        /// <param name="elementType">Element type</param>
        /// <param name="items">Array of elements</param>
        public OclSet(OclClassifier elementType, params OclAny[] items) :
            this(elementType, (IEnumerable<OclAny>)items)
        {  
        }

        /// <summary>
        /// Create set with the specified element type and fill it with elements or integer ranges
        /// </summary>
        /// <param name="elementType">Element type</param>
        /// <param name="items">Array of elements or integer ranges</param>
        public OclSet(OclClassifier elementType, params OclCollectionLiteralPart[] items) :
            base(elementType)
        {
            this.set = new HashSet<OclAny>();
            foreach(OclCollectionLiteralPart item in items)
                set.UnionWith(item);
        }
        #endregion
        #region Equality
        /// <summary>
        /// Compare for equality.
        /// </summary>
        /// <param name="obj">Object to compare to.</param>
        /// <returns>True if obj is OclSet and has equal elements.</returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as OclSet);
        }
        public bool Equals(OclSet obj)
        {
            if (IsNull(obj))
                return false;
            return HashSet<OclAny>.CreateSetComparer().Equals(set, obj.set);
        }

        public override int GetHashCode()
        {
            return HashSet<OclAny>.CreateSetComparer().GetHashCode(set);
        }
        #endregion
        #region OCL Operations from Collection
        public override OclInteger size()
        {
            return (OclInteger)set.Count;
        }

        public override OclBoolean includes<T>(T obj)
        {
            return (OclBoolean)set.Contains(obj);
        }

        public override OclBoolean excludes<T>(T obj)
        {
            return (OclBoolean)!set.Contains(obj);
        }

        public override OclInteger count<T>(T obj)
        {
            return (OclInteger)(set.Contains(obj) ? 1 : 0);
        }

        public override OclBoolean isEmpty()
        {
            return (OclBoolean)(set.Count == 0);
        }

        public override OclBoolean notEmpty()
        {
            return (OclBoolean)(set.Count != 0);
        }

        public override OclSet asSet()
        {
            return this;
        }

        public override OclCollection flatten()
        {
            return flattenToSet();
        }
        #endregion
        #region OCL Operations
        [Pure]
        public OclSet union(OclClassifier newElementType, OclSet s2)
        {
            OclSet newSet = new OclSet(newElementType, set);
            newSet.set.UnionWith(s2.set);
            return newSet;
        }
        [Pure]
        public OclBag union(OclClassifier newElementType, OclBag s2)
        {
            if (IsNull(s2))
                throw new ArgumentNullException();
            //Use Bag implemetation
            return s2.union(newElementType, this);
        }
        [Pure]
        public OclSet intersection(OclSet s2)
        {
            if (IsNull(s2))
                throw new ArgumentNullException();
            OclSet newSet = new OclSet(elementType, set);
            newSet.set.IntersectWith(s2.set);
            return newSet;
        }
        [Pure]
        public OclSet intersection(OclBag bag)
        {
            if (IsNull(bag))
                throw new ArgumentNullException();
            OclSet newSet = new OclSet(elementType,set);
            newSet.set.IntersectWith(bag.map.Keys);
            return newSet;
        }
        /// <summary>
        /// Difference of two sets
        /// </summary>
        /// <param name="s">Set of elements to exclude</param>
        /// <returns>Set containing elements of this set except the elements in s</returns>
        [Pure]
        public OclSet op_Subtraction(OclSet s)
        {
            if (IsNull(s))
                throw new ArgumentNullException();
            OclSet newSet = new OclSet(elementType, set);
            newSet.set.ExceptWith(s.set);
            return newSet;
        }

        [Pure]
        public OclSet including<T>(OclClassifier newElementType, T item) where T : OclAny
        {
            OclSet newSet = new OclSet(newElementType, set);
            newSet.set.Add(item);
            return newSet;
        }

        [Pure]
        public OclSet excluding<T>(T item) where T:OclAny
        {
            OclSet newSet = new OclSet(elementType, set);
            newSet.set.Remove(item);
            return newSet;
        }
        [Pure]
        public OclSet symmetricDifference(OclSet s2)
        {
            if (IsNull(s2))
                throw new ArgumentNullException();
            OclSet newSet = new OclSet(elementType, set);
            newSet.set.SymmetricExceptWith(s2.set);
            return newSet;
        }
        [Pure]
        public OclSet flattenToSet()
        {
            List<OclAny> list = new List<OclAny>();
            FlattenToList(list, OclCollectionType.Depth(elementType));
            return new OclSet(OclCollectionType.BasicElementType(elementType), list);
        }
        #endregion
        #region OCL Iterations from Collection
        public override OclCollection closure<T>(OclClassifier newElementType,Func<T, OclAny> body)
        {
            return closureToSet(newElementType, body);
        }
        [Pure]
        public OclSet closureToSet<T>(OclClassifier newElementType, Func<T, OclAny> body) where T : OclAny
        {
            return ClosureToSet(newElementType, body);
        }
        public override OclCollection collect<T>(OclClassifier newElementType, Func<T, OclAny> f)
        {
            return collectToBag<T>(newElementType, f);
        }

        [Pure]
        public OclBag collectToBag<T>(OclClassifier newElementType, Func<T, OclAny> f)
            where T : OclAny
        {
            return collectNested(newElementType, f).flattenToBag();
        }
        #endregion
        #region OCL Iterations
        [Pure]
        public OclOrderedSet sortedBy<T,K>(Func<T, K> f) where T: OclAny where K: OclAny
        {
            return new OclOrderedSet(elementType, set.OrderBy(x => f((T)x)));
        }
        [Pure]
        public OclBag collectNested<T, K>(OclClassifier newElementType, Func<T, K> f)
            where T : OclAny
            where K : OclAny
        {
            return new OclBag(newElementType, set.Select(x => f((T)x)));
        }
        [Pure]
        public OclSet select<T>(Func<T, OclBoolean> predicate)
            where T : OclAny
        {
            return new OclSet(elementType, set.Where(x => (bool)predicate((T)x)));
        }
        [Pure]
        public OclSet reject<T>(Func<T, OclBoolean> f)
            where T : OclAny
        {
            return new OclSet(elementType, set.Where(item => !(bool)f((T)item)));
        }
        #endregion
        #region OCL Type
        public override OclClassifier oclType()
        {
            return OclCollectionType.Collection(OclCollectionKind.Set, elementType);
        }
        #endregion
        #region IEnumerable
        public override IEnumerator<OclAny> GetEnumerator()
        {
            return set.GetEnumerator();
        }
        #endregion
    }

}
