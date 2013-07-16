using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
namespace Exolutio.CodeContracts.Support
{
    /// <summary>
    /// OCL Bag implementation. Collection with unordered, not unique elements.
    /// 
    /// </summary>
 
    public class OclBag : OclCollection
    {
        internal readonly Dictionary<OclAny, int> map;

        #region Constructors
        /// <summary>
        /// Construct empty bag of the specified element type
        /// </summary>
        /// <param name="type"></param>
        public OclBag(OclClassifier type):
            base(type)
        {
            map = new Dictionary<OclAny, int>();
        }
        /// <summary>
        /// Copy constructor.
        /// </summary>
        /// <param name="bag">The bag to be copied.</param>
        private OclBag(OclBag bag):
            base(bag.elementType)
        {
            map = new Dictionary<OclAny, int>(bag.map);
        }

        /// <summary>
        /// Construct a new bag containing elements from the specified collection
        /// </summary>
        /// <param name="type">Element type</param>
        /// <param name="elements">Collection of values</param>
        public OclBag(OclClassifier type, IEnumerable<OclAny> elements):
            base(type)
        {
            map = new Dictionary<OclAny, int>();
            AddRange(elements);
        }

        public OclBag(OclClassifier type, params OclAny[] elements):
            this(type, (IEnumerable<OclAny>)elements)
        {
        }

        public OclBag(OclClassifier type, params CollectionLiteralPart[] items):
            base(type)
        {
            this.map = new Dictionary<OclAny, int>();
            foreach(CollectionLiteralPart item in items)
                AddRange(item);
        }

        #endregion

