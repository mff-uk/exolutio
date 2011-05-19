using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using EvoX.Model.Serialization;
using EvoX.ResourceLibrary;

namespace EvoX.View.Commands.Project
{
    public class guiSaveProjectCommand : guiProjectCommand
    {
        #region Overrides of EvoXGuiCommandBase

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
            get { return EvoXResourceNames.GetResourceImageSource(EvoXResourceNames.Save); }
        }

        #endregion

        public override void Execute(object parameter = null)
        {
            if (Current.Project.ProjectFile == null)
            {
                EvoXGuiCommands.SaveAsProjectCommand.Execute();
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