using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Model;
using Exolutio.Model.PSM;

namespace Exolutio.Controller.Commands.Atomic.PSM
{
    public class acmdReorderPSMAttributes : MacroCommand
    {
        public acmdReorderPSMAttributes()
        {
            CheckFirstOnlyInCanExecute = true;
        }

        public acmdReorderPSMAttributes(Controller c)
            : base(c)
        {
            CheckFirstOnlyInCanExecute = true;
        }

        protected override void GenerateSubCommands()
        {
           
        }

        public override bool CanExecute()
        {
            return base.CanExecute();
        }

        internal override void CommandOperation()
        {
            base.CommandOperation();
            Report = new CommandReport(CommandReports.COMPLEX_UPDATE_PSM_ATTRIBUTE);
        }
    }
}
