using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Exolutio.Controller.Commands;
using Exolutio.Model;
using Exolutio.Model.PIM;
using Exolutio.Model.ViewHelper;
using Exolutio.ViewToolkit;
using Component = Exolutio.Model.Component;
using Label = Exolutio.ViewToolkit.Label;

namespace Exolutio.View
{
    public class PIMAssociationView : ComponentViewBaseVH<PIMAssociationViewHelper>, IConnectorViewBase
    {
        public PIMAssociation PIMAssociation { get; private set; }

        public override Component ModelComponent
        {
            get { return PIMAssociation; }
            protected set { PIMAssociation = (PIMAssociation)value; }
        }

        public override PIMAssociationViewHelper ViewHelper { get; protected set; }

        protected override void BindModelView()
        {
            base.BindModelView();
            foreach (PIMAssociationEnd pimAssociationEnd in PIMAssociation.PIMAssociationEnds)
            {
                pimAssociationEnd.PropertyChanged += AssociationEnd_PropertyChanged;
            }
            ((ExolutioContextMenu)ContextMenu).ScopeObject = PIMAssociation;
            ((ExolutioContextMenu)ContextMenu).Diagram = DiagramView.Diagram;
            ((ExolutioContextMenu)Connector.StartPoint.ContextMenu).ScopeObject = PIMAssociation.PIMAssociationEnds[0];
            ((ExolutioContextMenu)Connector.StartPoint.ContextMenu).Diagram = DiagramView.Diagram;
            ((ExolutioContextMenu)Connector.EndPoint.ContextMenu).ScopeObject = PIMAssociation.PIMAssociationEnds[1];
            ((ExolutioContextMenu)Connector.EndPoint.ContextMenu).Diagram = DiagramView.Diagram;
        }

        protected override void UnBindModelView()
        {
            foreach (PIMAssociationEnd pimAssociationEnd in PIMAssociation.PIMAssociationEnds)
            {
                pimAssociationEnd.PropertyChanged -= AssociationEnd_PropertyChanged;
            }
            base.UnBindModelView();
        }

        private void AssociationEnd_PropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            UpdateView();
        }

        public override void UpdateView()
        {
            base.UpdateView();
            Node sourceNode = ((INodeComponentViewBase)DiagramView.RepresentantsCollection[SourceClass]).MainNode;
            Node targetNode = ((INodeComponentViewBase)DiagramView.RepresentantsCollection[TargetClass]).MainNode;
            if (sourceNode != Connector.StartNode || targetNode != Connector.EndNode)
            {
                Connector.Connect(sourceNode, targetNode);
            }
            
            // labels, multiplicities
            NameLabel.Text = PIMAssociation.Name;
            SourceCardinality = PIMAssociation.PIMAssociationEnds[0].GetCardinalityString();
            TargetCardinality = PIMAssociation.PIMAssociationEnds[1].GetCardinalityString();

            NameLabel.X = ViewHelper.MainLabelViewHelper.X;
            NameLabel.Y = ViewHelper.MainLabelViewHelper.Y;
            NameLabel.UpdateCanvasPosition(true);

            SourceCardinalityLabel.X = ViewHelper.AssociationEndsViewHelpers[0].CardinalityLabelViewHelper.X;
            SourceCardinalityLabel.Y = ViewHelper.AssociationEndsViewHelpers[0].CardinalityLabelViewHelper.Y;
            SourceCardinalityLabel.UpdateCanvasPosition(true);

            TargetCardinalityLabel.X = ViewHelper.AssociationEndsViewHelpers[1].CardinalityLabelViewHelper.X;
            TargetCardinalityLabel.Y = ViewHelper.AssociationEndsViewHelpers[1].CardinalityLabelViewHelper.Y;
            TargetCardinalityLabel.UpdateCanvasPosition(true);
        }

        public Connector Connector { get; private set; }

        public PIMAssociationEnd SourceEnd
        {
            get { return PIMAssociation.PIMAssociationEnds[0]; }
        }

        public PIMClass SourceClass
        {
            get { return PIMAssociation.PIMClasses[0]; }
        }

        public PIMClassView SourceClassView
        {
            get { return (PIMClassView)DiagramView.RepresentantsCollection[SourceClass]; }
        }

