using System;
using System.Collections;
using System.Collections.Generic;
using Exolutio.Model.PIM;

namespace Exolutio.Model.Versioning
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

        public List<Guid> ImmutableGuids = new List<Guid>(); 

        public ElementCopiesMap(Project sourceProject, Project targetProject)
        {
            SourceProject = sourceProject;
            TargetProject = targetProject;
            foreach (AttributeType pimBuiltInType in SourceProject.PIMBuiltInTypes)
            {
                ImmutableGuids.Add(pimBuiltInType.ID);
            }
            foreach (AttributeType psmBuiltInType in SourceProject.PSMBuiltInTypes)
            {
                ImmutableGuids.Add(psmBuiltInType.ID);
            }
        }

        public Guid SuggestGuid(IExolutioCloneable item)
        {
            return GetGuidForCopyOf(item);
        }

        public Guid GetGuidForCopyOf(IExolutioCloneable item)
        {
            Guid foundItem;
            if (copyDictionaryGuid.TryGetValue(item.ID, out foundItem))
            {
                return foundItem;
            }
            else if (ImmutableGuids.Contains(item.ID))
            {
                return item.ID;
            }
            else
            {
                //if (optional)
                //{
                //    return Guid.Empty;
                //}
                //else
                //{
                throw new ExolutioModelException(string.Format("Can not find guid for copy of {0}", item));
                //}    
            }
        }

        public void PrepareGuids(IEnumerable<ExolutioObject> items)
        {
            foreach (ExolutioObject exolutioObject in items)
            {
                PrepareGuid(exolutioObject);
            }
        }

        public void PrepareGuid(ExolutioObject exolutioObject)
        {
            Guid copyGuid = KeepGuids ? exolutioObject.ID : Guid.NewGuid();
            copyDictionaryGuid[exolutioObject.ID] = copyGuid;
        }

        public IEnumerator<KeyValuePair<IVersionedItem, IVersionedItem>> GetEnumerator()
        {
            foreach (KeyValuePair<Guid, Guid> keyValuePair in copyDictionaryGuid)
            {
                ExolutioObject objectKey = SourceProject.TranslateComponent<ExolutioObject>(keyValuePair.Key);
                if (objectKey is ProjectVersion)
                {
                    continue;
                }
                ExolutioObject objectValue = TargetProject.TranslateComponent<ExolutioObject>(keyValuePair.Value);
                if (objectKey.GetType() != objectValue.GetType())
                {
                    throw new ExolutioModelException();
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