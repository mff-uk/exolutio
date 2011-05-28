using System;
using System.ComponentModel;
using System.Windows.Input;
using System.Diagnostics;

namespace Exolutio.Controller.Commands
{
    /// <summary>
    /// Performs "Undo" of the last command on the "Undo" stack. 
    /// </summary>
	/// <remarks>This command is never put on any of the "Undo" or "Redo" stacks</remarks>
    /// <seealso cref="RedoCommand"/>
    public class UndoCommand: INotifyPropertyChanged
    {
        private readonly Controller Controller;

        public UndoCommand(Controller controller)
        {
            Controller = controller;

        	controller.UndoStack.ItemsChanged += undoStack_ItemsChanged;
        }

        void undoStack_ItemsChanged(object sender, EventArgs e)
        {
			if (CanExecuteChanged != null)
				CanExecuteChanged(sender, e);
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs("UnderlyingCommandDescription"));
        }

        //public string UnderlyingCommandDescription
        //{
        //    get
        //    {
        //        if (CanExecute())
        //        {
        //            return GetUndoneCommand().Description;
        //        }
        //        else return null;
        //    }
        //}

    	public event EventHandler CanExecuteChanged;

    	public bool CanExecute()
        {
            return Controller.UndoStack.Count > 0;
        }

        public void Execute()
        {
            StackedCommand undoneCommand = GetUndoneCommand();

        	undoneCommand.UnExecute();
            
            if (UndoExecuted != null)
				UndoExecuted(undoneCommand);

            Controller.OnUndoExecuted();
        }

    	private StackedCommand GetUndoneCommand()
    	{
            StackedCommand topStackedCommandModel = Controller.UndoStack.Empty ? null : Controller.UndoStack.Peek();
    		StackedCommand undoneCommand;

    		if (topStackedCommandModel != null)
    		{
  				undoneCommand = topStackedCommandModel;
    		}
    		else
    		{
                throw new InvalidOperationException(CommandErrors.CMDERR_STACK_INCONSISTENT);
    		}
    		return undoneCommand;
    	}

    	public event Action<StackedCommand> UndoExecuted; 

    	#region Implementation of INotifyPropertyChanged

    	public event PropertyChangedEventHandler PropertyChanged;

    	#endregion
    }
}
