using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EvoX.Model;
using EvoX.Model.PIM;
using EvoX.Controller.Commands.Atomic;
using EvoX.Controller.Commands.Atomic.PIM;

namespace EvoX.Controller.Commands.Complex.PIM
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
            Commands.Add(new acmdDeletePIMClass(Controller, ClassGuid));
        }

        public override bool CanExecute()
        {
            if (ClassGuid == Guid.Empty) return false;
            return base.CanExecute();
        }
        
    }
}
