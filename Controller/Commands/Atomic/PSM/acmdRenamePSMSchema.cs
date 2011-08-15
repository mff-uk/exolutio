using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Controller.Commands;
using Exolutio.Model.PSM;
using Exolutio.Model;

namespace Exolutio.Controller.Commands.Atomic
{
    internal class acmdRenamePSMSchema : StackedCommand
    {
        public Guid SchemaGuid { get; set; }

        public string NewName { get; set; }

        private string oldName;

        public acmdRenamePSMSchema()
        {

        }

        public acmdRenamePSMSchema(Controller c, Guid schemaGuid, string name)
            : base(c)
        {
            SchemaGuid = schemaGuid;
            NewName = name;
        }

        public override bool CanExecute()
        {
            return NewName != null;
        }

        internal override void CommandOperation()
        {
            PSMSchema s = Project.TranslateComponent<PSMSchema>(SchemaGuid);
            oldName = s.Caption;
            s.Caption = NewName;
            PSMDiagram psmDiagram = s.PSMDiagram;
            if (psmDiagram != null && psmDiagram.Caption == oldName)
            {
                psmDiagram.Caption = NewName;
            }
            Report = new CommandReport(CommandReports.SCHEMA_RENAMED, oldName, NewName);
        }

        internal override CommandBase.OperationResult UndoOperation()
        {
            PSMSchema s = Project.TranslateComponent<PSMSchema>(SchemaGuid);
            s.Caption = oldName;
            PSMDiagram psmDiagram = s.PSMDiagram;
            if (psmDiagram != null && psmDiagram.Caption == NewName)
            {
                psmDiagram.Caption = oldName;
            }
            return OperationResult.OK;
        }
    }
}
