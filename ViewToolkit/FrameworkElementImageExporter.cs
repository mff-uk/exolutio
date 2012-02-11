using System;
using System.Globalization;
using System.IO;
using System.IO.Packaging;
using System.Printing;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Xps;
using System.Windows.Xps.Packaging;
using Microsoft.Win32;

namespace Exolutio.ViewToolkit
{
    public class FrameworkElementImageExporter
    {
        public enum EExportToImageMethod
        {
            PNG,
            PNGClipBoard,
            XPS
        }

        /// <summary>
        /// Exports the visual to an image, uses interactive dialogs to select filename.
        /// </summary>
        /// <param name="frameworkElement">exported frameworkElement</param>
        /// <param name="method">image format</param>
        /// <param name="useFrameAndCaption">if set to <c>true</c> frame and caption is added to the image</param>
        /// <param name="caption">caption</param>
        /// <param name="boundingRectangle">bounding rectangle</param>
        public void ExportToImage(FrameworkElement frameworkElement, EExportToImageMethod method, bool useFrameAndCaption, string caption = null, Rect? boundingRectangle = null)
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
                    ExportToImage(frameworkElement, method, dialog.FileName, caption, useFrameAndCaption, boundingRectangle);
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
                    ExportToImage(frameworkElement, method, dialog.FileName, caption, useFrameAndCaption, boundingRectangle);
                }
            }
            if (method == EExportToImageMethod.PNGClipBoard)
            {
                ExportToImage(frameworkElement, EExportToImageMethod.PNGClipBoard, null, caption, useFrameAndCaption, boundingRectangle);
            }
        }

        /// <summary>
        /// Exports the framework element to an image.
        /// </summary>
        /// <param name="frameworkElement">exported framework element</param>
        /// <param name="method">image format</param>
        /// <param name="filename">file name</param>
        /// <param name="title">image title</param>
        /// <param name="useFrameAndCaption"></param>
        /// <param name="boundingRectangle">bounding rectangle</param>
        public void ExportToImage(FrameworkElement frameworkElement, EExportToImageMethod method, string filename, string title, bool useFrameAndCaption,
            Rect? boundingRectangle = null)
        {
            const int bounds = 10;
            const int textoffset = 20;

            if (method == EExportToImageMethod.PNG || method == EExportToImageMethod.PNGClipBoard)
            {
                FormattedText titleText = 
                    useFrameAndCaption ? 
                    new FormattedText(title, new CultureInfo("en-us"), FlowDirection.LeftToRight, new Typeface("Verdana"), 20, Brushes.Gray) : null;

                RenderTargetBitmap rtb;

                if (!boundingRectangle.HasValue)
                {
                    boundingRectangle = new Rect(0, 0, frameworkElement.ActualWidth, frameworkElement.ActualHeight);
                }

                if (useFrameAndCaption)
                    rtb = new RenderTargetBitmap((int)(Math.Max(bounds + boundingRectangle.Value.Width + bounds, textoffset + titleText.Width + textoffset)), (int)(textoffset + titleText.Height + textoffset + boundingRectangle.Value.Height + bounds), 96, 96, PixelFormats.Pbgra32);
                else
                    rtb = new RenderTargetBitmap((int)(boundingRectangle.Value.Width), (int)(boundingRectangle.Value.Height), 96, 96, PixelFormats.Pbgra32);

                frameworkElement.InvalidateVisual();
                DrawingVisual drawingVisual = new DrawingVisual();
                DrawingContext drawingContext = drawingVisual.RenderOpen();
                drawingContext.DrawRectangle(ViewToolkitResources.WhiteBrush, null, new Rect(0, 0, rtb.Width, rtb.Height));
                VisualBrush canvasBrush = new VisualBrush(frameworkElement) {Stretch = Stretch.None, AlignmentX = 0, AlignmentY = 0};
                if (useFrameAndCaption)
                {
                    Rect rect = new Rect(bounds, textoffset + titleText.Height + textoffset, rtb.Width - 2 * bounds, rtb.Height - bounds - textoffset - titleText.Height - textoffset);
                    drawingContext.DrawRectangle(canvasBrush, new Pen(Brushes.LightGray, 1), rect);
                    drawingContext.DrawText(titleText, new Point(rtb.Width / 2 - titleText.Width / 2, textoffset));
                }
                else
                {
                    drawingContext.DrawRectangle(canvasBrush, null, new Rect(-boundingRectangle.Value.Left, -boundingRectangle.Value.Top, boundingRectangle.Value.Width + boundingRectangle.Value.Left, boundingRectangle.Value.Height + boundingRectangle.Value.Top));
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
                    if (!boundingRectangle.HasValue)
                    {
                        boundingRectangle = new Rect(0, 0, frameworkElement.ActualWidth, frameworkElement.ActualHeight);
                    }

                    // Save current canvas transorm
                    Transform transform = frameworkElement.LayoutTransform;
                    // Temporarily reset the layout transform before saving
                    frameworkElement.LayoutTransform = null;


                    // Get the size of the canvas
                    Size size = new Size(boundingRectangle.Value.Width, boundingRectangle.Value.Height);
                    // Measure and arrange elements
                    frameworkElement.Measure(size);
                    frameworkElement.Arrange(new Rect(size));

                    // Open new package
                    System.IO.Packaging.Package package = System.IO.Packaging.Package.Open(filename, FileMode.Create);
                    // Create new xps document based on the package opened
                    XpsDocument doc = new XpsDocument(package);
                    // Create an instance of XpsDocumentWriter for the document
                    XpsDocumentWriter writer = XpsDocument.CreateXpsDocumentWriter(doc);
                    // Write the canvas (as Visual) to the document
                    writer.Write(frameworkElement);
                    // Close document
                    doc.Close();
                    // Close package
                    package.Close();

                    // Restore previously saved layout
                    frameworkElement.LayoutTransform = transform;
                }
            }
        }

        /// <summary>
        ///   Returns a PrintTicket based on the current default printer.</summary>
        /// <returns>
        ///   A PrintTicket for the current local default printer.</returns>
        private static PrintTicket GetPrintTicketFromPrinter()
        {
            PrintQueue printQueue = null;

            LocalPrintServer localPrintServer = new LocalPrintServer();

            // Retrieving collection of local printer on user machine
            PrintQueueCollection localPrinterCollection =
                localPrintServer.GetPrintQueues();

            System.Collections.IEnumerator localPrinterEnumerator =
                localPrinterCollection.GetEnumerator();

            if (localPrinterEnumerator.MoveNext())
            {
                // Get PrintQueue from first available printer
                printQueue = (PrintQueue)localPrinterEnumerator.Current;
            }
            else
            {
                // No printer exist, return null PrintTicket
                return null;
            }

            // Get default PrintTicket from printer
            PrintTicket printTicket = printQueue.DefaultPrintTicket;

            PrintCapabilities printCapabilites = printQueue.GetPrintCapabilities();

            // Modify PrintTicket
            if (printCapabilites.CollationCapability.Contains(Collation.Collated))
            {
                printTicket.Collation = Collation.Collated;
            }

            if (printCapabilites.DuplexingCapability.Contains(
                    Duplexing.TwoSidedLongEdge))
            {
                printTicket.Duplexing = Duplexing.TwoSidedLongEdge;
            }

            if (printCapabilites.StaplingCapability.Contains(Stapling.StapleDualLeft))
            {
                printTicket.Stapling = Stapling.StapleDualLeft;
            }

            return printTicket;
        }// end:GetPrintTicketFromPrinter()
    }
}