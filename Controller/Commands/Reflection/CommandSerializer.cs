using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Remoting;
using System.Xml.Linq;
using Exolutio.Model;

namespace Exolutio.Controller.Commands.Reflection
{
    public static class CommandSerializer
    {
        #region instantiation

        public static CommandBase CreateCommandObject(string selectedOperationStr)
        {
#if SILVERLIGHT
            object objectHandle;
            objectHandle = Activator.CreateInstance(Assembly.GetExecutingAssembly().GetType(selectedOperationStr));
            return (CommandBase)objectHandle;
#else
            ObjectHandle objectHandle;
            objectHandle = Activator.CreateInstance(null, selectedOperationStr);
            return (CommandBase)objectHandle.Unwrap();
#endif
        }

        public static CommandBase CreateCommandObject(Type commandType)
        {
            object instance = Activator.CreateInstance(commandType);
            return (CommandBase)instance;
        }

        public static CommandBase CreateCommandObject(Type commandType, Controller controller)
        {
            object instance = Activator.CreateInstance(commandType);
            CommandBase commandObject = (CommandBase)instance;
            if (commandObject is StackedCommand)
            {
                ((StackedCommand)commandObject).Controller = controller;
            }
            return commandObject;
        }

        public static CommandBase CreateCommand(Controller controller, CommandDescriptor commandDescriptor)
        {
            CommandBase commandObject = CreateCommandObject(commandDescriptor.CommandName);

            if (commandObject is StackedCommand)
            {
                ((StackedCommand)commandObject).Controller = controller;
            }

            FillParameters(commandObject, commandDescriptor);

            return commandObject;
        }

        public static void FillParameters(CommandBase commandObject, CommandDescriptor parametersDescriptors)
        {
            foreach (ParameterDescriptor parameter in parametersDescriptors.Parameters)
            {
                PropertyInfo propertyInfo = parameter.ParameterPropertyInfo;
                object value = parameter.ParameterValue;
                if (value == null && parameter.CreateControlInEdtors == false)
                    continue;
                propertyInfo.SetValue(commandObject, value, null);
            }
        }

        #endregion

        public static XElement SerializeCommand(CommandBase commandBase, bool isUndo, bool isRedo)
        {
            return SerializeRec(commandBase, true, isUndo, isRedo);
        }

        private static XElement SerializeRec(CommandBase command, bool isInitial, bool isUndo, bool isRedo)
        {
            Type commandType = command.GetType();
            
            XElement commandElement;
            commandElement = new XElement(SERIALIZATION_NS + "Command");
            if (isInitial)
            {
                commandElement.Add(new XAttribute("initial", "true"));
            }
            if (isUndo)
            {
                commandElement.Add(new XAttribute("undo", "true"));
            }
            if (isRedo)
            {
                commandElement.Add(new XAttribute("redo", "true"));
            }

            if (command is PropagationMacroCommand)
            {
                commandElement.Add(new XAttribute("propagation", "true"));
            }
            
            XAttribute nameAttribute = new XAttribute("Name", commandType.Name);
            commandElement.Add(nameAttribute);

            XElement fullNameElement = new XElement(SERIALIZATION_NS + "FullName");
            fullNameElement.Add(new XText(commandType.FullName));
            commandElement.Add(fullNameElement);

            if (command.Report != null && !string.IsNullOrEmpty(command.Report.Contents))
            {
                XElement report = new XElement(SERIALIZATION_NS + "Report");
                report.Add(new XText(command.Report.Contents));
                commandElement.Add(report);
            }

            #region operation parameters
            if (PublicCommandsHelper.IsPublicCommand(commandType))
            {
                CommandDescriptor commandParametersDescriptors = PublicCommandsHelper.GetCommandDescriptor(commandType);
                if (commandParametersDescriptors.Parameters.Count > 0)
                {
                    XElement parametersElement = new XElement(SERIALIZATION_NS + "Parameters");
                    commandElement.Add(parametersElement);
                    foreach (ParameterDescriptor parameter in commandParametersDescriptors.Parameters)
                    {
                        XElement parameterElement = new XElement(SERIALIZATION_NS + "Parameter");
                        parameterElement.Add(new XElement(SERIALIZATION_NS + "Name", parameter.ParameterName));
                        parameterElement.Add(new XElement(SERIALIZATION_NS + "PropertyName", parameter.ParameterPropertyName));

                        object value = parameter.ParameterPropertyInfo.GetValue(command, new object[0]);
                        if (value is Guid)
                        {
                            string idText = value.ToString();
                            parameterElement.Add(new XElement(SERIALIZATION_NS + "Value", idText));
                            if (command is StackedCommand)
                            {
                                Project p = ((StackedCommand)command).Controller.Project;
                                ExolutioObject component;
                                if (p.TryTranslateObject((Guid)value, out component))
                                {
                                    parameterElement.Add(new XElement(SERIALIZATION_NS + "ValueText", component.ToString()));
                                }
                            }
                        }
                        else if (value is ExolutioObject)
                        {
                            string idText = ((ExolutioObject)value).ID.ToString();
                            parameterElement.Add(new XElement(SERIALIZATION_NS + "ValueID", idText));
                            parameterElement.Add(new XElement(SERIALIZATION_NS + "ValueText", idText));
                        }
                        else
                        {
                            string valueText = value.ToString();
                            parameterElement.Add(new XElement(SERIALIZATION_NS + "Value", valueText));
                        }
                        parametersElement.Add(parameterElement);
                    }
                }
            }

            #endregion

            if (command is MacroCommand && ((MacroCommand)command).Commands.Count > 0)
            {
                XElement subCommandsElement = new XElement(SERIALIZATION_NS + "SubCommands");
                subCommandsElement.Add(new XAttribute("count", ((MacroCommand)command).Commands.Count));
                commandElement.Add(subCommandsElement);
                foreach (CommandBase subCommand in ((MacroCommand)command).Commands)
                {
                    SerializeRec(subCommand, false, isUndo, isRedo);
                }
            }

            return commandElement;
        }

