using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using EvoX.Model.Resources;
using EvoX.Model.Serialization;
using EvoX.SupportingClasses;

namespace EvoX.Model.Versioning
{
    public class VersionManager
    {
        /// <summary>
        /// Connects all versions of the same <see cref="IVersionedItem"/>
        /// </summary>
        private class VersionedItemPivot
        {
            private readonly EvoXDictionary<Version, IVersionedItem> pivotMapping = new EvoXDictionary<Version, IVersionedItem>();

            public EvoXDictionary<Version, IVersionedItem> PivotMapping
            {
                get { return pivotMapping; }
            }
        }

        public Project Project { get; set; }

        private bool branching = false;
        private bool firstVersionBranch = false; 

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

        private readonly EvoXDictionary<VersionedItemPivot, List<IVersionedItem>> linkedVersionedItems = new EvoXDictionary<VersionedItemPivot, List<IVersionedItem>>();
        private ReadOnlyDictionary<VersionedItemPivot, List<IVersionedItem>> LinkedVersionedItems
        {
            get { return linkedVersionedItems.AsReadOnly(); }
        }

        private readonly EvoXDictionary<IVersionedItem, VersionedItemPivot> pivotLookupDictionary = new EvoXDictionary<IVersionedItem, VersionedItemPivot>();
        private ReadOnlyDictionary<IVersionedItem, VersionedItemPivot> PivotLookupDictionary
        {
            get { return pivotLookupDictionary.AsReadOnly(); }
        }
        
        /// <summary>
        /// Creates new verion of the project from the existing version <param name="branchedVersion" />.
        /// The created version is fully integrated into versioning system 
        /// (dictionaries <see cref="EvoX.Model.Project" />.<see cref="EvoX.Model.Project.ProjectVersions" /> and version links
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
            IEnumerable<EvoXObject> allModelItems = ModelIterator.GetAllModelItems(branchedVersion);
            List<EvoXObject> evoXObjects = allModelItems.ToList();
            elementCopiesMap.PrepareGuids(evoXObjects);
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

            List<IVersionedItem> evoXList = deletedVersion.Items.ToList();
            foreach (IVersionedItem versionedItem in evoXList)
            {
                RemoveVersionedItem(versionedItem);
            }

            foreach (IVersionedItem versionedItem in evoXList)
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
                throw new EvoXModelException(Exceptions.VersionManager_RegisterVersionLink_itemOldVersion_Version_must_point_to_the_same_object_as_oldVersion);
            }

            if (itemVersion1.GetType() != itemVersion2.GetType())
            {
                throw new EvoXModelException();
            }

            VersionedItemPivot pivot;
            if (!pivotLookupDictionary.TryGetValue(itemVersion1, out pivot))
            {
                AddVersionedItem(itemVersion1);
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
        /// Adds item to versioning infrastracture. 
        /// </summary>
        /// <param name="item"></param>
        public void AddVersionedItem(IVersionedItem item)
        {
            if (pivotLookupDictionary.ContainsKey(item))
            {
                throw new EvoXModelException("Item already added into versioning infrastracture. ");
            }
            
            VersionedItemPivot pivot = new VersionedItemPivot();
            pivotLookupDictionary[item] = pivot;
            pivot.PivotMapping.Add(item.Version, item);
            linkedVersionedItems.CreateSubCollectionIfNeeded(pivot);
            linkedVersionedItems[pivot].Add(item);
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
                    throw new EvoXModelException("Inconsistent record in version links.");
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

        public void DeserializeVersionLinks(XElement parentNode, SerializationContext context)
        {
            foreach (XElement linkedItemsElement in parentNode.Elements(context.EvoXNS + "LinkedItems"))
            {
                VersionedItemPivot pivot = new VersionedItemPivot();

                Dictionary<Version, Guid> linkedItemsIds = new Dictionary<Version, Guid>();
                foreach (XElement linkedItemElement in linkedItemsElement.Elements(context.EvoXNS + "LinkedItem"))
                {
                    Guid id = SerializationContext.DecodeGuid(linkedItemElement.Attribute("itemID").Value);
                    int versionNumber = SerializationContext.DecodeInt(linkedItemElement.Attribute("versionNumber").Value);
                    Version version = this.GetVersionByNumber(versionNumber);
                    linkedItemsIds[version] = id;
                }

                if (linkedItemsIds.Count > 0)
                {
                    Version version1 = null;
                    IVersionedItem itemVersion1 = null;

                    foreach (KeyValuePair<Version, Guid> kvp in linkedItemsIds)
                    {
                        if (version1 == null)
                        {
                            version1 = kvp.Key;
                            itemVersion1 = (IVersionedItem)Project.TranslateComponent<EvoXObject>(kvp.Value);
                        }
                        else
                        {
                            Version otherVersion = kvp.Key;
                            IVersionedItem otherItemVersion = (IVersionedItem)Project.TranslateComponent<EvoXObject>(kvp.Value);
                            RegisterVersionLink(version1, otherVersion, itemVersion1, otherItemVersion);
                        }
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
            IEnumerable<EvoXObject> allModelItems = ModelIterator.GetAllModelItems(projectVersion);
            List<EvoXObject> evoXObjects = allModelItems.ToList();
            elementCopiesMap.PrepareGuids(evoXObjects);
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
            IEnumerable<EvoXObject> allModelItems = ModelIterator.GetAllModelItems(embededVersion);
            List<EvoXObject> evoXObjects = allModelItems.ToList();
            elementCopiesMap.PrepareGuids(evoXObjects);
            elementCopiesMap.PrepareGuid(embededVersion);

            Project.ProjectVersions.Add(newProjectVersion);
            embededVersion.FillCopy(newProjectVersion, newProjectVersion, elementCopiesMap);

            

            if (createVersionLinks)
            {
                foreach (KeyValuePair<IVersionedItem, IVersionedItem> kvp in elementCopiesMap)
                {
                    IVersionedItem fromEmbedded = kvp.Key;
                    IVersionedItem newlyCreated = kvp.Value;
                    EvoXObject previousObject;
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