        public PIMClassView TargetClassView
        {
            get { return (PIMClassView)DiagramView.RepresentantsCollection[TargetClass]; }
        }


        public PIMAssociationEnd TargetEnd
        {
            get { return PIMAssociation.PIMAssociationEnds[1]; }
        }

        public PIMClass TargetClass
        {
            get { return PIMAssociation.PIMClasses[1]; }
        }

        public Label NameLabel { get; private set; }
        public Label SourceCardinalityLabel { get; private set; }
        public Label TargetCardinalityLabel { get; private set; }

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

        private string sourceCardinality;
        public string SourceCardinality
        {
            get { return sourceCardinality; }
            set
            {
                sourceCardinality = value;
                if (SourceCardinalityLabel != null)
                {
                    SourceCardinalityLabel.Text = value;
                    SourceCardinalityLabel.Visibility = !string.IsNullOrEmpty(value) && value != "1" ? Visibility.Visible : Visibility.Collapsed;
                }
            }
        }

        private string targetCardinality;
        public string TargetCardinality
        {
            get { return targetCardinality; }
            set
            {
                targetCardinality = value;
                if (TargetCardinalityLabel != null)
                {
                    TargetCardinalityLabel.Text = value;
                    TargetCardinalityLabel.Visibility = !string.IsNullOrEmpty(value) && value != "1" ? Visibility.Visible : Visibility.Collapsed;
                }
            }
        }

        public override bool CanPutInDiagram(DiagramView diagramView)
        {
            if (!diagramView.RepresentantsCollection.ContainsKey(SourceClass) ||
               !diagramView.RepresentantsCollection.ContainsKey(SourceClass))
            {
                /* 
                 * since parent and child may change, it is necessary
                 * to hook to this possible change
                 */
                if (!parentChildUpdateBound)
                {
                    PIMAssociation.PropertyChanged += PIMAssociation_PropertyChanged_ForParentChildUpdate;
                    parentChildUpdateBound = true;
                }
            }

            return base.CanPutInDiagram(diagramView)
                && diagramView.RepresentantsCollection.ContainsKey(SourceClass)
                && diagramView.RepresentantsCollection.ContainsKey(TargetClass);
        }

        bool parentChildUpdateBound = false; 

        private void PIMAssociation_PropertyChanged_ForParentChildUpdate(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName.IsAmong("PIMAssociationEnds")
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

            Connector = new Connector();
            NameLabel = new Label();
            SourceCardinalityLabel = new Label();
            TargetCardinalityLabel = new Label();
            CreatedControls.Add(Connector);
            CreatedControls.Add(NameLabel);
            CreatedControls.Add(SourceCardinalityLabel);
            CreatedControls.Add(TargetCardinalityLabel);
            DiagramView.ExolutioCanvas.AddConnector(Connector);
            Connector.Connect(SourceClassView.MainNode, TargetClassView.MainNode);
            if (ViewHelper.Points.Count == 0)
            {
                ViewHelper.Points.AppendRange(Connector.Points.Select(p => (Point)p));
            }
            else if (ViewHelper.Points.Count == Connector.Points.Count)
            {
                Connector.SetPoints(ViewHelper.Points);
            }
            DiagramView.ExolutioCanvas.AddNode(NameLabel);
            DiagramView.ExolutioCanvas.AddNode(SourceCardinalityLabel);
            DiagramView.ExolutioCanvas.AddNode(TargetCardinalityLabel);
            NameLabel.PlacementCenter = EPlacementCenter.Center;
            Connector.SnapNodeToConnector(NameLabel);
            SourceCardinalityLabel.SnapTo(Connector.StartPoint, true);
            TargetCardinalityLabel.SnapTo(Connector.EndPoint, true);

            ExolutioContextMenu associationMenu = MenuHelper.GetContextMenu(ScopeAttribute.EScope.PIMAssociation, DiagramView.Diagram);
            ContextMenu = associationMenu;
            NameLabel.ContextMenu = associationMenu;
            ExolutioContextMenu startPointMenu = MenuHelper.GetContextMenu(ScopeAttribute.EScope.PIMAssociationEnd, DiagramView.Diagram);
            Connector.StartPoint.ContextMenu = startPointMenu;
            SourceCardinalityLabel.ContextMenu = startPointMenu;
            ExolutioContextMenu endPointMenu = MenuHelper.GetContextMenu(ScopeAttribute.EScope.PIMAssociationEnd, DiagramView.Diagram);
            Connector.EndPoint.ContextMenu = endPointMenu;
            TargetCardinalityLabel.ContextMenu = endPointMenu;

#if SILVERLIGHT
            ContextMenuService.SetContextMenu(Connector, associationMenu);
            ContextMenuService.SetContextMenu(NameLabel, associationMenu);
            ContextMenuService.SetContextMenu(Connector.StartPoint, startPointMenu);
            ContextMenuService.SetContextMenu(Connector.EndPoint, endPointMenu);
#endif
            BindModelView();

            NameLabel.SelectedChanged += new Action(NameLabel_SelectedChanged);
            NameLabel.PositionChanged += NameLabel_PositionChanged;
            SourceCardinalityLabel.PositionChanged += SourceCardinalityLabel_PositionChanged;
            TargetCardinalityLabel.PositionChanged += TargetCardinalityLabel_PositionChanged;
            Connector.SelectedChanged += Connector_SelectedChanged;
            Connector.ConnectorPointMoved += Connector_ConnectorPointMoved;
            //Connector.MouseDown += Connector_MouseDown;
            #if SILVERLIGHT
            #else
            //Connector.MouseUp += Connector_MouseUp;
            #endif
        }

