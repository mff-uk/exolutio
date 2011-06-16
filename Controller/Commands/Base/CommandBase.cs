using System;
using System.Collections.Generic;
using Exolutio.Model;

namespace Exolutio.Controller.Commands
{
	/// <summary>
    /// Abstract command, all other Commands have to inherit from this base class. Prescribes 
    /// command infrastructure, handles command stacks. Execution does not automatically put the command on the 
    /// "Undo"/"Redo" stack. Consider using <see cref="StackedCommand"/> if this is desirable.
    /// </summary>
	/// <remarks>
	/// Inheriting classes have to implement <see cref="CommandOperation"/> and <see cref="UndoOperation"/> and 
	/// and <see cref="CanExecute"/> methods. <see cref="OnCanExecuteChanged"/> should be called each time some
	/// changes occur that affect whether or not the command should execute.
	/// </remarks>
	/// <seealso cref="StackedCommand"/>
    public abstract class CommandBase
	{
        ///// <summary>
        ///// Description of the command (is shown to the user)
        ///// </summary>
        //public string Description { get; set; }

    	private static int commandCounter = 1;

		/// <summary>
		/// Returns next command number and increases the counter. 
		/// The counter is shared among all commands. 
		/// </summary>
		/// <returns></returns>
		protected static int getNextCommandNumber()
		{
			return commandCounter++;
		}

    	private int ? commandNumber = null;

		/// <summary>
		/// Number of the command. 
		/// </summary>
		/// <seealso cref="getNextCommandNumber"/>
    	public int ? CommandNumber
    	{
    		get
    		{
    			return commandNumber;
    		}
    		protected set
    		{
				if (commandNumber != null)
				{
                    throw new InvalidOperationException(CommandErrors.CMDERR_COMMAND_NUMBER_ALTERED);
				}
    			commandNumber = value;
    		}
    	}

        protected readonly List<Component> associatedElements = new List<Component>();
		
		/// <summary>
		/// Elements associated with the command. 
		/// </summary>
        public virtual IList<Component> AssociatedElements
		{
			get { return associatedElements; }
		}

		/// <summary>
		/// Possible operation results
		/// </summary>
    	public enum OperationResult
        {
			/// <summary>
			/// Operation completed successfully
			/// </summary>
            OK,
			/// <summary>
			/// Operation failed
			/// </summary>
            Failed
        }
        
		/// <summary>
		/// This event should be raised each some condition that determines whether 
		/// the command can execute or not changes
		/// </summary>
        public virtual event EventHandler CanExecuteChanged;

		/// <summary>
		/// Holds description of the error that caused <see cref="CanExecute"/> to fail. 
		/// Should be set in <see cref="CanExecute"/>.
		/// </summary>
		public string ErrorDescription { get; protected set; }

        /// <summary>
        /// Indicates whether the command should or should not be put to the Undo stack
        /// </summary>
        protected virtual bool Undoable{ get { return true; }}

	    private CommandReport report;
	    public CommandReport Report
	    {
	        get
	        {
	            return report;
	        }
	        set { report = value; }
	    }

        public bool HideReport { get; set; }

	    /// <summary>
	    /// False before execution, true afterwards
	    /// </summary>
	    protected bool Executed { get; set; }

	    /// <summary>
        /// Returns <c>true</c> if command can be executed.
        /// </summary>
        /// <returns>True if command can be executed</returns>
        public abstract bool CanExecute();
        
        /// <summary>
        /// This method should be called each time some
        /// changes occur that affect whether or not the command should execute.
        /// </summary>
        /// <param name="sender">subscribers of the <see cref="CanExecuteChanged"/> event will get this value in the sender argument</param>
        /// <param name="args">subscribers of the <see cref="CanExecuteChanged"/> event will get this value in the args argument</param>
        public virtual void OnCanExecuteChanged(object sender, EventArgs args)
        {
            if (CanExecuteChanged != null)
            {
                CanExecuteChanged(sender, args);
            }
        }

		private static CommandFieldsChecker fieldsChecker;
		
		/// <summary>
		/// Returns <see cref="CommandFieldsChecker"/> instance. Can be used to check 
		/// mandatory arguments and command results. 
		/// </summary>
		/// <seealso cref="PublicArgumentAttribute"/>
		/// <seealso cref="CommandResultAttribute"/>
		protected static CommandFieldsChecker FieldsChecker
		{
			get
			{
				if (fieldsChecker == null)
				{
					fieldsChecker = new CommandFieldsChecker();
				}
				return fieldsChecker;
			}
		}

	    /// <summary>
	    /// Executive function of a command
	    /// </summary>
	    /// <seealso cref="UndoOperation"/>
	    internal abstract void CommandOperation();

	    /// <summary>
	    /// Undo executive function of a command. Should revert the <see cref="CommandOperation"/> executive 
	    /// function and return the state to the state before <see cref="CommandOperation"/> was execute.
	    /// <returns>returns <see cref="OperationResult.OK"/> if operation succeeded, <see cref="OperationResult.Failed"/> otherwise</returns>
	    /// </summary>
	    /// <remarks>
	    /// <para>If  <see cref="OperationResult.Failed"/> is returned, whole undo stack is invalidated</para>
	    /// </remarks>
	    internal abstract OperationResult UndoOperation();

        /// <summary>
        /// Redo executive function of a command. Should revert the <see cref="UndoOperation"/>.
        /// Usually, this is the same as <see cref="CommandOperation"/>
        /// but commands adding new elements to model need to redefine this method to avoid
        /// creation of a completely new instance
        /// </summary>
        internal virtual void RedoOperation()
        {
            CommandOperation();
        }
        ///<summary>
        ///Defines the method to be called when the command is invoked.
        ///</summary>
		public virtual void Execute()
		{
			CommandNumber = getNextCommandNumber();
			// check mandatory arguments
			#if DEBUG 
			FieldsChecker.CheckMandatoryArguments(this);
			#endif
			if (CanExecute())
			{
                CommandOperation();
                Executed = true;
			}
            else
			{
                throw new ExolutioCommandException(CommandErrors.COMMAND_CANT_EXECUTE, this);
			}
			#if DEBUG
			FieldsChecker.CheckCommandResults(this);
			#endif
		}

        /// <summary>
        /// Defines method to be called when the command rollback/undo is invoked
        /// </summary>
        public virtual void UnExecute()
        {
            UndoOperation();
        }

		/// <summary>
		/// Returns a <see cref="T:System.String" /> that represents the current <see cref="T:System.Object" />.     
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.String" /> that represents the current <see cref="T:System.Object" />.      
		/// </returns>
		public override string ToString()
		{
			return this.GetType().Name;
		}

        /// <summary>
        /// If true, CanExecute should always return true (in commands where applicable - allows temporary inconsistency when propagating)
        /// </summary>
        internal bool ForceExecute = false;
	}
}
