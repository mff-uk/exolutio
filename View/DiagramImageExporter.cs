using System.Linq;
using System.Windows;
using Exolutio.ViewToolkit;

namespace Exolutio.View
{
    public class DiagramImageExporter : FrameworkElementImageExporter
    {
        /// <summary>
        /// Exports the diagram to an image (with frame and caption), 
        /// uses interactive dialogs to select filename. 
        /// </summary>
        /// <param name="diagramView">exported canvas</param>
        /// <param name="method">image format</param>
        public void ExportToImage(DiagramView diagramView, EExportToImageMethod method)
        {
            diagramView.ExolutioCanvas.EnterScreenshotView();
            base.ExportToImage(diagramView.ExolutioCanvas, method, true, diagramView.Diagram.Caption, GetCanvasBoundingRectangle(diagramView));
            diagramView.ExolutioCanvas.ExitScreenshotView();
        }

        /// <summary>
        /// Exports the diagram to an image, uses interactive dialogs to select filename.
        /// </summary>
        /// <param name="diagramView">exported diagram</param>
        /// <param name="method">image format</param>
        /// <param name="useFrameAndCaption">if set to <c>true</c> frame and caption is added to the image</param>
        public void ExportToImage(DiagramView diagramView, EExportToImageMethod method, bool useFrameAndCaption)
        {
            diagramView.ExolutioCanvas.EnterScreenshotView();
            base.ExportToImage(diagramView.ExolutioCanvas, method, useFrameAndCaption, diagramView.Diagram.Caption, GetCanvasBoundingRectangle(diagramView));
            diagramView.ExolutioCanvas.ExitScreenshotView();
        }

        public Rect GetCanvasBoundingRectangle(DiagramView diagramView)
        {
            double canvasWidth;
            double canvasHeight;
            if (diagramView.ExolutioCanvas.Children.OfType<Node>().Count() > 0)
            {
                canvasWidth = diagramView.ExolutioCanvas.Children.OfType<Node>().Max(thumb => thumb.Right) + 20;
                canvasHeight = diagramView.ExolutioCanvas.Children.OfType<Node>().Max(thumb => thumb.Bottom) + 20;
            }
            else
            {
                canvasWidth = 40;
                canvasHeight = 40;
            }
            Rect r = new Rect(0, 0, canvasWidth, canvasHeight);
            Rect transformed = diagramView.LayoutTransform.TransformBounds(r);
            return transformed; 
        }
    }
}