using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using EvoX.Controller.Commands;
using EvoX.Model;
using EvoX.Model.PIM;
using EvoX.Model.ViewHelper;
using EvoX.ViewToolkit;
using Component = EvoX.Model.Component;
using Label = EvoX.ViewToolkit.Label;

namespace EvoX.View
{
    public class PIMAssociationView : ComponentViewBaseVH<PIMAssociationViewHelper>
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
            ((EvoXContextMenu)ContextMenu).ScopeObject = PIMAssociation;
            ((EvoXContextMenu)ContextMenu).Diagram = DiagramView.Diagram;
            ((EvoXContextMenu)Connector.StartPoint.ContextMenu).ScopeObject = PIMAssociation.PIMAssociationEnds[0];
            ((EvoXContextMenu)Connector.StartPoint.ContextMenu).Diagram = DiagramView.Diagram;
            ((EvoXContextMenu)Connector.EndPoint.ContextMenu).ScopeObject = PIMAssociation.PIMAssociationEnds[1];
            ((EvoXContextMenu)Connector.EndPoint.ContextMenu).Diagram = DiagramView.Diagram;
        }

        protected override void UnBindModelView()
        {
            base.UnBindModelView();
            foreach (PIMAssociationEnd pimAssociationEnd in PIMAssociation.PIMAssociationEnds)
            {
                pimAssociationEnd.PropertyChanged -= AssociationEnd_PropertyChanged;
            }
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
            if (sourceNode != Connector.StartNode)
            {
                Connector.Connect(sourceNode, targetNode);
            }
            if (targetNode != Connector.EndNode)
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
            return base.CanPutInDiagram(diagramView)
                && diagramView.RepresentantsCollection.ContainsKey(SourceClass)
                && diagramView.RepresentantsCollection.ContainsKey(TargetClass);
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
            DiagramView.EvoXCanvas.AddConnector(Connector);
            Connector.Connect(SourceClassView.MainNode, TargetClassView.MainNode);
            if (ViewHelper.Points.Count == 0)
            {
                ViewHelper.Points.AppendRange(Connector.Points.Select(p => (Point)p));
            }
            else if (ViewHelper.Points.Count == Connector.Points.Count)
            {
                Connector.SetPoints(ViewHelper.Points);
            }
            DiagramView.EvoXCanvas.AddNode(NameLabel);
            DiagramView.EvoXCanvas.AddNode(SourceCardinalityLabel);
            DiagramView.EvoXCanvas.AddNode(TargetCardinalityLabel);
            NameLabel.PlacementCenter = EPlacementCenter.Center;
            Connector.SnapNodeToConnector(NameLabel);
            SourceCardinalityLabel.SnapTo(Connector.StartPoint, true);
            TargetCardinalityLabel.SnapTo(Connector.EndPoint, true);

            EvoXContextMenu associationMenu = MenuHelper.GetContextMenu(ScopeAttribute.EScope.PIMAssociation, DiagramView.Diagram);
            ContextMenu = associationMenu;
            EvoXContextMenu startPointMenu = MenuHelper.GetContextMenu(ScopeAttribute.EScope.PIMAssociationEnd, DiagramView.Diagram);
            Connector.StartPoint.ContextMenu = startPointMenu;
            EvoXContextMenu endPointMenu = MenuHelper.GetContextMenu(ScopeAttribute.EScope.PIMAssociationEnd, DiagramView.Diagram);
            Connector.EndPoint.ContextMenu = endPointMenu;

#if SILVERLIGHT
            ContextMenuService.SetContextMenu(Connector, associationMenu);
            ContextMenuService.SetContextMenu(NameLabel, associationMenu);
            ContextMenuService.SetContextMenu(Connector.StartPoint, startPointMenu);
            ContextMenuService.SetContextMenu(Connector.EndPoint, endPointMenu);
#endif
            BindModelView();

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
//            DiagramView.EvoXCanvas.SelectedItems.Clear();
//            DiagramView.EvoXCanvas.SelectedItems.Add(this.Connector);
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
            UnBindModelView();
            DiagramView.EvoXCanvas.RemoveConnector(Connector);
            DiagramView.EvoXCanvas.RemoveNode(NameLabel);
            DiagramView.EvoXCanvas.RemoveNode(SourceCardinalityLabel);
            DiagramView.EvoXCanvas.RemoveNode(TargetCardinalityLabel);
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