using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;

namespace Exolutio.Model.PSM.Grammar.SchematronTranslation
{
    public static class XDocumentSchematronExtensions
    {
        public static XNamespace SCHEMATRON_NAMESPACE = @"http://purl.oclc.org/dsdl/schematron";

        private static XElement SchematronGenericElement(this XElement parentElement, string elementName)
        {
            XElement element = new XElement(SCHEMATRON_NAMESPACE + elementName);
            parentElement.Add(element);
            return element;
        }

        public static XElement OutputLiteralElement(this XElement parentElement, string elementName)
        {
            XElement element = new XElement(elementName);
            parentElement.Add(element);
            return element;
        }

        public static XElement SchematronSchema(this XDocument document)
        {
            XElement schemaElement = new XElement(SCHEMATRON_NAMESPACE + "schema");
            schemaElement.Add(new XAttribute(XNamespace.Xmlns + "sch", SCHEMATRON_NAMESPACE.NamespaceName));
            document.Add(schemaElement);
            return schemaElement;
        }

        public static void AddAttributeWithValue(this XElement element, string attributeName, string value)
        {
            XAttribute XElement = new XAttribute(attributeName, value ?? string.Empty);
            element.Add(XElement);
        }

        public static XElement SchematronPattern(this XElement parentElement, string id = null)
        {
            XElement pattern = parentElement.SchematronGenericElement("pattern");
            if (!string.IsNullOrEmpty(id))
                pattern.AddAttributeWithValue("id", id);
            return pattern;
        }

        public static XElement SchematronRule(this XElement parentElement, string context)
        {
            XElement rule = parentElement.SchematronGenericElement("rule");
            rule.AddAttributeWithValue("context", context);
            return rule;
        }

        public static XElement SchematronAssert(this XElement parentElement, string test, string message = null)
        {
            XElement assert = parentElement.SchematronGenericElement("assert");
            assert.AddAttributeWithValue("test", test);
            if (!string.IsNullOrEmpty(message))
            {
                assert.Add(new XText(message));
            }
            return assert;
        }

        public static XElement SchematronReport(this XElement parentElement, string test, string message = null)
        {
            XElement report = parentElement.SchematronGenericElement("report");
            report.AddAttributeWithValue("test", test);
            if (!string.IsNullOrEmpty(message))
            {
                report.Add(new XText(message));
            }
            return report;
        }

        public static XElement SchematronVariable(this XElement parentElement, string name, string value)
        {
            XElement let = parentElement.SchematronGenericElement("let");
            let.AddAttributeWithValue("name", value);
            return let;
        }

        public static XElement SchematronValueOf(this XElement parentElement, string select)
        {
            XElement valueOf = parentElement.SchematronGenericElement("value-of");
            valueOf.AddAttributeWithValue("select", select);
            return valueOf;
        }

        public static XElement SchematronParam(this XElement parentElement, string name, string value)
        {
            XElement param = parentElement.SchematronGenericElement("param");
            param.AddAttributeWithValue("name", name);
            param.AddAttributeWithValue("value", value);
            return param;
        }

        public static XElement SchematronLet(this XElement parentElement, string name, string value)
        {
            XElement param = parentElement.SchematronGenericElement("let");
            param.AddAttributeWithValue("name", name);
            param.AddAttributeWithValue("value", value);
            return param;
        }
    }
}