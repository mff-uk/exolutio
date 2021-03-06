using System.Windows.Media;
using Exolutio.Model;
using Exolutio.Model.Versioning;
using Exolutio.ResourceLibrary;

namespace Exolutio.View.Commands.Versioning
{
    public class guiBranchCurrentVersion : guiCurrentVersionCommand
    {
        public override void Execute(object parameter)
        {
            ProjectVersion sourceVersion;
            ProjectVersion branchedVersion;
            if (Current.Project.UsesVersioning)
            {
                sourceVersion = Current.ProjectVersion;
                Version newVersion = new Version(Current.Project) { Label = "v" + Current.Project.ProjectVersions.Count + 1 };
                Current.Project.VersionManager.BranchProject(sourceVersion, newVersion);
                branchedVersion = Current.Project.GetProjectVersion(newVersion);
            }
            else
            {
                sourceVersion = Current.Project.LatestVersion;
                Current.Project.StartVersioning();
                Version newVersion = new Version(Current.Project) { Label = "v2" };
                Current.Project.VersionManager.BranchProject(sourceVersion, newVersion);
                branchedVersion = Current.Project.GetProjectVersion(newVersion);
            }

            Current.ProjectVersion = branchedVersion;

#if DEBUG
#if SILVERLIGHT
#else
            Tests.Versioning.BranchTest.VersionsEquivalent(sourceVersion, branchedVersion);
            Tests.ModelIntegrity.ModelConsistency.CheckProject(Current.Project);
#endif
#endif
            
            Current.MainWindow.RefreshMenu();
        }

        public override string Text
        {
            get { return "Branch this version"; }
        }

        public override string ScreenTipText
        {
            get { return "Create new branch of the project based on the current version."; }
        }

        public override bool CanExecute(object parameter)
        {
            return true;
        }
        
        public override ImageSource Icon
        {
            get { return ExolutioResourceNames.GetResourceImageSource(ExolutioResourceNames.branch_element); }
        }

    }
}