using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Exolutio.Model.Resources;
using Exolutio.Model.Serialization;
using Exolutio.SupportingClasses;

namespace Exolutio.Model.Versioning
{
    public class VersionManager
    {
        /// <summary>
        /// Connects all versions of the same <see cref="IVersionedItem"/>
        /// </summary>
        private class VersionedItemPivot
        {
            private readonly ExolutioDictionary<Version, IVersionedItem> pivotMapping = new ExolutioDictionary<Version, IVersionedItem>();

            public ExolutioDictionary<Version, IVersionedItem> PivotMapping
            {
                get { return pivotMapping; }
            }
        }

        public Project Project { get; set; }

        private bool branching = false;
        internal bool Loading { get; set; }

        public VersionManager(Project project)
        {
            Project = project;
            InitializeCollections();
        }

        private void InitializeCollections()
        {
            Versions = new UndirectCollection<Version>(Project);
        }

        #region Versions collection

        public UndirectCollection<Version> Versions { get; private set; }

        public Version GetVersionByNumber(int number)
        {
            return Versions.First(v => v.Number == number);
        }

        #endregion

        private readonly ExolutioDictionary<VersionedItemPivot, List<IVersionedItem>> linkedVersionedItems = new ExolutioDictionary<VersionedItemPivot, List<IVersionedItem>>();
        private ReadOnlyDictionary<VersionedItemPivot, List<IVersionedItem>> LinkedVersionedItems
        {
            get { return linkedVersionedItems.AsReadOnly(); }
        }

        private readonly ExolutioDictionary<IVersionedItem, VersionedItemPivot> pivotLookupDictionary = new ExolutioDictionary<IVersionedItem, VersionedItemPivot>();
        private ReadOnlyDictionary<IVersionedItem, VersionedItemPivot> PivotLookupDictionary
        {
            get { return pivotLookupDictionary.AsReadOnly(); }
        }
        
        /// <summary>
        /// Creates new verion of the project from the existing version <param name="branchedVersion" />.
        /// The created version is fully integrated into versioning system 
        /// (dictionaries <see cref="Exolutio.Model.Project" />.<see cref="Exolutio.Model.Project.ProjectVersions" /> and version links
        /// among <see cref="IVersionedItem"/>s).
        /// </summary>
        /// <param name="branchedVersion">Existing version in the project</param>
        /// <param name="newVersion">New version marker. Created, but "emtpy", must not be member of Project.Versions collection</param>
        public ProjectVersion BranchProject(ProjectVersion branchedVersion, Version newVersion)
        {
            branching = true;
            newVersion.BranchedFrom = branchedVersion.Version;
            Versions.Add(newVersion);

            ProjectVersion newProjectVersion = new ProjectVersion(Project);
            newProjectVersion.Version = newVersion;

            ElementCopiesMap elementCopiesMap = new ElementCopiesMap(Project, Project);
            IEnumerable<ExolutioObject> allModelItems = ModelIterator.GetAllModelItems(branchedVersion);
            List<ExolutioObject> exolutioObjects = allModelItems.ToList();
            elementCopiesMap.PrepareGuids(exolutioObjects);
            elementCopiesMap.PrepareGuid(branchedVersion);

            branchedVersion.FillCopy(newProjectVersion, newProjectVersion, elementCopiesMap);

            Project.ProjectVersions.Add(newProjectVersion);

            foreach (KeyValuePair<IVersionedItem, IVersionedItem> keyValuePair in elementCopiesMap)
            {
                RegisterVersionLink(branchedVersion.Version, newVersion, keyValuePair.Key, keyValuePair.Value);
            }

            branching = false;
            return newProjectVersion;
        }

        public void DeleteVersion(Version deletedVersion)
        {
            Versions.Remove(deletedVersion);
            ProjectVersion removedProjectVersion = Project.GetProjectVersion(deletedVersion);
            Project.ProjectVersions.Remove(removedProjectVersion);

            // update versions deleted from the branched version
            foreach (Version version in Versions.Where(version => version.BranchedFrom == deletedVersion))
            {
                version.BranchedFrom = deletedVersion.BranchedFrom;
            }

            List<IVersionedItem> exolutioList = deletedVersion.Items.ToList();
            foreach (IVersionedItem versionedItem in exolutioList)
            {
                RemoveVersionedItem(versionedItem);
            }

            foreach (IVersionedItem versionedItem in exolutioList)
            {
                Project.mappingDictionary.Remove(versionedItem.ID);
            }
        }

        public IList<IVersionedItem> GetAllVersionsOfItem(IVersionedItem item)
        {
            VersionedItemPivot pivot = PivotLookupDictionary[item];
            return LinkedVersionedItems[pivot].AsReadOnly();
        }

