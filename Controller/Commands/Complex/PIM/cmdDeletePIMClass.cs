using System;
using System.Collections.Generic;
using System.Linq;
using Exolutio.Model;
using Exolutio.Model.PIM;
using Exolutio.Controller.Commands.Atomic;
using Exolutio.Controller.Commands.Atomic.PIM;

namespace Exolutio.Controller.Commands.Complex.PIM
{
    [PublicCommand("Delete PIM class (complex)", PublicCommandAttribute.EPulicCommandCategory.PIM_complex)]
    public class cmdDeletePIMClass : MacroCommand
    {
        [PublicArgument("Deleted class", typeof(PIMClass))]
        [Scope(ScopeAttribute.EScope.PIMClass)]
        public Guid ClassGuid { get; set; }
        
        public cmdDeletePIMClass()
        {
            CheckFirstOnlyInCanExecute = true;
        }

        public cmdDeletePIMClass(Controller c)
            : base(c)
        {
            CheckFirstOnlyInCanExecute = true;
        }

        public void Set(Guid classGuid)
        {
            ClassGuid = classGuid;
            
        }

        protected override void GenerateSubCommands()
        {
            PIMClass pimClass = Project.TranslateComponent<PIMClass>(ClassGuid);
            foreach (PIMAttribute a in pimClass.PIMAttributes)
            {
                cmdDeletePIMAttribute da = new cmdDeletePIMAttribute(Controller);
                da.Set(a);
                Commands.Add(da);
            }
            
            List<PIMAssociation> associations = 
                pimClass.PIMAssociationEnds
                .Select<PIMAssociationEnd, PIMAssociation>(e => e.PIMAssociation)
                .Distinct<PIMAssociation>()
                .ToList<PIMAssociation>();
            
            foreach (PIMAssociation a in associations)
            {
                cmdDeletePIMAssociation da = new cmdDeletePIMAssociation(Controller);
                da.Set(a);
                Commands.Add(da);
            }

            Commands.Add(new acmdRenameComponent(Controller, ClassGuid, ""));
            Commands.AddRange(acmdRemoveComponentFromDiagram.CreateCommandsToRemoveFromAllDiagrams(Controller, ClassGuid));
            Commands.Add(new acmdDeletePIMClass(Controller, ClassGuid));
        }

        public override bool CanExecute()
        {
            if (ClassGuid == Guid.Empty) return false;
            return base.CanExecute();
        }

        internal override void CommandOperation()
        {
            base.CommandOperation();
            Report = new CommandReport(CommandReports.COMPLEX_DELETE_PIM_CLASS);
        }

    }
}
