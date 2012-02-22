using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Controller.Commands;
using Exolutio.Model.PSM;
using Exolutio.Model;

namespace Exolutio.Controller.Commands.Atomic.PSM
{
    /// <summary>
    /// Atomic operation that deletes the content model and moves its associations to a parent PSMAssociationMember
    /// </summary>
    [PublicCommand("Delete PSM content model", PublicCommandAttribute.EPulicCommandCategory.PSM_atomic)]
    public class cmdDeletePSMContentModel : MacroCommand
    {
        [PublicArgument("Content Model", typeof(PSMContentModel))]
        [Scope(ScopeAttribute.EScope.PSMContentModel)]
        public Guid ContentModelGuid { get; set; }

        public cmdDeletePSMContentModel() { }

        public cmdDeletePSMContentModel(Controller c)
            : base(c) { }

        public void Set(Guid psmContentModelGuid)
        {
            ContentModelGuid = psmContentModelGuid;
            
        }

        internal override void GenerateSubCommands()
        {
            //TODO: what if the content model was empty and is part of another content model with its last association?
            Commands.Add(new acmdDeletePSMContentModel(Controller, ContentModelGuid));
        }

        public override bool CanExecute()
        {
            return ContentModelGuid != Guid.Empty;
        }
    }
}
