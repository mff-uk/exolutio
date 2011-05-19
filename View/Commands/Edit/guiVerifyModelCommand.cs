using System;
using EvoX.Dialogs;
using EvoX.ResourceLibrary;

namespace EvoX.View.Commands.Edit
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
            EvoXMsgBox.Show("Verified", "Test passed", "Model consistency checked successfuly.", Current.MainWindow.FloatingWindowHost);
            #else
            EvoXMsgBox.Show("Verified", "Test passed", "Model consistency checked successfuly.");
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
            get { return EvoXResourceNames.GetResourceImageSource(EvoXResourceNames.Validate); }
        }
    }
}