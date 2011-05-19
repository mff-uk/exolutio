using System;
using System.Collections.Generic;
using EvoX.Controller.Commands;
using EvoX.Model;

namespace EvoX.Controller
{
    public class Controller
    {
        public Project Project { get; private set; }
        
        public CommandStack UndoStack { get; private set; }

        public CommandStack RedoStack { get; private set; }

        public Controller(Project p)
        {
            UndoStack = new CommandStack();
            RedoStack = new CommandStack();
            Project = p;
            ExecutedCommand += new CommandEventHandler(Controller_ExecutedCommand);
        }

        void Controller_ExecutedCommand(CommandBase command, bool isPartOfMacro, CommandBase macroCommand)
        {
            Project.HasUnsavedChanges = true; 
        }

        /// <summary>
        /// This method raises the <see cref="ExecutingCommand"/> event
        /// </summary>
        /// <param name="commandBase">Either DiagramCommandBase or ModelCommandBase</param>
        /// <param name="isPartOfMacro">Tells whether this command is or is not a part of a <see cref="MacroCommand"/></param>
        /// <param name="macroCommand">References the <see cref="MacroCommand"/> of which this command is a part</param>
        public void InvokeExecutingCommand(CommandBase commandBase, bool isPartOfMacro, CommandBase macroCommand)
        {
            if (ExecutingCommand != null)
            {
                ExecutingCommand(commandBase, isPartOfMacro, macroCommand);
            }
        }

        /// <summary>
        /// This method raises the <see cref="ExecutedCommand"/> event
        /// </summary>
        /// <param name="commandBase">Either DiagramCommandBase or ModelCommandBase</param>
        /// <param name="isPartOfMacro">Tells whether this command is or is not a part of a <see cref="MacroCommand"/></param>
        /// <param name="macroCommand">References the <see cref="MacroCommand"/> of which this command is a part</param>
        public void InvokeExecutedCommand(CommandBase commandBase, bool isPartOfMacro, CommandBase macroCommand)
        {
            if (ExecutedCommand != null)
            {
                ExecutedCommand(commandBase, isPartOfMacro, macroCommand);
            }
        }

        /// <summary>
        /// All commands executed after this call will be stored in a queue and executed when <see cref="CommitMacro"/> is called
        /// </summary>
        /// <returns>The <see cref="MacroCommand"/> created</returns>
        public MacroCommand BeginMacro()
        {
            CreatedMacro = new MacroCommand(this);
            return CreatedMacro;
        }

        /// <summary>
        /// The created <see cref="MacroCommand"/> from the <see cref="BeginMacro"/>/<see cref="CommitMacro"/> call pair
        /// </summary>
        public MacroCommand CreatedMacro { get; protected set; }

        /// <summary>
        /// Executes all commands stored after the <see cref="BeginMacro"/> call
        /// </summary>
        public void CommitMacro()
        {
            MacroCommand tmp = CreatedMacro;
            CreatedMacro = null;
            if (tmp.Commands.Count > 0)
                tmp.Execute();
        }

        /// <summary>
        /// All stored commands after the <see cref="BeginMacro"/> call are thrown away
        /// </summary>
        public void CancelMacro()
        {
            CreatedMacro = null;
        }

        /// <summary>
        /// Raised when a command starts executing
        /// </summary>
        public event CommandEventHandler ExecutingCommand;

        /// <summary>
        /// Raised when a command finished executing
        /// </summary>
        public event CommandEventHandler ExecutedCommand;

        public event Action UndoExecuted;

        internal void OnUndoExecuted()
        {
            Action handler = UndoExecuted;
            if (handler != null) handler();
        }

        public event Action RedoExecuted;

        internal void OnRedoExecuted()
        {
            Action handler = RedoExecuted;
            if (handler != null) handler();
        }

        public void ExecuteCommands(IEnumerable<CommandBase> commands)
        {
            foreach (CommandBase command in commands)
            {
                StackedCommand stackedCommand = command as StackedCommand;
                if (stackedCommand != null)
                {
                    stackedCommand.Controller = this;
                }
                command.Execute();
            }
        }
    }

    /// <summary>
    /// Event handler of ExecutingCommand/ExecutedCommand events
    /// </summary>
    /// <param name="command">Either DiagramCommandBase or ModelCommandBase</param>
    /// <param name="isPartOfMacro">Tells whether this command is or is not a part of a <see cref="MacroCommand"/></param>
    /// <param name="macroCommand">References the <see cref="MacroCommand"/> of which this command is a part</param>
    public delegate void CommandEventHandler(CommandBase command, bool isPartOfMacro, CommandBase macroCommand);
}
