using System;
using EvoX.Controller.Commands.Reflection;

namespace EvoX.Controller.Commands
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class PublicCommandAttribute : Attribute
    {
        public string Name { get; set; }

        public enum EPulicCommandCategory
        {
            None = 0,
            [Description("PIM atomic")]
            PIM_atomic = 1,
            [Description("PSM atomic")]
            PSM_atomic = 2,
            [Description("Common atomic")]
            Common_atomic = 3,
            [Description("PIM complex")]
            PIM_complex = 4,
            [Description("PSM complex")]
            PSM_complex = 5,
            [Description("Common complex")]
            Common_complex = 6
        }

        public EPulicCommandCategory Category { get; set; }

        public PublicCommandAttribute(string name, EPulicCommandCategory category)
        {
            Name = name;
            Category = category;
        }
    }
}