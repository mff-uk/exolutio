using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Exolutio.Model.OCL.AST;
using Exolutio.Model.OCL.Bridge;
using Exolutio.Model.OCL.Types;
using Exolutio.Model.PSM;
using Exolutio.SupportingClasses;

namespace Exolutio.Model.OCL.ConstraintConversion
{
    public class PSMPath : ICloneable
    {
        public class PathContext
        {
            private readonly List<LoopExp> loopStack = new List<LoopExp>();
            public List<LoopExp> LoopStack
            {
                get { return loopStack; }
            }

            public PSMBridge Bridge { get; set; }

            public ClassifierConstraint ClassifierConstraint { get; set; }

            public VariableNamer VariableNamer { get; set; }
        }

        public PathContext Context { get; private set; }

        public PSMPath()
        {
            Context = new PathContext();
        }

        private readonly List<PSMPathStep> steps = new List<PSMPathStep>();

        public VariableExp StartingVariableExp
        {
            get { return ((PSMPathVariableStep)steps[0]).VariableExp; }
        }

        public PSMClass StartingClass
        {
            get { return (PSMClass)StartingVariableExp.Type.Tag; }
        }

        public bool StartsInContext
        {
            get { return StartingVariableExp.referredVariable.IsContextVariable; }
        }

        public string StartingVariableName
        {
            get { return StartingVariableExp.referredVariable.Name; }
        }

        public List<PSMPathStep> Steps
        {
            get { return steps; }
        }

        public PSMClass LastClass
        {
            get
            {
                PSMPathStep step = Steps.Last(s => s is PSMPathVariableStep || s is PSMPathAssociationStep);
                if (step is PSMPathVariableStep)
                    return ((PSMPathVariableStep)step).VariableType;
                else
                    return (PSMClass) ((PSMPathAssociationStep) step).To;
            }
        }


        public override string ToString()
        {
            return Steps.ConcatWithSeparator(@".");
        }

        public object Clone()
        {
            PSMPath clone = new PSMPath();
            clone.Context = Context;
            foreach (PSMPathStep navigationStep in Steps)
            {
                PSMPathStep stepClone = (PSMPathStep)navigationStep.Clone(clone);
                clone.Steps.Add(stepClone);
            }
            return clone;
        }

        public string ToXPath(bool selfIsCurrent)
        {
            if (selfIsCurrent && 
                StartingVariableExp.referredVariable == Context.ClassifierConstraint.Self)
            {
                string path = Steps.Select(s => s == Steps.First() ? "." : s.ToXPath()).ConcatWithSeparator(String.Empty);
                if (path.StartsWith(@"./"))
                    return path.Substring(2);
                else 
                    return path;
            }
            else
            {
                return Steps.Select(s => s.ToXPath()).ConcatWithSeparator(String.Empty);
            }
        }
    }


    public abstract class PSMPathStep
    {
        public abstract object Clone(PSMPath newContainingPath);

        public abstract string ToXPath();

        public PSMPath Path { get; private set; }

        protected PSMPathStep(PSMPath path)
        {
            Path = path;
        }
    }

    class PSMPathVariableStep : PSMPathStep
    {
        public PSMPathVariableStep(PSMPath path) : base(path)
        {
        }

        public PSMClass VariableType
        {
            get { return (PSMClass)Variable.PropertyType.Tag; }
        }
        public VariableExp VariableExp { get; set; }
        public VariableDeclaration Variable
        {
            get { return VariableExp.referredVariable; }
        }

        public override string ToString()
        {
            return Variable.Name;
        }

        public override object Clone(PSMPath newContainingPath)
        {
            return new PSMPathVariableStep(newContainingPath) { VariableExp = this.VariableExp };
        }

        public override string ToXPath()
        {
            //if (Variable == Path.Context.ClassifierConstraint.Self)
            //{
            //    //if (Path.Context.LoopStack.IsEmpty())
            //    //{
            //    //    return @".";
            //    //}
            //    //else
            //    {
            //        return @"$self";
            //    }
            //}
            //else
            //{
            return string.Format(@"${0}", Variable.Name);
            //}
        }
    }

    class PSMPathAttributeStep : PSMPathStep
    {
        public PSMPathAttributeStep(PSMPath path) : base(path)
        {
        }

        public PSMAttribute Attribute { get; set; }
        public PSMClass Class { get { return Attribute.PSMClass; } }
        public override string ToString()
        {
            return Attribute.Name;
        }

        public override object Clone(PSMPath newContainingPath)
        {
            return new PSMPathAttributeStep(newContainingPath) { Attribute = this.Attribute };
        }

