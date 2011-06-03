using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Controller.Commands;
using Exolutio.Model.PIM;
using Exolutio.Model;
using Exolutio.Model.PSM;
using Exolutio.Controller.Commands.Atomic.PSM;
using Exolutio.Controller.Commands.Complex.PSM;
using System.Collections.ObjectModel;

namespace Exolutio.Controller.Commands.Atomic.PIM
{
    public class acmdSynchroPIMAttributes : StackedCommand
    {
        public List<Guid> X1 = new List<Guid>();
        public List<Guid> X2 = new List<Guid>();

        public acmdSynchroPIMAttributes(Controller c)
            : base(c) { }

        public override bool CanExecute()
        {
            ReadOnlyCollection<PIMAttribute> aX1 = Project.TranslateComponentCollection<PIMAttribute>(X1);
            ReadOnlyCollection<PIMAttribute> aX2 = Project.TranslateComponentCollection<PIMAttribute>(X2);
            if (aX1.Count == 0 || aX2.Count == 0)
            {
                ErrorDescription = CommandErrors.CMDERR_CANNOT_SYNCHRO_EMPTY_SETS;
                return false;
            }
            PIMClass pimClass = aX2[0].PIMClass;
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

        internal override PropagationMacroCommand PrePropagation()
        {
            IEnumerable<PIMAttribute> aX1 = Project.TranslateComponentCollection<PIMAttribute>(X1);
            IEnumerable<PIMAttribute> aX2 = Project.TranslateComponentCollection<PIMAttribute>(X2);
            if (aX1.Count() == 0 || aX2.Count() == 0) return null;

            PropagationMacroCommand command = new PropagationMacroCommand(Controller) { CheckFirstOnlyInCanExecute = true };
            command.Report = new CommandReport("Pre-propagation (synchronize PIM attribute sets)");
            PIMClass pimClass = aX2.First().PIMClass;

            //Twice... X1 => X2, X2 => X1
            for (int i = 0; i < 2; i++)
            {
                IEnumerable<PSMClass> psmClasses =
                    pimClass.GetInterpretedComponents()
                    .Cast<PSMClass>()
                    .Where(c => c.UnInterpretedSubClasses()
                        .SelectMany(cl => cl.PSMAttributes)
                        .Union(c.PSMAttributes)
                        .Where(a => a.Interpretation != null)
                        .Select(psma => psma.Interpretation as PIMAttribute)
                        .Intersect(aX1)
                        .OrderBy(k => k.ID)
                        .SequenceEqual(aX1.OrderBy(j => j.ID))
                        );

                foreach (PSMClass psmClass in psmClasses)
                {
                    IEnumerable<PSMAttribute> psmAttributesInSubClasses = psmClass.UnInterpretedSubClasses()
                        .SelectMany<PSMClass, PSMAttribute>(c => c.PSMAttributes);

                    IEnumerable<PIMAttribute> interpretations = psmAttributesInSubClasses
                        .Union(psmClass.PSMAttributes)
                        .Where(a => a.Interpretation != null)
                        .Select(a => a.Interpretation as PIMAttribute);
                    IEnumerable<PIMAttribute> attributesToPropagate = aX2.Where(a => !interpretations.Contains(a));

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

                    IEnumerable<PSMAttribute> psmAttributesToMove = psmAttributesInSubClasses.Where(a => aX1.Contains((PIMAttribute)a.Interpretation) || aX2.Contains((PIMAttribute)a.Interpretation));

                    foreach (PSMAttribute a in psmAttributesToMove)
                    {
                        command.Commands.Add(new cmdMovePSMAttribute(Controller) { AttributeGuid = a, ClassGuid = psmClass, Propagate = false });
                    }

                    IEnumerable<Guid> synchroGroup1 = psmAttributesInSubClasses.Union(psmClass.PSMAttributes).Where(a => aX1.Contains((PIMAttribute)a.Interpretation)).Select<PSMAttribute, Guid>(a => a);
                    IEnumerable<Guid> synchroGroup2 = psmAttributesInSubClasses.Union(psmClass.PSMAttributes).Where(a => aX2.Contains((PIMAttribute)a.Interpretation)).Select<PSMAttribute, Guid>(a => a).Union(newAttributesGuid);

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
