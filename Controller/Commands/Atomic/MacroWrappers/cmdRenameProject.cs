using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Controller.Commands;
using Exolutio.Model.PIM;
using Exolutio.Model;
using Exolutio.Model.PSM;
using Exolutio.Controller.Commands.Atomic.PSM;

namespace Exolutio.Controller.Commands.Atomic.MacroWrappers
{
    [PublicCommand("Rename project", PublicCommandAttribute.EPulicCommandCategory.Common_atomic)]
    public class cmdRenameProject : MacroCommand
    {
        [PublicArgument("New name", ModifiedPropertyName = "Name")]
        public string NewName { get; set; }

        public cmdRenameProject()
        {
            CheckFirstOnlyInCanExecute = true;
        }

        public cmdRenameProject(Controller c)
            : base(c)
        {
            CheckFirstOnlyInCanExecute = true;
        }

        public void Set(string newName)
        {
            NewName = newName;
        }

        protected override void GenerateSubCommands()
        {
            Commands.Add(new acmdRenameProject(Controller, NewName));
        }
    }
}
