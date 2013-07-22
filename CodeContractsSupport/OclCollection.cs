using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Exolutio.CodeContracts.Support
{
    /// <summary>
    /// Implementation of the Collection type
    /// </summary>
    public abstract class OclCollection : OclAny, IEnumerable<OclAny>
    {
        internal readonly OclClassifier elementType;

        protected OclCollection(OclClassifier elementType)
        {
            this.elementType = elementType;
        }

        #region OCL Operations
        /// <summary>
        /// Count elements.
        /// </summary>
        /// <returns>Number of elements.</returns>
        public virtual OclInteger size()
        {
            return (OclInteger)this.Count();
        }
        /// <summary>
        /// Find element in the collection
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public virtual OclBoolean includes<T>(T obj)
            where T : OclAny
        {
            return (OclBoolean)this.Contains(obj);
        }
        /// <summary>
        /// Find element in the collection
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public virtual OclBoolean excludes<T>(T obj)
            where T : OclAny
        {
            return (OclBoolean)!this.Contains(obj);
        }
        /// <summary>
        /// Count element in the collection
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public virtual OclInteger count<T>(T obj)
            where T : OclAny
        {
            return (OclInteger)this.Count(t => Object.Equals(t, obj));
        }

        public virtual OclBoolean includesAll(OclCollection c2)
        {
            HashSet<OclAny> set1 = new HashSet<OclAny>(this);
            HashSet<OclAny> set2 = new HashSet<OclAny>(c2);
            return (OclBoolean)set1.IsSupersetOf(set2);
        }
        public virtual OclBoolean excludesAll(OclCollection c2)
        {
            HashSet<OclAny> set1 = new HashSet<OclAny>(this);
            HashSet<OclAny> set2 = new HashSet<OclAny>(c2);
            return (OclBoolean)!set1.Overlaps(set2);
        }

        public abstract OclBoolean isEmpty();
        public abstract OclBoolean notEmpty();


        public virtual T max<T>(Func<T,T,T> maxFunc) where T : OclAny
        {
            return MinMax(maxFunc);
        }
        public virtual T min<T>(Func<T, T, T> minFunc) where T : OclAny
        {
            return MinMax(minFunc);
        }
        public virtual T sum<T>(T zero, Func<T,T,T> addFunc) where T : OclAny
        {
            return this.Aggregate(zero, (current, item) => addFunc(current, (T) item));
        }

        public virtual OclSet product(OclCollection c2)
        {
            OclTupleType newElementType = OclTupleType.Tuple(OclTupleType.Part("first", elementType), OclTupleType.Part("second", c2.elementType));
            OclSet set = new OclSet(newElementType);
            foreach (OclAny e1 in this)
                foreach (OclAny e2 in c2)
                    set.set.Add(new OclTuple(newElementType, e1, e2));
            return set;
        }

        public abstract OclCollection flatten();

        #endregion
        #region OCL Conversions
        /// <summary>
        /// Convert to Set
        /// </summary>
        /// <returns>Set containing the same elements destroying multiple elements and ordering</returns>
        public virtual OclSet asSet()
        {
            return new OclSet(elementType, (IEnumerable<OclAny>)this);
        }
        /// <summary>
        /// Convert to OrderedSet
        /// </summary>
        /// <returns>Sequence containing the same elements in the same ordering, destroying multiple elements</returns>
        public virtual OclOrderedSet asOrderedSet()
        {
            return new OclOrderedSet(elementType, (IEnumerable<OclAny>)this);
        }
        /// <summary>
        /// Convert to Sequence
        /// </summary>
        /// <returns>Sequence containing the same elements in the same count and ordering.</returns>
        public virtual OclSequence asSequence()
        {
            return new OclSequence(elementType, (IEnumerable<OclAny>)this);
        }
        /// <summary>
        /// Convert to Bag
        /// </summary>
        /// <returns>Bag containing the same elements in the same count, destroying ordering.</returns>
        public virtual OclBag asBag()
        {
            return new OclBag(elementType, (IEnumerable<OclAny>)this);
        }
        
        #endregion
        #region OCL Iterations

        public K iterate<T, K>(K acc, Func<T, K, K> f) where T:OclAny where K:OclAny
        {
            foreach (OclAny item in this)
                acc = f((T)item, acc);
            return acc;
        }

        public abstract OclCollection closure<T>(OclClassifier newElementType, Func<T, OclAny> body) where T : OclAny;
        public OclBoolean exists<T>(Func<T,OclBoolean> body)where T:OclAny{
            OclBoolean e = OclBoolean.False;
            foreach (OclAny item in this)
            
                e = OclBoolean.or(() => e, () => body((T)item));
            
            return e;
        }
        public OclBoolean forAll<T>(Func<T,OclBoolean> body)where T:OclAny{
            OclBoolean e = OclBoolean.True;
            foreach (OclAny item in this)
            
                e = OclBoolean.and(() => e, () => body((T)item));
            
            return e;
        }

        public OclBoolean isUnique<T, K>(Func<T, K> f)
            where T : OclAny
            where K : OclAny
        {
            HashSet<OclAny> set = new HashSet<OclAny>();
            foreach (OclAny t in this)
            {
                if (!set.Add(f((T)t)))
                {
                    return OclBoolean.False;
                }
            }
            return OclBoolean.True;
        }
        public T any<T>(Func<T, OclBoolean> f)
            where T : OclAny
        {
            foreach(OclAny t in this){
                if ((bool)f((T)t))
                    return (T)t;
            }
            return null;
        }
        public OclBoolean one<T>(Func<T, OclBoolean> f)
            where T : OclAny
        {
            int count = this.Count(t => (bool) f((T) t));
            return (OclBoolean)(count == 1);
        }
        public abstract OclCollection collect<T>(OclClassifier newElementType,Func<T, OclAny> body) where T : OclAny;

        
        #endregion
        #region Helpers
        private T MinMax<T>(Func<T, T, T> maxFunc)
            where T : OclAny
        {
            T currentMax = null;
            foreach (OclAny item in this)
            {
                if (IsNull(currentMax))
                    currentMax = (T)item;
                else
                    currentMax = maxFunc(currentMax, (T)item);
            }
            return currentMax;
        }

        protected OclSet ClosureToSet<T>(OclClassifier newElementType, Func<T,OclAny> body)
            where T:OclAny
        {
            OclSet resultSet = new OclSet(newElementType);

            ClosureTo(newElementType, this, resultSet.set, body);

            return resultSet;
        }

        protected OclOrderedSet ClosureToOrderedSet<T>(OclClassifier newElementType, Func<T, OclAny> body)
            where T : OclAny
        {
            OclOrderedSet resultSet = new OclOrderedSet(newElementType);
            ClosureTo(newElementType, this, resultSet.list, body);
            return resultSet;
        }

        private static void ClosureTo<T>(OclClassifier newElementType, IEnumerable<OclAny> source, ICollection<OclAny> dst, Func<T, OclAny> body)
            where T : OclAny
        {
            //Iterate over added items
            foreach (OclAny s in source)
            {
                //Do not add duplicates
                if (!dst.Contains(s))
                {
                    dst.Add(s);
                    //Execute body for newly added item
                    OclAny newItems = body((T)s);
                    //Ignore null
                    if(!IsNull(newItems))
                    {
                        OclClassifier type = newItems.oclType();
                        if (type.ConformsToInternal(OclCollectionType.Collection(OclAny.Type)))
                        {
                            //Collection must be of new element type
                            if(type.ConformsToInternal(OclCollectionType.Collection(newElementType)))
                                ClosureTo(newElementType, (OclCollection) newItems, dst, body);
                            else
                                throw new InvalidCastException();
                        }
                        else
                        {
                            //Non-collection must be kind of new element type
                            if (type.ConformsToInternal(newElementType))
                            {
                                //Add the result
                                OclAny[] arr = {newItems};
                                ClosureTo(newElementType, arr, dst, body);
                            }
                            else
                                throw new InvalidCastException();
                        }
                    }
                }
            }
        }

        protected void FlattenToList(List<OclAny> dst, int depth)
        {
            if (depth <= 0)
                dst.AddRange(this);
            else
                foreach (OclAny n in this)
                    ((OclCollection)n).FlattenToList(dst, depth - 1);
        }
        #endregion
        #region IEnumerable
        public abstract IEnumerator<OclAny> GetEnumerator();

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion

    }
}
