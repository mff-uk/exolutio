using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EvoX.Controller.Commands;
using EvoX.Model.PSM;
using EvoX.Model;

namespace EvoX.Controller.Commands.Atomic
{
    [PublicCommand("Rename PSM schema", PublicCommandAttribute.EPulicCommandCategory.PSM_atomic)]
    public class acmdRenamePSMSchema : StackedCommand
    {
        [PublicArgument("Schema", typeof(PSMSchema))]
        [Scope(ScopeAttribute.EScope.PSMSchema)]
        public Guid SchemaGuid { get; set; }

        [PublicArgument("New name", ModifiedPropertyName = "Name")]
        public string NewName { get; set; }

        private string oldName;

        public acmdRenamePSMSchema()
        {

        }

        public acmdRenamePSMSchema(Controller c, string name)
            : base(c)
        {
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
