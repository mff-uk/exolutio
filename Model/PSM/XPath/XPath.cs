using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.SupportingClasses;

namespace Exolutio.Model.PSM.XPath
{
    public abstract class Path : IFormattable, ISupportsDeepCopy<Path>
    {
        public PSMSchema Schema { get; protected set; }

        protected Path(PSMSchema schema)
        {
            Schema = schema;
        }

        public bool IsAbsolute { get; set; }

        protected virtual void FillCopy(Path copy)
        {
            copy.IsAbsolute = this.IsAbsolute;
        }

        public abstract Path DeepCopy();

        public abstract void AddStep(Step step);
        
        public abstract void RemoveLastStep();

        public abstract string ToString(string format, IFormatProvider formatProvider);

        public override string ToString()
        {
            return this.ToString(null, null);
        }

        public abstract bool GetOptimized(out Path optimizedPath);
    }

    public class UnionPath : Path
    {
        private readonly List<Path> componentPaths = new List<Path>();

        public UnionPath(PSMSchema schema) : base(schema)
        {
        }

        public List<Path> ComponentPaths
        {
            get { return componentPaths; }
        }

        public override string ToString(string format, IFormatProvider formatProvider)
        {
            
            Path optimizedPath;
            if (this.GetOptimized(out optimizedPath))
            {
                return optimizedPath.ToString();
            }
            else
            {
                StringBuilder result = new StringBuilder();
                result.Append(@"(");
                for (int index = 0; index < ComponentPaths.Count; index++)
                {
                    Path componentPath = ComponentPaths[index];
                    if (componentPath is SimplePath)
                        result.AppendFormat(@"{0}", componentPath.ToString(format, formatProvider));
                    else
                        result.AppendFormat(@"({0})", componentPath.ToString(format, formatProvider));
                    if (index < ComponentPaths.Count - 1)
                        result.Append(@" | ");
                }
                result.Append(@")");
                return result.ToString();
            }
        }

        public override bool GetOptimized(out Path optimizedPath)
        {
            UnionPath optimizedComponents = new UnionPath(Schema);
            bool componentsOptimized = false;  
            foreach (Path componentPath in ComponentPaths)
            {
                Path optimizedComponent;
                componentsOptimized |= componentPath.GetOptimized(out optimizedComponent);
                optimizedComponents.ComponentPaths.Add(optimizedComponent);
            }

            if (optimizedComponents.ComponentPaths.Count == 1)
            {
                optimizedPath = optimizedComponents.ComponentPaths[0];
                return true; 
            }

            // small expression optimization: (###/X | ###/descendant:X) is combined to (###/descendant:X)
            if (optimizedComponents.ComponentPaths.Count > 0 && optimizedComponents.ComponentPaths.TransitiveTrue(CanBeJoinedInSteps))
            {
                optimizedPath = JoinInSteps(optimizedComponents);
                return true; 
            }

            if (componentsOptimized)
            {
                optimizedPath = optimizedComponents;
                return true; 
            }

            optimizedPath = this;
            return false;
        }

        private Path JoinInSteps(UnionPath optimizedComponents)
        {
            SimplePath joined = new SimplePath(Schema);
            for (int i = 0; i < ((SimplePath) optimizedComponents.ComponentPaths.First()).Steps.Count; i++)
            {
                if (optimizedComponents.ComponentPaths.Select(path => ((SimplePath)path).Steps[i]).TransitiveTrue((s1, s2) => s1 == s2))
                {
                    // ale are equal, so take first one
                    Step imageStep = ((SimplePath) optimizedComponents.ComponentPaths.First()).Steps[i];
                    joined.AddStep(imageStep.DeepCopy());
                }
                else
                {
                    Step imageStep = ((SimplePath) optimizedComponents.ComponentPaths.First()).Steps[i];
                    // they must be joined
                    Step newStep = new Step() { Axis = Axis.descendant, NodeTest = imageStep.NodeTest };
                    joined.AddStep(newStep);
                }
            }

            return joined;
        }
        
        private bool CanBeJoinedInSteps(Path path1, Path path2)
        {
            if (path1 is SimplePath && path2 is SimplePath)
            {
                SimplePath sp1 = (SimplePath) path1;
                SimplePath sp2 = (SimplePath) path2;
                if (sp1.Steps.Count == sp2.Steps.Count)
                {
                    for (int i = 0; i < sp1.Steps.Count; i++)
                    {
                        Step step1 = sp1.Steps[i];
                        Step step2 = sp2.Steps[i];
                        bool stepOk = false;
                        if (step1 == step2)
                        {
                            stepOk = true;
                        }
                        
                        if (!stepOk 
                            && step1.Axis.IsAmong(Axis.child, Axis.descendant)
                            && step2.Axis.IsAmong(Axis.child, Axis.descendant)
                            && step1.NodeTest == step2.NodeTest)
                        {
                            stepOk = true; 
                        }

                        if (!stepOk)
                        {
                            return false; 
                        }
                    }
                }
                return true; 
            }
            else
            {
                return false;
            } 
        }

