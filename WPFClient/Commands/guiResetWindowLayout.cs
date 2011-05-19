using System;
using System.IO;
using System.Windows.Media;
using EvoX.View;
using EvoX.View.Commands;

namespace EvoX.WPFClient.Commands
{
    public class guiResetWindowLayout : guiCommandBase
    {
        public override void Execute(object parameter)
        {
            TextReader r = new StringReader(Properties.Resources.defaultLayout);
            ((MainWindow)Current.MainWindow).dockManager.RestoreLayout(r);
            r.Close();
            Current.MainWindow.CloseRibbonBackstage();
        }

        #region Overrides of guiCommandBase

        public override string Text
        {
            get { return "Reset Window Layout"; }
        }

        
        #endregion

        public override string ScreenTipText
        {
            get { return "Restores layout of dockable windows to the original state."; }
        }

        public override bool CanExecute(object parameter)
        {
            return true; 
        }
    }
}