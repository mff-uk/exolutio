namespace EvoX.View.Commands.Project
{
    /// <summary>
    /// Parent command for all project commands
    /// </summary>
    public abstract class guiProjectCommand : guiCommandBase
    {
        protected guiProjectCommand()
        {
            Current.ProjectChanged += CurrentProjectChanged;
        }

        void CurrentProjectChanged(object sender, CurrentProjectChangedEventArgs e)
        {
            if (e.OldProject != null)
            {
                e.OldProject.PropertyChanged -= Project_PropertyChanged;
            }
            if (e.NewProject != null)
            {
                e.NewProject.PropertyChanged += Project_PropertyChanged;
            }
            OnCanExecuteChanged(e);
        }

        void Project_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "HasUnsavedChanges")
            {
                OnCanExecuteChanged(e);
            }
        }
    }
}