        public static CommandBase DeserializeCommand(XElement commandElement)
        {
            XElement fullNameElement = commandElement.Element(SERIALIZATION_NS + "FullName");
            CommandBase commandObject = CreateCommandObject(fullNameElement.Value);

            CommandDescriptor commandParametersDescriptors = PublicCommandsHelper.GetCommandDescriptor(commandObject.GetType());

            foreach (XElement parameterElement in commandElement.Element(SERIALIZATION_NS + "Parameters").Elements(SERIALIZATION_NS + "Parameter"))
            {
                string propertyName = parameterElement.Element(SERIALIZATION_NS + "PropertyName").Value;
                ParameterDescriptor parameter = commandParametersDescriptors.GetParameterByPropertyName(propertyName);
                PropertyInfo propertyInfo = parameter.ParameterPropertyInfo;
                string stringValue = parameterElement.Element(SERIALIZATION_NS + "Value").Value;
                object value = DeserializePropertyValue(propertyInfo, stringValue);
                parameter.ParameterValue = value;
            }
            
            FillParameters(commandObject, commandParametersDescriptors);

            return commandObject;
        }

        private static object DeserializePropertyValue(PropertyInfo propertyInfo, string stringValue)
        {
            if (propertyInfo.PropertyType == typeof(string))
            {
                return stringValue;
            }
            else if (propertyInfo.PropertyType.IsSubclassOf(typeof(ExolutioObject)))
            {
                throw new NotImplementedException();
            }
            else
            {
                MethodInfo parse = propertyInfo.PropertyType.GetMethod("Parse", new Type[] { typeof(string) });
                object parsed = parse.Invoke(null, new object[] { stringValue });
                return parsed;
            }
        }

        private static readonly XNamespace SERIALIZATION_NS = @"http://eXolutio.eu/Commands/CommandLog/";

        public static XDocument CreateEmptySerializationDocument()
        {
            XDocument serializationDocument = new XDocument(new XDeclaration("1.0", "utf-8", null));
            
            XElement elCommandLog = new XElement(SERIALIZATION_NS + "CommandLog");
            elCommandLog.Add(new XAttribute(XNamespace.Xmlns + "eXoLog", SERIALIZATION_NS.NamespaceName));
            serializationDocument.Add(elCommandLog);
            return serializationDocument;
        }

        public static List<CommandBase> DeserializeDocument(XDocument document)
        {
            List<CommandBase> result = new List<CommandBase>();
            foreach (XElement commandElement in document.Element(SERIALIZATION_NS + "CommandLog").Elements(SERIALIZATION_NS + "Command"))
            {
                XAttribute initialAttribute = commandElement.Attribute("initial");
                if (initialAttribute == null || initialAttribute.Value != "true")
                {
                    continue;
                }
                CommandBase c = DeserializeCommand(commandElement);
                result.Add(c);
            }

            return result;
        }

        public static IList<CommandBase> DeserializeDocument(string fileName)
        {
            XDocument document = XDocument.Load(fileName);
            return DeserializeDocument(document);
        }
    }

    public delegate CommandBase ControllerCommandFactoryMethodDelegate();

    public static class CommandFactory<TCommand>
        where TCommand : CommandBase, new()
    {
        public static CommandBase Factory()
        {
            return new TCommand();
        }
    }
}