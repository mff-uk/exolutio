using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Xps;
using Exolutio.Model;
using Exolutio.ViewToolkit;
using System.Windows.Xps.Packaging;
using Microsoft.Win32;

namespace Exolutio.View
{
    public class ImageExporter
    {
        public enum EExportToImageMethod
        {
            PNG,
            PNGClipBoard,
            XPS
        }

        /// <summary>
        /// Exports the diagram to an image (with frame and caption), 
        /// uses interactive dialogs to select filename. 
        /// </summary>
        /// <param name="diagramView">exported canvas</param>
        /// <param name="method">image format</param>
        public void ExportToImage(DiagramView diagramView, EExportToImageMethod method)
        {
            ExportToImage(diagramView, method, true);
        }

        /// <summary>
        /// Exports the diagram to an image, uses interactive dialogs to select filename.
        /// </summary>
        /// <param name="diagramView">exported diagram</param>
        /// <param name="method">image format</param>
        /// <param name="useFrameAndCaption">if set to <c>true</c> frame and caption is added to the image</param>
        public void ExportToImage(DiagramView diagramView, EExportToImageMethod method, bool useFrameAndCaption)
        {
            if (method == EExportToImageMethod.PNG)
            {
                SaveFileDialog dialog = new SaveFileDialog
                {
                    Title = "Export to file...",
                    Filter = "PNG images (*.png)|*.png|All files|*.*"
                };
                if (dialog.ShowDialog() == true)
                {
                    ExportToImage(diagramView, method, dialog.FileName, diagramView.Diagram.Caption, true);
                }
            }
            if (method == EExportToImageMethod.XPS)
            {
                SaveFileDialog dialog = new SaveFileDialog
                {
                    Title = "Export to file...",
                    Filter = "XPS images (*.xps)|*.xps|All files|*.*"
                };
                if (dialog.ShowDialog() == true)
                {
                    ExportToImage(diagramView, method, dialog.FileName, diagramView.Diagram.Caption, true);
                }
            }
            if (method == EExportToImageMethod.PNGClipBoard)
            {
                ExportToImage(diagramView, EExportToImageMethod.PNGClipBoard, null, diagramView.Diagram.Caption, useFrameAndCaption);
            }
        }

        /// <summary>
        /// Exports the diagram to an image.
        /// </summary>
        /// <param name="diagramView">exported diagram</param>
        /// <param name="method">image format</param>
        /// <param name="filename">file name</param>
        /// <param name="title">diagram title</param>
        /// <param name="useFrameAndCaption"></param>
        public void ExportToImage(DiagramView diagramView, EExportToImageMethod method, string filename, string title, bool useFrameAndCaption)
        {
            const int bounds = 10;
            const int textoffset = 20;

            if (method == EExportToImageMethod.PNG || method == EExportToImageMethod.PNGClipBoard)
            {
                FormattedText titleText = new FormattedText(title, new CultureInfo("en-us"), FlowDirection.LeftToRight, new Typeface("Verdana"), 20, Brushes.Gray);

                double canvasWidth;
                double canvasHeight;
                GetCanvasWidthAndHeight(diagramView, out canvasWidth, out canvasHeight);

                RenderTargetBitmap rtb;

                if (useFrameAndCaption)
                    rtb = new RenderTargetBitmap((int)(Math.Max(bounds + canvasWidth + bounds, textoffset + titleText.Width + textoffset)), (int)(textoffset + titleText.Height + textoffset + canvasHeight + bounds), 96, 96, PixelFormats.Pbgra32);
                else
                    rtb = new RenderTargetBitmap((int)(canvasWidth), (int)(canvasHeight), 96, 96, PixelFormats.Pbgra32);

                diagramView.InvalidateVisual();
                DrawingVisual drawingVisual = new DrawingVisual();
                DrawingContext drawingContext = drawingVisual.RenderOpen();
                drawingContext.DrawRectangle(ViewToolkitResources.WhiteBrush, null, new Rect(0, 0, rtb.Width, rtb.Height));
                VisualBrush canvasBrush = new VisualBrush(diagramView.ExolutioCanvas);
                canvasBrush.Stretch = Stretch.None;
                canvasBrush.AlignmentX = 0;
                canvasBrush.AlignmentY = 0;
                if (useFrameAndCaption)
                {
                    Rect rect = new Rect(bounds, textoffset + titleText.Height + textoffset, rtb.Width - 2 * bounds, rtb.Height - bounds - textoffset - titleText.Height - textoffset);
                    drawingContext.DrawRectangle(canvasBrush, new Pen(Brushes.LightGray, 1), rect);
                    drawingContext.DrawText(titleText, new Point(rtb.Width / 2 - titleText.Width / 2, textoffset));
                }
                else
                {
                    drawingContext.DrawRectangle(canvasBrush, null, new Rect(0, 0, (canvasWidth), (canvasHeight)));
                }
                drawingContext.Close();

                rtb.Render(drawingVisual);
                PngBitmapEncoder png = new PngBitmapEncoder();
                png.Frames.Add(BitmapFrame.Create(rtb));
                if (method == EExportToImageMethod.PNG)
                {
                    using (Stream stm = File.Create(filename))
                    {
                        png.Save(stm);
                    }
                }
                if (method == EExportToImageMethod.PNGClipBoard)
                {
                    Clipboard.SetImage(rtb);
                }
            }
            else if (method == EExportToImageMethod.XPS)
            {
                {
                    double canvasWidth;
                    double canvasHeight;
                    GetCanvasWidthAndHeight(diagramView, out canvasWidth, out canvasHeight);


                    // Save current canvas transorm
                    Transform transform = diagramView.LayoutTransform;
                    // Temporarily reset the layout transform before saving
                    diagramView.LayoutTransform = null;


                    // Get the size of the canvas
                    Size size = new Size(canvasWidth, canvasHeight);
                    // Measure and arrange elements
                    diagramView.Measure(size);
                    diagramView.Arrange(new Rect(size));

                    // Open new package
                    System.IO.Packaging.Package package = System.IO.Packaging.Package.Open(filename, FileMode.Create);
                    // Create new xps document based on the package opened
                    XpsDocument doc = new XpsDocument(package);
                    // Create an instance of XpsDocumentWriter for the document
                    XpsDocumentWriter writer = XpsDocument.CreateXpsDocumentWriter(doc);
                    // Write the canvas (as Visual) to the document
                    writer.Write(diagramView.ExolutioCanvas);
                    // Close document
                    doc.Close();
                    // Close package
                    package.Close();

                    // Restore previously saved layout
                    diagramView.LayoutTransform = transform;
                }
            }
        }


        public void GetCanvasWidthAndHeight(DiagramView diagramView, out double canvasWidth, out double canvasHeight)
        {
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
            canvasWidth = transformed.Width;
            canvasHeight = transformed.Height;
        }

    }
}