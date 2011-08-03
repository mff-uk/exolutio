using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Exolutio.Revalidation.XSLT
{
    public static class XDocumentXsltExtensions
    {
        public static XNamespace XSLT_NAMESPACE = @"http://www.w3.org/1999/XSL/Transform";

        private static XElement XslGenericElement(this XElement parentElement, string elementName)
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
            XElement templateElement = element.XslGenericElement("template");
            templateElement.AddAttributeWithValue("match", matchXPath);
            if (parameters != null)
                foreach (string parameter in parameters)
                {
                    XElement param = templateElement.XslGenericElement("param");
                    param.AddAttributeWithValue("name", parameter);
                }
            return templateElement;
        }
        
        public static XElement XslNamedTemplate(this XElement element, string name, params TemplateParameter[] parameters)
        {
            XElement templateElement = element.XslGenericElement("template");
            templateElement.AddAttributeWithValue("name", name);
            if (parameters != null)
            {
                foreach (TemplateParameter parameter in parameters)
                {
                    XElement param = templateElement.XslGenericElement("param");
                    param.AddAttributeWithValue("name", parameter.Name);
                    if (!string.IsNullOrEmpty(parameter.Type))
                    {
                        param.AddAttributeWithValue("as", parameter.Type);
                    }
                    if (!XPathExpr.IsNullOrEmpty(parameter.DefaultValue))
                    {
                        param.AddAttributeWithValue("select", parameter.DefaultValue);
                    }
                }
            }
            return templateElement;
        }

        public static XElement XslStylesheet(this XDocument document, string version)
        {
            XElement styleSheetElement = new XElement(XSLT_NAMESPACE + "stylesheet");
            styleSheetElement.Add(new XAttribute(XNamespace.Xmlns + "xsl", XSLT_NAMESPACE.NamespaceName));
            styleSheetElement.AddAttributeWithValue("version", version);
            document.Add(styleSheetElement);
            return styleSheetElement;
        }

        public static XElement XslCallTemplate(this XElement element, string templateName, params TemplateParameter[] parameters)
        {
            return XslCallTemplate(element, templateName, (IEnumerable<TemplateParameter>)parameters);
        }

        public static XElement XslCallTemplate(this XElement element, string templateName, IEnumerable<TemplateParameter> parameters)
        {
            XElement callTemplateElement = element.XslGenericElement("call-template");
            callTemplateElement.AddAttributeWithValue("name", templateName);
            if (parameters != null)
                foreach (TemplateParameter keyValuePair in parameters)
                {
                    XElement param = callTemplateElement.XslGenericElement("with-param");
                    param.AddAttributeWithValue("name", keyValuePair.Name);
                    param.AddAttributeWithValue("select", keyValuePair.Value);
                }
            return callTemplateElement;
        }

        public static XElement XslAttribute(this XElement element, string name, string value = null)
        {
            XElement attributeElement = element.XslGenericElement("attribute");
            attributeElement.AddAttributeWithValue("name", name);
            if (value != null)
            {
                XText valueText = new XText(value);
                attributeElement.Add(valueText);
            }
            return attributeElement;
        }

        public static XElement XslElement(this XElement element, string name, string value = null)
        {
            XElement elementConstructor = element.XslGenericElement("element");
            elementConstructor.AddAttributeWithValue("name", name);
            if (value != null)
            {
                XText valueText = new XText(value);
                elementConstructor.Add(valueText);
            }
            return elementConstructor;
        }

        public static XElement XslApplyTemplates(this XElement element, XPathExpr selectXPath, params TemplateParameter[] parameters)
        {
            return XslApplyTemplates(element, selectXPath, (IEnumerable<TemplateParameter>)parameters);
        }

        private static XElement XslApplyTemplates(this XElement element, XPathExpr selectXPath, IEnumerable<TemplateParameter> parameters)
        {
            XElement applyTemplatesElement = element.XslGenericElement("apply-templates");
            applyTemplatesElement.AddAttributeWithValue("select", selectXPath);
            if (parameters != null)
                foreach (TemplateParameter keyValuePair in parameters)
                {
                    XElement param = applyTemplatesElement.XslGenericElement("with-param");
                    param.AddAttributeWithValue("name", keyValuePair.Name);
                    param.AddAttributeWithValue("select", keyValuePair.Value);
                }
            return applyTemplatesElement;
        }

        public static XElement XslCopy(this XElement element)
        {
            return element.XslGenericElement("copy");
        }

        public static XElement XslCopyOf(this XElement element, XPathExpr selectXPath)
        {
            XElement copyOfElement = element.XslGenericElement("copy-of");
            copyOfElement.AddAttributeWithValue("select", selectXPath);
            return copyOfElement;
        }

        public static XElement XslForEach(this XElement element, XPathExpr selectXPath)
        {
            XElement forEachElement = element.XslGenericElement("for-each");
            forEachElement.AddAttributeWithValue("select", selectXPath);
            return forEachElement;
        }

        public static XElement XslForEachGroup(this XElement element, XPathExpr selectXPath)
        {
            XElement copyOfElement = element.XslGenericElement("for-each-group");
            copyOfElement.AddAttributeWithValue("select", selectXPath);
            return copyOfElement;
        }

        public static XElement XslValueOf(this XElement element, XPathExpr selectXPath)
        {
            XElement valueOfElement = element.XslGenericElement("value-of");
            valueOfElement.AddAttributeWithValue("select", selectXPath);
            return valueOfElement;
        }

        public static XElement XslVariable(this XElement element, string name, XPathExpr selectXPath)
        {
            XElement variableElement = element.XslGenericElement("variable");
            variableElement.AddAttributeWithValue("name", name);
            if (selectXPath != null)
                variableElement.AddAttributeWithValue("select", selectXPath);
            return variableElement;
        }

        public static XElement XslIf(this XElement element, XPathExpr test)
        {
            XElement valueOfElement = element.XslGenericElement("if");
            valueOfElement.AddAttributeWithValue("test", test);
            return valueOfElement;
        }

        public static XElement XslChoose(this XElement element)
        {
            XElement choose = element.XslGenericElement("choose");
            return choose;
        }

        public static XElement XslWhen(this XElement element, XPathExpr test)
        {
            XElement option = element.XslGenericElement("when");
            option.AddAttributeWithValue("test", test);
            return option;
        }

        public static XElement XslOtherwise(this XElement element)
        {
            XElement otherwise = element.XslGenericElement("otherwise");
            return otherwise;
        }

        public const string ParamExcept = "except";

        public const string ParamOnly = "only";

        public const string ParamCurrentInstance = "ci";

        public const string ParamAttributes = "attributes";

        public static TemplateParameter CreateCurrentInstanceParameterCall(string select)
        {
            return new TemplateParameter(ParamCurrentInstance, new XPathExpr(select));
        }

        public static TemplateParameter CreateCurrentInstanceParameterDeclaration()
        {
            return new TemplateParameter(ParamCurrentInstance, null) { Type = "item()*" };
        }

        public static TemplateParameter CreateCurrentInstanceParameterCall(XPathExpr select)
        {
            return new TemplateParameter(ParamCurrentInstance, select);
        }

        public struct TemplateParameter
        {
            public string Name { get; set; }
            public XPathExpr Value { get; set; }
            public string Type { get; set; }
            public XPathExpr DefaultValue { get; set; }

            public TemplateParameter(string name, XPathExpr value)
                : this()
            {
                Name = name;
                Value = value;
            }
        }
    }
}