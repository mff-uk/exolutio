using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EvoX.Controller.Commands;
using EvoX.Model.PSM;
using EvoX.Model;
using EvoX.Model.PIM;
using EvoX.Controller.Commands.Complex.PIM;

namespace EvoX.Controller.Commands.Atomic.PSM
{
    public class acmdSynchroPSMAttributes : StackedCommand
    {
        public List<Guid> X1 = new List<Guid>();
        public List<Guid> X2 = new List<Guid>();

        public acmdSynchroPSMAttributes(Controller c)
            : base(c) { }

        public override bool CanExecute()
        {
            List<PSMAttribute> aX1 = X1.Select<Guid, PSMAttribute>(G => Project.TranslateComponent<PSMAttribute>(G)).ToList<PSMAttribute>();
            List<PSMAttribute> aX2 = X2.Select<Guid, PSMAttribute>(G => Project.TranslateComponent<PSMAttribute>(G)).ToList<PSMAttribute>();
            if (aX1.Count == 0 && aX2.Count == 0)
            {
                ErrorDescription = CommandErrors.CMDERR_CANNOT_SYNCHRO_EMPTY_SETS;
                return false;
            }
            PSMClass psmClass = aX1.Count == 0 ? aX2[0].PSMClass : aX1[0].PSMClass;
            if (!aX1.All<PSMAttribute>(a => a.PSMClass == psmClass) || !aX2.All<PSMAttribute>(a => a.PSMClass == psmClass))
            {
                ErrorDescription = CommandErrors.CMDERR_CANNOT_SYNCHRO_ATTS_DIFFERENT_CLASSES;
                return false;
            }
            return true;
        }

        internal override MacroCommand PostPropagation()
        {
            MacroCommand command = new MacroCommand(Controller) { CheckFirstOnlyInCanExecute = true };
            command.Report = new CommandReport("Post-propagation (synchronize PSM attribute sets)");
            IEnumerable<PSMAttribute> aX1 = X1.Select<Guid, PSMAttribute>(G => Project.TranslateComponent<PSMAttribute>(G));
            IEnumerable<PSMAttribute> aX2 = X2.Select<Guid, PSMAttribute>(G => Project.TranslateComponent<PSMAttribute>(G));
            if (aX1.Count() == 0 && aX2.Count() == 0) return command;
            PSMClass psmClass = aX1.Count() == 0 ? aX2.First().PSMClass : aX1.First().PSMClass;

            if (aX1.Any(att => att.Interpretation == null) && aX2.Any(att => att.Interpretation == null)) return command;
            
            //Twice... X1 => X2, X2 => X1
            for (int i = 0; i < 2; i++)
            {
                if (aX1.All(att => att.Interpretation != null))
                {
                    PIMClass pimClass = (aX1.First().Interpretation as PIMAttribute).PIMClass;
                
                    IEnumerable<PSMAttribute> attributesToPropagate = aX2.Where(a => a.Interpretation == null);

                    foreach (PSMAttribute a in attributesToPropagate)
                    {
                        cmdCreateNewPIMAttribute c = new cmdCreateNewPIMAttribute(Controller);
                        Guid attrGuid = Guid.NewGuid();
                        c.AttributeGuid = attrGuid;
                        c.Set(pimClass, a.AttributeType, a.Name, a.Lower, a.Upper, a.DefaultValue);
                        command.Commands.Add(c);

                        acmdSetInterpretation cmdi = new acmdSetPSMAttributeInterpretation(Controller, a, attrGuid);
                        command.Commands.Add(cmdi);
                    }
                }
                //Swap the two lists and do it again
                IEnumerable<PSMAttribute> temp = aX1;
                aX1 = aX2;
                aX2 = temp;
            }
            return command;
        }
    }
}
