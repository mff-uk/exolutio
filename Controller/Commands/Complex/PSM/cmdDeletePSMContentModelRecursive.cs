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
    /// Deletes the subtree rooted in this root content model
    /// </summary>
    [PublicCommand("Delete root PSM content model (recursive)", PublicCommandAttribute.EPulicCommandCategory.PSM_atomic)]
    public class cmdDeletePSMContentModelRecursive : ComposedCommand
    {
        [PublicArgument("Content Model", typeof(PSMContentModel))]
        [Scope(ScopeAttribute.EScope.PSMContentModel)]
        public Guid ContentModelGuid { get; set; }
        
        public cmdDeletePSMContentModelRecursive()
        {
            CheckFirstOnlyInCanExecute = true;
        }

        public cmdDeletePSMContentModelRecursive(Controller c)
            : base(c)
        {
            CheckFirstOnlyInCanExecute = true;
        }

        public void Set(Guid contentModelGuid)
        {
            ContentModelGuid = contentModelGuid;
            
        }

        protected override void GenerateSubCommands()
        {
            PSMContentModel contentModel = Project.TranslateComponent<PSMContentModel>(ContentModelGuid);
            foreach (PSMAssociation a in contentModel.ChildPSMAssociations)
            {
                cmdDeletePSMAssociationRecursive da = new cmdDeletePSMAssociationRecursive(Controller);
                da.Set(a);
                Commands.Add(da);
            }
            Commands.Add(new acmdDeletePSMContentModel(Controller, contentModel));
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
            Report = new CommandReport(CommandReports.COMPLEX_DELETE_PSM_CM_RECURSIVE);
        }

    }
}
