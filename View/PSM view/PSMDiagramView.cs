using System;
using System.Collections.Generic;
using System.Windows.Controls;
using EvoX.Controller.Commands;
using EvoX.Model;
using EvoX.Model.PSM;
using EvoX.Model.ViewHelper;
using EvoX.ViewToolkit;
using EvoX.SupportingClasses;

namespace EvoX.View
{
    public class PSMDiagramView : DiagramView
    {
        public LayoutManager LayoutManager { get; set; }

        public PSMDiagram PSMDiagram
        {
            get { return (PSMDiagram)Diagram; }
        }

        public PSMDiagramView()
        {
            this.RepresentantsCollection.Registrations.Add(typeof(PSMClass), new RepresentantsCollection.RegistrationClass(
                () => new PSMClassView(),
                () => new PSMClassViewHelper(this.Diagram),
                typeof(PSMClass),
                typeof(PSMClassViewHelper),
                typeof(PSMClassView)));

            this.RepresentantsCollection.Registrations.Add(typeof(PSMSchemaClass), new RepresentantsCollection.RegistrationClass(
                () => new PSMSchemaClassView(),
                () => new PSMSchemaClassViewHelper(this.Diagram),
                typeof(PSMSchemaClass),
                typeof(PSMSchemaClassViewHelper),
                typeof(PSMSchemaClassView)));

            this.RepresentantsCollection.Registrations.Add(typeof(PSMContentModel), new RepresentantsCollection.RegistrationClass(
                () => new PSMContentModelView(),
                () => new PSMContentModelViewHelper(this.Diagram),
                typeof(PSMContentModel),
                typeof(PSMContentModelViewHelper),
                typeof(PSMContentModelView)));

            this.RepresentantsCollection.Registrations.Add(typeof(PSMAssociation), new RepresentantsCollection.RegistrationClass(
                () => new PSMAssociationView(),
                () => new PSMAssociationViewHelper(this.Diagram),
                typeof(PSMAssociation),
                typeof(ConnectionViewHelper),
                typeof(PSMAssociationView)));

            LayoutManager = new LayoutManager();

            EvoXCanvas.MouseLeftButtonUp += new System.Windows.Input.MouseButtonEventHandler(EvoXCanvas_MouseLeftButtonUp);
            EvoXCanvas.Loaded += new System.Windows.RoutedEventHandler(EvoXCanvas_Loaded);
            EvoXCanvas.ContentChanged += new Action(EvoXCanvas_ContentChanged);

            EvoXContextMenu diagramMenu = MenuHelper.GetContextMenu(ScopeAttribute.EScope.PSMSchema, this.Diagram);
            EvoXCanvas.ContextMenu = diagramMenu;

#if SILVERLIGHT
            ContextMenuService.SetContextMenu(EvoXCanvas, diagramMenu);
            MenuHelper.CreateSubmenuForCommandsWithoutScope(diagramMenu);
#else
            ContextMenuItem otherItemsMenu = new ContextMenuItem("Other operations");
            MenuHelper.CreateSubmenuForCommandsWithoutScope(otherItemsMenu);
            EvoXCanvas.ContextMenu.Items.Add(otherItemsMenu);
#endif 
        }

        void EvoXCanvas_ContentChanged()
        {
            DoLayout();
        }

        private void DoLayout()
        {
            if (!this.Loading)
            {
                LayoutManager.DoLayout(this);
            }
        }

        void EvoXCanvas_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            DoLayout();
        }

        void EvoXCanvas_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            DoLayout();
        }

        public override IEnumerable<ComponentViewBase> LoadDiagram(Diagram diagram)
        {
            IEnumerable<ComponentViewBase> result = base.LoadDiagram(diagram);
            DoLayout();
            ((EvoXContextMenu)EvoXCanvas.ContextMenu).ScopeObject = PSMDiagram.PSMSchema;
            ((EvoXContextMenu)EvoXCanvas.ContextMenu).Diagram = PSMDiagram;            
            return result;
        }
    }
}