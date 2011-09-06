using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Exolutio.Controller.Commands;
using Exolutio.Model;
using Exolutio.Model.ViewHelper;
using Exolutio.SupportingClasses;
using Exolutio.View.Commands.View;
using Exolutio.ViewToolkit;
using Component = Exolutio.Model.Component;
using Label = Exolutio.ViewToolkit.Label;
using Exolutio.Model.PSM;

namespace Exolutio.View
{
    public class PSMGeneralizationView : ComponentViewBaseVH<PSMGeneralizationViewHelper>, IConnectorViewBase, IChangesInScreenShotView
    {
        public PSMGeneralization PSMGeneralization { get; private set; }

        public override Component ModelComponent
        {
            get { return PSMGeneralization; }
            protected set { PSMGeneralization = (PSMGeneralization)value; }
        }

        public override PSMGeneralizationViewHelper ViewHelper { get; protected set; }

        protected override void BindModelView()
        {
            base.BindModelView();
            BindMenus();
        }

        private void BindMenus()
        {
            ((ExolutioContextMenu)ContextMenu).ScopeObject = PSMGeneralization;
            ((ExolutioContextMenu)ContextMenu).Diagram = DiagramView.Diagram;
        }

        protected override void UnBindModelView()
        {
            UnBindMenus();
            base.UnBindModelView();
        }

        private void UnBindMenus()
        {
            ((ExolutioContextMenu)ContextMenu).ScopeObject = null;
            ((ExolutioContextMenu)ContextMenu).Diagram = null;
        }

        public override void UpdateView(string propertyName = null)
        {
            base.UpdateView(propertyName);
            Node sourceNode = ((INodeComponentViewBase)DiagramView.RepresentantsCollection[SpecificClass]).MainNode;
            Node targetNode = ((INodeComponentViewBase)DiagramView.RepresentantsCollection[GeneralClass]).MainNode;
            if (sourceNode != Connector.StartNode || targetNode != Connector.EndNode)
            {
                UnBindMenus();
                Connector.Connect(sourceNode, targetNode);                
                CreateMenus();
                BindMenus();
            }
        }

        public Connector Connector { get; private set; }

        public PSMClass SpecificClass
        {
            get { return PSMGeneralization.Specific; }
        }

        public PSMClassView SpecificClassView
        {
            get { return (PSMClassView)DiagramView.RepresentantsCollection[SpecificClass]; }
        }

        public PSMClassView GemeralClassView
        {
            get { return (PSMClassView)DiagramView.RepresentantsCollection[GeneralClass]; }
        }
        
        public PSMClass GeneralClass
        {
            get { return PSMGeneralization.General; }
        }

        public override bool CanPutInDiagram(DiagramView diagramView)
        {
            if (!diagramView.RepresentantsCollection.ContainsKey(SpecificClass) ||
               !diagramView.RepresentantsCollection.ContainsKey(GeneralClass))
            {
                /* 
                 * since parent and child may change, it is necessary
                 * to hook to this possible change
                 */
                if (!parentChildUpdateBound)
                {
                    PSMGeneralization.PropertyChanged += PSMGeneralization_PropertyChanged_ForParentChildUpdate;
                    parentChildUpdateBound = true;
                }
            }

            return base.CanPutInDiagram(diagramView)
                && diagramView.RepresentantsCollection.ContainsKey(SpecificClass)
                && diagramView.RepresentantsCollection.ContainsKey(GeneralClass);
        }

        bool parentChildUpdateBound = false; 

        private void PSMGeneralization_PropertyChanged_ForParentChildUpdate(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName.IsAmong("General", "Specific")
                && DiagramView == null && pendingDiagramView != null)
            {
                pendingDiagramView.DefferedAddCheck();
            }
        }

        public override void PutInDiagram(DiagramView diagramView, ViewHelper viewHelper)
        {
            base.PutInDiagram(diagramView, viewHelper);

            Connector = new Connector();
            Connector.EndCapStyle = EConnectorCapStyle.Triangle;
            CreatedControls.Add(Connector);
            DiagramView.ExolutioCanvas.AddConnector(Connector);
            Connector.Connect(SpecificClassView.MainNode, GemeralClassView.MainNode);
            if (ViewHelper.Points.Count == 0)
            {
                ViewHelper.Points.AppendRange(Connector.Points.Select(p => (Point)p));
            }
            else if (ViewHelper.Points.Count >= Connector.Points.Count)
            {
                Connector.SetPoints(ViewHelper.Points);
            }
            
            CreateMenus();

#if SILVERLIGHT
            //ContextMenuService.SetContextMenu(Connector, associationMenu);
            //ContextMenuService.SetContextMenu(NameLabel, associationMenu);
            //ContextMenuService.SetContextMenu(Connector.StartPoint, startPointMenu);
            //ContextMenuService.SetContextMenu(Connector.EndPoint, endPointMenu);
#endif
            BindModelView();

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
            ExolutioContextMenu generalizationMenu = MenuHelper.GetContextMenu(ScopeAttribute.EScope.PSMGeneralization, DiagramView.Diagram);
            AddConnectorCommands(generalizationMenu);
            ContextMenu = generalizationMenu;
            
            #if SILVERLIGHT
            // for some reason the menus can not be shared and have to be created again
            associationMenu = MenuHelper.GetContextMenu(ScopeAttribute.EScope.PIMGeneralization, DiagramView.Diagram);
            AddConnectorCommands(associationMenu);
            NameLabel.ContextMenu = associationMenu;
            startPointMenu = MenuHelper.GetContextMenu(ScopeAttribute.EScope.PIMGeneralizationEnd, DiagramView.Diagram);
            SourceCardinalityLabel.ContextMenu = startPointMenu;
            endPointMenu = MenuHelper.GetContextMenu(ScopeAttribute.EScope.PIMGeneralizationEnd, DiagramView.Diagram);
            TargetCardinalityLabel.ContextMenu = endPointMenu;
            #else
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
        private void AddConnectorCommands(ExolutioContextMenu generalizationMenu)
        {
            guiBreakLineCommand cBreak = new guiBreakLineCommand();
            cBreak.Connector = Connector;
            ContextMenuItem miBreak;
            miBreak = new ContextMenuItem("Break line here");
            miBreak.Command = cBreak;
            generalizationMenu.Items.Add(miBreak);
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