using System;
using EvoX.Controller.Commands;
using EvoX.ResourceLibrary;

namespace EvoX.View.Commands.Edit
{
    public class guiRedoCommand : guiCommandBase
    {
        public guiRedoCommand()
        {
            Current.ProjectChanged += Current_ProjectChanged;
            OnCanExecuteChanged(null);
        }

        void Current_ProjectChanged(object sender, CurrentProjectChangedEventArgs e)
        {
            if (e.OldController != null)
            {
                e.OldController.ExecutedCommand -= Controller_ExecutedCommand;
                e.OldController.UndoExecuted += Controller_UndoRedoExecuted;
            }
            if (e.NewController != null)
            {
                e.NewController.ExecutedCommand += Controller_ExecutedCommand;
                e.NewController.UndoExecuted += Controller_UndoRedoExecuted;
            }
        }

        void Controller_UndoRedoExecuted()
        {
            OnCanExecuteChanged(null);
        }

        private void Controller_ExecutedCommand(CommandBase command, bool ispartofmacro, CommandBase macrocommand)
        {
            OnCanExecuteChanged(null);
        }

        public override bool CanExecute(object parameter)
        {
            return Current.Controller != null && Current.Controller.RedoStack.Count > 0;
        }

        public override void Execute(object parameter)
        {
            RedoCommand RedoCommand = new RedoCommand(Current.Controller);
            RedoCommand.Execute();
            OnCanExecuteChanged(null);
        }

        public override System.Windows.Media.ImageSource Icon
        {
            get { return EvoXResourceNames.GetResourceImageSource(EvoXResourceNames.redo); }
        }

        public override string Text
        {
            get { return "Redo"; }
        }

        public override string ScreenTipText
        {
            get { return "Redo last undone operation"; }
        }
    }
}