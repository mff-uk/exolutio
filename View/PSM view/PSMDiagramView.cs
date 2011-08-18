using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using Exolutio.Controller.Commands;
using Exolutio.Model;
using Exolutio.Model.PSM;
using Exolutio.Model.ViewHelper;
using Exolutio.ViewToolkit;
using Exolutio.SupportingClasses;

namespace Exolutio.View
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

            ExolutioCanvas.MouseLeftButtonUp += ExolutioCanvas_MouseLeftButtonUp;
            ExolutioCanvas.Loaded += ExolutioCanvas_Loaded;
            ExolutioCanvas.ContentChanged += ExolutioCanvas_ContentChanged;

            ExolutioContextMenu diagramMenu = MenuHelper.GetContextMenu(ScopeAttribute.EScope.PSMSchema, this.Diagram);
            ExolutioCanvas.ContextMenu = diagramMenu;

#if SILVERLIGHT
            ContextMenuService.SetContextMenu(ExolutioCanvas, diagramMenu);
            MenuHelper.CreateSubmenuForCommandsWithoutScope(diagramMenu);
#else
            ContextMenuItem otherItemsMenu = new ContextMenuItem("Other operations");
            MenuHelper.CreateSubmenuForCommandsWithoutScope(otherItemsMenu);
            ExolutioCanvas.ContextMenu.Items.Add(otherItemsMenu);
#endif 
        }

        void ExolutioCanvas_ContentChanged()
        {
            DoLayout();            
        }

        private void DoLayout()
        {
            if (!this.Loading &&
                !this.SuspendBindingInChildren
                #if SILVERLIGHT
                #else
                && this.IsArrangeValid
                #endif
                )
            {
                LayoutManager.DoLayout(this);
            }
        }

        void ExolutioCanvas_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            DoLayout();
        }

        void ExolutioCanvas_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            DoLayout();
        }

        public override IEnumerable<ComponentViewBase> LoadDiagram(Diagram diagram)
        {
            IEnumerable<ComponentViewBase> result = base.LoadDiagram(diagram);
            DoLayout();
            ((ExolutioContextMenu)ExolutioCanvas.ContextMenu).ScopeObject = PSMDiagram.PSMSchema;
            ((ExolutioContextMenu)ExolutioCanvas.ContextMenu).Diagram = PSMDiagram;
            ((PSMDiagram)Diagram).PSMSchema.Roots.CollectionChanged += Roots_CollectionChanged;
            return result;
        }

        public override void UnLoadDiagram()
        {
            ((PSMDiagram) Diagram).PSMSchema.Roots.CollectionChanged -= Roots_CollectionChanged;
            base.UnLoadDiagram();
        }

        private void Roots_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            DoLayout();
        }
    }
}