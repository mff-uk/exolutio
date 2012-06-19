using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using Exolutio.Dialogs;
using Exolutio.Model;
using Exolutio.Model.PIM;
using Exolutio.Model.PSM;
using Exolutio.Model.Versioning;
using Exolutio.ResourceLibrary;
using Exolutio.Revalidation;
using Exolutio.ViewToolkit;

namespace Exolutio.View.Commands.Versioning
{
    public class guiVisualizeLinks : guiActiveDiagramCommand
    {
        public guiVisualizeLinks()
        {
            GlobalViewEvents.CanvasContentChanged += GlobalViewEvents_CanvasContentChanged;
            GlobalViewEvents.DiagramDisplayChanged += GlobalViewEvents_DiagramDisplayChanged;
            Current.ExecutedCommand += Current_ExecutedCommand;
        }

        private void GlobalViewEvents_DiagramDisplayChanged()
        {
            //ClearAdorners();
        }

        private void GlobalViewEvents_CanvasContentChanged()
        {
            ClearAdorners();
        }

        void Current_ExecutedCommand(Controller.Commands.CommandBase command, bool isPartOfMacro, Controller.Commands.CommandBase macroCommand, bool isUndo, bool isRedo)
        {
            ClearAdorners();
        }

        internal class LinkedPairInfo
        {
            public Component Element1 { get; set; }
            public Component Element2 { get; set; }
            public ComponentViewBase View1 { get; set; }
            public ComponentViewBase View2 { get; set; }
        }

        public override void OnCanExecuteChanged(EventArgs e)
        {
            base.OnCanExecuteChanged(e);
            //ClearAdorners();
        }


        public void ClearAdorners()
        {
            UIElement topElement = Current.MainWindow.DiagramTabManager.TopElement;
            AdornerLayer topAdornerLayer = AdornerLayer.GetAdornerLayer(topElement);
            IEnumerable<Adorner> existing = topAdornerLayer.GetAdorners(topElement);
            if (existing != null)
            {
                existing = existing.OfType<LinkedComponentsAdorner>();
                foreach (Adorner adorner in existing)
                {
                    topAdornerLayer.Remove(adorner);
                }
            }
        }

        public override void Execute(object parameter = null)
        {
            IEnumerable<ExolutioVersionedObject> openedVersions =
                Current.MainWindow.DiagramTabManager.AnotherOpenedVersions(Current.ActiveDiagram);

            DiagramView dv1 = Current.MainWindow.DiagramTabManager.GetOpenedDiagramView(Current.ActiveDiagram);
            DiagramView dv2 = Current.MainWindow.DiagramTabManager.GetOpenedDiagramView((Diagram)openedVersions.First());

            List<LinkedPairInfo> linkedComponents = FindLinkedComponents(dv2, dv1);

            ClearAdorners();
            UIElement topElement = Current.MainWindow.DiagramTabManager.TopElement;
            AdornerLayer topAdornerLayer = AdornerLayer.GetAdornerLayer(topElement);
            topAdornerLayer.Add(new LinkedComponentsAdorner(topElement, linkedComponents));
        }

        private static List<LinkedPairInfo> FindLinkedComponents(DiagramView dv2, DiagramView dv1)
        {
            List<LinkedPairInfo> linkedComponents = new List<LinkedPairInfo>();
            foreach (Component component in dv1.Diagram.Schema.SchemaComponents)
            {
                IVersionedItem itemV2 = Current.Project.VersionManager.GetItemInVersion(component, dv2.Diagram.Version);
                if (itemV2 != null)
                {
                    var lpi = new LinkedPairInfo
                                  {
                                      Element1 = component,
                                      Element2 = (Component)itemV2
                                  };
                    if (dv1.RepresentantsCollection.ContainsKey(component))
                    {
                        lpi.View1 = dv1.RepresentantsCollection[component];
                        lpi.View2 = dv2.RepresentantsCollection[(Component)itemV2];
                    }
                    linkedComponents.Add(lpi);
                }
            }
            return linkedComponents;
        }

        public override string Text
        {
            get { return "Visualize links"; }
        }

        public override string ScreenTipText
        {
            get { return "Visualize links"; }
        }

