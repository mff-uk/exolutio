using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using Exolutio.Controller.Commands;
using Exolutio.Model;
using Exolutio.Model.PIM;
using Exolutio.Model.ViewHelper;
using Exolutio.SupportingClasses;
using Exolutio.View.Commands.View;
using Exolutio.ViewToolkit;
using Component = Exolutio.Model.Component;
using Label = Exolutio.ViewToolkit.Label;

namespace Exolutio.View
{
    public class PIMAssociationView : ComponentViewBaseVH<PIMAssociationViewHelper>, IConnectorViewBase, IChangesInScreenShotView
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
            BindMenus();
        }

        private void BindMenus()
        {
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
            UnBindMenus();
            base.UnBindModelView();
        }

        private void UnBindMenus()
        {
            ((ExolutioContextMenu)ContextMenu).ScopeObject = null;
            ((ExolutioContextMenu)ContextMenu).Diagram = null;
            ((ExolutioContextMenu)Connector.StartPoint.ContextMenu).ScopeObject = null;
            ((ExolutioContextMenu)Connector.StartPoint.ContextMenu).Diagram = null;
            ((ExolutioContextMenu)Connector.EndPoint.ContextMenu).ScopeObject = null;
            ((ExolutioContextMenu)Connector.EndPoint.ContextMenu).Diagram = null;
        }

        private void AssociationEnd_PropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            UpdateView();
        }

        public override void UpdateView(string propertyName = null)
        {
            base.UpdateView(propertyName);
            Node sourceNode = ((INodeComponentViewBase)DiagramView.RepresentantsCollection[SourceClass]).MainNode;
            Node targetNode = ((INodeComponentViewBase)DiagramView.RepresentantsCollection[TargetClass]).MainNode;
            if (sourceNode != Connector.StartNode || targetNode != Connector.EndNode)
            {
                UnBindMenus();
                Connector.Connect(sourceNode, targetNode);                
                CreateMenus();
                BindMenus();
            }
            
            // labels, multiplicities
            AssociationName = PIMAssociation.Name;
            SourceCardinality = PIMAssociation.PIMAssociationEnds[0].GetCardinalityString();
            TargetCardinality = PIMAssociation.PIMAssociationEnds[1].GetCardinalityString();
            SourceRole = PIMAssociation.PIMAssociationEnds[0].Name;
            TargetRole = PIMAssociation.PIMAssociationEnds[1].Name;

            if (NameLabel != null)
            {
                NameLabel.X = ViewHelper.MainLabelViewHelper.X;
                NameLabel.Y = ViewHelper.MainLabelViewHelper.Y;
                NameLabel.UpdateCanvasPosition(true);
            }

            if (SourceCardinalityLabel != null)
            {
                SourceCardinalityLabel.X = ViewHelper.AssociationEndsViewHelpers[0].CardinalityLabelViewHelper.X;
                SourceCardinalityLabel.Y = ViewHelper.AssociationEndsViewHelpers[0].CardinalityLabelViewHelper.Y;
                SourceCardinalityLabel.UpdateCanvasPosition(true);
            }

            if (TargetCardinalityLabel != null)
            {
                TargetCardinalityLabel.X = ViewHelper.AssociationEndsViewHelpers[1].CardinalityLabelViewHelper.X;
                TargetCardinalityLabel.Y = ViewHelper.AssociationEndsViewHelpers[1].CardinalityLabelViewHelper.Y;
                TargetCardinalityLabel.UpdateCanvasPosition(true);
            }

            if (SourceRoleLabel != null)
            {
                SourceRoleLabel.X = ViewHelper.AssociationEndsViewHelpers[0].RoleLabelViewHelper.X;
                SourceRoleLabel.Y = ViewHelper.AssociationEndsViewHelpers[0].RoleLabelViewHelper.Y;
                SourceRoleLabel.UpdateCanvasPosition(true);
            }

            if (TargetRoleLabel != null)
            {
                TargetRoleLabel.X = ViewHelper.AssociationEndsViewHelpers[1].RoleLabelViewHelper.X;
                TargetRoleLabel.Y = ViewHelper.AssociationEndsViewHelpers[1].RoleLabelViewHelper.Y;
                TargetRoleLabel.UpdateCanvasPosition(true);
            }
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

        #region label properties

        private Label nameLabel;

        public Label NameLabel
        {
            get { return nameLabel; }
            private set { nameLabel = value; }
        }

        private Label sourceCardinalityLabel;

        public Label SourceCardinalityLabel
        {
            get { return sourceCardinalityLabel; }
            private set { sourceCardinalityLabel = value; }
        }

        private Label sourceRoleLabel;

        public Label SourceRoleLabel
        {
            get { return sourceRoleLabel; }
            private set { sourceRoleLabel = value; }
        }

        private Label targetCardinalityLabel;

        public Label TargetCardinalityLabel
        {
            get { return targetCardinalityLabel; }
            private set { targetCardinalityLabel = value; }
        }

        private Label targetRoleLabel;

        public Label TargetRoleLabel
        {
            get { return targetRoleLabel; }
            private set { targetRoleLabel = value; }
        }

        #endregion

        private string associationName;
        public string AssociationName
        {
            get { return associationName; }
            set
            {
                associationName = value;
                NameLabel = CreateOrRemoveLabel(ref nameLabel, ViewHelper.MainLabelViewHelper, value, null, NameLabel_SelectedChanged, NameLabel_PositionChanged, associationMenu,
                    null, Connector, EPlacementCenter.Center);
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
                SourceCardinalityLabel = CreateOrRemoveLabel(ref sourceCardinalityLabel, ViewHelper.AssociationEndsViewHelpers[0].CardinalityLabelViewHelper, null, value, null, SourceCardinalityLabel_PositionChanged, startPointMenu, Connector.StartPoint);
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
                TargetCardinalityLabel = CreateOrRemoveLabel(ref targetCardinalityLabel, ViewHelper.AssociationEndsViewHelpers[1].CardinalityLabelViewHelper, null, value, null, TargetCardinalityLabel_PositionChanged, endPointMenu, Connector.EndPoint);
                if (TargetCardinalityLabel != null)
                {
                    TargetCardinalityLabel.Text = value;
                    TargetCardinalityLabel.Visibility = !string.IsNullOrEmpty(value) && value != "1" ? Visibility.Visible : Visibility.Collapsed;
                }
            }
        }

        private string sourceRole;
        public string SourceRole
        {
            get { return sourceRole; }
            set
            {
                sourceRole = value;
                SourceRoleLabel = CreateOrRemoveLabel(ref sourceRoleLabel, ViewHelper.AssociationEndsViewHelpers[0].RoleLabelViewHelper, value, null, null, SourceRoleLabel_PositionChanged, startPointMenu, Connector.StartPoint);
                if (SourceRoleLabel != null)
                {
                    SourceRoleLabel.Text = value;
                    SourceRoleLabel.Visibility = !string.IsNullOrEmpty(value) && value != "1" ? Visibility.Visible : Visibility.Collapsed;
                }
            }
        }

        private string targetRole;
        public string TargetRole
        {
            get { return targetRole; }
            set
            {
                targetRole = value;
                TargetRoleLabel = CreateOrRemoveLabel(ref targetRoleLabel, ViewHelper.AssociationEndsViewHelpers[1].RoleLabelViewHelper, value, null, null, TargetRoleLabel_PositionChanged, endPointMenu, Connector.EndPoint);
                if (TargetRoleLabel != null)
                {
                    TargetRoleLabel.Text = value;
                    TargetRoleLabel.Visibility = !string.IsNullOrEmpty(value) && value != "1" ? Visibility.Visible : Visibility.Collapsed;
                }
            }
        }

        public override bool CanPutInDiagram(DiagramView diagramView)
        {
            if (!diagramView.RepresentantsCollection.ContainsKey(SourceClass) ||
               !diagramView.RepresentantsCollection.ContainsKey(TargetClass))
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
        private ExolutioContextMenu associationMenu;
        private ExolutioContextMenu startPointMenu;
        private ExolutioContextMenu endPointMenu;

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

        private Label CreateOrRemoveLabel(ref Label currentLabel, LabelViewHelper viewHelper, string name, string cardinality, Action selectedChanged, Action<DragDeltaEventArgs> positionChanged, ContextMenu contextMenu, ConnectorPoint snapPoint = null, Connector snapConnector = null, EPlacementCenter placementCenter = EPlacementCenter.TopLeftCorner)
        {
            if (string.IsNullOrEmpty(name) && (cardinality == "1" || string.IsNullOrEmpty(cardinality)))
            {
                if (currentLabel != null)
                {
                    if (selectedChanged != null)
                        currentLabel.SelectedChanged -= selectedChanged;
                    if (positionChanged != null)
                        currentLabel.PositionChanged -= positionChanged;                    
                    if (snapPoint != null)
                        currentLabel.UnSnap();
                    if (snapConnector != null)
                        currentLabel.UnSnap();
                    CreatedControls.Remove(currentLabel);
                    DiagramView.ExolutioCanvas.RemoveNode(currentLabel);
                }
                return null;
            }
            
            if (currentLabel == null)
            {
                Point tmpPosition = viewHelper.Position;
                currentLabel = new Label();                
                CreatedControls.Add(currentLabel);
                DiagramView.ExolutioCanvas.AddNode(currentLabel);
                currentLabel.PlacementCenter = placementCenter;
                if (snapPoint != null)
                    currentLabel.SnapTo(snapPoint, true);
                if (snapConnector != null)
                    snapConnector.SnapNodeToConnector(currentLabel);
                currentLabel.X = viewHelper.X;
                currentLabel.Y = viewHelper.Y;
                if (selectedChanged != null)
                    currentLabel.SelectedChanged += selectedChanged;
                if (positionChanged != null)
                    currentLabel.PositionChanged += positionChanged;
                currentLabel.ContextMenu = contextMenu;
                //UpdateView();
                //viewHelper.SetPositionSilent(tmpPosition.X, tmpPosition.Y);
                //currentLabel.X = 
            }
            return currentLabel;
        }

        public override void PutInDiagram(DiagramView diagramView, ViewHelper viewHelper)
        {
            base.PutInDiagram(diagramView, viewHelper);

            Connector = new Connector();
            ////NameLabel = new Label();
            ////SourceCardinalityLabel = new Label();
            ////SourceRoleLabel = new Label();
            ////TargetCardinalityLabel = new Label();
            ////TargetRoleLabel = new Label();
            CreatedControls.Add(Connector);
            ////CreatedControls.Add(NameLabel);
            ////CreatedControls.Add(SourceCardinalityLabel);
            ////CreatedControls.Add(SourceRoleLabel);
            ////CreatedControls.Add(TargetCardinalityLabel);
            ////CreatedControls.Add(TargetRoleLabel);
            DiagramView.ExolutioCanvas.AddConnector(Connector);
            Connector.Connect(SourceClassView.MainNode, TargetClassView.MainNode);
            if (ViewHelper.Points.Count == 0)
            {
                ViewHelper.Points.AppendRange(Connector.Points.Select(p => (Point)p));
            }
            else if (ViewHelper.Points.Count >= Connector.Points.Count)
            {
                Connector.SetPoints(ViewHelper.Points);
            }
            ////DiagramView.ExolutioCanvas.AddNode(NameLabel);
            ////DiagramView.ExolutioCanvas.AddNode(SourceCardinalityLabel);
            ////DiagramView.ExolutioCanvas.AddNode(SourceRoleLabel);
            ////DiagramView.ExolutioCanvas.AddNode(TargetCardinalityLabel);
            ////DiagramView.ExolutioCanvas.AddNode(TargetRoleLabel);
            ////NameLabel.PlacementCenter = EPlacementCenter.Center;
            ////Connector.SnapNodeToConnector(NameLabel);
            ////SourceCardinalityLabel.SnapTo(Connector.StartPoint, true);
            ////SourceRoleLabel.SnapTo(Connector.StartPoint, true);
            ////TargetCardinalityLabel.SnapTo(Connector.EndPoint, true);
            ////TargetRoleLabel.SnapTo(Connector.EndPoint, true);

            CreateMenus();

            BindModelView();

            ////NameLabel.SelectedChanged += NameLabel_SelectedChanged;
            ////NameLabel.PositionChanged += NameLabel_PositionChanged;
            ////SourceCardinalityLabel.PositionChanged += SourceCardinalityLabel_PositionChanged;
            ////SourceRoleLabel.PositionChanged += SourceRoleLabel_PositionChanged;
            ////TargetCardinalityLabel.PositionChanged += TargetCardinalityLabel_PositionChanged;
            ////SourceRoleLabel.PositionChanged += TargetRoleLabel_PositionChanged;
            
            Connector.SelectedChanged += Connector_SelectedChanged;
            Connector.ConnectorPointMoved += Connector_ConnectorPointMoved;
            Connector.PointsCountChanged += Connector_PointsCountChanged;
            //Connector.MouseDown += Connector_MouseDown;
            #if SILVERLIGHT
            #else
            //Connector.MouseUp += Connector_MouseUp;
            #endif
        }

        private void CreateMenus()
        {
            associationMenu = MenuHelper.GetContextMenu(ScopeAttribute.EScope.PIMAssociation, DiagramView.Diagram);
            AddConnectorCommands(associationMenu);
            ContextMenu = associationMenu;
            
            startPointMenu = MenuHelper.GetContextMenu(ScopeAttribute.EScope.PIMAssociationEnd, DiagramView.Diagram);
            Connector.StartPoint.ContextMenu = startPointMenu;
            
            endPointMenu = MenuHelper.GetContextMenu(ScopeAttribute.EScope.PIMAssociationEnd, DiagramView.Diagram);
            Connector.EndPoint.ContextMenu = endPointMenu;

            #if SILVERLIGHT
            // for some reason the menus can not be shared and have to be created again
            associationMenu = MenuHelper.GetContextMenu(ScopeAttribute.EScope.PIMAssociation, DiagramView.Diagram);
            AddConnectorCommands(associationMenu);
            NameLabel.ContextMenu = associationMenu;
            startPointMenu = MenuHelper.GetContextMenu(ScopeAttribute.EScope.PIMAssociationEnd, DiagramView.Diagram);
            SourceCardinalityLabel.ContextMenu = startPointMenu;
            endPointMenu = MenuHelper.GetContextMenu(ScopeAttribute.EScope.PIMAssociationEnd, DiagramView.Diagram);
            TargetCardinalityLabel.ContextMenu = endPointMenu;
            #else
            //NameLabel.ContextMenu = associationMenu;
            //SourceCardinalityLabel.ContextMenu = startPointMenu;
            //TargetCardinalityLabel.ContextMenu = endPointMenu;
            #endif
        }

        void Connector_SelectedChanged()
        {
            this.Selected = Connector.Selected;
        }
        
        private void SourceCardinalityLabel_PositionChanged(DragDeltaEventArgs dragDeltaEventArgs)
        {
            ViewHelper.AssociationEndsViewHelpers[0].CardinalityLabelViewHelper.SetPositionSilent(SourceCardinalityLabel.Position);
        }

        void TargetCardinalityLabel_PositionChanged(DragDeltaEventArgs dragDeltaEventArgs)
        {
            ViewHelper.AssociationEndsViewHelpers[1].CardinalityLabelViewHelper.SetPositionSilent(TargetCardinalityLabel.Position);
        }

        private void SourceRoleLabel_PositionChanged(DragDeltaEventArgs dragDeltaEventArgs)
        {
            ViewHelper.AssociationEndsViewHelpers[0].RoleLabelViewHelper.SetPositionSilent(SourceRoleLabel.Position);
        }

        void TargetRoleLabel_PositionChanged(DragDeltaEventArgs dragDeltaEventArgs)
        {
            ViewHelper.AssociationEndsViewHelpers[1].RoleLabelViewHelper.SetPositionSilent(TargetRoleLabel.Position);
        }

        void NameLabel_SelectedChanged()
        {
            if (NameLabel.Selected && !this.Selected)
            {
                Connector.Selected = true;
            }
        }

        void NameLabel_PositionChanged(DragDeltaEventArgs dragDeltaEventArgs)
        {
            ViewHelper.MainLabelViewHelper.SetPositionSilent(NameLabel.Position);
        }

        void Connector_PointsCountChanged()
        {
            ViewHelper.Points.Clear();

            foreach (ConnectorPoint connectorPoint in Connector.Points)
            {
                ViewHelper.Points.Add(new rPoint(connectorPoint.Position));    
            }
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
            if (NameLabel != null)
            DiagramView.ExolutioCanvas.RemoveNode(NameLabel);
            if (SourceCardinalityLabel != null)
            DiagramView.ExolutioCanvas.RemoveNode(SourceCardinalityLabel);
            if (TargetCardinalityLabel != null)
            DiagramView.ExolutioCanvas.RemoveNode(TargetCardinalityLabel);
            if (SourceRoleLabel != null)
            DiagramView.ExolutioCanvas.RemoveNode(SourceRoleLabel);
            if (TargetRoleLabel != null)
            DiagramView.ExolutioCanvas.RemoveNode(TargetRoleLabel);
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
        private void AddConnectorCommands(ExolutioContextMenu associationMenu)
        {
            guiBreakLineCommand cBreak = new guiBreakLineCommand();
            cBreak.Connector = Connector;
            ContextMenuItem miBreak;
            miBreak = new ContextMenuItem("Break line here");
            miBreak.Command = cBreak;
            associationMenu.Items.Add(miBreak);
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

        public void EnterScreenShotView()
        {
            Connector.HideAllPoints();
        }

        public void ExitScreenShotView()
        {
            Connector.UnHideAllPoints();
        }
    }
}