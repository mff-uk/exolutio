using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using EvoX.Controller.Commands;
using EvoX.Model;
using EvoX.Model.PIM;
using EvoX.Model.ViewHelper;
using EvoX.ViewToolkit;
using EvoX.SupportingClasses;

namespace EvoX.View
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
            EvoXContextMenu diagramMenu = MenuHelper.GetContextMenu(ScopeAttribute.EScope.PIMDiagram, this.Diagram);
            EvoXCanvas.ContextMenu = diagramMenu;

            ((EvoXContextMenu)EvoXCanvas.ContextMenu).ScopeObject = PIMDiagram;
            ((EvoXContextMenu)EvoXCanvas.ContextMenu).Diagram = PIMDiagram;

#if SILVERLIGHT
            ContextMenuService.SetContextMenu(EvoXCanvas, diagramMenu);
            MenuHelper.CreateSubmenuForCommandsWithoutScope(diagramMenu);
#else
            ContextMenuItem otherItemsMenu = new ContextMenuItem("Other operations");
            MenuHelper.CreateSubmenuForCommandsWithoutScope(otherItemsMenu);
            EvoXCanvas.ContextMenu.Items.Add(otherItemsMenu);
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
                        else
                        {
                            pimAttributeTextBox.Selected = false; 
                        }
                    }
                }
            }
        }
    }
}