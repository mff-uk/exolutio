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
    public class cmdSplitPSMAssociation : ComposedCommand
    {
        [PublicArgument("PSM association", typeof(PSMAssociation))]
        [Scope(ScopeAttribute.EScope.PSMAssociation)]
        public Guid PSMAssociationGuid { get; set; }

        public IEnumerable<Tuple<Guid, Guid>> NewGuids { get; set; }

        private uint cnt = 2;
        [PublicArgument("Count")]
        public uint Count { get { return cnt; } set { cnt = value; } }

        public cmdSplitPSMAssociation()
        {
            CheckFirstOnlyInCanExecute = true;
        }

        public cmdSplitPSMAssociation(Controller c)
            : base(c)
        {
            CheckFirstOnlyInCanExecute = true;
        }

        public void Set(Guid psmAssociationGuid, IEnumerable<Tuple<Guid, Guid>> newGuids)
        {
            PSMAssociationGuid = psmAssociationGuid;
            NewGuids = newGuids;
        }

        public void Set(Guid psmAssociationGuid, uint count)
        {
            PSMAssociationGuid = psmAssociationGuid;
            Count = count;
        }

        protected override void GenerateSubCommands()
        {
            if (NewGuids == null)
            {
                List<Tuple<Guid, Guid>> guids = new List<Tuple<Guid, Guid>>();
                for (int i = 0; i < Count; i++)
                {
                    guids.Add(new Tuple<Guid, Guid>(Guid.NewGuid(), Guid.NewGuid()));
                }
                NewGuids = guids;
            }

            PSMAssociation original = Project.TranslateComponent<PSMAssociation>(PSMAssociationGuid);
            PSMClass originalChild = original.Child as PSMClass;

            int counter = 0;
            foreach (Tuple<Guid, Guid> t in NewGuids)
            {
                counter++;
                Guid classGuid = t.Item1;
                Guid associationGuid = t.Item2;
                Commands.Add(new acmdNewPSMClass(Controller, original.PSMSchema) { ClassGuid = classGuid });
                Commands.Add(new acmdRenameComponent(Controller, classGuid, originalChild.Name + counter));
                Commands.Add(new acmdSetPSMClassInterpretation(Controller, classGuid, originalChild.Interpretation));
                Commands.Add(new acmdSetRepresentedClass(Controller, classGuid, originalChild));

                Commands.Add(new acmdNewPSMAssociation(Controller, original.Parent, classGuid, original.PSMSchema) { AssociationGuid = associationGuid });
                Commands.Add(new acmdRenameComponent(Controller, associationGuid, original.Name + counter));
                Commands.Add(new acmdUpdatePSMAssociationCardinality(Controller, associationGuid, original.Lower, original.Upper));
            }

            if ((original.Parent is PSMContentModel) || ((original.Parent is PSMClass) && original.Parent.Interpretation == null))
            {
                PSMClass nic = original.Parent.NearestInterpretedParentClass();
                if (nic == null)
                {
                    Commands.Add(new cmdReconnectPSMAssociation(Controller) { AssociationGuid = original, NewParentGuid = original.PSMSchema.PSMSchemaClass });
                    foreach (Tuple<Guid, Guid> t in NewGuids)
                    {
                        Guid associationGuid = t.Item2;
                        Commands.Add(new cmdReconnectPSMAssociation(Controller) { AssociationGuid = associationGuid, NewParentGuid = original.PSMSchema.PSMSchemaClass });
                    }
                }
                else
                {
                    Commands.Add(new cmdReconnectPSMAssociation(Controller) { AssociationGuid = original, NewParentGuid = nic });
                    foreach (Tuple<Guid, Guid> t in NewGuids)
                    {
                        Guid associationGuid = t.Item2;
                        Commands.Add(new cmdReconnectPSMAssociation(Controller) { AssociationGuid = associationGuid, NewParentGuid = nic });
                    }
                }
            }
            
            Commands.Add(new acmdSynchroPSMAssociations(Controller) { X1 = Enumerable.Repeat(original.ID, 1).ToList(), X2 = NewGuids.Select(t => t.Item2).ToList() });

            if ((original.Parent is PSMContentModel) || ((original.Parent is PSMClass) && original.Parent.Interpretation == null))
            {
                Commands.Add(new cmdReconnectPSMAssociation(Controller) { AssociationGuid = original, NewParentGuid = original.Parent });
                foreach (Tuple<Guid, Guid> t in NewGuids)
                {
                    Guid associationGuid = t.Item2;
                    Commands.Add(new cmdReconnectPSMAssociation(Controller) { AssociationGuid = associationGuid, NewParentGuid = original.Parent });
                }
            }

            Commands.Add(new cmdDeletePSMAssociation(Controller) { AssociationGuid = original });
        }

        public override bool CanExecute()
        {
            if (PSMAssociationGuid == null) return false;
            PSMAssociation original = Project.TranslateComponent<PSMAssociation>(PSMAssociationGuid);
            if (!(original.Child is PSMClass)) return false;
            if (Count < 2 && NewGuids == null) return false;
            return base.CanExecute();
        }

        internal override void CommandOperation()
        {
            base.CommandOperation();
            Report = new CommandReport(CommandReports.COMPLEX_SPLIT_PSM_ASSOCIATION);
        }

    }
}
