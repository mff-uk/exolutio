using System;

namespace EvoX.View.Commands
{
    public class guiModelTreeCommand: guiCommandBase
    {
        public override void Execute(object parameter)
        {
            
        }

        public override string ScreenTipText
        {
            get { return Text; }
        }

        public override bool CanExecute(object parameter = null)
        {
            return true; 
        }
    }
}