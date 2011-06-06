using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Model;
using Exolutio.Model.PIM;
using Exolutio.Model.PSM;
using Exolutio.Controller.Commands.Atomic;
using Exolutio.Controller.Commands.Atomic.PSM;

namespace Exolutio.Controller.Commands.Complex.PSM
{
    [PublicCommand("Split PSM association (complex)", PublicCommandAttribute.EPulicCommandCategory.PSM_complex)]
    public class cmdSplitPSMAssociation : MacroCommand
    {
        [PublicArgument("PSM association", typeof(PSMAssociation))]
        [Scope(ScopeAttribute.EScope.PSMAssociation)]
        public Guid PSMAssociationGuid { get; set; }

        public Guid NewAssociationGuid { get; set; }

        public Guid NewClassGuid { get; set; }

        public cmdSplitPSMAssociation()
        {
            CheckFirstOnlyInCanExecute = true;
        }

        public cmdSplitPSMAssociation(Controller c)
            : base(c)
        {
            CheckFirstOnlyInCanExecute = true;
        }

        public void Set(Guid psmAssociationGuid, Guid newAssociationGuid, Guid newClassGuid)
        {
            NewClassGuid = newClassGuid;
            PSMAssociationGuid = psmAssociationGuid;
            NewAssociationGuid = newAssociationGuid;
        }

        protected override void GenerateSubCommands()
        {
            if (NewAssociationGuid == Guid.Empty) NewAssociationGuid = Guid.NewGuid();
            if (NewClassGuid == Guid.Empty) NewClassGuid = Guid.NewGuid();

            PSMAssociation original = Project.TranslateComponent<PSMAssociation>(PSMAssociationGuid);
            PSMClass originalChild = original.Child as PSMClass;

            Commands.Add(new acmdNewPSMClass(Controller, original.PSMSchema) { ClassGuid = NewClassGuid });
            Commands.Add(new acmdRenameComponent(Controller, NewClassGuid, originalChild.Name + "2"));
            Commands.Add(new acmdSetPSMClassInterpretation(Controller, NewClassGuid, originalChild.Interpretation));
            Commands.Add(new acmdSetRepresentedClass(Controller, NewClassGuid, originalChild));
            
            Commands.Add(new acmdNewPSMAssociation(Controller, original.Parent, NewClassGuid, original.PSMSchema) { AssociationGuid = NewAssociationGuid });
            Commands.Add(new acmdRenameComponent(Controller, NewAssociationGuid, original.Name + "2"));
            Commands.Add(new acmdUpdatePSMAssociationCardinality(Controller, NewAssociationGuid, original.Lower, original.Upper));
            if ((original.Parent is PSMContentModel) || ((original.Parent is PSMClass) && original.Parent.Interpretation == null))
            {
                PSMClass nic = original.Parent.NearestInterpretedParentClass();
                if (nic == null)
                {
                    Commands.Add(new cmdReconnectPSMAssociation(Controller) { AssociationGuid = original, NewParentGuid = original.PSMSchema.PSMSchemaClass });
                    Commands.Add(new cmdReconnectPSMAssociation(Controller) { AssociationGuid = NewAssociationGuid, NewParentGuid = original.PSMSchema.PSMSchemaClass });
                }
                else
                {
                    Commands.Add(new cmdReconnectPSMAssociation(Controller) { AssociationGuid = original, NewParentGuid = nic });
                    Commands.Add(new cmdReconnectPSMAssociation(Controller) { AssociationGuid = NewAssociationGuid, NewParentGuid = nic });
                }
            }
            
            Commands.Add(new acmdSynchroPSMAssociations(Controller) { X1 = Enumerable.Repeat(original.ID, 1).ToList(), X2 = Enumerable.Repeat(NewAssociationGuid, 1).ToList() });

            if ((original.Parent is PSMContentModel) || ((original.Parent is PSMClass) && original.Parent.Interpretation == null))
            {
                Commands.Add(new cmdReconnectPSMAssociation(Controller) { AssociationGuid = original, NewParentGuid = original.Parent });
                Commands.Add(new cmdReconnectPSMAssociation(Controller) { AssociationGuid = NewAssociationGuid, NewParentGuid = original.Parent });
            }
        }

        public override bool CanExecute()
        {
            if (PSMAssociationGuid == null) return false;
            PSMAssociation original = Project.TranslateComponent<PSMAssociation>(PSMAssociationGuid);
            if (!(original.Child is PSMClass)) return false;
            return base.CanExecute();
        }

        internal override void CommandOperation()
        {
            base.CommandOperation();
            Report = new CommandReport(CommandReports.COMPLEX_SPLIT_PSM_ASSOCIATION);
        }

    }
}
