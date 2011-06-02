using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Model;
using Exolutio.Model.PSM;
using Exolutio.Controller.Commands.Atomic;
using Exolutio.Controller.Commands.Atomic.PSM;

namespace Exolutio.Controller.Commands.Complex.PSM
{
    /// <summary>
    /// Deletes root PSM content model and its content associations
    /// </summary>
    [PublicCommand("Delete root PSM content model and its content associations (complex)", PublicCommandAttribute.EPulicCommandCategory.PSM_complex)]
    public class cmdDeleteRootPSMContentModel : MacroCommand
    {
        [PublicArgument("Deleted PSM content model", typeof(PSMContentModel))]
        [Scope(ScopeAttribute.EScope.PSMContentModel)]
        public Guid ContentModelGuid { get; set; }

        public cmdDeleteRootPSMContentModel()
        {
            CheckFirstOnlyInCanExecute = true;
        }

        public cmdDeleteRootPSMContentModel(Controller c)
            : base(c)
        {
            CheckFirstOnlyInCanExecute = true;
        }

        public void Set(Guid classGuid)
        {
            ContentModelGuid = classGuid;
            
        }

        protected override void GenerateSubCommands()
        {
            PSMContentModel psmContentModel = Project.TranslateComponent<PSMContentModel>(ContentModelGuid);
            foreach (PSMAssociation a in psmContentModel.ChildPSMAssociations)
            {
                cmdDeletePSMAssociation da = new cmdDeletePSMAssociation(Controller);
                da.Set(a);
                Commands.Add(da);
            }

            Commands.Add(new acmdDeletePSMContentModel(Controller, ContentModelGuid));
        }

        public override bool CanExecute()
        {
            if (ContentModelGuid == Guid.Empty) return false;
            if (Project.TranslateComponent<PSMContentModel>(ContentModelGuid).ParentAssociation != null)
            {
                ErrorDescription = CommandErrors.CMDERR_CM_NOT_ROOT;
                return false;
            }
            return base.CanExecute();
        }

        internal override void CommandOperation()
        {
            base.CommandOperation();
            Report = new CommandReport(CommandReports.COMPLEX_DELETE_PSM_CM);
        }

    }
}
