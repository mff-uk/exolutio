using System;
using System.IO;
using System.Windows.Media;
using EvoX.View;
using EvoX.View.Commands;

namespace EvoX.WPFClient.Commands
{
    public class guiCloseActiveTab : guiCommandBase
    {
       
        public override void Execute(object parameter)
        {
            Current.MainWindow.DiagramTabManager.CloseActiveTab();
        }

        #region Overrides of guiCommandBase

        public override string Text
        {
            get { return "Close active tab"; }
        }

        public override System.Windows.Input.KeyGesture Gesture
        {
            get
            {
                return KeyGestures.ControlF4;
            }
        }
        
        #endregion

        public override string ScreenTipText
        {
            get { return "Closes the focused diagram tab."; }
        }

        public override bool CanExecute(object parameter)
        {
            return true; 
        }
    }
}