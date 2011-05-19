using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EvoX.Model;
using EvoX.Model.PSM;
using EvoX.Controller.Commands.Atomic;
using EvoX.Controller.Commands.Atomic.PSM;

namespace EvoX.Controller.Commands.Complex.PSM
{
    /// <summary>
    /// DOES NOT DELETE PARENT PSM ASSOCIATION
    /// </summary>
    public class cmdDeletePSMClass : MacroCommand
    {
        public Guid ClassGuid { get; set; }

        public cmdDeletePSMClass()
        {
            CheckFirstOnlyInCanExecute = true;
        }

        public cmdDeletePSMClass(Controller c)
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
            PSMClass psmClass = Project.TranslateComponent<PSMClass>(ClassGuid);
            foreach (PSMAttribute a in psmClass.PSMAttributes)
            {
                cmdDeletePSMAttribute da = new cmdDeletePSMAttribute(Controller);
                da.Set(a);
                Commands.Add(da);
            }
            foreach (PSMAssociation a in psmClass.ChildPSMAssociations)
            {
                cmdDeletePSMAssociation da = new cmdDeletePSMAssociation(Controller);
                da.Set(a);
                Commands.Add(da);
            }

            Commands.Add(new acmdSetPSMClassInterpretation(Controller, ClassGuid, Guid.Empty));
            Commands.Add(new acmdRenameComponent(Controller, ClassGuid, ""));
            Commands.Add(new acmdDeletePSMClass(Controller, ClassGuid));
        }

        public override bool CanExecute()
        {
            if (ClassGuid == Guid.Empty) return false;
            return base.CanExecute();
        }
        
    }
}