        #region Equality
        public override bool Equals(object obj)
        {
            if (!(obj is OclBag))
                return false;
            OclBag bag = (OclBag)obj;
            foreach (KeyValuePair<OclAny,int> t in map)
            {
                int i;
                if (!bag.map.TryGetValue(t.Key, out i) || i != t.Value)
                    return false;
            }
            foreach (KeyValuePair<OclAny, int> t in bag.map)
            {
                int i;
                if (!map.TryGetValue(t.Key, out i) || i != t.Value)
                    return false;
            }
            return true;
        }

        
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 21;
                foreach (KeyValuePair<OclAny, int> pair in map)
                {
                    OclAny item = pair.Key;
                    hash += pair.Value * (isNull(item) ? 0 : item.GetHashCode());
                }
                return hash;
            }
        }
        #endregion

        #region IEnumerable<OclAny> implementation
        [Pure]
        public override IEnumerator<OclAny> GetEnumerator()
        {
            foreach (KeyValuePair<OclAny, int> kv in map)
            {
                for (int i = 0; i < kv.Value; ++i)
                    yield return kv.Key;
            }
        }
        #endregion

        #region Helper methods
        private int Count
        {
            get { return map.Sum(kv => kv.Value); }
        }

        private void AddRange(IEnumerable<OclAny> c)
        {
            foreach (OclAny t in c)
                Add(t);
        }


        private void Add(OclAny item)
        {
            int count;
            map.TryGetValue(item, out count);
            map[item] = count + 1;
        }

        private void Clear()
        {
            map.Clear();
        }

        [Pure]
        private bool Contains(OclAny item)
        {
            return map.ContainsKey(item);
        }

        #endregion

        #region OCL Operations from Collection
        /// <summary>
        /// Count elements.
        /// </summary>
        /// <returns>Number of elements.</returns>
        public override OclInteger size()
        {
            int sum = 0;
            foreach (KeyValuePair<OclAny, int> kv in map)
            {
                sum += kv.Value;
            }
            return (OclInteger)sum;
        }

        public override OclBoolean includes<T>(T obj)
        {
            return (OclBoolean)map.ContainsKey(obj);
        }

        public override OclBoolean excludes<T>(T obj)
        {
            return (OclBoolean)!map.ContainsKey(obj);
        }

        public override OclInteger count<T>(T obj)
        {
            int cnt;
            if (map.TryGetValue(obj, out cnt))
                return (OclInteger)cnt;
            return (OclInteger)0;
        }

        public override OclBoolean isEmpty()
        {
            return (OclBoolean)(map.Count == 0);
        }

        public override OclBoolean notEmpty()
        {
            return (OclBoolean)(map.Count != 0);
        }
        
        public override OclBag asBag()
        {
            return this;
        }
        
        public override OclCollection flatten()
        {
            return flattenToBag();
        }
        #endregion
        #region OCL Operations

      
    
        [Pure]
        public OclBag union(OclBag bag)
        {
            if (isNull(bag))
                throw new ArgumentNullException();
            OclBag bg = new OclBag(this);
            bg.AddRange(bag);
            return bg;
        }
        [Pure]
        public OclBag union(OclSet set)
        {
            if (isNull(set))
                throw new ArgumentNullException();
            OclBag bg = new OclBag(this);
            bg.AddRange(set);
            return bg;
        }
        /// <summary>
        /// Intersect with another bag. The result contains elements that are in both bags and the count is minimum of counts in the bags.
        /// </summary>
        /// <param name="bag">The bag to intersect with</param>
        /// <returns>Bag intersection</returns>
        [Pure]
        public OclBag intersection(OclBag bag)
        {
            if (isNull(bag))
                throw new ArgumentNullException();
            OclBag bg = new OclBag(elementType);
            foreach (KeyValuePair<OclAny, int> pair in map)
            {
                int count;
                if (bag.map.TryGetValue(pair.Key, out count))
                    bg.map.Add(pair.Key, Math.Min(count, pair.Value));
            }
            return bg;
        }

        /// <summary>
        /// Intersect with a set. The result contains elements that are in both bags and the count is minimum of counts in the bags.
        /// </summary>
        /// <param name="bag">The set to intersect with</param>
        /// <returns>Set intersection</returns>
        [Pure]
        public OclSet intersection(OclSet set)
        {
            if (isNull(set))
                throw new ArgumentNullException();
            //Use Set implementation
            return set.intersection(this);
        }
        [Pure]
        public OclBag including<T>(T item) where T : OclAny
        {
            OclBag bg = new OclBag(this);
            bg.Add(item);
            return bg;
        }

        [Pure]
        public OclBag excluding<T>(T item) where T : OclAny
        {
            OclBag bg = new OclBag(this);
            bg.map.Remove(item);
            return bg;
        }
        [Pure]
        public OclBag flattenToBag()
        {
            //TODO:
            throw new NotImplementedException();
        }
      

        #endregion

        #region OCL Iterations from Collection
        public override OclCollection closure<T>(Func<T, OclAny> body)
        {
            return closureToSet(body);
        }
        [Pure]
        public OclSet closureToSet<T>(Func<T, OclAny> body) where T : OclAny
        {
            //TODO:
            throw new NotImplementedException();
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
        #region OCL iterations
        [Pure]
        public OclSequence sortedBy<T, K>(Func<T, K> f) where T: OclAny where K: OclAny
        {
            return new OclSequence(elementType, this.OrderBy(x => f((T)x)));
        }
        [Pure]
        public OclBag collectNested<T, K>(OclClassifier newElementType, Func<T, K> f)
            where T : OclAny
            where K : OclAny
        {
            return new OclBag(newElementType, this.Select(x => f((T)x)));
        }
        [Pure]
        public OclBag select<T>(Func<T, OclBoolean> predicate) where T: OclAny
        {
            return new OclBag(elementType, this.Where(x => (bool)predicate((T)x)));
        }
        [Pure]
        public OclBag reject<T>(Func<T, OclBoolean> f) where T : OclAny
        {
            return new OclBag(elementType, this.Where(item => !(bool)f((T)item)));
        }

        #endregion
    }   
}
