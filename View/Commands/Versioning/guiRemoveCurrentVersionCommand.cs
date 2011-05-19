using System.Windows.Media;
using EvoX.Model.Versioning;
using EvoX.ResourceLibrary;

namespace EvoX.View.Commands.Versioning
{
    public class guiRemoveCurrentVersionCommand : guiCurrentVersionCommand
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
            get { return EvoXResourceNames.GetResourceImageSource(EvoXResourceNames.branch_delete); }
        }

    }
}