using System.Collections.Generic;
using System.Xml.Linq;

namespace Exolutio.Revalidation.XSLT
{
    public static class XDocumentXsltExtensions
    {
        public static XNamespace XSLT_NAMESPACE = @"http://www.w3.org/1999/XSL/Transform";

        public static XElement XslElement(this XElement parentElement, string elementName)
        {
            XElement element = new XElement(XSLT_NAMESPACE + elementName);
            parentElement.Add(element);
            return element;
        }
        
        public static void AddAttributeWithValue(this XElement element, string attributeName, string value)
        {
            XAttribute XElement = new XAttribute(attributeName, value);
            element.Add(XElement);
        }

        public static XElement XslTemplate(this XElement element, XPathExpr matchXPath, params string[] parameters)
        {
            XElement templateElement = element.XslElement("template");
            templateElement.AddAttributeWithValue("match", matchXPath);
            if (parameters != null)
                foreach (string parameter in parameters)
                {
                    XElement param = templateElement.XslElement("param");
                    param.AddAttributeWithValue("name", parameter);
                }
            return templateElement;
        }

        public static XElement XslNamedTemplate(this XElement element, string name, params string[] parameters)
        {
            XElement templateElement = element.XslElement("template");
            templateElement.AddAttributeWithValue("name", name);
            if (parameters != null)
                foreach (string parameter in parameters)
                {
                    XElement param = templateElement.XslElement("param");
                    param.AddAttributeWithValue("name", parameter);
                }
            return templateElement;
        }

        public static XElement XslStylesheet(this XDocument document, string version)
        {
            XElement element = new XElement(XSLT_NAMESPACE + "stylesheet");
            element.AddAttributeWithValue("version", version);
            document.Add(element);
            return element;
        }

        public static XElement XslCallTemplate(this XElement element, string templateName, params TemplateParameter[] parameters)
        {
            return XslCallTemplate(element, templateName, (IEnumerable<TemplateParameter>)parameters);
        }

        public static XElement XslCallTemplate(this XElement element, string templateName, IEnumerable<TemplateParameter> parameters)
        {
            XElement callTemplateElement = element.XslElement("call-template");
            callTemplateElement.AddAttributeWithValue("name", templateName);
            if (parameters != null)
                foreach (TemplateParameter keyValuePair in parameters)
                {
                    XElement param = callTemplateElement.XslElement("with-param");
                    param.AddAttributeWithValue("name", keyValuePair.Name);
                    param.AddAttributeWithValue("select", keyValuePair.Value);
                }
            return callTemplateElement;
        }

        public static XElement XslAttribute(this XElement element, string name, string value)
        {
            XElement attributeElement = element.XslElement("attribute");
            attributeElement.AddAttributeWithValue("name", name);
            if (value != null)
            {
                XText valueText = new XText(value);
                attributeElement.Add(valueText);
            }
            return attributeElement;
        }

        public static XElement XslApplyTemplates(this XElement element, XPathExpr selectXPath, params TemplateParameter[] parameters)
        {
            return XslApplyTemplates(element, selectXPath, (IEnumerable<TemplateParameter>)parameters);
        }

        private static XElement XslApplyTemplates(this XElement element, XPathExpr selectXPath, IEnumerable<TemplateParameter> parameters)
        {
            XElement applyTemplatesElement = element.XslElement("apply-templates");
            applyTemplatesElement.AddAttributeWithValue("select", selectXPath);
            if (parameters != null)
                foreach (TemplateParameter keyValuePair in parameters)
                {
                    XElement param = applyTemplatesElement.XslElement("with-param");
                    param.AddAttributeWithValue("name", keyValuePair.Name);
                    param.AddAttributeWithValue("select", keyValuePair.Value);
                }
            return applyTemplatesElement;
        }

        public static XElement XslCopy(this XElement element)
        {
            return element.XslElement("copy");
        }

        public static XElement XslCopyOf(this XElement element, XPathExpr selectXPath)
        {
            XElement copyOfElement = element.XslElement("copy-of");
            copyOfElement.AddAttributeWithValue("select", selectXPath);
            return copyOfElement;
        }

        public static XElement XslForEach(this XElement element, XPathExpr selectXPath)
        {
            XElement forEachElement = element.XslElement("for-each");
            forEachElement.AddAttributeWithValue("select", selectXPath);
            return forEachElement;
        }

        public static XElement XslForEachGroup(this XElement element, XPathExpr selectXPath)
        {
            XElement copyOfElement = element.XslElement("for-each-group");
            copyOfElement.AddAttributeWithValue("select", selectXPath);
            return copyOfElement;
        }

        public static XElement XslValueOf(this XElement element, XPathExpr selectXPath)
        {
            XElement valueOfElement = element.XslElement("value-of");
            valueOfElement.AddAttributeWithValue("select", selectXPath);
            return valueOfElement;
        }

        public static XElement XslVariable(this XElement element, string name, XPathExpr selectXPath)
        {
            XElement variableElement = element.XslElement("variable");
            variableElement.AddAttributeWithValue("name", name);
            if (selectXPath != null)
                variableElement.AddAttributeWithValue("select", selectXPath);
            return variableElement;
        }

        public static XElement XslIf(this XElement element, XPathExpr test)
        {
            XElement valueOfElement = element.XslElement("if");
            valueOfElement.AddAttributeWithValue("test", test);
            return valueOfElement;
        }

        public static XElement XslChoose(this XElement element)
        {
            XElement choose = element.XslElement("choose");
            return choose;
        }

        public static XElement XslWhen(this XElement element, XPathExpr test)
        {
            XElement option = element.XslElement("when");
            option.AddAttributeWithValue("test", test);
            return option;
        }

        public static XElement XslOtherwise(this XElement element)
        {
            XElement otherwise = element.XslElement("otherwise");
            return otherwise;
        }

        public const string ParamExcept = "except";

        public const string ParamOnly = "only";

        public const string ParamCurrentGroup = "cg";

        public const string ParamAttributes = "attributes";

        public struct TemplateParameter
        {
            public string Name { get; set; }
            public XPathExpr Value { get; set; }

            public TemplateParameter(string name, XPathExpr value)
                : this()
            {
                Name = name;
                Value = value;
            }
        }
    }
}