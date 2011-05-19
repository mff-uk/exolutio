using System;
using System.Linq;
using EvoX.SupportingClasses;
using System.Collections.Generic;
using System.Reflection;
using EvoX.SupportingClasses.Reflection;

namespace EvoX.Controller.Commands.Reflection
{
    public static class PublicCommandsHelper
    {
        public static List<Type> publicCommandsTypes;

        public static Dictionary<Type, CommandDescriptor> publicCommandsByType; 

        public static Dictionary<ScopeAttribute.EScope, List<CommandDescriptor>> publicCommandsByScope; 

        public static Dictionary<string, Tuple<string, PublicCommandAttribute.EPulicCommandCategory>> GetAvailableOperations()
        {
            Dictionary<string, Tuple<string, PublicCommandAttribute.EPulicCommandCategory>> result 
                = new Dictionary<string, Tuple<string, PublicCommandAttribute.EPulicCommandCategory>>();
            
            foreach (Type commandType in publicCommandsTypes)
            {
                PublicCommandAttribute a;
                commandType.TryGetAttribute(out a);
                result.Add(commandType.FullName, new Tuple<string, PublicCommandAttribute.EPulicCommandCategory>(a.Name, a.Category));
                foreach (PropertyInfo propertyInfo in commandType.GetProperties())
                {
                    PublicArgumentAttribute attribute;
                    if (propertyInfo.TryGetAttribute(out attribute))
                    {
                        if (propertyInfo.PropertyType.IsAmong(typeof(Guid), typeof(List<Guid>)))
                        {
                            if (attribute.ComponentType == null)
                            {
                                throw new EvoXCommandException(
                                    "When MandatoryArgument is applied on a property of type System.Guid, property ComponentType must be defined for the attribute.");
                            }
                        }
                        else if (attribute.ComponentType != null)
                        {
                            throw new EvoXCommandException(
                                    "When MandatoryArgument is applied on a property of type other than System.Guid, property ComponentType must not be defined for the attribute.");
                        }
                    }
                }
            }

            return result;
        }

        static PublicCommandsHelper()
        {
            LoadCommandsTypes();
        }