        void Connector_SelectedChanged()
        {
            this.Selected = Connector.Selected;
        }

//#if SILVERLIGHT
//        void Connector_MouseDown()
//#else
//        void Connector_MouseDown(object sender, MouseButtonEventArgs e)
//#endif
//        {
//            DiagramView.ExolutioCanvas.SelectedItems.Clear();
//            DiagramView.ExolutioCanvas.SelectedItems.Add(this.Connector);
//            DiagramView.SetSelection(ModelComponent);
//#if SILVERLIGHT
//#else
//            e.Handled = true;
//#endif
//        }
        
//        private void Connector_MouseUp(object sender, MouseButtonEventArgs e)
//        {
//            //otherwise MouseUp is catched by DiagramView which clears selection 
//            e.Handled = true;
//        }
        
        private void SourceCardinalityLabel_PositionChanged()
        {
            ViewHelper.AssociationEndsViewHelpers[0].CardinalityLabelViewHelper.SetPositionSilent(SourceCardinalityLabel.Position);
        }

        void TargetCardinalityLabel_PositionChanged()
        {
            ViewHelper.AssociationEndsViewHelpers[1].CardinalityLabelViewHelper.SetPositionSilent(TargetCardinalityLabel.Position);
        }

        void NameLabel_SelectedChanged()
        {
            if (NameLabel.Selected && !this.Selected)
            {
                Connector.Selected = true;
            }
        }

        void NameLabel_PositionChanged()
        {
            ViewHelper.MainLabelViewHelper.SetPositionSilent(NameLabel.Position);
        }

        void Connector_ConnectorPointMoved(ConnectorPoint point)
        {
            if (ViewHelper.Points.Count > point.OrderInConnector)
            {
                ViewHelper.Points[point.OrderInConnector].SetPosition(point.Position);
            }
        }
        
        public override void RemoveFromDiagram()
        {
            if (Selected)
            {
                Selected = false;
            }
            UnBindModelView();
            DiagramView.ExolutioCanvas.RemoveConnector(Connector);
            DiagramView.ExolutioCanvas.RemoveNode(NameLabel);
            DiagramView.ExolutioCanvas.RemoveNode(SourceCardinalityLabel);
            DiagramView.ExolutioCanvas.RemoveNode(TargetCardinalityLabel);
            base.RemoveFromDiagram();
        }

#if SILVERLIGHT
        public override ContextMenu ContextMenu
        {
            get
            {
                return Connector.ContextMenu;
            }
            set
            {
                Connector.ContextMenu = value;
            }
        }
#else

        public override ContextMenu ContextMenu
        {
            get { return Connector.ContextMenu; }
            set { Connector.ContextMenu = value; }
        }
#endif

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

        public override void Focus()
        {
            base.Focus();
            Connector.Focus();
        }
    }
}