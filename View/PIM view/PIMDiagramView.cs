using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using Exolutio.Controller.Commands;
using Exolutio.Model;
using Exolutio.Model.PIM;
using Exolutio.Model.ViewHelper;
using Exolutio.ViewToolkit;
using Exolutio.SupportingClasses;

namespace Exolutio.View
{
    public class PIMDiagramView : DiagramView
    {
        public PIMDiagramView()
        {
            this.RepresentantsCollection.Registrations.Add(typeof(PIMClass), new RepresentantsCollection.RegistrationClass(
                () => new PIMClassView(),
                () => new PIMClassViewHelper(this.Diagram),
                typeof(PIMClass),
                typeof(PIMClassViewHelper),
                typeof(PIMClassView)));

            this.RepresentantsCollection.Registrations.Add(typeof(PIMAssociation), new RepresentantsCollection.RegistrationClass(
                () => new PIMAssociationView(),
                () => new PIMAssociationViewHelper(this.Diagram),
                typeof(PIMAssociation),
                typeof(ConnectionViewHelper),
                typeof(PIMAssociationView)));
        }

        public PIMDiagram PIMDiagram
        {
            get { return (PIMDiagram)Diagram; }
        }

        public override IEnumerable<ComponentViewBase> LoadDiagram(Diagram diagram)
        {
            IEnumerable<ComponentViewBase> withoutViewHelpers = base.LoadDiagram(diagram);
            foreach (ComponentViewBase withoutViewHelper in withoutViewHelpers)
            {
                if (withoutViewHelper is PIMClassView)
                {
                    ((PIMClassView)withoutViewHelper).ViewHelper.X = RandomGenerator.Next(300);
                    ((PIMClassView)withoutViewHelper).ViewHelper.Y = RandomGenerator.Next(300);
                }
            }
            ExolutioContextMenu diagramMenu = MenuHelper.GetContextMenu(ScopeAttribute.EScope.PIMDiagram, this.Diagram);
            ExolutioCanvas.ContextMenu = diagramMenu;

            ((ExolutioContextMenu)ExolutioCanvas.ContextMenu).ScopeObject = PIMDiagram;
            ((ExolutioContextMenu)ExolutioCanvas.ContextMenu).Diagram = PIMDiagram;

#if SILVERLIGHT
            ContextMenuService.SetContextMenu(ExolutioCanvas, diagramMenu);
            MenuHelper.CreateSubmenuForCommandsWithoutScope(diagramMenu);
#else
            ContextMenuItem otherItemsMenu = new ContextMenuItem("Other operations");
            MenuHelper.CreateSubmenuForCommandsWithoutScope(otherItemsMenu);
            ExolutioCanvas.ContextMenu.Items.Add(otherItemsMenu);
#endif            
            
            return withoutViewHelpers;
        }

        protected override void Current_SelectionChanged()
        {
            base.Current_SelectionChanged();
        }

        protected override void Current_SelectComponents(IEnumerable<Component> components)
        {
            base.Current_SelectComponents(components);

            bool clearThis = components.Any(c => c.Schema == this.Diagram.Schema);
            foreach (ComponentViewBase componentViewBase in RepresentantsCollection.Values)
            {
                PIMClassView pimClassView = componentViewBase as PIMClassView;
                if (pimClassView != null)
                {
                    foreach (PIMAttributeTextBox pimAttributeTextBox in pimClassView)
                    {
                        if (components.Contains(pimAttributeTextBox.PIMAttribute))
                        {
                            pimAttributeTextBox.Selected = true;
                        }
                        else if (clearThis)
                        {
                            pimAttributeTextBox.Selected = false; 
                        }
                    }
                }
            }
        }
    }
}