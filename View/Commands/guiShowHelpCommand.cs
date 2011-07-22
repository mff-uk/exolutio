using System;
using Exolutio.Dialogs;

namespace Exolutio.View.Commands
{
    public class guiShowHelpCommand : guiCommandBase
    {
        public override bool CanExecute(object parameter)
        {
            return true;
        }

        public override void Execute(object parameter)
        {
#if SILVERLIGHT

            HelpBox helpBox = new HelpBox();
            helpBox.ShowDialog();
#else
#endif
        }

        public override string Text
        {
            get { return "Help"; }
            set
            {
                base.Text = value;
            }
        }

        public override string ScreenTipText
        {
            get { return "Show basic info/help. "; }
        }
    }
}