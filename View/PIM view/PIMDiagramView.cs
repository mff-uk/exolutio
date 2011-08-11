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

            this.RepresentantsCollection.Registrations.Add(typeof(PIMGeneralization), new RepresentantsCollection.RegistrationClass(
                () => new PIMGeneralizationView(),
                () => new PIMGeneralizationViewHelper(this.Diagram),
                typeof(PIMGeneralization),
                typeof(ConnectionViewHelper),
                typeof(PIMGeneralizationView)));
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
    }
}