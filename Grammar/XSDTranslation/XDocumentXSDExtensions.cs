using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;

namespace Exolutio.Model.PSM.Grammar.XSDTranslation
{
    public static class XDocumentXSDExtensions
    {
        public static XNamespace XSD_NAMESPACE = @"http://www.w3.org/2001/XMLSchema";

        private static XElement XsdGenericElement(this XElement parentElement, string elementName)
        {
            XElement element = new XElement(XSD_NAMESPACE + elementName);
            parentElement.Add(element);
            return element;
        }

        public static XElement OutputLiteralElement(this XElement parentElement, string elementName)
        {
            XElement element = new XElement(elementName);
            parentElement.Add(element);
            return element;
        }

        public static XElement XsdSchema(this XDocument document)
        {
            XElement schemaElement = new XElement(XSD_NAMESPACE + "schema");
            schemaElement.Add(new XAttribute(XNamespace.Xmlns + "xs", XSD_NAMESPACE.NamespaceName));
            document.Add(schemaElement);
            return schemaElement;
        }

        public static void AddAttributeWithValue(this XElement element, string attributeName, string value)
        {
            XAttribute XElement = new XAttribute(attributeName, value ?? string.Empty);
            element.Add(XElement);
        }

        /// <summary>
        /// Writes complexType element
        /// </summary>
        public static XElement XsdComplexType(this XElement parentElement)
        {
            return parentElement.XsdGenericElement("complexType");
        }
        /// <summary>
        /// Writes complexType element (with specified name)
        /// </summary>
        public static XElement XsdComplexType(this XElement parentElement, string name)
        {
            XElement complexType = parentElement.XsdGenericElement("complexType");
            complexType.AddAttributeWithValue("name", name);
            return complexType;
        }

        /// <summary>
        /// Writes sequence element
        /// </summary>
        public static XElement XsdSequence(this XElement parentElement)
        {
            return parentElement.XsdGenericElement("sequence");
        }

        /// <summary>
        /// Writes complexContent element
        /// </summary>
        public static XElement XsdComplexContent(this XElement parentElement)
        {
            return parentElement.XsdGenericElement("complexContent");
        }

        /// <summary>
        /// Writes extension element
        /// </summary>
        /// <param name="base"></param>
        public static XElement XsdExtension(this XElement parentElement, string @base)
        {
            XElement extension = parentElement.XsdGenericElement("extension");
            extension.AddAttributeWithValue("base", @base);
            return extension;
        }

        /// <summary>
        /// writes "element" element. with specified name
        /// </summary>
        /// <param name="name">name attribute of the element</param>
        public static XElement XsdElement(this XElement parentElement, string name)
        {
            XElement element = parentElement.XsdGenericElement("element");
            element.AddAttributeWithValue("name", name);
            return element;
        }

        /// <summary>
        /// Writes minOccurs and maxOccurs attributes (if cardinalities are not equal to 1)
        /// </summary>
        public static void CardinalityAttributes(this XElement parentElement, IHasCardinality component)
        {
            CardinalityAttributes(parentElement, component.Lower, component.Upper);
        }

        /// <summary>
        /// Writes minOccurs and maxOccurs attributes (if parameters are not equal to 1)
        /// </summary>
        /// <param name="lower">value for minOccurs attribute</param>
        /// <param name="upper">value for maxOccurs attribute</param>
        public static void CardinalityAttributes(this XElement parentElement, uint lower, UnlimitedInt upper)
        {
            if (lower != 1)
                parentElement.AddAttributeWithValue("minOccurs", lower.ToString());
            if (upper.Value != 1)
            {
                if (upper.IsInfinity)
                    parentElement.AddAttributeWithValue("maxOccurs", "unbounded");
                else
                    parentElement.AddAttributeWithValue("maxOccurs", upper.ToString());
            }
        }

        /// <summary>
        /// Writes abstract="true" attribute.
        /// </summary>
        public static void AbstractAttribute(this XElement parentElement)
        {
            parentElement.AddAttributeWithValue("abstract", "true");
        }

        /// <summary>
        /// Writes type attribute
        /// </summary>
        /// <param name="psmAttribute">psmAttribute whose <see cref="Model.TypedElement.Type"/> attribute is being written</param>
        /// <param name="simpleTypeWriter">writer where simple type definition is written if the type was not
        /// yet used</param>
        /// <param name="useOccurs">if set to true, minOccurs and maxOccurs attributes are also written if
        /// <paramref name="psmAttribute"/> multipicity is non-default</param>
        /// <param name="forceOptional">if set to <c>true</c> multiplicity of the attribute is ignored and 
        /// use="optional" is written.</param>
        public static void TypeAttribute(this XElement parentElement, PSMAttribute psmAttribute, bool useOccurs, bool forceOptional)
        {
            if (psmAttribute.AttributeType == null)
            {
                
            }
            else
            {
                parentElement.AddAttributeWithValue("type", "xs:" + psmAttribute.AttributeType.Name);
            }
            
            if (!String.IsNullOrEmpty(psmAttribute.DefaultValue))
            {
                parentElement.AddAttributeWithValue("default", psmAttribute.DefaultValue);
            }
            
            if (forceOptional)
            {
                parentElement.AddAttributeWithValue("use", "optional");
            }
            else
            {
                if (!useOccurs)
                {
                    if (psmAttribute.Lower == 0)
                    {
                        parentElement.AddAttributeWithValue("use", "optional");
                    }
                    else
                    {
                        if (String.IsNullOrEmpty(psmAttribute.DefaultValue))
                            parentElement.AddAttributeWithValue("use", "required");
                    }
                }
                else
                {
                    parentElement.CardinalityAttributes(psmAttribute.Lower, psmAttribute.Upper);
                }
            }
        }

