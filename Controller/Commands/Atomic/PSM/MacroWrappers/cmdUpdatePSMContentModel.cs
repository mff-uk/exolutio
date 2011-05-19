using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EvoX.Controller.Commands;
using EvoX.Model.PSM;
using EvoX.Model;

namespace EvoX.Controller.Commands.Atomic.PSM.MacroWrappers
{
    /// <summary>
    /// Atomic operation that updates the content model's type
    /// </summary>
    [PublicCommand("Update PSM content model type", PublicCommandAttribute.EPulicCommandCategory.PSM_atomic)]
    public class cmdUpdatePSMContentModel : MacroCommand
    {
        [PublicArgument("Content Model", typeof(PSMContentModel))]
        [Scope(ScopeAttribute.EScope.PSMContentModel)]
        public Guid CmodelGuid { get; set; }

        [PublicArgument("Type", ModifiedPropertyName = "Type")]
        public PSMContentModelType Type { get; set; }

        public cmdUpdatePSMContentModel() { }

        public cmdUpdatePSMContentModel(Controller c)
            : base(c) { }

        public void Set(Guid psmContentModelGuid, PSMContentModelType type)
        {
            CmodelGuid = psmContentModelGuid;
            Type = type;
            
        }

        protected override void GenerateSubCommands()
        {
            Commands.Add(new acmdUpdatePSMContentModel(Controller, CmodelGuid, Type));
        }

    }
}
