using System;
using System.Collections.Generic;
using System.Text;
using Exolutio.SupportingClasses;

namespace Exolutio.Model.PSM.XPath
{
    public abstract class Path : IFormattable, ISupportsDeepCopy<Path>
    {
        public bool IsAbsolute { get; set; }
        
        public abstract string ToString(string format, IFormatProvider formatProvider);

        protected virtual void FillCopy(Path copy)
        {
            copy.IsAbsolute = this.IsAbsolute;
        }

        public abstract Path DeepCopy();
        public abstract void AddStep(Step step);

        public override string ToString()
        {
            return this.ToString(null, null);
        }
    }

    public class UnionPath : Path
    {
        private readonly List<Path> componentPaths = new List<Path>();

        public List<Path> ComponentPaths
        {
            get { return componentPaths; }
        }

        public override string ToString(string format, IFormatProvider formatProvider)
        {
            StringBuilder result = new StringBuilder();
            for (int index = 0; index < ComponentPaths.Count; index++)
            {
                Path componentPath = ComponentPaths[index];
                result.AppendFormat(@"({0})", componentPath.ToString(format, formatProvider));
                if (index < ComponentPaths.Count - 1)
                    result.Append(@" | ");
            }
            return result.ToString();
        }

        public override Path DeepCopy()
        {
            UnionPath copy = new UnionPath();
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
    }

    public class SimplePath : Path
    {
        private readonly List<Step> steps = new List<Step>();

        public List<Step> Steps
        {
            get { return steps; }
        }

        public override string ToString(string format, IFormatProvider formatProvider)
        {
            if (IsAbsolute)
                return @"/" + Steps.ConcatWithSeparator(step => step.ToString(format, formatProvider), @"/");
            else
                return Steps.ConcatWithSeparator(step => step.ToString(format, formatProvider), @"/");
        }

        public override Path DeepCopy()
        {
            SimplePath copy = new SimplePath();
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
    }

    public class Step : IFormattable, ISupportsDeepCopy<Step>
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