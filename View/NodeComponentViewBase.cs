using System;
using System.Windows.Controls.Primitives;
using Exolutio.Model.ViewHelper;
using Exolutio.SupportingClasses;
using Exolutio.ViewToolkit;
using System.Linq;

namespace Exolutio.View
{
    public abstract class NodeComponentViewBase<TViewHelper> : ComponentViewBaseVH<TViewHelper>, INodeComponentViewBase where TViewHelper : ViewHelper
    {
        private double x;
        private double y;

        public double X
        {
            get { return x; }
            set
            {
                x = value;
                if (MainNode != null)
                {
                    MainNode.X = value; 
                }
            }
        }

        public double Y
        {
            get { return y; }
            set
            {
                y = value;
                if (MainNode != null)
                {
                    MainNode.Y = value;
                }
            }
        }

        public double ActualWidth { get { return MainNode.ActualWidth; } }

        public double ActualHeight { get { return MainNode.ActualHeight; } }

        public Node MainNode { get; private set; }

        ViewHelper INodeComponentViewBase.ViewHelper
        {
            get { return ViewHelper; }
        }

        public override bool CanPutInDiagram(DiagramView diagramView)
        {
            return true;
        }

        public override void PutInDiagram(DiagramView diagramView, ViewHelper viewHelper)
        {
            base.PutInDiagram(diagramView, viewHelper);

            MainNode = new Node();
            MainNode.PositionChanged += MainNode_PositionChanged;
            MainNode.SelectedChanged += MainNode_SelectedChanged;
            CreatedControls.Add(MainNode);
            CreateInnerControls(diagramView.ExolutioCanvas);
            diagramView.ExolutioCanvas.AddNode(MainNode);

            BindModelView();
        }

        private void MainNode_SelectedChanged()
        {
            this.Selected = MainNode.Selected;
        }

        void MainNode_PositionChanged(DragDeltaEventArgs dragDeltaEventArgs)
        {
            if (ViewHelper is PositionableElementViewHelper)
            {
                (ViewHelper as PositionableElementViewHelper).SetPositionSilent(MainNode.CanvasPosition);
            }
        }

        protected virtual void CreateInnerControls(ExolutioCanvas canvas) { }

        public override bool CanRemoveFromDiagram()
        {
            return MainNode.Connectors.IsEmpty();
        }

        public override void RemoveFromDiagram()
        {
            if (Selected)
            {
                Selected = false;
            }
            if (MainNode == null || MainNode.ExolutioCanvas == null)
            {
                throw new ExolutioViewException("Component view is not visualized on a canvas");
            }
            MainNode.PositionChanged -= MainNode_PositionChanged;
            UnBindModelView();
            MainNode.ExolutioCanvas.RemoveNode(MainNode);
            base.RemoveFromDiagram();
        }
    }
}