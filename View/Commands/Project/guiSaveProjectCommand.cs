using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Exolutio.Model.Serialization;
using Exolutio.ResourceLibrary;

namespace Exolutio.View.Commands.Project
{
    public class guiSaveProjectCommand : guiProjectCommand
    {
        #region Overrides of ExolutioGuiCommandBase

#if SILVERLIGHT 
#else
        public override KeyGesture Gesture
        {
            get { return KeyGestures.ControlS; }
        }
#endif

        public override string Text
        {
            get { return CommandsResources.guiSaveProjectCommand_Text_Save_project; }
        }

        public override string ScreenTipText
        {
            get { return CommandsResources.guiSaveProjectCommand_ScreenTipText_Saves_changes_made_in_the_project; }
        }

        public override ImageSource Icon
        {
            get { return ExolutioResourceNames.GetResourceImageSource(ExolutioResourceNames.Save); }
        }

        #endregion

        public override void Execute(object parameter = null)
        {
            if (Current.Project.ProjectFile == null)
            {
                GuiCommands.SaveAsProjectCommand.Execute();
            }
            else
            {
                // Save only if there are some new unsaved changes
                if (Current.Project.HasUnsavedChanges)
                {
                    (new ProjectSerializationManager()).SaveProject(Current.Project, Current.Project.ProjectFile);
                }
            }

            Current.MainWindow.CloseRibbonBackstage();
        }

        public override bool CanExecute(object parameter = null)
        {
            return Current.Project != null && Current.Project.HasUnsavedChanges;
        }
    }
}