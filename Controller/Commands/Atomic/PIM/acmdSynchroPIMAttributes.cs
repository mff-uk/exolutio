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
        {
            Report = new CommandReport(CommandReports.PIM_ATTR_SYNCHRO,
                String.Concat(Project.TranslateComponentCollection<PIMAttribute>(X1).Select<PIMAttribute, String>(a => a.ToString() + " ")),
                String.Concat(Project.TranslateComponentCollection<PIMAttribute>(X2).Select<PIMAttribute, String>(a => a.ToString() + " ")));
        }

        internal override CommandBase.OperationResult UndoOperation()
        {
            return OperationResult.OK;
        }

        internal override MacroCommand PrePropagation()
        {
            MacroCommand command = new MacroCommand(Controller) { CheckFirstOnlyInCanExecute = true };
            command.Report = new CommandReport("Pre-propagation (synchronize PIM attribute sets)");
            IEnumerable<PIMAttribute> aX1 = X1.Select<Guid, PIMAttribute>(G => Project.TranslateComponent<PIMAttribute>(G));
            IEnumerable<PIMAttribute> aX2 = X2.Select<Guid, PIMAttribute>(G => Project.TranslateComponent<PIMAttribute>(G));
            if (aX1.Count() == 0 && aX2.Count() == 0) return command;
            PIMClass pimClass = aX1.Count() == 0 ? aX2.First().PIMClass : aX1.First().PIMClass;

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
                    IEnumerable<PSMAttribute> psmAttributesInSubClasses = psmClass.UnInterpretedSubClasses()
                        .SelectMany<PSMClass, PSMAttribute>(c => c.PSMAttributes);

                    IEnumerable<PIMAttribute> interpretations = psmAttributesInSubClasses
                        .Union<PSMAttribute>(psmClass.PSMAttributes)
                        .Where<PSMAttribute>(a => a.Interpretation != null)
                        .Select<PSMAttribute, PIMAttribute>(a => a.Interpretation as PIMAttribute);
                    IEnumerable<PIMAttribute> attributesToPropagate = aX2.Where<PIMAttribute>(a => !interpretations.Contains<PIMAttribute>(a));

                    List<Guid> newAttributesGuid = new List<Guid>();
                    foreach (PIMAttribute a in attributesToPropagate)
                    {
                        cmdCreateNewPSMAttribute c = new cmdCreateNewPSMAttribute(Controller);
                        Guid attrGuid = Guid.NewGuid();
                        c.AttributeGuid = attrGuid;
                        c.Set(psmClass, a.AttributeType, a.Name, a.Lower, a.Upper, false);
                        command.Commands.Add(c);
                        newAttributesGuid.Add(attrGuid);

                        acmdSetInterpretation cmdi = new acmdSetPSMAttributeInterpretation(Controller, attrGuid, a);
                        command.Commands.Add(cmdi);
                    }

                    IEnumerable<PSMAttribute> psmAttributesToMove = psmAttributesInSubClasses.Where(a => aX1.Contains(a.Interpretation) || aX2.Contains(a.Interpretation));

                    foreach (PSMAttribute a in psmAttributesToMove)
                    {
                        command.Commands.Add(new cmdMovePSMAttribute(Controller) { AttributeGuid = a, ClassGuid = psmClass, Propagate = false });
                    }

                    IEnumerable<Guid> synchroGroup1 = psmAttributesInSubClasses.Union(psmClass.PSMAttributes).Where(a => aX1.Contains(a.Interpretation)).Select<PSMAttribute, Guid>(a => a);
                    IEnumerable<Guid> synchroGroup2 = psmAttributesInSubClasses.Union(psmClass.PSMAttributes).Where(a => aX2.Contains(a.Interpretation)).Select<PSMAttribute, Guid>(a => a).Union(newAttributesGuid);

                    command.Commands.Add(new acmdSynchroPSMAttributes(Controller) { X1 = synchroGroup1.ToList(), X2 = synchroGroup2.ToList(), Propagate = false});
                    
                    foreach (PSMAttribute a in psmAttributesToMove)
                    {
                        command.Commands.Add(new cmdMovePSMAttribute(Controller) { AttributeGuid = a, ClassGuid = a.PSMClass, Propagate = false });
                    }

                }
                
                //Swap the two lists and do it again
                IEnumerable<PIMAttribute> temp = aX1;
                aX1 = aX2;
                aX2 = temp;
            }

            return command;
        }
    }
}
