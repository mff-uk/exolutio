using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting;
using Exolutio.Model;
using Exolutio.SupportingClasses.Reflection;

namespace Exolutio.Controller.Commands.Reflection
{
    public class CommandDescriptor
    {
        public Type CommandType { get; set; }

        public ScopeAttribute.EScope Scope { get; set; }

        public string CommandName { get; set; }

        public PropertyInfo ScopeProperty { get; set; }
        
        public ExolutioObject ScopeObject { get; set; }

        public string ShortCommandName
        {
            get { return CommandName.Substring(CommandName.LastIndexOf('.') + 1); }
        }

        public List<ParameterDescriptor> Parameters { get; set; }

        public string CommandDescription { get; set; }

        public ParameterDescriptor GetParameterByPropertyName(string propertyName)
        {
            return Parameters.Where(p => p.ParameterPropertyName == propertyName).SingleOrDefault();
        }

        public void ClearParameterValues()
        {
            foreach (ParameterDescriptor parameterDescriptor in Parameters)
            {
                parameterDescriptor.ParameterValue = null;
            }
        }
    }

    public class ParameterDescriptor
    {
        public Type ComponentType { get; set; }

        public string ParameterPropertyName { get { return ParameterPropertyInfo.Name; } }

        public PropertyInfo ParameterPropertyInfo { get; set; }

        public string ParameterName { get; set; }

        public object ParameterValue { get; set; }

        public object SuggestedValue { get; set; }

        public bool AllowNullInput { get; set; }

        public string ModifiedComponentPropertyName { get; set; }

        public PropertyInfo ModifiedComponentProperty { get; set; }

        public bool IsScopeParamater { get; set; }
    }
}