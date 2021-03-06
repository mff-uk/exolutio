using System;
using Exolutio.Controller.Commands;
using Exolutio.ResourceLibrary;
using System.Windows.Input;

namespace Exolutio.View.Commands.Edit
{
    public class guiRedoCommand : guiCommandBase
    {
        public guiRedoCommand()
        {
            Current.ProjectChanged += Current_ProjectChanged;
            OnCanExecuteChanged(null);
            Gesture = KeyGestures.ControlY;
        }

        void Current_ProjectChanged(object sender, CurrentProjectChangedEventArgs e)
        {
            if (e.OldController != null)
            {
                e.OldController.ExecutedCommand -= Controller_ExecutedCommand;
            }
            if (e.NewController != null)
            {
                e.NewController.ExecutedCommand += Controller_ExecutedCommand;
            }
        }

        void Controller_UndoRedoExecuted()
        {
            OnCanExecuteChanged(null);
        }

        private void Controller_ExecutedCommand(CommandBase command, bool ispartofmacro, CommandBase macrocommand, bool isUndo, bool isRedo)
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
            get { return ExolutioResourceNames.GetResourceImageSource(ExolutioResourceNames.redo); }
        }

        public override string Text
        {
            get { return "Redo"; }
        }

        public override string ScreenTipText
        {
            get { return "Redo last undone operation"; }
        }

        #if SILVERLIGHT
        #else
        private KeyGesture gesture;
        public override KeyGesture Gesture
        {
            get
            {
                return gesture;
            }
            set
            {
                gesture = value;
                if (keyBinding != null)
                {
                    throw new InvalidOperationException("Key binding cannot be defined twice.");
                }
                if (gesture != null && Current.MainWindow != null)
                {
                    keyBinding = new System.Windows.Input.KeyBinding(this, Gesture);
                    Current.MainWindow.InputBindings.Add(keyBinding);
                    Current.MainWindow.CommandBindings.Add(new System.Windows.Input.CommandBinding(this));
                }
            }
        }
        #endif

    }
}