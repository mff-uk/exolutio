using Exolutio.Model.PSM.Normalization;
using System;

namespace Exolutio.View.Commands
{
    public abstract class guiActiveDiagramCommand: guiCommandBase
    {
        protected guiActiveDiagramCommand()
        {
            Current.ProjectChanged += Current_ProjectChanged;
            Current.ActiveDiagramChanged += Current_ActiveDiagramChanged;
        }

        void Current_ActiveDiagramChanged()
        {
            OnCanExecuteChanged(null);
        }

        void Current_ProjectChanged(object sender, CurrentProjectChangedEventArgs e)
        {
            OnCanExecuteChanged(e);
        }

        private System.Windows.Input.KeyGesture gesture;
        public override System.Windows.Input.KeyGesture Gesture
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

    }
}