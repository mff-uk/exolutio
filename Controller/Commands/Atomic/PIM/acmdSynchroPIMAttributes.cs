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
                IEnumerable<PSMClass> allPSMClasses = pimClass.GetInterpretedComponents().Cast<PSMClass>();
                List<PSMClass> psmClasses = new List<PSMClass>();
                foreach (PSMClass c in allPSMClasses)
                {
                    IEnumerable<PSMAttribute> atts = c.GetContextPSMAttributes();
                    if (atts.Where(a => a.Interpretation != null)
                            .Select(psma => psma.Interpretation as PIMAttribute)
                            .Intersect(aX1)
                            .OrderBy(k => k.ID)
                            .SequenceEqual(aX1.OrderBy(j => j.ID)))
                        psmClasses.Add(c);
                }
                
                //Elimination of SRs where C is affected and SR is a SR of C
                bool found = true;
                while (found)
                {
                    found = false;
                    List<PSMClass> list = new List<PSMClass>();
                    foreach (PSMClass psmClass in psmClasses)
                    {
                        if (psmClass.RepresentedClass != null
                            && psmClasses.Contains(psmClass.RepresentedClass)
                            && !psmClass.GetContextPSMAttributes(true)
                                        .Where(a => a.Interpretation != null)
                                        .Select(psma => psma.Interpretation as PIMAttribute)
                                        .Intersect(aX1)
                                        .OrderBy(k => k.ID)
                                        .SequenceEqual(aX1.OrderBy(j => j.ID)))
                        {
                            found = true;
                        }
                        else list.Add(psmClass);
                    }
                    psmClasses = list;
                }

                foreach (PSMClass psmClass in psmClasses)
                {

                    IEnumerable<Tuple<PSMAttribute, IEnumerable<ModelIterator.MoveStep>>> attributesAndPaths = psmClass.GetContextPSMAttributesWithPaths();
                    IEnumerable<PSMAttribute> attributes = attributesAndPaths.Select(t => t.Item1);
                    IEnumerable<PSMAttribute> interpretedAttributes = attributes.Where(a => a.Interpretation != null);
                    IEnumerable<PSMAttribute> responsibleAttributes = interpretedAttributes.Where(a => aX1.Contains(a.Interpretation));
                    IEnumerable<PIMAttribute> interpretations = interpretedAttributes.Select(a => a.Interpretation as PIMAttribute);
                    IEnumerable<PIMAttribute> attributesToPropagate = aX2.Where(a => !interpretations.Contains(a));
                    
                    IEnumerable<PSMAttribute> attributesToMoveList = interpretedAttributes.Where(a => a.PSMClass != psmClass && (aX1.Contains((PIMAttribute)a.Interpretation) || aX2.Contains((PIMAttribute)a.Interpretation)));
                    Dictionary<PSMAttribute, IEnumerable<ModelIterator.MoveStep>> psmAttributesToMove = new Dictionary<PSMAttribute, IEnumerable<ModelIterator.MoveStep>>();
                    foreach (PSMAttribute t in attributesToMoveList)
                    {
                        Tuple<PSMAttribute, IEnumerable<ModelIterator.MoveStep>> tuple = attributesAndPaths.Single(tup => tup.Item1 == t);
                        psmAttributesToMove.Add(tuple.Item1, tuple.Item2);
                    }

                    List<Guid> newAttributesGuid = new List<Guid>();
                    foreach (PIMAttribute a in attributesToPropagate)
                    {
                        cmdCreateNewPSMAttribute c = new cmdCreateNewPSMAttribute(Controller);
                        Guid attrGuid = Guid.NewGuid();
                        c.AttributeGuid = attrGuid;
                        c.Set(psmClass, a.AttributeType, a.Name, a.Lower, a.Upper, responsibleAttributes.First().Element);
                        command.Commands.Add(c);
                        newAttributesGuid.Add(attrGuid);

                        acmdSetInterpretation cmdi = new acmdSetPSMAttributeInterpretation(Controller, attrGuid, a);
                        command.Commands.Add(cmdi);
                    }

                    foreach (KeyValuePair<PSMAttribute, IEnumerable<ModelIterator.MoveStep>> kvp in psmAttributesToMove)
                    {
                        foreach (ModelIterator.MoveStep s in kvp.Value)
                        {
                            if (s.StepType == ModelIterator.MoveStep.MoveStepType.None) continue;
                            command.Commands.Add(new acmdMovePSMAttribute(Controller, kvp.Key, s.StepTarget) { Propagate = false });
                        }
                    }

                    IEnumerable<Guid> synchroGroup1 = responsibleAttributes.Select<PSMAttribute, Guid>(a => a);
                    IEnumerable<Guid> synchroGroup2 = attributes.Where(a => aX2.Contains((PIMAttribute)a.Interpretation)).Select<PSMAttribute, Guid>(a => a).Union(newAttributesGuid);

                    command.Commands.Add(new acmdSynchroPSMAttributes(Controller) { X1 = synchroGroup1.ToList(), X2 = synchroGroup2.ToList(), Propagate = false});

                    foreach (KeyValuePair<PSMAttribute, IEnumerable<ModelIterator.MoveStep>> kvp in psmAttributesToMove)
                    {
                        bool first = true;
                        foreach (ModelIterator.MoveStep s in kvp.Value.Reverse<ModelIterator.MoveStep>())
                        {
                            if (first) first = false;
                            else command.Commands.Add(new acmdMovePSMAttribute(Controller, kvp.Key, s.StepTarget) { Propagate = false });
                        }
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
