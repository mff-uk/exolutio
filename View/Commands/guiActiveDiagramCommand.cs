using Exolutio.Model.PSM.Normalization;

namespace Exolutio.View.Commands
{
    public abstract class guiActiveDiagramCommand: guiCommandBase
    {
        protected guiActiveDiagramCommand()
        {
            Current.ProjectChanged += Current_ProjectChanged;
            Current.ActiveDiagramChanged += Current_ActiveDiagramChanged;
        }

        void Current_ActiveDiagramChanged()
        {
            OnCanExecuteChanged(null);
        }

        void Current_ProjectChanged(object sender, CurrentProjectChangedEventArgs e)
        {
            OnCanExecuteChanged(e);
        }
    }
}