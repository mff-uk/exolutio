using System;
using System.Collections;
using System.Collections.Generic;
using EvoX.Model.PIM;

namespace EvoX.Model.Versioning
{
    /// <summary>
    /// Used during creating copies / new versions of multiple components in one session.
    /// </summary>
    public class ElementCopiesMap: 
        //IEnumerable<KeyValuePair<Guid, Guid>>,
        IEnumerable<KeyValuePair<IVersionedItem, IVersionedItem>>
    {
        private readonly Dictionary<Guid, Guid> copyDictionaryGuid = new Dictionary<Guid, Guid>();

        public Project SourceProject { get; private set; }

        public Project TargetProject { get; private set; }

        public bool KeepGuids { get; set; }

        public ElementCopiesMap(Project sourceProject, Project targetProject)
        {
            SourceProject = sourceProject;
            TargetProject = targetProject;
        }

        public Guid SuggestGuid(IEvoXCloneable item)
        {
            return GetGuidForCopyOf(item);
        }

        public Guid GetGuidForCopyOf(IEvoXCloneable item)
        {
            Guid foundItem;
            if (copyDictionaryGuid.TryGetValue(item.ID, out foundItem))
            {
                return foundItem;
            }
            else
            {
                //if (optional)
                //{
                //    return Guid.Empty;
                //}
                //else
                //{
                throw new EvoXModelException(string.Format("Can not find guid for copy of {0}", item));
                //}    
            }
        }

        public void PrepareGuids(IEnumerable<EvoXObject> items)
        {
            foreach (EvoXObject evoXObject in items)
            {
                PrepareGuid(evoXObject);
            }
        }

        public void PrepareGuid(EvoXObject evoXObject)
        {
            Guid copyGuid = KeepGuids ? evoXObject.ID : Guid.NewGuid();
            copyDictionaryGuid[evoXObject.ID] = copyGuid;
        }

        public IEnumerator<KeyValuePair<IVersionedItem, IVersionedItem>> GetEnumerator()
        {
            foreach (KeyValuePair<Guid, Guid> keyValuePair in copyDictionaryGuid)
            {
                EvoXObject objectKey = SourceProject.TranslateComponent<EvoXObject>(keyValuePair.Key);
                if (objectKey is ProjectVersion)
                {
                    continue;
                }
                EvoXObject objectValue = TargetProject.TranslateComponent<EvoXObject>(keyValuePair.Value);
                if (objectKey.GetType() != objectValue.GetType())
                {
                    throw new EvoXModelException();
                }
                IVersionedItem keyItem = (IVersionedItem) objectKey;
                IVersionedItem valueItem = (IVersionedItem) objectValue;
                yield return new KeyValuePair<IVersionedItem, IVersionedItem>(keyItem, valueItem);
            }
        }

        public IEnumerator<KeyValuePair<Guid, Guid>> GetGuidEnumerator()
        {
            return copyDictionaryGuid.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}