        public IVersionedItem GetItemInVersion(IVersionedItem item, Version version)
        {
            VersionedItemPivot pivot = PivotLookupDictionary[item];
            IVersionedItem result;
            pivot.PivotMapping.TryGetValue(version, out result);
            return result;
        }

        public void RegisterVersionLink(Version version1, Version version2, IVersionedItem itemVersion1, IVersionedItem itemVersion2)
        {
            if (version1 != itemVersion1.Version)
            {
                throw new ExolutioModelException(Exceptions.VersionManager_RegisterVersionLink_itemOldVersion_Version_must_point_to_the_same_object_as_oldVersion);
            }

            if (itemVersion1.GetType() != itemVersion2.GetType())
            {
                throw new ExolutioModelException();
            }

            VersionedItemPivot pivot;
            if (!pivotLookupDictionary.TryGetValue(itemVersion1, out pivot))
            {
                AddVersionedItem(itemVersion1, true);
                pivot = pivotLookupDictionary[itemVersion1];
            }
            
            pivotLookupDictionary[itemVersion2] = pivot;
            linkedVersionedItems.CreateSubCollectionIfNeeded(pivot);
            linkedVersionedItems[pivot].Add(itemVersion2);
            pivot.PivotMapping[version2] = itemVersion2;
#if DEBUG            
            VerifyConsistency();
#endif
        }

        public void UnregisterVersionLink(IVersionedItem item1, IVersionedItem item2)
        {
            throw new NotImplementedException("Member VersionManager.UnregisterVersionLink not implemented.");  
        }

        /// <summary>
        /// Adds new item to versioning infrastracture (item must not be linked to any other existing item).
        /// </summary>
        public void AddVersionedItem(IVersionedItem item, bool addWhenBranchingOrLoading = false)
        {
            if ((!Loading && !branching) || addWhenBranchingOrLoading)
            {
                if (pivotLookupDictionary.ContainsKey(item))
                {
                    throw new ExolutioModelException("Item already added into versioning infrastracture. ");
                }

                VersionedItemPivot pivot = new VersionedItemPivot();
                pivotLookupDictionary[item] = pivot;
                pivot.PivotMapping.Add(item.Version, item);
                linkedVersionedItems.CreateSubCollectionIfNeeded(pivot);
                linkedVersionedItems[pivot].Add(item);
            }
        }


        /// <summary>
        /// Removes item from versioning infrastructure
        /// </summary>
        public void RemoveVersionedItem(IVersionedItem removedItem)
        {
            if (!PivotLookupDictionary.ContainsKey(removedItem) && branching)
            {
                return;
            }
            
            VersionedItemPivot pivot = PivotLookupDictionary[removedItem];
            Version deletedVersion = removedItem.Version;

            // versioned item is going to be removed, thus it is removed from pivot lookup
            pivotLookupDictionary.Remove(removedItem);
            List<IVersionedItem> list;

            // remove version links concerning the items created in the removed version
            if (LinkedVersionedItems.TryGetValue(pivot, out list))
            {
                // there should always be exactly one item fullfiling the condition
                if (list.Count(item => item.Version == deletedVersion) != 1)
                {
                    throw new ExolutioModelException("Inconsistent record in version links.");
                }
                // and this item is removed
                list.RemoveAll(item => item.Version == deletedVersion);
            }

            // in the case versinedItem was the only version connected to pivot, 
            // the pivot is also removed from the dictionary
            if (list.IsEmpty())
            {
                linkedVersionedItems.Remove(pivot);
            }
        }

        public bool AreItemsLinked(ExolutioVersionedObject item1, ExolutioVersionedObject item2)
        {
            return pivotLookupDictionary[item1] == pivotLookupDictionary[item2];
        }

#if DEBUG
        public void VerifyConsistency()
        {
            foreach (KeyValuePair<IVersionedItem, VersionedItemPivot> kvp in PivotLookupDictionary)
            {
                VersionedItemPivot pivot = kvp.Value;
                IVersionedItem versionedItem = kvp.Key;

                Debug.Assert(pivot.PivotMapping.ContainsKey(versionedItem.Version));
                Debug.Assert(pivot.PivotMapping[versionedItem.Version] == versionedItem);

                foreach (IVersionedItem item in LinkedVersionedItems[pivot])
                {
                    Debug.Assert(pivot.PivotMapping.ContainsKey(item.Version));
                    Debug.Assert(pivot.PivotMapping[item.Version] == item);

                }
            }
        }
#endif

        #region serialization