        public override bool CanExecute(object parameter = null)
        {
            return Current.ActiveDiagram != null && Current.Project.UsesVersioning &&
                   Current.MainWindow.DiagramTabManager.AnotherOpenedVersions(Current.ActiveDiagram).Count() == 1;
        }

        public override ImageSource Icon
        {
            get { return null; }
        }
    }

    internal class LinkedComponentsAdorner : Adorner
    {
        private List<guiVisualizeLinks.LinkedPairInfo> linkedComponents = new List<guiVisualizeLinks.LinkedPairInfo>();

        public LinkedComponentsAdorner(UIElement topElement,
                                       IEnumerable<guiVisualizeLinks.LinkedPairInfo> linkedComponents)
            : base(topElement)
        {
            this.linkedComponents.AddRange(linkedComponents);
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            if (linkedComponents.Count == 0)
                return;

            try
            {
                DiagramView dv1 = linkedComponents[0].View1.DiagramView;
                DiagramView dv2 = linkedComponents[0].View2.DiagramView;
                GeneralTransform snTransform = dv1.ExolutioCanvas.TransformToAncestor(this.AdornedElement);
                GeneralTransform tnTransform = dv2.ExolutioCanvas.TransformToAncestor(this.AdornedElement);

                foreach (guiVisualizeLinks.LinkedPairInfo linkedComponent in linkedComponents)
                {
                    Point p1;
                    Point p2;
                    Pen pen = ViewToolkitResources.StrongVersionLinkPen;

                    if (linkedComponent.View1 is INodeComponentViewBase)
                    {
                        Node sourceNode = ((INodeComponentViewBase)linkedComponent.View1).MainNode;
                        Node targetNode = ((INodeComponentViewBase)linkedComponent.View2).MainNode;

                        Rect snBoundsTransformed = snTransform.TransformBounds(sourceNode.GetBounds());
                        Rect tnBoundsTransformed = tnTransform.TransformBounds(targetNode.GetBounds());
                        Point[] points = GeometryHelper.ComputeOptimalConnection(snBoundsTransformed,
                                                                                 tnBoundsTransformed);
                        p1 = points[0];
                        p2 = points[1];
                    }
                    else if (linkedComponent.View1 is IConnectorViewBase)
                    {
                        Connector sourceConnector = ((IConnectorViewBase)linkedComponent.View1).Connector;
                        Connector targetConnector = ((IConnectorViewBase)linkedComponent.View2).Connector;
                        p1 = snTransform.Transform(sourceConnector.GetVirtualCenterPosition());
                        p2 = tnTransform.Transform(targetConnector.GetVirtualCenterPosition());
                    }
                    else if (linkedComponent.Element1 is PIMAttribute || linkedComponent.Element1 is PSMAttribute)
                    {
                        EditableTextBox tb1, tb2;
                        if (linkedComponent.Element1 is PIMAttribute)
                        {
                            tb1 = ((PIMDiagramView)dv1).GetTextBoxOfAttribute((PIMAttribute)linkedComponent.Element1);
                            tb2 = ((PIMDiagramView)dv2).GetTextBoxOfAttribute((PIMAttribute)linkedComponent.Element2);
                        }
                        else
                        {
                            tb1 = ((PSMDiagramView)dv1).GetTextBoxOfAttribute((PSMAttribute)linkedComponent.Element1);
                            tb2 = ((PSMDiagramView)dv2).GetTextBoxOfAttribute((PSMAttribute)linkedComponent.Element2);
                        }

                        Rect b1 = tb1.TransformToAncestor(this.AdornedElement).TransformBounds(tb1.GetBounds());
                        Rect b2 = tb2.TransformToAncestor(this.AdornedElement).TransformBounds(tb2.GetBounds());
                        Point[] points = GeometryHelper.ComputeOptimalConnection(b1, b2);
                        p1 = points[0];
                        p2 = points[1];
                        pen = ViewToolkitResources.LightVersionLinkPen;
                    }
                    else
                    {
                        continue;
                    }
                    drawingContext.DrawLine(pen, p1, p2);
                }
            }
            catch
            {
                
            }
        }
    }
}