        public override string ToXPath()
        {
            if (Attribute.Element)
            {
                return string.Format(@"/{0}", Attribute.Name);
            }
            else
            {
                return string.Format(@"/@{0}", Attribute.Name);
            }
        }
    }

    public class PSMPathAssociationStep : PSMPathStep
    {
        public PSMPathAssociationStep(PSMPath path) : base(path)
        {
        }

        public PSMAssociationMember From { get; set; }
        public PSMAssociation Association { get; set; }
        public PSMAssociationMember To { get; set; }

        public bool IsUp { get; set; }

        public override string ToString()
        {
            if (!string.IsNullOrEmpty(Association.Name))
                return Association.Name;
            if (!string.IsNullOrEmpty(To.Name))
                return To.Name;
            if (From.ParentAssociation == Association)
                return @"parent";
            else 
                return string.Format(@"child_{0}", Association.Parent.ChildPSMAssociations.IndexOf(Association));
        }

        public override object Clone(PSMPath newContainingPath)
        {
            return new PSMPathAssociationStep(newContainingPath) { Association = this.Association, From = this.From, To = this.To };
        }

        public override string ToXPath()
        {
            if (Association.IsNamed)
            {
                if (IsUp)
                {
                    return string.Format(@"/..");   
                }
                else
                {
                    return string.Format(@"/{0}", Association.Name);
                }
            }
            else
            {
                return string.Empty;
            }
        }
    }

    public class PSMPathBuilder
    {
        public static PSMPath BuildPSMPath(NavigationCallExp node, ClassifierConstraint constraint, VariableNamer variableNamer)
        {
            PSMPath path = new PSMPath();
            path.Context.ClassifierConstraint = constraint;
            path.Context.VariableNamer = variableNamer;
            throw new NotImplementedException();
        }


        public static PSMPath BuildPSMPath(PropertyCallExp node, ClassifierConstraint constraint, VariableNamer variableNamer)
        {
            PSMPath path = new PSMPath();
            path.Context.ClassifierConstraint = constraint;
            path.Context.VariableNamer = variableNamer;

            OclExpression s;
            if (node.ReferredProperty.Tag is PSMAssociation)
            {
                s = node;
            }
            else
            {
                PSMAttribute a = (PSMAttribute)node.ReferredProperty.Tag;

                PSMPathAttributeStep pathAttributeStep = new PSMPathAttributeStep(path) { Attribute = a };
                path.Steps.Add(pathAttributeStep);
                s = node.Source;
            }

            while (!(s is VariableExp))
            {
                PSMPathAssociationStep step = new PSMPathAssociationStep(path);
                var sp = ((PropertyCallExp)s);
                // HACK: WFJT turn class tag into association tag
                PSMBridgeAssociation bridgeAssociation = (PSMBridgeAssociation) sp.ReferredProperty;
                step.Association = bridgeAssociation.SourceAsscociation;
                if (bridgeAssociation.Direction == PSMBridgeAssociation.AssociationDirection.Down)
                {
                    step.From = bridgeAssociation.SourceAsscociation.Parent;
                    step.To = bridgeAssociation.SourceAsscociation.Child;
                    step.IsUp = false;
                }
                else
                {
                    step.From = bridgeAssociation.SourceAsscociation.Child;
                    step.To = bridgeAssociation.SourceAsscociation.Parent;
                    step.IsUp = true;
                }
                path.Steps.Insert(0, step);
                s = sp.Source;
            }

            VariableExp variableExp = (VariableExp) s;
            PSMPathVariableStep pathVariableStep = new PSMPathVariableStep(path) { VariableExp = variableExp };
            if (string.IsNullOrEmpty(variableExp.referredVariable.Name))
            {
                variableExp.referredVariable.Name = path.Context.VariableNamer.GetName(variableExp.referredVariable.PropertyType);
            }
            path.Steps.Insert(0, pathVariableStep);
            return path;
        }

        public static PSMPath BuildPSMPath(VariableExp variableExp, ClassifierConstraint constraint, VariableNamer variableNamer)
        {
            PSMPath path = new PSMPath();
            path.Context.ClassifierConstraint = constraint;
            path.Context.VariableNamer = variableNamer;

            PSMPathVariableStep pathVariableStep = new PSMPathVariableStep(path) { VariableExp = variableExp };
            if (string.IsNullOrEmpty(variableExp.referredVariable.Name))
            {
                variableExp.referredVariable.Name = path.Context.VariableNamer.GetName(variableExp.referredVariable.PropertyType);
            }
            path.Steps.Insert(0, pathVariableStep);
            return path;
        }
    }
}