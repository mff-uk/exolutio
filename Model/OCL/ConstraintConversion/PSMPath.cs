using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Exolutio.Model.OCL.AST;
using Exolutio.Model.OCL.Types;
using Exolutio.Model.PSM;
using Exolutio.SupportingClasses;

namespace Exolutio.Model.OCL.ConstraintConversion
{
    public class PSMPath : ICloneable
    {
        private readonly List<PSMPathStep> steps = new List<PSMPathStep>();

        public VariableExp StartingVariable
        {
            get { return ((PSMPathVariableStep)steps[0]).VariableExp; }
        }

        public PSMClass StartingClass
        {
            get { return (PSMClass)StartingVariable.Type.Tag; }
        }

        public bool StartsInContext
        {
            get { return StartingVariableName == @"self"; }
        }

        public string StartingVariableName
        {
            get { return StartingVariable.referredVariable.Name; }
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
            foreach (PSMPathStep navigationStep in Steps)
            {
                PSMPathStep stepClone = (PSMPathStep)navigationStep.Clone();
                clone.Steps.Add(stepClone);
            }
            return clone;
        }
    }


    public abstract class PSMPathStep : ICloneable
    {
        public abstract object Clone();
    }

    class PSMPathVariableStep : PSMPathStep
    {
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

        public override object Clone()
        {
            return new PSMPathVariableStep { VariableExp = this.VariableExp };
        }
    }

    class PSMPathAttributeStep : PSMPathStep
    {
        public PSMAttribute Attribute { get; set; }
        public PSMClass Class { get { return Attribute.PSMClass; } }
        public override string ToString()
        {
            return Attribute.Name;
        }

        public override object Clone()
        {
            return new PSMPathAttributeStep { Attribute = this.Attribute };
        }
    }

    public class PSMPathAssociationStep : PSMPathStep
    {
        public PSMAssociationMember From { get; set; }
        public PSMAssociation Association { get; set; }
        public PSMAssociationMember To { get; set; }
        public override string ToString()
        {
            if (!string.IsNullOrEmpty(Association.Name))
                return Association.Name;
            if (!string.IsNullOrEmpty(To.Name))
                return To.Name;
            if (From.ParentAssociation == Association)
                return "parent";
            else 
                return string.Format(@"child_{0}", Association.Parent.ChildPSMAssociations.IndexOf(Association));
        }

        public override object Clone()
        {
            return new PSMPathAssociationStep { Association = this.Association, From = this.From, To = this.To };
        }
    }

    public class PSMPathBuilder
    {
        public static PSMPath BuildPSMPath(NavigationCallExp node)
        {
            throw new NotImplementedException();
        }


        public static PSMPath BuildPSMPath(PropertyCallExp node)
        {
            throw new NotImplementedException();
        }

    }
}