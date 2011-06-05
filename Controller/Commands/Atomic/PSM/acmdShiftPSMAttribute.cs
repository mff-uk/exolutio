using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Controller.Commands;
using Exolutio.Model.PSM;
using Exolutio.Model;

namespace Exolutio.Controller.Commands.Atomic.PSM
{
    public class acmdShiftPSMAttribute : StackedCommand
    {
        Guid attributeGuid = Guid.Empty;
        bool up = true;

        public acmdShiftPSMAttribute(Controller c, Guid psmAttributeGuid, bool shiftUp)
            : base(c)
        {
            attributeGuid = psmAttributeGuid;
            up = shiftUp;
        }

        public override bool CanExecute()
        {
            return Project.TranslateComponent<PSMAttribute>(attributeGuid).PSMClass.PSMAttributes.Count > 1;
        }

        internal override void CommandOperation()
        {
            PSMAttribute a = Project.TranslateComponent<PSMAttribute>(attributeGuid);
            PSMClass psmClass = a.PSMClass;
            int index = psmClass.PSMAttributes.IndexOf(a);
            int count = psmClass.PSMAttributes.Count;

            if (up) index = (index + count - 1) % count;
            else index = (index + 1) % count;
            psmClass.PSMAttributes.Remove(a);
            psmClass.PSMAttributes.Insert(a, index);

            Report = new CommandReport(CommandReports.PSM_ATTR_SHIFT, a, up ? "up" : "down");
        }

        internal override CommandBase.OperationResult UndoOperation()
        {
            PSMAttribute a = Project.TranslateComponent<PSMAttribute>(attributeGuid);
            PSMClass psmClass = a.PSMClass;
            int index = psmClass.PSMAttributes.IndexOf(a);
            int count = psmClass.PSMAttributes.Count;

            if (up) index = (index + 1) % count;
            else index = (index - 1) % count;
            psmClass.PSMAttributes.Remove(a);
            psmClass.PSMAttributes.Insert(a, index);
            return OperationResult.OK;
        }
    }
}