        /// <summary>
        /// Writes type attribute.
        /// </summary>
        /// <param name="type">value of the attribute</param>
        public static void TypeAttribute(this XElement parentElement, string type)
        {
            parentElement.AddAttributeWithValue("type", type);
        }


        /// <summary>
        /// Writes attributeGroup element
        /// </summary>
        /// <param name="name">name of the attributeGroup</param>
        public static XElement XsdAttributeGroup(this XElement parentElement, string name)
        {
            XElement attributeGroup = parentElement.XsdGenericElement("attributeGroup");
            attributeGroup.AddAttributeWithValue("name", name);
            return attributeGroup;
        }

        /// <summary>
        /// Writes reference to an attributeGroup. 
        /// </summary>
        /// <param name="ref">name of the referenced group</param>
        public static XElement XsdAttributeGroupRef(this XElement parentElement, string @ref)
        {
            XElement attributeGroupRef = parentElement.XsdGenericElement("attributeGroup");
            attributeGroupRef.AddAttributeWithValue("ref", @ref);
            return attributeGroupRef;
        }

        /// <summary>
        /// Writes group element
        /// </summary>
        /// <param name="name">name of the group</param>
        public static XElement XsdGroup(this XElement parentElement, string name)
        {
            XElement group = parentElement.XsdGenericElement("group");
            group.AddAttributeWithValue("name", name);
            return group;
        }

        /// <summary>
        /// Writes reference to a group
        /// </summary>
        /// <param name="ref">name of the referenced group</param>
        public static XElement XsdGroupRef(this XElement parentElement, string @ref)
        {
            XElement groupRef = parentElement.XsdGenericElement("group");
            groupRef.AddAttributeWithValue("ref", @ref);
            return groupRef;
        }

        /// <summary>
        /// Writes <see cref="PSMAttribute"/> as an XML attribute (with name and value and default value)
        /// </summary>
        /// <param name="name">name of the attribute</param>
        /// <param name="psmAttribute">written attribute</param>
        /// <param name="forceOptional">if set to <c>true</c> multiplicity of the attribute is ignored and 
        /// use="optional" is written.</param>
        public static XElement XsdAttribute(this XElement parentElement, string name, PSMAttribute psmAttribute, bool forceOptional)
        {
            if (psmAttribute.Lower == 0 && psmAttribute.Upper == 0)
                return null;
            XElement attribute = parentElement.XsdGenericElement("attribute");
            attribute.AddAttributeWithValue("name", name);
            TypeAttribute(attribute, psmAttribute, false, forceOptional);
            return attribute;
        }

        /// <summary>
        /// Writes <see cref="PSMAttribute"/> as an XML element (with name, value and multiplicity)
        /// </summary>
        /// <param name="name">name of the attribute</param>
        /// <param name="psmAttribute">written attribute</param>
        public static XElement XsdAttributeAsElement(this XElement parentElement, string name, PSMAttribute psmAttribute)
        {
            XElement attribute = parentElement.XsdGenericElement("element");
            attribute.AddAttributeWithValue("name", name);
            attribute.TypeAttribute(psmAttribute, true, false);
            return attribute;
        }

        /// <summary>
        /// Writes choice element
        /// </summary>
        public static XElement XsdChoice(this XElement parentElement)
        {
            return parentElement.XsdGenericElement("choice");
        }

        /// <summary>
        /// Writes all element
        /// </summary>
        public static XElement XsdAll(this XElement parentElement)
        {
            return parentElement.XsdGenericElement("all");
        }

        public static void WriteAllowAnyAttribute(this XElement parentElement)
        {
            parentElement.XsdGenericElement("anyAttribute");
        }

        public static XElement XsdInclude(this XElement parentElement, string schemaLocation)
        {
            XElement include = parentElement.XsdGenericElement("include");
            include.AddAttributeWithValue("schemaLocation", schemaLocation);
            return include; 
        }

        public static XElement XsdImport(this XElement parentElement, string schemaLocation, string @namespace)
        {
            XElement import = parentElement.XsdGenericElement("include");
            import.AddAttributeWithValue("schemaLocation", schemaLocation);
            import.AddAttributeWithValue("namespace", @namespace);
            return import; 
        }

        public static XElement XsdSimpleType(this XElement parentElement, string name, string restrictionBase, string facets)
        {
            XElement simpleType = parentElement.XsdGenericElement("simpleType");
            simpleType.AddAttributeWithValue("name", name);
            XElement restriction = simpleType.XsdGenericElement("restriction");
            restriction.AddAttributeWithValue("base", restrictionBase);

            facets = "<foo xmlns:xs=\"" + XSD_NAMESPACE + "\">" + facets + "</foo>";
            using (System.IO.StringReader sr = new System.IO.StringReader(facets))
            {
                XElement e = XElement.Load(sr);

                foreach (XElement xElement in e.Elements())
                {
                    restriction.Add(xElement);
                }
            }
            
            return simpleType;
        }
    }
}