        private static void LoadCommandsTypes()
        {
            if (publicCommandsTypes == null)
            {
                Assembly assembly = typeof(CommandBase).Assembly;
                publicCommandsTypes = assembly.GetTypesWithAttribute<PublicCommandAttribute>();
                publicCommandsByScope = new Dictionary<ScopeAttribute.EScope, List<CommandDescriptor>>();
                publicCommandsByType = new Dictionary<Type, CommandDescriptor>();

                foreach (Type commandType in publicCommandsTypes)
                {
                    PublicCommandAttribute a;
                    commandType.TryGetAttribute(out a);

                    CommandDescriptor commandDescriptor = new CommandDescriptor();
                    commandDescriptor.CommandName = commandType.FullName;
                    commandDescriptor.CommandDescription = a.Name;
                    commandDescriptor.CommandType = commandType;
                    commandDescriptor.Parameters = GetCommandParameters(commandType);

                    publicCommandsByType.Add(commandType, commandDescriptor);
                    
                    foreach (PropertyInfo propertyInfo in commandType.GetProperties())
                    {
                        PublicArgumentAttribute publicArgumentAttribute;
                        if (propertyInfo.TryGetAttribute(out publicArgumentAttribute))
                        {
                            if (propertyInfo.PropertyType.IsAmong(typeof(Guid), typeof(List<Guid>)))
                            {
                                if (publicArgumentAttribute.ComponentType == null)
                                {
                                    throw new EvoXCommandException(
                                        "When MandatoryArgument is applied on a property of type System.Guid, property ComponentType must be defined for the attribute.");
                                }
                            }
                            else if (publicArgumentAttribute.ComponentType != null)
                            {
                                throw new EvoXCommandException(
                                        "When MandatoryArgument is applied on a property of type other than System.Guid, property ComponentType must not be defined for the attribute.");
                            }
                        }

                        ScopeAttribute scopeAttribute;
                        if (propertyInfo.TryGetAttribute(out scopeAttribute))
                        {
                            commandDescriptor.ScopeProperty = propertyInfo;
                            commandDescriptor.Scope = scopeAttribute.Scope;
                            ParameterDescriptor scopeParameter = commandDescriptor.Parameters.First(p => p.ParameterPropertyInfo == commandDescriptor.ScopeProperty);
                            scopeParameter.IsScopeParamater = true;
                            publicCommandsByScope.CreateSubCollectionIfNeeded(scopeAttribute.Scope);
#if SILVERLIGHT
                            foreach (ScopeAttribute.EScope scope in EnumHelper.GetValues(typeof(ScopeAttribute.EScope)))
#else 
                            foreach (ScopeAttribute.EScope scope in Enum.GetValues(typeof(ScopeAttribute.EScope)))
#endif
                            {
                                if (scope == ScopeAttribute.EScope.None)
                                    continue;
                                if (scopeAttribute.Scope.HasFlag(scope))
                                {
                                    publicCommandsByScope.CreateSubCollectionIfNeeded(scope);
                                    publicCommandsByScope[scope].Add(commandDescriptor);
                                }
                            }
                        }
                    }
                    if (commandDescriptor.ScopeProperty == null)
                    {
                        publicCommandsByScope.CreateSubCollectionIfNeeded(ScopeAttribute.EScope.None);
                        publicCommandsByScope[ScopeAttribute.EScope.None].Add(commandDescriptor);
                    }

                    if (commandDescriptor.ScopeProperty != null)
                    {
                        foreach (ParameterDescriptor parameterDescriptor in commandDescriptor.Parameters)
                        {
                            if (parameterDescriptor.ModifiedComponentPropertyName != null)
                            {
                                Type componentType = commandDescriptor.Parameters.First(p => p.IsScopeParamater).ComponentType;
                                parameterDescriptor.ModifiedComponentProperty = componentType.GetProperty(parameterDescriptor.ModifiedComponentPropertyName);
                            }
                        }
                    }
                }
                publicCommandsTypes.Sort(CommandTypeComparer);
            }
        }

        private static List<ParameterDescriptor> GetCommandParameters(Type commandType)
        {
            List<PropertyInfo> parameterProperties
                = AttributesHelper.FindPropertiesWithAttribute<PublicArgumentAttribute>(commandType);

            List<ParameterDescriptor> result = new List<ParameterDescriptor>();

            foreach (PropertyInfo parameterProperty in parameterProperties)
            {
                ParameterDescriptor parameter = new ParameterDescriptor();
                parameter.ParameterPropertyInfo = parameterProperty;

                PublicArgumentAttribute a;
                parameterProperty.TryGetAttribute(out a);
                parameter.ParameterName = a.ArgumentName;

                if (a.ComponentType != null)
                {
                    parameter.ComponentType = a.ComponentType;
                }
                if (a.SuggestedValue != null)
                {
                    parameter.SuggestedValue = a.SuggestedValue;
                }
                if (a.AllowNullInput)
                {
                    parameter.AllowNullInput = true;
                }
                if (!string.IsNullOrEmpty(a.ModifiedPropertyName))
                {
                    parameter.ModifiedComponentPropertyName = a.ModifiedPropertyName;
                }
                
                result.Add(parameter);
            }

            return result;
        }

        public static CommandDescriptor GetCommandDescriptor(string commandTypeName)
        {
            object command = CommandSerializer.CreateCommandObject(commandTypeName);
            return publicCommandsByType[command.GetType()];
        }

        public static CommandDescriptor GetCommandDescriptor(Type commandType)
        {
            return publicCommandsByType[commandType];
        }

        private static int CommandTypeComparer(Type type, Type type1)
        {
            PublicCommandAttribute a1;
            type.TryGetAttribute(out a1);

            PublicCommandAttribute a2;
            type1.TryGetAttribute(out a2);

            int catCompare = ((int)a1.Category).CompareTo((int)a2.Category);
            if (catCompare == 0)
            {
                return a1.Name.CompareTo(a2.Name);
            }
            else return catCompare;
        }
    }
}