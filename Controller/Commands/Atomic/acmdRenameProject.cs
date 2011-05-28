using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Controller.Commands;
using Exolutio.Model.PSM;
using Exolutio.Model;

namespace Exolutio.Controller.Commands.Atomic
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
