using System;
using System.ComponentModel;
using System.Windows.Input;
using System.Diagnostics;
using Exolutio.Controller;

namespace Exolutio.Controller.Commands
{
    /// <summary>
    /// Performs "Redo" of the last command on the "Redo" stack. 
    /// </summary>
	/// <remarks>This command is never put on any of the "Undo" or "Redo" stacks. </remarks>
    /// <seealso cref="UndoCommand"/>
    public class RedoCommand : INotifyPropertyChanged
    {
		private readonly CommandStack modelRedoStack;

        private readonly Controller Controller;
        
        public RedoCommand(Controller controller)
		{
			this.modelRedoStack = controller.RedoStack;
            Controller = controller;

			modelRedoStack.ItemsChanged += redoStack_ItemsChanged;
        }

        void redoStack_ItemsChanged(object sender, EventArgs e)
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
        //            return GetRedoneCommand().Description;
        //        }
        //        else return null;
        //    }
        //}


		public event EventHandler CanExecuteChanged;

        public bool CanExecute()
        {
			return modelRedoStack.Count > 0;
        }

        public void Execute()
        {
            StackedCommand redoneCommand = GetRedoneCommand();

        	redoneCommand.ExecuteAsRedo();
           
            if (RedoExecuted != null)
				RedoExecuted(redoneCommand);

            Controller.InvokeExecutedCommand(redoneCommand, false, null, false, true);
		}

    	private StackedCommand GetRedoneCommand()
    	{
            StackedCommand topStackedCommandModel = Controller.RedoStack.Empty ? null : Controller.RedoStack.Peek();
    		StackedCommand redoneCommand;

    		if (topStackedCommandModel != null)
    		{
   				redoneCommand = topStackedCommandModel;
    		}
    		else
    		{
                throw new InvalidOperationException(CommandErrors.CMDERR_STACK_INCONSISTENT);
    		}
    		return redoneCommand;
    	}

    	public event Action<StackedCommand> RedoExecuted;

		#region Implementation of INotifyPropertyChanged

		public event PropertyChangedEventHandler PropertyChanged;

		#endregion
    }
}
