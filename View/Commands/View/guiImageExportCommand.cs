using System;
using System.Windows.Input;
using Exolutio.Controller.Commands;
using Microsoft.Win32;

namespace Exolutio.View.Commands.View
{
    [Scope(ScopeAttribute.EScope.PSMSchema | ScopeAttribute.EScope.PIMDiagram)]
    public class guiImageExportPNGCommand : guiScopeCommand
    {
        public override string ScreenTipText
        {
            get { return "Export this diagram to image (PNG format). Hold Shift to export without diagram label and frame. "; }
        }

        public override bool CanExecute(object parameter)
        {
            return Current.ActiveDiagramView != null;
        }

        public override string Text
        {
            get
            {
                return "Export to image (PNG format)";
            }
        }

        public override void Execute(object parameter)
        {
            bool shiftPressed = Keyboard.Modifiers.HasFlag(ModifierKeys.Shift); 

            SaveFileDialog dialog = new SaveFileDialog
                                            {
                                                Title = "Export to file...",
                                                Filter = "PNG images (*.png)|*.png|All files|*.*"
                                            };
            if (dialog.ShowDialog() == true)
            {
                DiagramImageExporter e = new DiagramImageExporter();
                e.ExportToImage(Current.ActiveDiagramView, DiagramImageExporter.EExportToImageMethod.PNG,
                    dialog.FileName, Current.ActiveDiagramView.Diagram.Caption, !shiftPressed);
            }
        }
    }

    [Scope(ScopeAttribute.EScope.PSMSchema | ScopeAttribute.EScope.PIMDiagram)]
    public class guiImageExportXPSCommand : guiScopeCommand
    {
        public override string ScreenTipText
        {
            get { return "Export this diagram to image (XPS format). Hold Shift to export without diagram label and frame. "; }
        }

        public override bool CanExecute(object parameter)
        {
            return Current.ActiveDiagramView != null;
        }

        public override string Text
        {
            get
            {
                return "Export to image (XPS)";
            }
        }

        public override void Execute(object parameter)
        {
            bool shiftPressed = Keyboard.Modifiers.HasFlag(ModifierKeys.Shift);

            SaveFileDialog dialog = new SaveFileDialog
            {
                Title = "Export to file...",
                Filter = "XPS images (*.xps)|*.xps|All files|*.*"
            };
            if (dialog.ShowDialog() == true)
            {
                DiagramImageExporter e = new DiagramImageExporter();
                e.ExportToImage(Current.ActiveDiagramView, DiagramImageExporter.EExportToImageMethod.XPS,
                    dialog.FileName, Current.ActiveDiagramView.Diagram.Caption, !shiftPressed);
            }
        }
    }
}