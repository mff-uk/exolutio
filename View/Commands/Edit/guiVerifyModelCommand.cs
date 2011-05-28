using System;
using Exolutio.Dialogs;
using Exolutio.ResourceLibrary;

namespace Exolutio.View.Commands.Edit
{
    public class guiVerifyModelCommand: guiCommandBase
    {
        public override bool CanExecute(object parameter)
        {
            return true;
        }

        public override void Execute(object parameter)
        {
            Tests.ModelIntegrity.ModelConsistency.CheckProject(Current.Project);
            #if SILVERLIGHT
            ExolutioMsgBox.Show("Verified", "Test passed", "Model consistency checked successfuly.", Current.MainWindow.FloatingWindowHost);
            #else
            ExolutioMessageBox.Show("Verified", "Test passed", "Model consistency checked successfuly.");
            #endif
        }

        public override string Text
        {
            get
            {
                return "Verify model";
            }
        }

        public override string ScreenTipText
        {
            get { return "Verify internal consistency of the model."; }
        }

        public override System.Windows.Media.ImageSource Icon
        {
            get { return ExolutioResourceNames.GetResourceImageSource(ExolutioResourceNames.Validate); }
        }
    }
}