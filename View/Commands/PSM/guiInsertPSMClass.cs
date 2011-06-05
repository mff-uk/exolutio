using System;
using Exolutio.Dialogs;
using Exolutio.ResourceLibrary;
using Exolutio.Model.PSM;
using System.Linq;
using System.Collections.Generic;
using Exolutio.Controller.Commands.Complex.PSM;
using Exolutio.Controller.Commands;
using Exolutio.Controller.Commands.Atomic.PSM;

namespace Exolutio.View.Commands.PSM
{
    public class guiInsertPSMClass : guiSelectionDependentCommand
    {
        public override bool CanExecute(object parameter)
        {
            if (!(Current.ActiveDiagram is PSMDiagram)) return false;

            IEnumerable<PSMContentModel> selectedContentModels = Current.ActiveDiagramView.GetSelectedComponents()
                .Where(c => c is PSMContentModel).Cast<PSMContentModel>();
            
            IEnumerable<PSMAssociation> selectedAssociations = Current.ActiveDiagramView.GetSelectedComponents()
                .Where(c => c is PSMAssociation).Cast<PSMAssociation>();
            if (selectedContentModels.Count() > 0 && selectedAssociations.Count() == 0) return true;
            else if (selectedContentModels.Count() == 0 && selectedAssociations.Count() > 0)
            {
                PSMAssociationMember parent = selectedAssociations.First().Parent;
                if (selectedAssociations.Any(a => a.Parent != parent)) return false;
                return true;
            }
            else return false;
        }

        public override void Execute(object parameter)
        {
            IEnumerable<PSMContentModel> selectedContentModels = Current.ActiveDiagramView.GetSelectedComponents()
                .Where(c => c is PSMContentModel).Cast<PSMContentModel>();
            if (selectedContentModels.Count() > 0)
            {
                MacroCommand macro = new MacroCommand(Current.Controller);
                foreach (PSMContentModel cm in selectedContentModels)
                {
                    Guid newClassGuid = new Guid();
                    macro.Commands.Add(new cmdContentToPSMClass(Current.Controller)
                        {
                            Associations = cm.ChildPSMAssociations.Select(assoc => assoc.ID).ToList(),
                            ParentAssociationMemberGuid = cm,
                            ClassGuid = newClassGuid
                        });
                    macro.Commands.Add(new cmdLeaveOutUninterpretedAssociationMember(Current.Controller)
                        {
                            AssociationMemberGuid = cm
                        });
                }
                macro.Execute();
            }
            else
            {
                IEnumerable<PSMAssociation> selectedAssociations = Current.ActiveDiagramView.GetSelectedComponents()
                    .Where(c => c is PSMAssociation).Cast<PSMAssociation>();
                PSMAssociationMember parent = selectedAssociations.First().Parent;
                cmdContentToPSMClass command = new cmdContentToPSMClass(Current.Controller);
                command.Set(parent, selectedAssociations.Select(a => a.ID), Guid.NewGuid(), Guid.NewGuid());
                command.Execute();
            }
        }

        public override string Text
        {
            get
            {
                return "Class";
            }
        }

        public override string ScreenTipText
        {
            get { return "Contain selected associations in a class"; }
        }

        public override System.Windows.Media.ImageSource Icon
        {
            get
            {
                return ExolutioResourceNames.GetResourceImageSource(ExolutioResourceNames.ContentContainer);
            }
        }
    }
}