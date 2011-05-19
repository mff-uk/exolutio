using System.Windows.Media;
using EvoX.Model;
using EvoX.Model.PIM;
using EvoX.Model.PSM;
using EvoX.Model.Versioning;
using EvoX.ResourceLibrary;

namespace EvoX.View.Commands.Versioning
{
    public class guiAddVersionLinkCommand : guiCommandBase
    {
        public override void Execute(object parameter)
        {
            
        }

        public override string Text
        {
            get { return "Add version link"; }
        }

        public override string ScreenTipText
        {
            get { return "Add version link between two constructs."; }
        }

        public override bool CanExecute(object parameter)
        {
            return true;
        }

        public override ImageSource Icon
        {
            get { return EvoXResourceNames.GetResourceImageSource(EvoXResourceNames.associate); }
        }

    }
}