using System.Windows.Media;
using Exolutio.Model.Versioning;
using Exolutio.ResourceLibrary;

namespace Exolutio.View.Commands.Versioning
{
    public class guiRemoveCurrentVersion : guiCurrentVersionCommand
    {
        public override void Execute(object parameter)
        {
            Current.Project.VersionManager.DeleteVersion(Current.ProjectVersion.Version);
            
            Current.ProjectVersion = Current.Project.LatestVersion;

            #if SILVERLIGHT
            #else
            Tests.ModelIntegrity.ModelConsistency.CheckProject(Current.Project);
            #endif
        }

        public override string Text
        {
            get { return "Remove this version"; }
        }

        public override string ScreenTipText
        {
            get { return "Removes current version from the project."; }
        }

        public override bool CanExecute(object parameter)
        {
            return Current.Project != null && Current.Project.UsesVersioning && Current.Project.ProjectVersions.Count > 1;
        }
        
        public override ImageSource Icon
        {
            get { return ExolutioResourceNames.GetResourceImageSource(ExolutioResourceNames.branch_delete); }
        }

    }
}