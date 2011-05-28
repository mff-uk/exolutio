namespace Exolutio.View.Commands
{
    public abstract class guiCurrentVersionCommand : guiCommandBase
    {
        protected guiCurrentVersionCommand()
        {
            Current.ProjectVersionChanged += Current_ProjectVersionChanged;
        }

        void Current_ProjectVersionChanged(object sender, CurrentProjectVersionChangedEventArgs e)
        {
            OnCanExecuteChanged(e);
        }
    }
}