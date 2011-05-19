using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting;
using System.Xml;
using EvoX.Model;
using EvoX.Model.PIM;
using EvoX.SupportingClasses.Reflection;

namespace EvoX.Controller.Commands.Reflection
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
        public void Serialize(CommandBase commandBase, XmlElement parentElement)
        {
            Type commandType = commandBase.GetType();
            CommandDescriptor commandParametersDescriptors = PublicCommandsHelper.GetCommandDescriptor(commandType);

            XmlDocument ownerDocument = parentElement.OwnerDocument;

            if (ownerDocument == null)
            {
                throw new ArgumentException("Owner document of parentElement must not be null", "parentElement");
            }

            XmlElement commandElement = ownerDocument.CreateElement(commandType.Name);
            parentElement.AppendChild(commandElement);

            XmlElement fullNameElement = ownerDocument.CreateElement("FullName");
            XmlText fullNameTextNode = ownerDocument.CreateTextNode(commandType.FullName);
            fullNameElement.AppendChild(fullNameTextNode);
            commandElement.AppendChild(fullNameElement);

            #region operation parameters
            foreach (ParameterDescriptor parameter in commandParametersDescriptors.Parameters)
            {
                XmlElement propertyElement = ownerDocument.CreateElement(parameter.ParameterPropertyName);
                XmlAttribute xmlAttribute = ownerDocument.CreateAttribute("parameterName");
                xmlAttribute.Value = parameter.ParameterName;
                propertyElement.Attributes.Append(xmlAttribute);

                object value = parameter.ParameterPropertyInfo.GetValue(commandBase, new object[0]);
                if (value is EvoXObject)
                {
                    XmlText textNode = ownerDocument.CreateTextNode(((EvoXObject)value).ID.ToString());
                    propertyElement.AppendChild(textNode);
                }
                else
                {
                    propertyElement.InnerText = value.ToString();
                }
                commandElement.AppendChild(propertyElement);
            }
            #endregion
        }

        public CommandBase DeserializeCommand(XmlElement parentElement)
        {
            XmlElement fullNameElement = (XmlElement)parentElement.FirstChild;
            string fullName = fullNameElement.FirstChild.Value;

            CommandBase commandObject = CreateCommandObject(fullName);

            CommandDescriptor commandParametersDescriptors = PublicCommandsHelper.GetCommandDescriptor(commandObject.GetType());

            for (int i = 1; i < parentElement.ChildNodes.Count; i++)
            {
                XmlElement element = parentElement.ChildNodes[i] as XmlElement;
                if (element != null)
                {
                    string propertyName = element.Name;
                    ParameterDescriptor parameter = commandParametersDescriptors.GetParameterByPropertyName(propertyName);
                    PropertyInfo propertyInfo = parameter.ParameterPropertyInfo;
                    string stringValue = element.FirstChild.Value;
                    object value = DeserializePropertyValue(propertyInfo, stringValue);
                    parameter.ParameterValue = value;
                }
            }

            FillParameters(commandObject, commandParametersDescriptors);

            return commandObject;
        }

        private static Project dummyProject = new Project();

        private static object DeserializePropertyValue(PropertyInfo propertyInfo, string stringValue)
        {
            if (propertyInfo.PropertyType == typeof(string))
            {
                return stringValue;
            }
            else if (propertyInfo.PropertyType.IsSubclassOf(typeof(EvoXObject)))
            {
                Guid id = Guid.Parse(stringValue);
                ConstructorInfo constructorInfo = propertyInfo.PropertyType.GetConstructor(new Type[] { typeof(Project), typeof(Guid) });
                dummyProject.mappingDictionary.Clear();
                object evoxObject = constructorInfo.Invoke(new object[] { dummyProject, id });
                return evoxObject;
            }
            else
            {
                MethodInfo parse = propertyInfo.PropertyType.GetMethod("Parse", new Type[] { typeof(string) });
                object parsed = parse.Invoke(null, new object[] { stringValue });
                return parsed;
            }
        }

        #endregion

        public List<CommandBase> DeserializeScript(XmlDocument document)
        {
            List<CommandBase> result = new List<CommandBase>();

            XmlElement root = (XmlElement)document.ChildNodes[1];
            foreach (XmlElement commandNode in root.ChildNodes)
            {
                CommandBase command = DeserializeCommand(commandNode);
                result.Add(command);
            }

            return result;
        }

        public void RunScript(Controller controller, XmlDocument document)
        {
            List<CommandBase> commands = DeserializeScript(document);
            controller.ExecuteCommands(commands);
        }

        public XmlDocument CreateEmptySerializationDocument()
        {
            XmlDocument document = new XmlDocument();
            XmlDeclaration xmlDeclaration = document.CreateXmlDeclaration("1.0", "utf-8", null);
            document.AppendChild(xmlDeclaration);
            XmlElement rootElement = document.CreateElement("CommandScript");
            document.AppendChild(rootElement);
            return document;
        }
#endif

        #endregion
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