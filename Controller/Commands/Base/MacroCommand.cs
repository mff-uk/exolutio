using System.Collections.Generic;
using System.Text;
using System.Linq;
using Exolutio.Model;

namespace Exolutio.Controller.Commands
{
	/// <summary>
    /// Command that consists of other Commands
    /// </summary>
    public class MacroCommand: StackedCommand
    {
        public MacroCommand()
        {
            Commands = new List<CommandBase>();
        }
        
        public MacroCommand(Controller controller)
    		: base(controller)
    	{
			Commands = new List<CommandBase>();
    	}

		public List<CommandBase> Commands { get; private set; }

		public override IList<Component> AssociatedElements
		{
			get
			{
				if (associatedElements.Count == 0
					&& Commands.Any(commandBase => commandBase.AssociatedElements.Count > 0))
				{
                    List<Component> elements = new List<Component>();
					foreach (CommandBase command in Commands)
					{
						elements.AddRange(command.AssociatedElements);
					}
					return elements;
				}
				return base.AssociatedElements;
			}
		}

		public bool CheckFirstOnlyInCanExecute { get; set; }

		public bool CanExecuteFirst()
		{
            if (Commands.Count > 0)
            {
                if (Commands[0].CanExecute())
                    return true;
                else
                {
                    ErrorDescription = Commands[0].ErrorDescription;
                    return false;
                }
            }
            else return false;
		}

		/// <summary>
        /// Returns <c>true</c> if all partial Commands can execute.
        /// </summary>
        /// <returns>Returns <c>true</c> if all partial Commands can execute.</returns>
        public override bool CanExecute()
		{
            if (Commands.Count == 0) return true;
            int count = CheckFirstOnlyInCanExecute ? 1 : Commands.Count;
			for (int i = 0; i < count; i++)
			{
				#if DEBUG
				FieldsChecker.CheckMandatoryArguments(Commands[i]);
				#endif
				if (!(Commands[i].CanExecute()))
				{
					ErrorDescription = Commands[i].ErrorDescription;
					return false;
				}
			}
			return true;
		}

		/// <summary>
        /// Performs CommandOperation of all of the partial Commands.
        /// </summary>
        internal override void CommandOperation()
		{
            GenerateSubCommands();
            List<CommandBase> list = new List<CommandBase>(Commands);
            foreach (CommandBase t in list)
		    {
		        Controller.InvokeExecutingCommand(t, true, this);
                #if DEBUG 
		        FieldsChecker.CheckMandatoryArguments(t);
                #endif
                if (t.CanExecute())
                {
                    //PREPROPAGATION START
                    if (t is StackedCommand)
                    {
                        StackedCommand command = t as StackedCommand;
                        if (command.Propagate)
                        {
                            MacroCommand m = command.PrePropagation();
                            if (m != null)
                            {
                                if (m.Report == null)
                                {
                                    m.Report = new CommandReport("Pre-propagation");
                                }
                                if (m.CanExecute())
                                {
                                    m.CommandOperation();
                                    Commands.Insert(Commands.IndexOf(t), m);
                                }
                                else throw new ExolutioCommandException(CommandErrors.COMMAND_CANT_EXECUTE, m);
                            }
                        }
                    }
                    //PREPROPAGATION END

                    t.CommandOperation();

                    //POSTPROPAGATION START
                    if (t is StackedCommand)
                    {
                        StackedCommand command = t as StackedCommand;
                        if (command.Propagate)
                        {
                            MacroCommand m = command.PostPropagation();
                            if (m != null)
                            {
                                if (m.Report == null)
                                {
                                    m.Report = new CommandReport("Post-propagation");
                                }
                                if (m.CanExecute())
                                {
                                    m.CommandOperation();
                                    Commands.Insert(Commands.IndexOf(t) + 1, m);
                                }
                                else throw new ExolutioCommandException(CommandErrors.COMMAND_CANT_EXECUTE, m);
                            }
                        }
                    }
                    //POSTPROPAGATION END
                }
                else
                {
                    throw new ExolutioCommandException(t.ErrorDescription ?? CommandErrors.COMMAND_CANT_EXECUTE, t) { ExceptionTitle = CommandErrors.COMMAND_CANT_EXECUTE };
                }
                #if DEBUG
		        FieldsChecker.CheckCommandResults(this);
                #endif
		        Controller.InvokeExecutedCommand(t, true, this);
		    }
		    CommandsExecuted();
		}

	    /// <summary>
        /// Performs RedoOperation of all of the partial Commands.
        /// </summary>
        internal override void RedoOperation()
        {
            foreach (CommandBase t in Commands)
            {
                Controller.InvokeExecutingCommand(t, true, this);
                t.RedoOperation();
                Controller.InvokeExecutedCommand(t, true, this);
            }
            CommandsExecuted();
        }

	    /// <summary>
		/// Called after all commands from the macro are executed.
		/// Can be used as a finalizer of the command in derived classes, 
		/// but <strong>must not perform any operation in the model</strong>.
		/// </summary>
		public virtual void CommandsExecuted()
		{
			
		}

		/// <summary>
        /// Performs UndoOperation of all of the partial Commands (in reverse order).
        /// </summary>
        internal override OperationResult UndoOperation()
        {
            for (int i = Commands.Count - 1; i >= 0; i--)
            {
                if (Commands[i].UndoOperation() == OperationResult.Failed)
                {
                    ErrorDescription = "Command failed to Undo: " + Commands[i].ErrorDescription;
                    for (int j = i; j < Commands.Count; j++)
                    {
                        //REDO what was undone before the failure
                        if (Commands[j].CanExecute()) Commands[j].CommandOperation();
                    }
                    return OperationResult.Failed;
                }
            }
            return OperationResult.OK;
        }

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder(this.GetType().Name + " { ");
			foreach (CommandBase command in Commands)
			{
				sb.Append(command);
				sb.Append(", ");
			}
			sb.Remove(sb.Length - 2, 2);
			sb.Append(" }");
			return sb.ToString();
		}

	    /// <summary>
	    /// This method should generate subcommands of a macrocommand.
        /// It is called when the macrocommand begins execution and 
        /// requires that all settings of the macrocommand are correctly set.
	    /// </summary>
        protected virtual void GenerateSubCommands()
	    {
	        
	    }

        public virtual NestedCommandReport GetReport()
        {
            NestedCommandReport result = new NestedCommandReport();
            result.CommandType = this.GetType();

            if (this.Report != null && !this.HideReport)
            {
                result.Contents = this.Report.Contents;
            }

            foreach (CommandBase command in Commands)
            {
                if (!command.HideReport)
                {
                    if (command is MacroCommand)
                    {
                        NestedCommandReport nestedCommandReport = ((MacroCommand) command).GetReport();
                        nestedCommandReport.CommandType = command.GetType();
                        result.NestedReports.Add(nestedCommandReport);
                    } 
                    else if (command.Report != null)
                    {
                        CommandReport commandReport = command.Report;
                        commandReport.CommandType = command.GetType();
                        result.NestedReports.Add(commandReport);
                    }
                }
            }

            return result; 
        }
    }
}
