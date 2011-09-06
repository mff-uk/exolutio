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
    public class guiPSMDelete : guiSelectionDependentCommand
    {
        public override bool CanExecute(object parameter)
        {
            if (!(Current.ActiveDiagram is PSMDiagram)) return false;

            if (Current.ActiveDiagramView.IsSelectedComponentOfType(typeof(PSMSchemaClass))) return false;

            if (Current.ActiveDiagramView.GetSelectedComponents().Count() > 0) return true;

            return false;

        }

        public override void Execute(object parameter)
        {
            IEnumerable<PSMComponent> components = Current.ActiveDiagramView.GetSelectedComponents().Where(c => !(c is PSMSchemaClass)).Cast<PSMComponent>();

            IEnumerable<PSMAssociation> associations = components.Where(c => c is PSMAssociation).Cast<PSMAssociation>();
            IEnumerable<PSMAssociationMember> associationMembers = components.Where(c2 => (!(c2 is PSMSchemaClass) && (c2 is PSMAssociationMember))).Cast<PSMAssociationMember>();
            IEnumerable<PSMAssociation> nonrootAssociations = associationMembers.Where(am => am.ParentAssociation != null).Select(am2 => am2.ParentAssociation);
            IEnumerable<PSMAttribute> attributes = components.Where(c => c is PSMAttribute).Cast<PSMAttribute>();
            IEnumerable<PSMGeneralization> generalizations = components.Where(c => c is PSMGeneralization).Cast<PSMGeneralization>();
            MacroCommand macro = new MacroCommand(Current.Controller) { CheckFirstOnlyInCanExecute = true };
            foreach (PSMGeneralization g in generalizations)
            {
                macro.Commands.Add(new Exolutio.Controller.Commands.Atomic.PSM.MacroWrappers.cmdDeletePSMGeneralization(Current.Controller) { GeneralizationGuid = g });
            }
            foreach (PSMAssociation a in associations.Union(nonrootAssociations))
            {
                macro.Commands.Add(new cmdDeletePSMAssociation(Current.Controller) { AssociationGuid = a });
            }
            foreach (PSMAssociationMember am in associationMembers)
            {
                if (am is PSMContentModel)
                {
                    macro.Commands.Add(new cmdDeleteRootPSMContentModel(Current.Controller) { ContentModelGuid = am });
                }
                else
                {
                    macro.Commands.Add(new cmdDeleteRootPSMClass(Current.Controller) { ClassGuid = am });
                }
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
                return "Delete";
            }
        }

        public override string ScreenTipText
        {
            get { return "Delete PSM components"; }
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