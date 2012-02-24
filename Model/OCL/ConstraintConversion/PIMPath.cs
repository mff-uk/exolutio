using System;
using System.Linq;
using System.Collections.Generic;
using Exolutio.Model.OCL.AST;
using Exolutio.Model.OCL.Types;
using Exolutio.Model.PIM;
using Exolutio.SupportingClasses;

namespace Exolutio.Model.OCL.ConstraintConversion
{
    public class PIMPath: IEquatable<PIMPath>
    {
        private readonly List<PIMPathStep> steps = new List<PIMPathStep>();

        public VariableExp StartingVariable
        {
            get { return ((PIMPathVariableStep)steps[0]).VariableExp; }
        }

        public PIMClass StartingClass
        {
            get { return (PIMClass)StartingVariable.Type.Tag; }
        }

        public bool StartsInContext
        {
            get { return StartingVariable.referredVariable.IsContextVariable; }
        }

        public string StartingVariableName
        {
            get { return StartingVariable.referredVariable.Name; }
        }

        public List<PIMPathStep> Steps
        {
            get { return steps; }
        }


        public bool Equals(PIMPath other)
        {
            if (this.StartingClass != other.StartingClass)
                return false;

            for (int index = 0; index < Steps.Count; index++)
            {
                PIMPathStep thisStep = Steps[index];
                if (!thisStep.Equals(other.Steps[index]))
                    return false;
            }

            return true; 
        }

        #region equality

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (PIMPath)) return false;
            return Equals((PIMPath) obj);
        }

        public override int GetHashCode()
        {
            return Steps.Aggregate("", (a, b) => a.ToString() + b.ToString(), a => a.ToString()).GetHashCode();
        }

        public static bool operator ==(PIMPath left, PIMPath right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(PIMPath left, PIMPath right)
        {
            return !Equals(left, right);
        }

        #endregion

        public override string ToString()
        {
            return Steps.ConcatWithSeparator(@".");
        }
    }


    public abstract class PIMPathStep
    {
        public override int GetHashCode()
        {
            return this.ToString().GetHashCode();
        }
    }

    class PIMPathVariableStep : PIMPathStep, IEquatable<PIMPathVariableStep>
    {
        public PIMClass VariableType
        {
            get { return (PIMClass)Variable.PropertyType.Tag; }
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

        public bool Equals(PIMPathVariableStep other)
        {
            return Variable == other.Variable;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(PIMPathVariableStep)) return false;
            return Equals((PIMPathVariableStep)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (base.GetHashCode()*397) ^ (VariableExp != null ? VariableExp.GetHashCode() : 0);
            }
        }
    }

    class PIMPathAttributeStep : PIMPathStep, IEquatable<PIMPathAttributeStep>
    {
        public PIMAttribute Attribute { get; set; }
        public PIMClass Class { get { return Attribute.PIMClass; } }
        public override string ToString()
        {
            return Attribute.Name;
        }

        

        public bool Equals(PIMPathAttributeStep other)
        {
            return Attribute == other.Attribute;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(PIMPathAttributeStep)) return false;
            return Equals((PIMPathAttributeStep)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (base.GetHashCode()*397) ^ (Attribute != null ? Attribute.GetHashCode() : 0);
            }
        }
    }

    public class PIMPathAssociationStep : PIMPathStep, IEquatable<PIMPathAssociationStep>
    {
        public PIMAssociationEnd AssociationEnd { get; set; }
        public PIMClass Class { get { return AssociationEnd.PIMClass; } }
        public override string ToString()
        {
            return !string.IsNullOrEmpty(AssociationEnd.Name) ? AssociationEnd.Name : AssociationEnd.PIMClass.Name;
        }

        public bool Equals(PIMPathAssociationStep other)
        {
            return AssociationEnd == other.AssociationEnd;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(PIMPathAssociationStep)) return false;
            return Equals((PIMPathAssociationStep)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (base.GetHashCode()*397) ^ (AssociationEnd != null ? AssociationEnd.GetHashCode() : 0);
            }
        }
    }

    public class PIMPathBuilder
    {
        public static PIMPath BuildPIMPath(NavigationCallExp node)
        {
            throw new NotImplementedException();
        }

        public static PIMPath BuildPIMPath(PropertyCallExp node)
        {
            PIMPath path = new PIMPath();

            OclExpression s;
            if (node.ReferredProperty.Tag is PIMAssociationEnd)
            {
                s = node;
            }
            else
            {
                PIMAttribute a = (PIMAttribute) node.ReferredProperty.Tag;

                PIMPathAttributeStep pathAttributeStep = new PIMPathAttributeStep {Attribute = a};
                path.Steps.Add(pathAttributeStep);
                s = node.Source;
            }
            
            while (!(s is VariableExp))
            {
                PIMPathAssociationStep step = new PIMPathAssociationStep();
                step.AssociationEnd = (PIMAssociationEnd) ((PropertyCallExp)s).ReferredProperty.Tag;
                path.Steps.Insert(0, step);
                s = ((PropertyCallExp)s).Source;
            }

            PIMPathVariableStep pathVariableStep = new PIMPathVariableStep();
            pathVariableStep.VariableExp = (VariableExp)s;
            path.Steps.Insert(0, pathVariableStep);
            return path;
        }

    }
}