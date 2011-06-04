using Exolutio.Model.PSM.Normalization;

namespace Exolutio.View.Commands
{
    public abstract class guiSelectionDependentCommand : guiActiveDiagramCommand
    {
        protected guiSelectionDependentCommand()
        {
            Current.SelectionChanged += delegate { OnCanExecuteChanged(null); };
        }
    }
}