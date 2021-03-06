﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Model;
using Exolutio.Model.PSM;
using Exolutio.Controller.Commands.Atomic;
using Exolutio.Controller.Commands.Atomic.PSM;
using System.Diagnostics;

namespace Exolutio.Controller.Commands.Complex.PSM
{
    [PublicCommand("Move PSM attribute (complex)", PublicCommandAttribute.EPulicCommandCategory.PSM_complex)]
    public class cmdMovePSMAttribute : ComposedCommand
    {
        [PublicArgument("Moved PSM attribute", typeof(PSMAttribute))]
        [Scope(ScopeAttribute.EScope.PSMAttribute)]
        public Guid AttributeGuid { get; set; }

        [PublicArgument("Target PSM class", typeof(PSMClass))]
        public Guid ClassGuid { get; set; }

        public cmdMovePSMAttribute()
        {
            CheckFirstOnlyInCanExecute = true;
        }

        public cmdMovePSMAttribute(Controller c)
            : base(c)
        {
            CheckFirstOnlyInCanExecute = true;
        }

        public void Set(Guid attributeGuid, Guid classGuid)
        {
            AttributeGuid = attributeGuid;
            ClassGuid = classGuid;
        }

        internal override void GenerateSubCommands()
        {
            PSMAttribute attribute = Project.TranslateComponent<PSMAttribute>(AttributeGuid);
            PSMClass sourceClass = attribute.PSMClass;
            PSMClass targetClass = Project.TranslateComponent<PSMClass>(ClassGuid);

            if (sourceClass.GetSRs().Contains(targetClass))
            {
                List<PSMClass> intermediateClasses = new List<PSMClass>();
                PSMClass current = sourceClass;

                while (current != targetClass)
                {
                    current = current.RepresentedClass;
                    intermediateClasses.Add(current);
                }
                
                foreach (PSMClass i in intermediateClasses)
                {
                    Commands.Add(new acmdMovePSMAttribute(Controller, AttributeGuid, i) { Propagate = Propagate });
                }
            }
            else if (targetClass.GetSRs().Contains(sourceClass))
            {
                List<PSMClass> intermediateClasses = new List<PSMClass>();
                PSMClass current = targetClass;

                while (current != sourceClass)
                {
                    intermediateClasses.Add(current);
                    current = current.RepresentedClass;
                }

                foreach (PSMClass i in intermediateClasses.Reverse<PSMClass>())
                {
                    Commands.Add(new acmdMovePSMAttribute(Controller, AttributeGuid, i) { Propagate = Propagate });
                }
            }
            else
            {
                List<PSMClass> intermediateClasses = new List<PSMClass>();
                PSMClass common = sourceClass.GetNearestCommonAncestorClass(targetClass);
                Debug.Assert(common != null, "Common Ancestor Class Null");
                if (common != sourceClass)
                {
                    //move up to common class
                    PSMClass parent = sourceClass.NearestParentClass();
                    while (parent != common)
                    {
                        intermediateClasses.Add(parent);
                        parent = parent.NearestParentClass();
                    }
                    intermediateClasses.Add(common);
                }

                if (common.IsDescendantFrom(targetClass))
                {
                    //move up
                    PSMClass parent = common.NearestParentClass();
                    while (parent != targetClass)
                    {
                        intermediateClasses.Add(parent);
                        parent = parent.NearestParentClass();
                    }
                    intermediateClasses.Add(targetClass);
                }
                else if (targetClass.IsDescendantFrom(common))
                {
                    //move down
                    List<PSMClass> intermediateClasses2 = new List<PSMClass>();
                    intermediateClasses2.Add(targetClass);
                    PSMClass parent = targetClass.NearestParentClass();
                    while (parent != common)
                    {
                        intermediateClasses2.Add(parent);
                        parent = parent.NearestParentClass();
                    }
                    intermediateClasses2.Reverse();
                    intermediateClasses.AddRange(intermediateClasses2);
                }
                else if (targetClass == common)
                {
                    //nothing
                }
                else
                {
                    Debug.Assert(false, "error - common class not reachable?");
                }

                foreach (PSMClass psmClass in intermediateClasses)
                {
                    Commands.Add(new acmdMovePSMAttribute(Controller, AttributeGuid, psmClass) { Propagate = Propagate });
                }
            }
        }

        public override bool CanExecute()
        {
            PSMAttribute att = Project.TranslateComponent<PSMAttribute>(AttributeGuid);
            PSMClass target = Project.TranslateComponent<PSMClass>(ClassGuid);
            PSMClass source = att.PSMClass;
            if (source.GetSRs().Contains(target) || target.GetSRs().Contains(source)) return true;
            if (source.GetNearestCommonAncestorClass(target) == null)
            {
                ErrorDescription = CommandErrors.CMDERR_NO_COMMON_ANCESTOR_CLASS;
                return false;
            }
            return true;
        }

        internal override void CommandOperation()
        {
            base.CommandOperation();
            Report = new CommandReport(CommandReports.COMPLEX_MOVE_PSM_ATTRIBUTE);
        }
    }
}
