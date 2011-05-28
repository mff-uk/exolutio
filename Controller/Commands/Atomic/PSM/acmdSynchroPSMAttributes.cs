using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Controller.Commands;
using Exolutio.Model.PSM;
using Exolutio.Model;
using Exolutio.Model.PIM;
using Exolutio.Controller.Commands.Complex.PIM;
using System.Collections.ObjectModel;
using Exolutio.Controller.Commands.Atomic.PIM;

namespace Exolutio.Controller.Commands.Atomic.PSM
{
    public class acmdSynchroPSMAttributes : StackedCommand
    {
        public List<Guid> X1 = new List<Guid>();
        public List<Guid> X2 = new List<Guid>();

        public acmdSynchroPSMAttributes(Controller c)
            : base(c) { }

        public override bool CanExecute()
        {
            ReadOnlyCollection<PSMAttribute> aX1 = Project.TranslateComponentCollection<PSMAttribute>(X1);
            ReadOnlyCollection<PSMAttribute> aX2 = Project.TranslateComponentCollection<PSMAttribute>(X2);
            if (aX1.Count == 0 || aX2.Count == 0)
            {
                ErrorDescription = CommandErrors.CMDERR_CANNOT_SYNCHRO_EMPTY_SETS;
                return false;
            }
            PSMClass psmClass = aX2[0].PSMClass;
            if (!aX1.All<PSMAttribute>(a => a.PSMClass == psmClass) || !aX2.All<PSMAttribute>(a => a.PSMClass == psmClass))
            {
                ErrorDescription = CommandErrors.CMDERR_CANNOT_SYNCHRO_ATTS_DIFFERENT_CLASSES;
                return false;
            }
            return true;
        }

        internal override void CommandOperation()
        {
            Report = new CommandReport(CommandReports.PSM_ATTR_SYNCHRO,
                String.Concat(Project.TranslateComponentCollection<PSMAttribute>(X1).Select<PSMAttribute, String>(a => a.ToString() + " ")),
                String.Concat(Project.TranslateComponentCollection<PSMAttribute>(X2).Select<PSMAttribute, String>(a => a.ToString() + " ")));
        }

        internal override MacroCommand PostPropagation()
        {
            ReadOnlyCollection<PSMAttribute> aX1 = Project.TranslateComponentCollection<PSMAttribute>(X1);
            ReadOnlyCollection<PSMAttribute> aX2 = Project.TranslateComponentCollection<PSMAttribute>(X2);
            if (aX1.Count == 0 || aX2.Count == 0) return null;
            
            MacroCommand command = new MacroCommand(Controller) { CheckFirstOnlyInCanExecute = true };
            command.Report = new CommandReport("Post-propagation (synchronize PSM attribute sets)");
            PSMClass psmClass = aX2.First().PSMClass;

            if (aX1.Any(att => att.Interpretation == null) && aX2.Any(att => att.Interpretation == null)) return command;
            
            //Twice... X1 => X2, X2 => X1
            for (int i = 0; i < 2; i++)
            {
                if (aX1.All(att => att.Interpretation != null))
                {
                    PIMClass pimClass = (aX1.First().Interpretation as PIMAttribute).PIMClass;
                    IEnumerable<PIMAttribute> interpretations1 = aX1.Select<PSMAttribute, PIMAttribute>(a => a.Interpretation as PIMAttribute);
                    IEnumerable<PIMAttribute> interpretations2 = aX2.Where(a => a.Interpretation != null).Select<PSMAttribute, PIMAttribute>(a => a.Interpretation as PIMAttribute);
                    IEnumerable<PSMAttribute> attributesToPropagate = aX2.Where(a => a.Interpretation == null);

                    List<Guid> newAttributes = new List<Guid>();
                    foreach (PSMAttribute a in attributesToPropagate)
                    {
                        cmdCreateNewPIMAttribute c = new cmdCreateNewPIMAttribute(Controller);
                        Guid attrGuid = Guid.NewGuid();
                        c.AttributeGuid = attrGuid;
                        c.Set(pimClass, a.AttributeType, a.Name, a.Lower, a.Upper, a.DefaultValue);
                        command.Commands.Add(c);

                        acmdSetInterpretation cmdi = new acmdSetPSMAttributeInterpretation(Controller, a, attrGuid);
                        command.Commands.Add(cmdi);

                        newAttributes.Add(attrGuid);
                    }
                    
                    IEnumerable<Guid> synchroGroup1 = pimClass.PIMAttributes.Where(a => interpretations1.Contains(a)).Select<PIMAttribute, Guid>(g => g);
                    IEnumerable<Guid> synchroGroup2 = pimClass.PIMAttributes.Where(a => interpretations2.Contains(a)).Select<PIMAttribute, Guid>(g => g).Union(newAttributes);

                    //We could somehow add PropagateSource here...
                    command.Commands.Add(new acmdSynchroPIMAttributes(Controller) { X1 = synchroGroup1.ToList(), X2 = synchroGroup2.ToList() });
                }
                //Swap the two lists and do it again
                ReadOnlyCollection<PSMAttribute> temp = aX1;
                aX1 = aX2;
                aX2 = temp;
            }
            return command;
        }
    }
}
