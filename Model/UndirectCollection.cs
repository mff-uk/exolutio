using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using EvoX.Model.PIM;
using EvoX.Model.PSM;

namespace EvoX.Model
{
    public class UndirectCollection<TMember> : INotifyCollectionChanged, ICollection<TMember>, IEnumerable
        where TMember : EvoXObject
    {
        private readonly List<Guid> internalGuidCollection = new List<Guid>();

        public Project Project { get; private set; }

        public UndirectCollection(Project project)
        {
            Project = project;
        }

        #region Implementation of ICollection

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(TMember item)
        {
            return internalGuidCollection.Contains(item);
        }

        public void CopyTo(TMember[] array, int arrayIndex)
        {
            for (int i = 0; i < Count; i++)
            {
                array[i + arrayIndex] = Project.TranslateComponent<TMember>(internalGuidCollection[i]);
            }
        }

        bool ICollection<TMember>.Remove(TMember item)
        {
            if (internalGuidCollection.Contains(item))
            {
                Remove(item);
                return true;
            }
            else
            {
                return false; 
            }
        }

        public int Remove(TMember member)
        {
            int index = internalGuidCollection.IndexOf(member);
            if (index != -1)
            {
                internalGuidCollection.Remove(member);
                if (MemberRemoved != null)
                    MemberRemoved(member);
                InvokeCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, member, index));
            }
            return index;
        }

        public int RemoveChecked(TMember member)
        {
            int index = internalGuidCollection.IndexOf(member);
            if (index != -1)
            {
                internalGuidCollection.Remove(member);
                if (MemberRemoved != null)
                    MemberRemoved(member);
                InvokeCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, member, index));
                return index;
            }
            else
            {
                throw new EvoXModelException(string.Format("Removed item '{0}' not present in the collection.", member));
            }
        }

        public int Count
        {
            get { return internalGuidCollection.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        #endregion 

        public TMember this[int index]
        {
            get { return Project.TranslateComponent<TMember>(internalGuidCollection[index]); }
        }

        public void Add(TMember member)
        {
            Insert(member, internalGuidCollection.Count);
        }

        internal void AddAsGuid(Guid guid)
        {
            InsertAsGuid(guid, internalGuidCollection.Count);
        }

        internal void InsertAsGuid(Guid guid, int index)
        {
            internalGuidCollection.Insert(index, guid);
        }

        public void Insert(TMember member, int index)
        {
            if (internalGuidCollection.Contains(member))
            {
                throw new EvoXModelException(string.Format("Item '{0}' is already present in the collection. ", member));
            }
            internalGuidCollection.Insert(index, member);
            NotifyCollectionChangedEventArgs args =
                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, member, index);
            if (MemberAdded != null)
                MemberAdded(member);
            InvokeCollectionChanged(args);
        }

        internal delegate void MemberEvent(TMember member);

        internal event MemberEvent MemberAdded;
        internal event MemberEvent MemberRemoved;

        #region Implementation of INotifyCollectionChanged

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public void InvokeCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            NotifyCollectionChangedEventHandler handler = CollectionChanged;
            if (handler != null) handler(this, e);
        }

        #endregion

        #region Implementation of IEnumerable

        public IEnumerator<TMember> GetEnumerator()
        {
            foreach (Guid guid in internalGuidCollection)
            {
                yield return Project.TranslateComponent<TMember>(guid);
            }

            yield break;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        public int IndexOf(TMember member)
        {
            return internalGuidCollection.IndexOf(member);
        }
    }
}