using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Controller.Commands;
using Exolutio.Model;
using Exolutio.Model.PIM;

namespace Exolutio.Controller.Commands.Atomic.PIM
{
    internal class acmdShiftPIMAttribute : AtomicCommand
    {
        Guid attributeGuid = Guid.Empty;
        bool up = true;

        public acmdShiftPIMAttribute(Controller c, Guid psmAttributeGuid, bool shiftUp)
            : base(c)
        {
            attributeGuid = psmAttributeGuid;
            up = shiftUp;
        }

        public override bool CanExecute()
        {
            return Project.TranslateComponent<PIMAttribute>(attributeGuid).PIMClass.PIMAttributes.Count > 1;
        }

        internal override void CommandOperation()
        {
            PIMAttribute a = Project.TranslateComponent<PIMAttribute>(attributeGuid);
            PIMClass pimClass = a.PIMClass;
            int index = pimClass.PIMAttributes.IndexOf(a);
            int count = pimClass.PIMAttributes.Count;

            if (up) index = (index + count - 1) % count;
            else index = (index + 1) % count;
            pimClass.PIMAttributes.Remove(a);
            pimClass.PIMAttributes.Insert(a, index);

            Report = new CommandReport(CommandReports.PIM_ATTR_SHIFT, a, up ? "up" : "down");
        }

        internal override CommandBase.OperationResult UndoOperation()
        {
            PIMAttribute a = Project.TranslateComponent<PIMAttribute>(attributeGuid);
            PIMClass pimClass = a.PIMClass;
            int index = pimClass.PIMAttributes.IndexOf(a);
            int count = pimClass.PIMAttributes.Count;

            if (up) index = (index + 1) % count;
            else index = (index - 1) % count;
            pimClass.PIMAttributes.Remove(a);
            pimClass.PIMAttributes.Insert(a, index);
            return OperationResult.OK;
        }
    }
}
