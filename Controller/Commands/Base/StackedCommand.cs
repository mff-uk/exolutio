using System.Diagnostics;
using EvoX.Model;
using System;

namespace EvoX.Controller.Commands
{
    /// <summary>
    /// Abstract command, all other Commands with undo/redo semantics that 
    /// should be put to the "Undo"/"Redo" stacks
    /// should inherit from this base class. Prescribes 
    /// command infrastructure, handles command stacks.
    /// </summary>
    /// <remarks>
    /// Inheriting classes have to implement <see cref="CommandBase.CommandOperation"/> and 
    /// <see cref="CommandBase.UndoOperation"/>
    /// and <see cref="CommandBase.CanExecute"/> methods. 
    /// <see cref="CommandBase.OnCanExecuteChanged"/> should be called each time some
    /// changes occur that affect whether or not the command should execute.
    /// </remarks>
    /// <seealso cref="MacroCommand"/>
	public abstract class StackedCommand
		: CommandBase 
    {
		/// <summary>
		/// Controller of the command, handles command execution and contains 
		/// stacks. 
		/// </summary>
		public Controller Controller { get; set; }

        public Project Project { get { return Controller.Project; } }

        private bool propagate = true;

        /// <summary>
        /// Sets whether this command should Propagate to another level (PIM/PSM). Default is true. 
        /// </summary>
        public bool Propagate
        {
            get { return propagate; }
            set { propagate = value; }
        }

        private Guid propagateSource;
        
        /// <summary>
        /// Sets the source of propagation to avoid propagating back to the source. (PSM => PIM => PSMs)
        /// </summary>
        public Guid PropagateSource
        {
            get { return propagateSource; }
            set { propagateSource = value; }
        }

		/// <summary>
		/// Gets <see cref="Controller"/>'s UndoStack
		/// </summary>
        protected CommandStack UndoStack
        {
            get { return Controller.UndoStack; }
        }

		/// <summary>
		/// Gets <see cref="Controller"/>'s RedoStack
		/// </summary>
        protected CommandStack RedoStack
        {
            get { return Controller.RedoStack; }
        }

        protected StackedCommand()
        {
        }

        /// <summary>
		/// Creates new instance of StackedCommandBase. 
		/// </summary>
		/// <param name="controller">controller of the command, handles command execution and contains command stacks</param>
		protected StackedCommand(Controller controller)
		{
			Controller = controller;
		}

    	/// <summary>
    	/// Executes the command with the given parameter. This means calling 
    	/// <see cref="CommandBase.CommandOperation"/>,
    	/// pushing the command on the "Undo" stack and clearing the "Redo" stack.
    	/// </summary>
		public override sealed void Execute()
		{
			if (Controller.CreatedMacro != null)
			{
				Controller.CreatedMacro.Commands.Add(this);
				return;
			}

			CommandNumber = getNextCommandNumber();
			// call notifying method
			Controller.InvokeExecutingCommand(this, false, null);
			// check mandatory arguments
#if DEBUG
			FieldsChecker.CheckMandatoryArguments(this);
#endif
			if (!CanExecute())
			{
                throw new EvoXCommandException(CommandErrors.COMMAND_CANT_EXECUTE, this);
			}

            /*if (Propagate)
            {
                MacroCommand preCommand = PrePropagation();
                if (preCommand == null)
                {
                    this.paired = false;
                }
                else
                {
                    this.paired = true;
                    preCommand.paired = false;
                    preCommand.Propagate = false;
                    preCommand.Execute();
                }
            }*/

            if (Undoable)
            {
                UndoStack.Push(this);
                // all redo Commands are from now invalid
                RedoStack.Invalidate();
            }
            // call the actual executive method
			CommandOperation();
            Executed = true;
			//Debug.WriteLine(string.Format("Command {0} executed.", this));

            /*if (Propagate)
            {
                MacroCommand postCommand = PostPropagation();
                if (postCommand != null)
                {
                    postCommand.paired = true;
                    postCommand.Propagate = false;
                    postCommand.Execute();
                }
            }*/

#if DEBUG
			FieldsChecker.CheckCommandResults(this);
#endif
			// call successful notification method
			Controller.InvokeExecutedCommand(this, false, null);
		}

    	/// <summary>
    	/// Executes the command as "Redo", which is the same as <see cref="Execute"/> does, except it does not
    	/// clear the "Redo" stack.
    	/// </summary>
    	public void ExecuteAsRedo()
    	{
    		Debug.Assert(RedoStack != null && RedoStack.Count > 0 && RedoStack.Peek() == this);
    		RedoStack.Pop();
    		// push command on the undo stack
    		UndoStack.Push(this);
    		if (!CanExecute())
    		{
                throw new EvoXCommandException(CommandErrors.COMMAND_CANT_EXECUTE, this);
    		}
			// call the actual executive method
    		RedoOperation();
            //Example: propagation MacroCommand
            /*if (paired)
            {
                paired = false;
                StackedCommand NextCommand = RedoStack.Peek();
                NextCommand.ExecuteAsRedo();
                NextCommand.paired = true;
            }*/
            Debug.WriteLine(string.Format("Redo of command {0} executed", this));
    	}

    	/// <summary>
    	/// Executes "Undo" of the command. This means removing the command from the "Undo" stack and moving it 
    	/// to the "Redo" stack and calling 
    	/// <see cref="CommandBase.UndoOperation"/>.
    	/// </summary>
    	public override sealed void UnExecute()
    	{
    		// assert the command is at the top of the undo stack 
    		Debug.Assert(UndoStack != null && UndoStack.Count > 0 && UndoStack.Peek() == this);
    		// pop command from the undo stack
    		UndoStack.Pop();
    		// push command to the redo stack 
    		// call the actual undo method
    		if (UndoOperation() == OperationResult.OK)
    		{
    			Debug.WriteLine(string.Format("Undo of command {0} executed", this));
				RedoStack.Push(this);
                //Example: propagation MacroCommand
                /*if (paired)
                {
                    paired = false;
                    StackedCommand NextCommand = UndoStack.Peek();
                    NextCommand.UnExecute();
                    NextCommand.paired = true;
                }*/
    		}
    		else
    		{
    			Debug.WriteLine(string.Format("ERROR: undo of command {0} failed!", this));
				UndoStack.Invalidate();
    		}
    	}

        /// <summary>
        /// Creates a macrocommand containing what needs to be done before command execution
        /// </summary>
        /// <returns></returns>
        internal virtual MacroCommand PrePropagation()
        {
            return null;
        }

        /// <summary>
        /// Creates a macrocommand containing what needs to be done after command execution
        /// </summary>
        /// <returns></returns>
        internal virtual MacroCommand PostPropagation()
        {
            return null;
        }
    }
}
