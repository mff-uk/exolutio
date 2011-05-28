using System.Windows.Media;
using Exolutio.Model;
using Exolutio.Model.PIM;
using Exolutio.Model.PSM;
using Exolutio.Model.Versioning;
using Exolutio.ResourceLibrary;

namespace Exolutio.View.Commands.Versioning
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
            get { return ExolutioResourceNames.GetResourceImageSource(ExolutioResourceNames.associate); }
        }

    }
}