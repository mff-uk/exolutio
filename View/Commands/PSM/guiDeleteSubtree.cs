using System;
using Exolutio.Dialogs;
using Exolutio.ResourceLibrary;
using Exolutio.Model.PSM;
using Exolutio.Model;
using System.Linq;
using System.Collections.Generic;
using Exolutio.Controller.Commands.Complex.PSM;
using Exolutio.Controller.Commands;
using Exolutio.Controller.Commands.Atomic.PSM;
using System.Diagnostics;

namespace Exolutio.View.Commands.PSM
{
    public class guiDeleteSubtree : guiSelectionDependentCommand
    {
        public override bool CanExecute(object parameter)
        {
            if (!(Current.ActiveDiagram is PSMDiagram)) return false;

            if (Current.ActiveDiagramView.IsSelectedComponentOfType(typeof(PSMSchemaClass))
                || Current.ActiveDiagramView.IsSelectedComponentOfType(typeof(PSMAttribute))) return false;

            if (Current.ActiveDiagramView.GetSelectedComponents().Count() > 0) return true;

            return false;

        }

        public override void Execute(object parameter)
        {
            IEnumerable<PSMComponent> components = Current.ActiveDiagramView.GetSelectedComponents().Cast<PSMComponent>();
            bool found = true;
            IEnumerable<PSMComponent> current = components;
            while (found)
            {
                found = false;
                List<PSMComponent> next = new List<PSMComponent>();
                foreach (PSMComponent c in current)
                {
                    if (current.Any(co => co != c && c.IsDescendantFrom(co))) found = true;
                    else next.Add(c);
                }
                current = next;
            }

            IEnumerable<PSMAssociation> associations = current.Where(c => c is PSMAssociation).Cast<PSMAssociation>();
            IEnumerable<PSMAssociationMember> associationMembers = current.Where(c2 => (!(c2 is PSMSchemaClass) && (c2 is PSMAssociationMember))).Cast<PSMAssociationMember>();
            IEnumerable<PSMAssociationMember> roots = associationMembers.Where(am => am.ParentAssociation == null);
            IEnumerable<PSMAssociation> nonrootAssociations = associationMembers.Where(am => am.ParentAssociation != null).Select(am2 => am2.ParentAssociation);
            IEnumerable<PSMAttribute> attributes = current.Where(c => c is PSMAttribute).Cast<PSMAttribute>();
            MacroCommand macro = new MacroCommand(Current.Controller);
            foreach (PSMAssociation a in associations.Union(nonrootAssociations))
            {
                macro.Commands.Add(new cmdDeletePSMAssociationRecursive(Current.Controller) { AssociationGuid = a });
            }
            foreach (PSMAssociationMember am in roots)
            {
                macro.Commands.Add(new cmdDeletePSMAssociationMemberRecursive(Current.Controller) { AssociationMemberGuid = am });
            }
            foreach (PSMAttribute a in attributes)
            {
                macro.Commands.Add(new cmdDeletePSMAttribute(Current.Controller) { AttributeGuid = a });
            }
            
            macro.Execute();
        }

        public override string Text
        {
            get
            {
                return "Delete subtrees";
            }
        }

        public override string ScreenTipText
        {
            get { return "Delete PSM subtrees"; }
        }

        public override System.Windows.Media.ImageSource Icon
        {
            get
            {
                return ExolutioResourceNames.GetResourceImageSource(ExolutioResourceNames.delete);
            }
        }
    }
}