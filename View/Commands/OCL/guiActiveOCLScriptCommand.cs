namespace Exolutio.View.Commands.OCL
{
    public abstract class guiActiveOCLScriptCommand: guiActiveDiagramCommand
    {
        protected guiActiveOCLScriptCommand()
        {
            Current.ActiveOCLScriptChanged += new System.Action(Current_ActiveOCLScriptChanged);
        }

        void Current_ActiveOCLScriptChanged()
        {
            InvokeCanExecuteChanged();
        }
    }
}