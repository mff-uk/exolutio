using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Exolutio.Dialogs;
using Exolutio.ResourceLibrary;

namespace Exolutio.View.Commands.Project
{
    public class guiCloseAppCommand : guiProjectCommand
    {
        #region Overrides of ExolutioGuiCommandBase

        public override void Execute(object parameter = null)
        {
            if (Current.Project != null)
            {
                GuiCommands.CloseProjectCommand.Execute();
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
            get { return CommandsResources.guiCloseAppCommand_ScreenTipText_Closes_Exolutio_application; }
        }

        public override bool CanExecute(object parameter = null)
        {
            return true;
        }

        public override ImageSource Icon
        {
            get { return ExolutioResourceNames.GetResourceImageSource(ExolutioResourceNames.cancel); }
        }

        #endregion
    }
}