        public void SerializeVersionLinks(XElement versionLinksElement, SerializationContext context)
        {
            foreach (KeyValuePair<VersionedItemPivot, List<IVersionedItem>> kvp in LinkedVersionedItems)
            {
                XElement linkedItemsElement = new XElement(context.ExolutioNS + "LinkedItems");
                foreach (IVersionedItem versionedItem in kvp.Value)
                {
                    XElement linkedItemElement = new XElement(context.ExolutioNS + "LinkedItem");
                    Project.SerializeIDRef((IExolutioSerializable) versionedItem, "itemID", linkedItemElement, context);
                    Project.SerializeSimpleValueToAttribute("versionNumber", versionedItem.Version.ID, linkedItemElement, context);
                    linkedItemsElement.Add(linkedItemElement);
                }
                versionLinksElement.Add(linkedItemsElement);
            }
        }

        public void DeserializeVersionLinks(XElement parentNode, SerializationContext context)
        {
            foreach (XElement linkedItemsElement in parentNode.Elements(context.ExolutioNS + "LinkedItems"))
            {
                VersionedItemPivot pivot = new VersionedItemPivot();

                Dictionary<Version, Guid> linkedItemsIds = new Dictionary<Version, Guid>();
                foreach (XElement linkedItemElement in linkedItemsElement.Elements(context.ExolutioNS + "LinkedItem"))
                {
                    Guid id = SerializationContext.DecodeGuid(linkedItemElement.Attribute("itemID").Value);
                    Guid versionId = SerializationContext.DecodeGuid(linkedItemElement.Attribute("versionNumber").Value);
                    Version version = (Version) Project.TranslateComponent(versionId);
                    linkedItemsIds[version] = id;
                }

                if (linkedItemsIds.Count > 0)
                {
                    foreach (KeyValuePair<Version, Guid> kvp in linkedItemsIds)
                    {
                        IVersionedItem exolutioObject = (IVersionedItem) Project.TranslateComponent(kvp.Value);
                        pivotLookupDictionary[exolutioObject] = pivot;
                        pivot.PivotMapping.Add(kvp.Key, exolutioObject);
                        linkedVersionedItems.CreateSubCollectionIfNeeded(pivot);
                        linkedVersionedItems[pivot].Add(exolutioObject);
                    }
                }
            }
        }

        #endregion

        #region separate version, embed version

        public Project SeparateVersion(ProjectVersion projectVersion, bool keepGuids)
        {
            Project separatedProject = new Project();
            separatedProject.InitNewEmptyProject(false);

            ElementCopiesMap elementCopiesMap = new ElementCopiesMap(Project, separatedProject);
            elementCopiesMap.KeepGuids = keepGuids;
            IEnumerable<ExolutioObject> allModelItems = ModelIterator.GetAllModelItems(projectVersion);
            List<ExolutioObject> exolutioObjects = allModelItems.ToList();
            elementCopiesMap.PrepareGuids(exolutioObjects);
            elementCopiesMap.PrepareGuid(projectVersion);

            projectVersion.FillCopy(separatedProject.SingleVersion, separatedProject.SingleVersion, elementCopiesMap);

            return separatedProject;
        }

        public void EmbedVersion(ProjectVersion embededVersion, Version newVersion, Version branchedFrom, bool keepGuids, bool createVersionLinks)
        {
            newVersion.BranchedFrom = branchedFrom;
            Versions.Add(newVersion);
            ProjectVersion newProjectVersion = new ProjectVersion(Project);
            newProjectVersion.Version = newVersion;

            ElementCopiesMap elementCopiesMap = new ElementCopiesMap(embededVersion.Project, Project);
            elementCopiesMap.KeepGuids = keepGuids;
            IEnumerable<ExolutioObject> allModelItems = ModelIterator.GetAllModelItems(embededVersion);
            List<ExolutioObject> exolutioObjects = allModelItems.ToList();
            elementCopiesMap.PrepareGuids(exolutioObjects);
            elementCopiesMap.PrepareGuid(embededVersion);

            Project.ProjectVersions.Add(newProjectVersion);
            embededVersion.FillCopy(newProjectVersion, newProjectVersion, elementCopiesMap);

            

            if (createVersionLinks)
            {
                foreach (KeyValuePair<IVersionedItem, IVersionedItem> kvp in elementCopiesMap)
                {
                    IVersionedItem fromEmbedded = kvp.Key;
                    IVersionedItem newlyCreated = kvp.Value;
                    ExolutioObject previousObject;
                    if (Project.TryTranslateObject(fromEmbedded.ID, out previousObject) && previousObject is IVersionedItem)
                    {
                        if (Project.mappingDictionary.ContainsKey(previousObject.ID) &&
                            Project.mappingDictionary.ContainsKey(newlyCreated.ID))
                        {
                            RegisterVersionLink(((IVersionedItem)previousObject).Version, newVersion, (IVersionedItem)previousObject, newlyCreated);
                        }
                    }
                }
            }
        }

        #endregion
    }

}
