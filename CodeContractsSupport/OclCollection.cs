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
        protected readonly OclClassifier elementType;

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
            return (OclInteger)((IEnumerable<OclAny>)this).Count();
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
            return (OclBoolean)((IEnumerable<T>)this).Contains(obj);
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
            return (OclBoolean)!((IEnumerable<T>)this).Contains(obj);
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
            int count = 0;
            foreach (OclAny t in this)
            {
                if (Object.Equals(t, obj))
                    ++count;
            }
            return (OclInteger)count;
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


        public virtual T max<T>() where T : OclAny
        {
            //TODO
            throw new NotImplementedException();
        }
        public virtual T min<T>() where T : OclAny
        {
            //TODO
            throw new NotImplementedException();
        }
        public virtual T sum<T>() where T : OclAny
        {
            //TODO
            throw new NotImplementedException();
        }

        public virtual OclSet product(OclCollection c2)
        {
            OclSet set = new OclSet(null);//TODO:
            foreach (OclAny e1 in this)
                foreach (OclAny e2 in c2)
                    set.set.Add(new OclTuple(null));//TODO:
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

        public abstract OclCollection closure<T>(Func<T,OclAny> body)where T:OclAny;
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
            int count = 0;
            foreach (OclAny t in this)
            {
                if ((bool)f((T)t))
                    ++count;
            }
            return (OclBoolean)(count == 1);
        }
        public abstract OclCollection collect<T>(OclClassifier newElementType,Func<T, OclAny> body) where T : OclAny;

        
        #endregion

        public override OclClassifier oclType()
        {
            //return elementType;
            throw new NotImplementedException();
        }

        #region IEnumerable
        public abstract IEnumerator<OclAny> GetEnumerator();

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion
    }
}
