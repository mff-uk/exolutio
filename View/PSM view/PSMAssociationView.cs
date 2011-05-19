using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using EvoX.Controller.Commands;
using EvoX.Model;
using EvoX.Model.PSM;
using EvoX.Model.ViewHelper;
using EvoX.ViewToolkit;
using Label = EvoX.ViewToolkit.Label;

namespace EvoX.View
{
    public class PSMAssociationView : ComponentViewBaseVH<PSMAssociationViewHelper>
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
            ContextMenu = MenuHelper.GetContextMenu(ScopeAttribute.EScope.PSMAssociation, this.DiagramView.Diagram);
            #if SILVERLIGHT
            ContextMenuService.SetContextMenu(NameLabel, ContextMenu);
            ContextMenuService.SetContextMenu(Connector, ContextMenu);
            #endif
            ((EvoXContextMenu)ContextMenu).ScopeObject = PSMAssociation;
            ((EvoXContextMenu)ContextMenu).Diagram = DiagramView.Diagram;
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
            // labels, multiplicities
            NameLabel.Text = PSMAssociation.Name;
            Cardinality = PSMAssociation.GetCardinalityString();

            NameLabel.X = ViewHelper.MainLabelViewHelper.X;
            NameLabel.Y = ViewHelper.MainLabelViewHelper.Y;
            NameLabel.UpdateCanvasPosition(true);

            CardinalityLabel.X = ViewHelper.CardinalityLabelViewHelper.X;
            CardinalityLabel.Y = ViewHelper.CardinalityLabelViewHelper.Y;
            CardinalityLabel.UpdateCanvasPosition(true);
            
            DiagramView.EvoXCanvas.InvokeContentChanged();
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
            return base.CanPutInDiagram(diagramView)
                && diagramView.RepresentantsCollection.ContainsKey(Parent)
                && diagramView.RepresentantsCollection.ContainsKey(Child);
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
            CardinalityLabel = new Label();
            CreatedControls.Add(Connector);
            CreatedControls.Add(NameLabel);
            CreatedControls.Add(CardinalityLabel);
            DiagramView.EvoXCanvas.AddConnector(Connector);
            Connector.Connect(SourceClassView.MainNode, TargetClassView.MainNode);
            //if (ViewHelper.Points.Count == 0)
            //{
            //    ViewHelper.Points.AppendRange(Connector.Points.Select(p => (Point)p));
            //}
            //else if (ViewHelper.Points.Count == Connector.Points.Count)
            //{
            //    Connector.SetPoints(ViewHelper.Points);
            //}
            DiagramView.EvoXCanvas.AddNode(NameLabel);
            DiagramView.EvoXCanvas.AddNode(CardinalityLabel);
            NameLabel.PlacementCenter = EPlacementCenter.Center;
            Connector.SnapNodeToConnector(NameLabel);
            CardinalityLabel.SnapTo(Connector.EndPoint, true);
            BindModelView();

            NameLabel.PositionChanged += NameLabel_PositionChanged;
            CardinalityLabel.PositionChanged += CardinalityLabel_PositionChanged;
            //Connector.MouseDown += Connector_MouseDown;
            #if SILVERLIGHT
            #else
            //Connector.MouseUp += Connector_MouseUp;
            #endif
            //Connector.ConnectorPointMoved += Connector_ConnectorPointMoved;
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
//            #if SILVERLIGHT
//            #else
//            e.Handled = true;
//            #endif
//        }

//        private void Connector_MouseUp(object sender, MouseButtonEventArgs e)
//        {
//            //otherwise MouseUp is catched by DiagramView which clears selection 
//            e.Handled = true;
//        }

        private void CardinalityLabel_PositionChanged()
        {
            ViewHelper.CardinalityLabelViewHelper.SetPositionSilent(CardinalityLabel.Position);
        }
        
        void NameLabel_PositionChanged()
        {
            ViewHelper.MainLabelViewHelper.SetPositionSilent(NameLabel.Position);
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
            UnBindModelView();
            DiagramView.EvoXCanvas.RemoveConnector(Connector);
            DiagramView.EvoXCanvas.RemoveNode(NameLabel);
            DiagramView.EvoXCanvas.RemoveNode(CardinalityLabel);
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