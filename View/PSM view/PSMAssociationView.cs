using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Exolutio.Controller.Commands;
using Exolutio.Model;
using Exolutio.Model.PSM;
using Exolutio.Model.ViewHelper;
using Exolutio.ViewToolkit;
using Component = Exolutio.Model.Component;
using Label = Exolutio.ViewToolkit.Label;

namespace Exolutio.View
{
    public class PSMAssociationView : ComponentViewBaseVH<PSMAssociationViewHelper>, IConnectorViewBase
    {
        public PSMAssociation PSMAssociation { get; private set; }

        public override Component ModelComponent
        {
            get { return PSMAssociation; }
            protected set { PSMAssociation = (PSMAssociation)value; }
        }

        public override PSMAssociationViewHelper ViewHelper { get; protected set; }

        protected override void BindModelView()
        {
            base.BindModelView();
            ExolutioContextMenu psmAssociationMenu = MenuHelper.GetContextMenu(ScopeAttribute.EScope.PSMAssociation, this.DiagramView.Diagram);
            this.ContextMenu = psmAssociationMenu;
            NameLabel.ContextMenu = psmAssociationMenu;
            #if SILVERLIGHT
            ContextMenuService.SetContextMenu(NameLabel, ContextMenu);
            ContextMenuService.SetContextMenu(Connector, ContextMenu);
            #endif
            ((ExolutioContextMenu)ContextMenu).ScopeObject = PSMAssociation;
            ((ExolutioContextMenu)ContextMenu).Diagram = DiagramView.Diagram;
        }

        public override ContextMenu ContextMenu
        {
            get { return Connector.ContextMenu; }
            set 
            { 
                Connector.ContextMenu = value;
                #if SILVERLIGHT
                #else
                NameLabel.ContextMenu = value;
                #endif
            }
        }

        public override void UpdateView()
        {
            base.UpdateView();
            if (DiagramView != null)
            {
                Node parentNode = ((INodeComponentViewBase) DiagramView.RepresentantsCollection[Parent]).MainNode;
                Node childNode = ((INodeComponentViewBase) DiagramView.RepresentantsCollection[Child]).MainNode;
                if (parentNode != Connector.StartNode || childNode != Connector.EndNode)
                {
                    Connector.Connect(parentNode, childNode);
                }
                // labels, multiplicities
                NameLabel.Text = PSMAssociation.Name;
                Cardinality = PSMAssociation.GetCardinalityString();

                NameLabel.X = ViewHelper.MainLabelViewHelper.X;
                NameLabel.Y = ViewHelper.MainLabelViewHelper.Y;
                NameLabel.UpdateCanvasPosition(true);

                CardinalityLabel.X = ViewHelper.CardinalityLabelViewHelper.X;
                CardinalityLabel.Y = ViewHelper.CardinalityLabelViewHelper.Y;
                CardinalityLabel.UpdateCanvasPosition(true);

#if SILVERLIGHT
#else
                if (PSMAssociation.Interpretation == null)
                {
                    Connector.Pen = ViewToolkitResources.InterpretedAssociationPen;
                }
                else
                {
                    Connector.Pen = ViewToolkitResources.SolidBlackPen;
                }
#endif

                DiagramView.ExolutioCanvas.InvokeContentChanged();
            }
        }

        public Connector Connector { get; private set; }

        
        public PSMAssociationMember Parent
        {
            get { return PSMAssociation.Parent; }
        }

        public INodeComponentViewBase SourceClassView
        {
            get { return (INodeComponentViewBase)DiagramView.RepresentantsCollection[Parent]; }
        }

        public INodeComponentViewBase TargetClassView
        {
            get { return (INodeComponentViewBase)DiagramView.RepresentantsCollection[Child]; }
        }

        public PSMAssociationMember Child
        {
            get { return PSMAssociation.Child; }
        }

        public Label NameLabel { get; private set; }
        public Label CardinalityLabel { get; private set; }
        
        private string associationName;
        public string AssociationName
        {
            get { return associationName; }
            set
            {
                associationName = value;
                if (NameLabel != null)
                {
                    NameLabel.Text = value;
                    NameLabel.Visibility = !string.IsNullOrEmpty(value) ? Visibility.Visible : Visibility.Collapsed;
                }
            }
        }

        private string cardinality;
        public string Cardinality
        {
            get { return cardinality; }
            set
            {
                cardinality = value;
                if (CardinalityLabel != null)
                {
                    CardinalityLabel.Text = value;
                    CardinalityLabel.Visibility = !string.IsNullOrEmpty(value) && value != "1" ? Visibility.Visible : Visibility.Collapsed;
                }
            }
        }

