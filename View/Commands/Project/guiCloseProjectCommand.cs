using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using EvoX.Dialogs;
using EvoX.ResourceLibrary;

namespace EvoX.View.Commands.Project
{
    public class guiCloseProjectCommand : guiProjectCommand
    {
        #region Overrides of EvoXGuiCommandBase

        public override void Execute(object parameter = null)
        {
            // Save existing project before closing (if there are some unsaved changes)
            if (Current.Project != null && Current.Project.HasUnsavedChanges)
            {
                #if SILVERLIGHT
                MessageBoxResult r =
                    EvoXYesNoBox.Show(CommandsResources.guiCloseProjectCommand_Execute_Current_project_is_not_saved, CommandsResources.guiCloseProjectCommand_Execute_Do_you_want_to_save_it_, Current.MainWindow.FloatingWindowHost);

                #else
                MessageBoxResult r =
                    EvoXYesNoBox.Show(CommandsResources.guiCloseProjectCommand_Execute_Current_project_is_not_saved, CommandsResources.guiCloseProjectCommand_Execute_Do_you_want_to_save_it_);

                #endif

                if (r == MessageBoxResult.Yes)
                    EvoXGuiCommands.SaveProjectCommand.Execute();
                else
                    if (r == MessageBoxResult.Cancel)
                    {
                        if (parameter is System.ComponentModel.CancelEventArgs)
                            ((System.ComponentModel.CancelEventArgs)parameter).Cancel = true;
                        return;
                    }
            }

            Current.Project = null;
            Current.MainWindow.CloseProject();
            Current.MainWindow.CloseRibbonBackstage();
        }

        public override KeyGesture Gesture
        {
            get { return KeyGestures.ControlX; }
        }

        public override string Text
        {
            get { return CommandsResources.guiCloseProjectCommand_Text_Close_project; }
        }

        public override string ScreenTipText
        {
            get { return CommandsResources.guiCloseProjectCommand_ScreenTipText_Closes_current_project; }
        }

        public override bool CanExecute(object parameter = null)
        {
            return Current.Project != null;
        }

        public override ImageSource Icon
        {
            get { return EvoXResourceNames.GetResourceImageSource(EvoXResourceNames.cancel); }
        }

        #endregion
    }
}