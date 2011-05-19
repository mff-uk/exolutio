using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using EvoX.Dialogs;
using EvoX.ResourceLibrary;

namespace EvoX.View.Commands.Project
{
    public class guiCloseAppCommand : guiProjectCommand
    {
        #region Overrides of EvoXGuiCommandBase

        public override void Execute(object parameter = null)
        {
            if (Current.Project != null)
            {
                EvoXGuiCommands.CloseProjectCommand.Execute();
            }

            Current.MainWindow.Close();
            Current.MainWindow.CloseRibbonBackstage();
        }

        public override KeyGesture Gesture
        {
            get { return KeyGestures.ControlShiftX; }
        }

        public override string Text
        {
            get { return CommandsResources.guiCloseAppCommand_Text_Close; }
        }

        public override string ScreenTipText
        {
            get { return CommandsResources.guiCloseAppCommand_ScreenTipText_Closes_EvoX_application; }
        }

        public override bool CanExecute(object parameter = null)
        {
            return true;
        }

        public override ImageSource Icon
        {
            get { return EvoXResourceNames.GetResourceImageSource(EvoXResourceNames.cancel); }
        }

        #endregion
    }
}