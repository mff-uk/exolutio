using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EvoX.Controller.Commands;
using EvoX.Model.PIM;
using EvoX.Model;
using EvoX.Model.PSM;
using EvoX.Controller.Commands.Atomic.PSM;
using EvoX.Controller.Commands.Complex.PSM;

namespace EvoX.Controller.Commands.Atomic.PIM
{
    public class acmdSynchroPIMAttributes : StackedCommand
    {
        public List<Guid> X1 = new List<Guid>();
        public List<Guid> X2 = new List<Guid>();

        public acmdSynchroPIMAttributes(Controller c)
            : base(c) { }

        public override bool CanExecute()
        {
            List<PIMAttribute> aX1 = X1.Select<Guid, PIMAttribute>(G => Project.TranslateComponent<PIMAttribute>(G)).ToList<PIMAttribute>();
            List<PIMAttribute> aX2 = X2.Select<Guid, PIMAttribute>(G => Project.TranslateComponent<PIMAttribute>(G)).ToList<PIMAttribute>();
            if (aX1.Count == 0 && aX2.Count == 0)
            {
                ErrorDescription = CommandErrors.CMDERR_CANNOT_SYNCHRO_EMPTY_SETS;
                return false;
            }
            PIMClass pimClass = aX1.Count == 0 ? aX2[0].PIMClass : aX1[0].PIMClass;
            if (!aX1.All<PIMAttribute>(a => a.PIMClass == pimClass) || !aX2.All<PIMAttribute>(a => a.PIMClass == pimClass))
            {
                ErrorDescription = CommandErrors.CMDERR_CANNOT_SYNCHRO_ATTS_DIFFERENT_CLASSES;
                return false;
            }
            return true;
        }

        internal override void CommandOperation()
        { }

        internal override CommandBase.OperationResult UndoOperation()
        {
            return OperationResult.OK;
        }

        internal override MacroCommand PrePropagation()
        {
            MacroCommand command = new MacroCommand(Controller) { CheckFirstOnlyInCanExecute = true };
            command.Report = new CommandReport("Pre-propagation (synchronize PIM attribute sets)");
            List<PIMAttribute> aX1 = X1.Select<Guid, PIMAttribute>(G => Project.TranslateComponent<PIMAttribute>(G)).ToList<PIMAttribute>();
            List<PIMAttribute> aX2 = X2.Select<Guid, PIMAttribute>(G => Project.TranslateComponent<PIMAttribute>(G)).ToList<PIMAttribute>();
            if (aX1.Count == 0 && aX2.Count == 0) return command;
            PIMClass pimClass = aX1.Count == 0 ? aX2[0].PIMClass : aX1[0].PIMClass;

            //Twice... X1 => X2, X2 => X1
            for (int i = 0; i < 2; i++)
            {
                IEnumerable<PSMClass> psmClasses =
                    pimClass.GetInterpretedComponents()
                    .Cast<PSMClass>()
                    .Where<PSMClass>(c => c.UnInterpretedSubClasses()
                        .SelectMany<PSMClass, PSMAttribute>(cl => cl.PSMAttributes)
                        .Union<PSMAttribute>(c.PSMAttributes)
                        .Where<PSMAttribute>(a => a.Interpretation != null)
                        .Select<PSMAttribute, PIMAttribute>(psma => psma.Interpretation as PIMAttribute)
                        .Intersect<PIMAttribute>(aX1)
                        .SequenceEqual<PIMAttribute>(aX1)
                        );

                foreach (PSMClass psmClass in psmClasses)
                {
                    IEnumerable<PIMAttribute> interpretations = psmClass.UnInterpretedSubClasses()
                        .SelectMany<PSMClass, PSMAttribute>(c => c.PSMAttributes)
                        .Union<PSMAttribute>(psmClass.PSMAttributes)
                        .Where<PSMAttribute>(a => a.Interpretation != null)
                        .Select<PSMAttribute, PIMAttribute>(a => a.Interpretation as PIMAttribute);
                    IEnumerable<PIMAttribute> attributesToPropagate = aX2.Where<PIMAttribute>(a => !interpretations.Contains<PIMAttribute>(a));

                    foreach (PIMAttribute a in attributesToPropagate)
                    {
                        cmdCreateNewPSMAttribute c = new cmdCreateNewPSMAttribute(Controller);
                        Guid attrGuid = Guid.NewGuid();
                        c.AttributeGuid = attrGuid;
                        c.Set(psmClass, a.AttributeType, a.Name, a.Lower, a.Upper, false);
                        command.Commands.Add(c);

                        acmdSetInterpretation cmdi = new acmdSetPSMAttributeInterpretation(Controller, attrGuid, a);
                        command.Commands.Add(cmdi);
                    }
                }
                
                //Swap the two lists and do it again
                List<PIMAttribute> temp = aX1;
                aX1 = aX2;
                aX2 = temp;
            }

            return command;
        }
    }
}
