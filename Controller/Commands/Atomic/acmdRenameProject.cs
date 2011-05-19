using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EvoX.Controller.Commands;
using EvoX.Model.PSM;
using EvoX.Model;

namespace EvoX.Controller.Commands.Atomic
{
    [PublicCommand("Rename project", PublicCommandAttribute.EPulicCommandCategory.Common_atomic)]
    public class acmdRenameProject : StackedCommand
    {
        [PublicArgument("New name", ModifiedPropertyName = "Name")] 
        public string NewName { get; set; }
        
        private string oldname;

        public acmdRenameProject()
        {
            
        }

        public acmdRenameProject(Controller c, string name)
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
            oldname = Project.Name;
            Project.Name = NewName;
            Report = new CommandReport(CommandReports.PROJECT_RENAMED, oldname, NewName);
        }

        internal override CommandBase.OperationResult UndoOperation()
        {
            Project.Name = oldname;
            return OperationResult.OK;
        }
    }
}
