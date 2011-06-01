using System;

namespace Exolutio.Model.Versioning
{
    public interface IVersionedItem
    {
        Guid ID { get; }

        Version Version
        {
            get; 
        }

        ProjectVersion ProjectVersion { get; }
    }

    public static class IVersionedItemExt
    {
        public static IVersionedItem GetInVersion(this IVersionedItem item, Version version)
        {
            return item.ProjectVersion.Project.VersionManager.GetItemInVersion(item, version);
        }

        public static TComponent GetInVersion<TComponent>(this TComponent item, Version version)
            where TComponent : IVersionedItem
        {
            return (TComponent) item.ProjectVersion.Project.VersionManager.GetItemInVersion(item, version);
        }

        public static bool ExistsInVersion(this IVersionedItem item, Version version)
        {
            return item.ProjectVersion.Project.VersionManager.GetItemInVersion(item, version) != null;
        }
    }
    
}
