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
            private readonly ExolutioDictionary<Guid, Guid> pivotMapping = new ExolutioDictionary<Guid, Guid>();

            /// <summary>
            /// Dictionary Key: version guid, Value: versined item guid
            /// </summary>
            public ExolutioDictionary<Guid, Guid> PivotMapping
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

        public bool AreVersionsLinear
        {
            get
            {
                if (Versions.All(v => v.BranchedVersions.Count() <= 1) && Versions.Count(v => v.BranchedFrom == null) == 1)
                {
                    Version first = Versions.First(v => v.BranchedFrom == null);
                    int checkedVersions = 1;

                    Version it = first;
                    while (it.BranchedVersions.Count() == 1)
                    {
                        it = it.BranchedVersions.First();
                        checkedVersions++;
                    }
                    return checkedVersions == Versions.Count();
                }
                else
                {
                    return false; 
                }
            }
        }

        #endregion

        //private readonly ExolutioDictionary<VersionedItemPivot, List<Guid>> linkedVersionedItems = new ExolutioDictionary<VersionedItemPivot, List<Guid>>();

        ///// <summary>
        ///// Dictionary holds a list of versioned items for each pivot
        ///// </summary>
        //private ReadOnlyDictionary<VersionedItemPivot, List<Guid>> LinkedVersionedItems
        //{
        //    get { return linkedVersionedItems.AsReadOnly(); }
        //}

        private readonly ExolutioDictionary<Guid, VersionedItemPivot> pivotLookupDictionary = new ExolutioDictionary<Guid, VersionedItemPivot>();

        /// <summary>
        /// Dictionary used to lookup pivot (Value) for a versioned item (Key)
        /// </summary>
        private ReadOnlyDictionary<Guid, VersionedItemPivot> PivotLookupDictionary
        {
            get { return pivotLookupDictionary.AsReadOnly(); }
        }

        private List<VersionedItemPivot> pivotList = new List<VersionedItemPivot>();

        private List<VersionedItemPivot> PivotList
        {
            get { return pivotList; }
            set { pivotList = value; }
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

        public IEnumerable<IVersionedItem> GetAllVersionsOfItem(IVersionedItem item)
        {
            VersionedItemPivot pivot = PivotLookupDictionary[item.ID];
            return pivot.PivotMapping.Values.Select(ID => (IVersionedItem)Project.TranslateComponent(ID));
        }

        public IVersionedItem GetItemInVersion(IVersionedItem item, Version version)
        {
            VersionedItemPivot pivot = PivotLookupDictionary[item.ID];
            Guid result;
            if (pivot.PivotMapping.TryGetValue(version, out result))
            {
                return (IVersionedItem)Project.TranslateComponent(result);
            }
            else
            {
                return null;
            }
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

            VersionedItemPivot pivot1;
            VersionedItemPivot pivot2;

            VersionedItemPivot pivot;

            pivotLookupDictionary.TryGetValue(itemVersion1.ID, out pivot1);
            pivotLookupDictionary.TryGetValue(itemVersion2.ID, out pivot2);

            if (pivot1 == null && pivot2 == null) // new pivot is created
            {
                AddVersionedItem(itemVersion1, true);
                pivot = pivotLookupDictionary[itemVersion1.ID];
                pivotLookupDictionary[itemVersion2.ID] = pivot;
                pivot.PivotMapping[version2] = itemVersion2.ID;
            }
            else if (pivot1 == null) //existing pivot is used
            {
                pivot = pivot2;
                pivotLookupDictionary[itemVersion1.ID] = pivot;
                pivot.PivotMapping[version1] = itemVersion1.ID;
            }
            else if (pivot2 == null) //existing pivot is used
            {
                pivot = pivot1;
                pivotLookupDictionary[itemVersion2.ID] = pivot;
                pivot.PivotMapping[version2] = itemVersion2.ID;
            }
            else // in this case the two existing pivots are merged
            {
                pivot = new VersionedItemPivot();
                PivotList.Add(pivot);
                PivotList.Remove(pivot1);
                PivotList.Remove(pivot2);
                foreach (KeyValuePair<Guid, Guid> keyValuePair in pivot1.PivotMapping)
                {
                    Guid versionGuid = keyValuePair.Key;
                    Guid itemGuid = keyValuePair.Value;
                    pivotLookupDictionary[itemGuid] = pivot;
                    pivot.PivotMapping[versionGuid] = itemGuid;
                }
                foreach (KeyValuePair<Guid, Guid> keyValuePair in pivot2.PivotMapping)
                {
                    Guid versionGuid = keyValuePair.Key;
                    Guid itemGuid = keyValuePair.Value;
                    pivotLookupDictionary[itemGuid] = pivot;
                    pivot.PivotMapping[versionGuid] = itemGuid;
                }
            }
#if DEBUG            
            VerifyConsistency();
#endif
        }

        public void UnregisterVersionLink(IVersionedItem item1, IVersionedItem item2, IEnumerable<IVersionedItem> group1 = null, IEnumerable<IVersionedItem> group2 = null)
        {
            if (!AreItemsLinked(item1, item2))
            {
                throw new ExolutioModelException();
            }

            VersionedItemPivot pivot;
            if (!pivotLookupDictionary.TryGetValue(item1.ID, out pivot))
            {
                throw new ExolutioModelException();
            }

            if (group1 == null && group2 == null)
            {
                group1 = new List<IVersionedItem>(pivot.PivotMapping.Where(kvp => kvp.Key != item2.Version.ID).
                                                      Select(kvp => (IVersionedItem)Project.TranslateComponent(kvp.Value)));
                group2 = new IVersionedItem[] { item2 };
            }

            if (group1 == null)
            {
                group1 = new List<IVersionedItem>(pivot.PivotMapping.Where(kvp => !group2.Contains((IVersionedItem)Project.TranslateComponent(kvp.Value))).
                                                      Select(kvp => (IVersionedItem)Project.TranslateComponent(kvp.Value)));
            }
            if (group2 == null)
            {
                group2 = new List<IVersionedItem>(pivot.PivotMapping.Where(kvp => !group1.Contains((IVersionedItem)Project.TranslateComponent(kvp.Value))).
                                                      Select(kvp => (IVersionedItem)Project.TranslateComponent(kvp.Value)));
            }

            {
                VersionedItemPivot pivot1 = new VersionedItemPivot();
                PivotList.Add(pivot1);
                foreach (IVersionedItem versionedItem in group1)
                {
                    pivot1.PivotMapping[versionedItem.Version.ID] = versionedItem.ID;
                    pivotLookupDictionary[versionedItem.ID] = pivot1;
                }
            }

            {
                VersionedItemPivot pivot2 = new VersionedItemPivot();
                PivotList.Add(pivot2);
                foreach (IVersionedItem versionedItem in group2)
                {
                    pivot2.PivotMapping[versionedItem.Version.ID] = versionedItem.ID;
                    pivotLookupDictionary[versionedItem.ID] = pivot2;
                }
            }

            foreach (KeyValuePair<Guid, Guid> keyValuePair in pivot.PivotMapping)
            {
                IVersionedItem item = (IVersionedItem) Project.TranslateComponent(keyValuePair.Value);
                if (!group1.Contains(item) && !group2.Contains(item))
                {
                    throw new ExolutioModelException();
                }
            }

            PivotList.Remove(pivot);

#if DEBUG
            VerifyConsistency();
#endif
        }

        /// <summary>
        /// Adds new item to versioning infrastracture (item must not be linked to any other existing item).
        /// </summary>
        public void AddVersionedItem(IVersionedItem item, bool addWhenBranchingOrLoading = false)
        {
            if ((!Loading && !branching) || addWhenBranchingOrLoading)
            {
                if (pivotLookupDictionary.ContainsKey(item.ID))
                {
                    throw new ExolutioModelException("Item already added into versioning infrastracture. ");
                }

                VersionedItemPivot pivot = new VersionedItemPivot();
                PivotList.Add(pivot);
                pivotLookupDictionary[item.ID] = pivot;
                pivot.PivotMapping.Add(item.Version, item.ID);
            }
        }


        /// <summary>
        /// Removes item from versioning infrastructure
        /// </summary>
        public void RemoveVersionedItem(IVersionedItem removedItem)
        {
            if (!PivotLookupDictionary.ContainsKey(removedItem.ID) && branching)
            {
                return;
            }

            VersionedItemPivot pivot = PivotLookupDictionary[removedItem.ID];
            Version deletedVersion = removedItem.Version;

            // versioned item is going to be removed, thus it is removed from pivot lookup
            pivotLookupDictionary.Remove(removedItem.ID);
            pivot.PivotMapping.Remove(removedItem.Version.ID);

            if (pivot.PivotMapping.IsEmpty())
            {
                PivotList.Remove(pivot);
            }
        }

        public bool AreItemsLinked(IVersionedItem item1, IVersionedItem item2)
        {
            return pivotLookupDictionary[item1.ID] == pivotLookupDictionary[item2.ID];
        }

#if DEBUG
        public void VerifyConsistency()
        {
            foreach (KeyValuePair<Guid, VersionedItemPivot> kvp in PivotLookupDictionary)
            {
                VersionedItemPivot pivot = kvp.Value;
                IVersionedItem versionedItem = (IVersionedItem) Project.TranslateComponent(kvp.Key);

                Debug.Assert(pivot.PivotMapping.ContainsKey(versionedItem.Version));
                Debug.Assert(pivot.PivotMapping[versionedItem.Version.ID] == versionedItem.ID);
            }

            foreach (VersionedItemPivot pivot in PivotList)
            {
                foreach (KeyValuePair<Guid, Guid> itemID in pivot.PivotMapping)
                {
                    Version version = Project.TranslateComponent<Version>(itemID.Key);
                    IVersionedItem linkedItem = (IVersionedItem)Project.TranslateComponent(itemID.Value);
                    Debug.Assert(PivotLookupDictionary[linkedItem.ID] == pivot);
                }    
            }
        }
#endif

        #region serialization

        public void SerializeVersionLinks(XElement versionLinksElement, SerializationContext context)
        {
            foreach (VersionedItemPivot pivot in PivotList)
            {
                XElement linkedItemsElement = new XElement(context.ExolutioNS + "LinkedItems");
                foreach (Guid versionedItemID in pivot.PivotMapping.Values)
                {
                    IVersionedItem versionedItem = (IVersionedItem) Project.TranslateComponent(versionedItemID);
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
                PivotList.Add(pivot);

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
                        pivotLookupDictionary[exolutioObject.ID] = pivot;
                        pivot.PivotMapping.Add(kvp.Key, exolutioObject.ID);
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
