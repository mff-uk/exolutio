using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Exolutio.Model.OCL.SupportingClasses
{
    public abstract class ActionList<T> : IList<T>
    {
        protected List<T> Data;
        public ActionList()
        {
            Data = new List<T>();
        }


        protected virtual void OnPreDelete(T item)
        {
        }

        protected virtual void OnPreAdd(T item)
        {
        }

        protected virtual void OnAdded(T item)
        {
        }

        protected virtual void OnPreSet(T item)
        {
        }

        protected virtual void OnSet(T item)
        {
        }


        #region IList<T> Members

        public int IndexOf(T item)
        {
            return Data.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            OnPreAdd(item);
            Data.Insert(index, item);
            OnAdded(item);
           
        }

        public void AddRange(IEnumerable<T> items)
        {
            foreach (T item in items)
            {
                OnPreAdd(item);
            }

            Data.AddRange(items);
            
            foreach (T item in items)
            {
                OnAdded(item);
            }
        }



        public T this[int index]
        {
            get
            {
                return Data[index];
            }

            set
            {
                OnPreSet(value);
                Data[index] = value;
                OnSet(value);
            }
        }

        public void RemoveAt(int index)
        {
            T itemToDelete = Data[index];
            OnPreDelete(itemToDelete);
            Data.RemoveAt(index);
        }


        #endregion

        #region ICollection<Parameter> Members

        public void Add(T item)
        {
            OnPreAdd(item);
            Data.Add(item);
            OnAdded(item);
        }

        public bool Contains(T item)
        {
            return Data.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            Data.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return Data.Count; }
        }

        public void Clear()
        {
            foreach (T item in Data)
                OnPreDelete(item);
            Data.Clear();
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(T item)
        {
            if (Data.Contains(item))
            {
                OnPreDelete(item);
                return Data.Remove(item);
            }
            else
                return false;
        }

        #endregion

        #region IEnumerable<T> Members

        public IEnumerator<T> GetEnumerator()
        {
            return Data.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return Data.GetEnumerator();
        }

        #endregion

    }
}
