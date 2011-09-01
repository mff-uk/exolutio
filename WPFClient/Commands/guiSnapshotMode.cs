using System;
using System.IO;
using System.Windows.Media;
using Exolutio.ResourceLibrary;
using Exolutio.View;
using Exolutio.View.Commands;
using Exolutio.ViewToolkit;
using Fluent;

namespace Exolutio.WPFClient.Commands
{
    public class guiSnapshotMode : guiCommandBase
    {
        public override void Execute(object parameter = null)
        {
            if (parameter is ToggleButton)
            {
                ToggleButton = (ToggleButton)parameter;
            }

            if (Toggled)
            {
                foreach (DiagramView diagramView in Current.MainWindow.DiagramTabManager.GetOpenedDiagramViews())
                {
                    diagramView.ExolutioCanvas.State = ECanvasState.TakingSnapshot;    
                }
            }
            else
            {
                foreach (DiagramView diagramView in Current.MainWindow.DiagramTabManager.GetOpenedDiagramViews())
                {
                    diagramView.ExolutioCanvas.State = ECanvasState.Normal;
                }
            }
        }

        protected ToggleButton ToggleButton { get; set; }

        protected bool Toggled
        {
            get { return ToggleButton != null && ToggleButton.IsChecked == true; }
        }

        #region Overrides of guiCommandBase

        public override string Text
        {
            get { return "Snapshot tool"; }
        }
        
        #endregion

        public override string ScreenTipText
        {
            get { return "Snapshot tool - drag a rectangle in a diagram, the captured area is copied to the clipboard"; }
        }

        public override ImageSource Icon
        {
            get { return ExolutioResourceNames.GetResourceImageSource(ExolutioResourceNames.Camera); }
        }

        public override bool CanExecute(object parameter = null)
        {
            return true; 
        }
    }
}