        public override Path DeepCopy()
        {
            UnionPath copy = new UnionPath(Schema);
            base.FillCopy(copy);
            foreach (Path componentPath in ComponentPaths)
            {
                copy.ComponentPaths.Add(componentPath.DeepCopy());
            }
            return copy;
        }

        public override void AddStep(Step step)
        {
            this.ComponentPaths.ForEach(p => p.AddStep(step));
        }

        public override void RemoveLastStep()
        {
            this.ComponentPaths.ForEach(p => p.RemoveLastStep());
        }
    }

    public class SimplePath : Path
    {
        private readonly List<Step> steps = new List<Step>();

        public SimplePath(PSMSchema schema) : base(schema)
        {
        }

        public List<Step> Steps
        {
            get { return steps; }
        }

        public override void RemoveLastStep()
        {
            Steps.RemoveAt(Steps.Count - 1);
        }

        public override string ToString(string format, IFormatProvider formatProvider)
        {
            Path optimizedPath;
            if (GetOptimized(out optimizedPath))
            {
                return optimizedPath.ToString(format, formatProvider);
            }
            else
            {
                string result = (IsAbsolute ? @"/" : string.Empty) + Steps.ConcatWithSeparator(step => step.ToString(format, formatProvider), @"/");
                if (result.StartsWith(@"///"))
                    result = result.Substring(1);
                return result;
            }
        }

        public override Path DeepCopy()
        {
            SimplePath copy = new SimplePath(Schema);
            base.FillCopy(copy);
            foreach (Step step in Steps)
            {
                copy.Steps.Add(step.DeepCopy());
            }
            return copy;
        }

        public override void AddStep(Step step)
        {
            this.Steps.Add(step);
        }

        public override bool GetOptimized(out Path optimizedPath)
        {
            if (Steps.Count > 1
                && (Steps[0].NodeTest != Step.NODE_TEST_ANY_NODE)
                && (from psmClass in Schema.PSMClasses where psmClass.IsStructuralRepresentative select psmClass.RepresentedClass).All(rep => rep.Schema == Schema)
                && Steps.Take(Steps.Count - 1).All(s => s.Axis.IsAmong(Axis.child, Axis.descendant, Axis.descendantOrSelf)))
            {
                Step lastStep = Steps.Last();
                if (!string.IsNullOrEmpty(lastStep.NodeTest) 
                    && lastStep.NodeTest != Step.NODE_TEST_ANY_NODE
                    && Schema.GetAllXMLElementNames()[lastStep.NodeTest] == 1
                    && lastStep.Axis.IsAmong(Axis.child, Axis.descendant, Axis.descendantOrSelf, Axis.attribute)
                    )
                {
                    SimplePath simplePath = new SimplePath(Schema);
                    simplePath.AddStep(new Step { Axis = Axis.descendantOrSelf, NodeTest = Step.NODE_TEST_ANY_NODE });
                    simplePath.AddStep(new Step { Axis = lastStep.Axis != Axis.attribute ? Axis.child : Axis.attribute, NodeTest = lastStep.NodeTest});
                    optimizedPath = simplePath;
                    return true;
                }
            }

            optimizedPath = this;
            return false;
        }
    }

    public class Step : IFormattable, ISupportsDeepCopy<Step>, IEquatable<Step>
    {
        public Axis Axis { get; set; }

        public string NodeTest { get; set; }

        public const string NODE_TEST_ANY_NODE = @"node()";

        public Step DeepCopy()
        {
            Step copy = new Step {Axis = Axis, NodeTest = NodeTest};
            return copy;
        }

        public override string ToString()
        {
            return ToString(null, null);
        }

        public string ToString(string format, IFormatProvider provider)
        {
            if (String.IsNullOrEmpty(format)) format = @"S";

            if (format == @"S")
            {
                if (Axis == Axis.child)
                {
                    return NodeTest;
                }
                if (Axis == Axis.descendantOrSelf && NodeTest == NODE_TEST_ANY_NODE)
                {
                    return @"//";
                }
                if (Axis == Axis.parent && NodeTest == NODE_TEST_ANY_NODE)
                {
                    return @"..";
                }
                if (Axis == Axis.attribute)
                {
                    return @"@" + NodeTest;
                }
            }

            return string.Format(@"{0}::{1}", Axis.GetDescription(), NodeTest);
        }

        #region equality

        public bool Equals(Step other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other.Axis, Axis) && Equals(other.NodeTest, NodeTest);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (Step)) return false;
            return Equals((Step) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Axis.GetHashCode()*397) ^ (NodeTest != null ? NodeTest.GetHashCode() : 0);
            }
        }

        public static bool operator ==(Step left, Step right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Step left, Step right)
        {
            return !Equals(left, right);
        }

        #endregion 
    }

    public enum Axis
    {
        [EnumDescription("parent")]
        parent,
        [EnumDescription("child")]
        child,
        [EnumDescription("ancestor")]
        ancestor,
        [EnumDescription("descendant")]
        descendant,
        [EnumDescription("ancestor-or-self")]
        ancestorOrSelf,
        [EnumDescription("descendant-or-self")]
        descendantOrSelf,
        [EnumDescription("attribute")]
        attribute
    }
}