using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting;
using System.Xml.Linq;
using Exolutio.Model;
using Exolutio.Model.PIM;
using Exolutio.SupportingClasses.Reflection;

namespace Exolutio.Controller.Commands.Reflection
{
    public class CommandSerializer
    {
        #region reflection




        #endregion

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

        public CommandBase CreateCommand(Controller controller, CommandDescriptor commandDescriptor)
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

                propertyInfo.SetValue(commandObject, value, null);
            }
        }

        #endregion

        #region script serialization

#if SILVERLIGHT
#else
        #region XML serialization

        public void Serialize(CommandBase commandBase, bool isUndo, bool isRedo)
        {
            SerializeRec(commandBase, true, RootElement, isUndo, isRedo);
        }

        public void SerializeRec(CommandBase command, bool isInitial, XElement parentElement, bool isUndo, bool isRedo)
        {
            Type commandType = command.GetType();
            
            XElement commandElement;
            commandElement = new XElement(exolutioNS + "Command");
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
            
            parentElement.Add(commandElement);

            XAttribute nameAttribute = new XAttribute("Name", commandType.Name);
            commandElement.Add(nameAttribute);

            XElement fullNameElement = new XElement(exolutioNS + "FullName");
            fullNameElement.Add(new XText(commandType.FullName));
            commandElement.Add(fullNameElement);

            if (command.Report != null && !string.IsNullOrEmpty(command.Report.Contents))
            {
                XElement report = new XElement(exolutioNS + "Report");
                report.Add(new XText(command.Report.Contents));
                commandElement.Add(report);
            }

            #region operation parameters
            if (PublicCommandsHelper.IsPublicCommand(commandType))
            {
                CommandDescriptor commandParametersDescriptors = PublicCommandsHelper.GetCommandDescriptor(commandType);
                if (commandParametersDescriptors.Parameters.Count > 0)
                {
                    XElement parametersElement = new XElement(exolutioNS + "Parameters");
                    commandElement.Add(parametersElement);
                    foreach (ParameterDescriptor parameter in commandParametersDescriptors.Parameters)
                    {
                        XElement parameterElement = new XElement(exolutioNS + "Parameter");
                        parameterElement.Add(new XElement(exolutioNS + "Name", parameter.ParameterName));
                        parameterElement.Add(new XElement(exolutioNS + "PropertyName", parameter.ParameterPropertyName));

                        object value = parameter.ParameterPropertyInfo.GetValue(command, new object[0]);
                        if (value is Guid)
                        {
                            string idText = value.ToString();
                            parameterElement.Add(new XElement(exolutioNS + "Value", idText));
                            if (command is StackedCommand)
                            {
                                Project p = ((StackedCommand)command).Controller.Project;
                                ExolutioObject component;
                                if (p.TryTranslateObject((Guid)value, out component))
                                {
                                    parameterElement.Add(new XElement(exolutioNS + "ValueText", component.ToString()));
                                }
                            }
                        }
                        else if (value is ExolutioObject)
                        {
                            string idText = ((ExolutioObject)value).ID.ToString();
                            parameterElement.Add(new XElement(exolutioNS + "ValueID", idText));
                            parameterElement.Add(new XElement(exolutioNS + "ValueText", idText));
                        }
                        else
                        {
                            string valueText = value.ToString();
                            parameterElement.Add(new XElement(exolutioNS + "Value", valueText));
                        }
                        parametersElement.Add(parameterElement);
                    }
                }
            }

            #endregion

            if (command is MacroCommand && ((MacroCommand)command).Commands.Count > 0)
            {
                XElement subCommandsElement = new XElement(exolutioNS + "SubCommands");
                subCommandsElement.Add(new XAttribute("count", ((MacroCommand)command).Commands.Count));
                commandElement.Add(subCommandsElement);
                foreach (CommandBase subCommand in ((MacroCommand)command).Commands)
                {
                    SerializeRec(subCommand, false, subCommandsElement, isUndo, isRedo);
                }
            }
        }

        public CommandBase DeserializeCommand(XElement commandElement)
        {
            XElement fullNameElement = commandElement.Element(exolutioNS + "FullName");
            CommandBase commandObject = CreateCommandObject(fullNameElement.Value);

            CommandDescriptor commandParametersDescriptors = PublicCommandsHelper.GetCommandDescriptor(commandObject.GetType());

            foreach (XElement parameterElement in commandElement.Element(exolutioNS + "Parameters").Elements(exolutioNS + "Parameter"))
            {
                string propertyName = parameterElement.Element(exolutioNS + "PropertyName").Value;
                ParameterDescriptor parameter = commandParametersDescriptors.GetParameterByPropertyName(propertyName);
                PropertyInfo propertyInfo = parameter.ParameterPropertyInfo;
                string stringValue = parameterElement.Element(exolutioNS + "Value").Value;
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

        #endregion

        public List<CommandBase> DeserializeScript(XDocument document)
        {
            //List<CommandBase> result = new List<CommandBase>();

            //XmlElement root = (XmlElement)document.ChildNodes[1];
            //foreach (XmlElement commandNode in root.ChildNodes)
            //{
            //    CommandBase command = DeserializeCommand(commandNode);
            //    result.Add(command);
            //}

            //return result;
            throw new NotImplementedException("Member CommandSerializer.DeserializeScript not implemented.");
        }

        public void RunScript(Controller controller, XDocument document)
        {
            List<CommandBase> commands = DeserializeScript(document);
            controller.ExecuteCommands(commands);
        }

        private static readonly XNamespace exolutioNS = @"http://eXolutio.eu/Commands/CommandLog/";

        public XDocument SerializationDocument { get; private set; }
        private XElement RootElement { get; set; }

        public XDocument CreateEmptySerializationDocument()
        {
            SerializationDocument = new XDocument(new XDeclaration("1.0", "utf-8", null));
            
            XElement elCommandLog = new XElement(exolutioNS + "CommandLog");
            elCommandLog.Add(new XAttribute(XNamespace.Xmlns + "eXoLog", exolutioNS.NamespaceName));
            SerializationDocument.Add(elCommandLog);
            RootElement = elCommandLog;
            return SerializationDocument;
        }
#endif

        #endregion

        public IList<CommandBase> DeserializeDocument(string fileName)
        {
            List<CommandBase> result = new List<CommandBase>();
            SerializationDocument = XDocument.Load(fileName);
            foreach (XElement commandElement in SerializationDocument.Element(exolutioNS + "CommandLog").Elements(exolutioNS + "Command"))
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
    }

    public static class CommandFactory<TCommand>
        where TCommand : CommandBase, new()
    {
        public static CommandBase Factory()
        {
            return new TCommand();
        }
    }
}