        public override bool CanPutInDiagram(DiagramView diagramView)
        {
            if (!diagramView.RepresentantsCollection.ContainsKey(Parent) ||
                !diagramView.RepresentantsCollection.ContainsKey(Child))
            {
                /* 
                 * since parent and child may change, it is necessary
                 * to hook to this possible change
                 */
                if (!parentChildUpdateBound)
                {
                    PSMAssociation.PropertyChanged += PSMAssociation_PropertyChanged_ForParentChildUpdate;
                    parentChildUpdateBound = true;
                }
            }

            return base.CanPutInDiagram(diagramView)
                && diagramView.RepresentantsCollection.ContainsKey(Parent)
                && diagramView.RepresentantsCollection.ContainsKey(Child);
        }

        bool parentChildUpdateBound = false; 

        private void PSMAssociation_PropertyChanged_ForParentChildUpdate(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName.IsAmong("Child","Parent") 
                && DiagramView == null && pendingDiagramView != null)
            {
                pendingDiagramView.DefferedAddCheck();
            }
        }

        public override bool CanRemoveFromDiagram()
        {
            bool canRemoveFromDiagram = base.CanRemoveFromDiagram();
            return canRemoveFromDiagram;
            // && labels already removed
        }

        public override void PutInDiagram(DiagramView diagramView, ViewHelper viewHelper)
        {
            base.PutInDiagram(diagramView, viewHelper);
            if (parentChildUpdateBound)
            {
                PSMAssociation.PropertyChanged -= PSMAssociation_PropertyChanged_ForParentChildUpdate;
                parentChildUpdateBound = false;
            }
#if SILVERLIGHT
            Connector = new Connector();
#else
            Connector = new Connector() { EndCapStyle = EConnectorCapStyle.Arrow };
#endif
            NameLabel = new Label();
            CardinalityLabel = new Label();
            CreatedControls.Add(Connector);
            CreatedControls.Add(NameLabel);
            CreatedControls.Add(CardinalityLabel);
            DiagramView.ExolutioCanvas.AddConnector(Connector);
            Connector.Connect(SourceClassView.MainNode, TargetClassView.MainNode);
            DiagramView.ExolutioCanvas.AddNode(NameLabel);
            DiagramView.ExolutioCanvas.AddNode(CardinalityLabel);
            NameLabel.PlacementCenter = EPlacementCenter.Center;
            Connector.SnapNodeToConnector(NameLabel);
            CardinalityLabel.SnapTo(Connector.EndPoint, true);
            BindModelView();

            NameLabel.SelectedChanged += NameLabel_SelectedChanged;
            NameLabel.PositionChanged += NameLabel_PositionChanged;
            CardinalityLabel.PositionChanged += CardinalityLabel_PositionChanged;
            Connector.SelectedChanged += Connector_SelectedChanged;
            Connector.MouseEnter += Connector_MouseEnter;
            Connector.MouseLeave += Connector_MouseLeave;
        }

        private void Connector_MouseEnter(object sender, MouseEventArgs e)
        {
            DiagramView.InvokeVersionedElementMouseEnter(this, PSMAssociation);
        }

        void Connector_MouseLeave(object sender, MouseEventArgs e)
        {
            DiagramView.InvokeVersionedElementMouseLeave(this, PSMAssociation);
        }

        void Connector_SelectedChanged()
        {
            this.Selected = Connector.Selected;
        }

        private void CardinalityLabel_PositionChanged()
        {
            ViewHelper.CardinalityLabelViewHelper.SetPositionSilent(CardinalityLabel.Position);
        }
        
        void NameLabel_PositionChanged()
        {
            ViewHelper.MainLabelViewHelper.SetPositionSilent(NameLabel.Position);
        }

        void NameLabel_SelectedChanged()
        {
            if (NameLabel.Selected && !this.Selected)
            {
                Connector.Selected = true;
            }
        }

        //void Connector_ConnectorPointMoved(ConnectorPoint point)
        //{
        //    if (ViewHelper.Points.Count > point.OrderInConnector)
        //    {
        //        ViewHelper.Points[point.OrderInConnector].SetPosition(point.Position);
        //    }
        //}

        public override void RemoveFromDiagram()
        {
            if (Selected)
            {
                Selected = false;
            }
            UnBindModelView();
            DiagramView.ExolutioCanvas.RemoveConnector(Connector);
            DiagramView.ExolutioCanvas.RemoveNode(NameLabel);
            DiagramView.ExolutioCanvas.RemoveNode(CardinalityLabel);
            base.RemoveFromDiagram();
        }

        public override bool Selected
        {
            get
            {
                return base.Selected;
            }
            set
            {
                base.Selected = value;
            }
        }
    }
}