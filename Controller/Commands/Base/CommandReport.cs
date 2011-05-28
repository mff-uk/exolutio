using System;
using System.Collections.Generic;
using Exolutio.Model;
using Exolutio.SupportingClasses;
using Exolutio.SupportingClasses.Annotations;

namespace Exolutio.Controller.Commands
{
    [Serializable]
    public abstract class CommandReportBase
    {
        public string Contents { get; set; }

        public Type CommandType { get; set; }

        protected CommandReportBase()
        {
            
        }

        protected CommandReportBase(string contents) : this()
        {
            Contents = contents;
        }

        [StringFormatMethod("formatString")]
        protected CommandReportBase(string formatString, params object[] args)
            : this()
        {
            string replaced = formatString.Replace("}", ":SN}");
            for (int index = 0; index < args.Length; index++)
            {
                object arg = args[index];
                if (arg is Component)
                {
                    Component component = ((Component) arg);
                    if (string.IsNullOrEmpty(component.Name))
                    {
                        args[index] = component.ToString();//"(unnamed)";
                    }
                }
            }
            Contents = string.Format(DispNullFormatProvider.Instance, replaced, args);
        }
    }

    [Serializable]
    public class CommandReport: CommandReportBase
    {
        public CommandReport()
        {
        }

        public CommandReport(string contents) : base(contents)
        {
        }

        public CommandReport(string formatString, params object[] args) : base(formatString, args)
        {
        }
    }
    
    [Serializable]
    public class NestedCommandReport : CommandReportBase
    {
        private readonly List<CommandReportBase> nestedReports = new List<CommandReportBase>();

        public NestedCommandReport(string contents) : base(contents)
        {
        }

        public NestedCommandReport(string formatString, params object[] args) : base(formatString, args)
        {
        }

        public List<CommandReportBase> NestedReports
        {
            get { return nestedReports; }
        }

        public NestedCommandReport()
        {
        }
    }
}