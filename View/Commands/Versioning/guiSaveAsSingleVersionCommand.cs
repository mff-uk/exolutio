using System;
using System.Windows.Media;
using EvoX.ResourceLibrary;
using EvoX.View.Commands.Project;

namespace EvoX.View.Commands.Versioning
{
    public class guiSaveAsSingleVersionCommand : guiCurrentVersionCommand
    {
        public override void Execute(object parameter)
        {
            Model.Project separateVersion = Current.Project.VersionManager.SeparateVersion(Current.ProjectVersion, true);
            Project.guiSaveAsProjectCommand guiSaveAsProjectCommand = new guiSaveAsProjectCommand();

            guiSaveAsProjectCommand.SavedProject = separateVersion;
            guiSaveAsProjectCommand.Execute();
        }

        public override string Text
        {
            get { return "Save as new project"; }
        }

        public override string ScreenTipText
        {
            get { return "Save this version as a new project"; }
        }

        public override bool CanExecute(object parameter)
        {
            return Current.Project != null && Current.Project.UsesVersioning;
        }

        public override ImageSource Icon
        {
            get { return EvoXResourceNames.GetResourceImageSource(EvoXResourceNames.Save); }
        }

    }
}