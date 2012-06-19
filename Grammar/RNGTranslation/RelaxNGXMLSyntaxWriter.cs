using System;
using System.Xml;
using System.Xml.Linq;
using Exolutio.Model.OCL.AST;

namespace Exolutio.Model.PSM.Grammar.RNGTranslation
{
    public class RelaxNGXmlSyntaxWriter
    {
        public static XNamespace RNG_NAMESPACE = @"http://relaxng.org/ns/structure/1.0";
        public static XNamespace XSD_TYPES_NAMESPACE = @"http://www.w3.org/2001/XMLSchema-datatypes";

        protected XElement RngGenericElement(XElement parentElement, string elementName)
        {
            XElement element = new XElement(RNG_NAMESPACE + elementName);
            parentElement.Add(element);
            return element;
        }

        public XElement RngGrammar(XDocument document)
        {
            XElement grammarElement = new XElement(RNG_NAMESPACE + "grammar");
            grammarElement.Add(new XAttribute(XNamespace.Xmlns + "rng", RNG_NAMESPACE.NamespaceName));
            AddAttributeWithValue(grammarElement, "datatypeLibrary", XSD_TYPES_NAMESPACE.NamespaceName);
            document.Add(grammarElement);
            return grammarElement;
        }

        public XElement RngChoice(XElement parentElement)
        {
            return RngGenericElement(parentElement, "choice");
        }

        public XElement RngElement(XElement parentElement, string name)
        {
            XElement elementElement = RngGenericElement(parentElement, "element");
            AddAttributeWithValue(elementElement, "name", name);
            return elementElement;
        }

        public XElement RngDefine(XElement parentElement, string name)
        {
            XElement elementElement = RngGenericElement(parentElement, "define");
            AddAttributeWithValue(elementElement, "name", name);
            return elementElement;
        }

        public XElement RngAttributeAsElement(XElement parentElement, string name, PSMAttribute psmAttribute)
        {
            XElement attribute = RngElement(parentElement, name);
            GiveTypeToAttribute(attribute, psmAttribute);
            return attribute;
        }

        public XElement RngAttribute(XElement parentElement, string name, PSMAttribute psmAttribute)
        {
            XElement attribute = RngGenericElement(parentElement, "attribute");
            AddAttributeWithValue(attribute, "name", name);
            GiveTypeToAttribute(attribute, psmAttribute);
            return attribute;
        }

        public void GiveTypeToAttribute(XElement parentElement, PSMAttribute psmAttribute)
        {
            if (psmAttribute.AttributeType == null)
            {
                RngText(parentElement);
            }
            else
            {
                RngData(parentElement, psmAttribute.AttributeType.Name);
            }

            if (!String.IsNullOrEmpty(psmAttribute.DefaultValue))
            {
                // TODO: default value, check, whether RNG allows that, if not, give warning
                //parentElement.AddAttributeWithValue("default", psmAttribute.DefaultValue);
            }
            
            HandleCardinality(parentElement, psmAttribute.Lower, psmAttribute.Upper);
        }

        public XElement RngEmpty(XElement parentElement)
        {
            return RngGenericElement(parentElement, "empty");
        }
        
        public XElement RngGroup(XElement parentElement)
        {
            return RngGenericElement(parentElement, "group");
        }
        
        public XElement RngInterleave(XElement parentElement)
        {
            return RngGenericElement(parentElement, "interleave");
        }
        
        public XElement RngOneOrMore(XElement parentElement)
        {
            return RngGenericElement(parentElement, "oneOrMore");
        }
        
        public XElement RngOptional(XElement parentElement)
        {
            return RngGenericElement(parentElement, "optional");
        }
        
        public XElement RngRef(XElement parentElement, string name)
        {
            XElement refElement = RngGenericElement(parentElement, "ref");
            AddAttributeWithValue(refElement, "name", name);
            return refElement;
        }

        public XElement RngText(XElement parentElement)
        {
            return RngGenericElement(parentElement, "text");
        }

        public XElement RngData(XElement parentElement, string typeName)
        {
            XElement dataElement = RngGenericElement(parentElement, "data");
            AddAttributeWithValue(dataElement, "type", typeName);
            return dataElement;
        }

        public XElement RngValue(XElement parentElement)
        {
            return RngGenericElement(parentElement, "value");
        }
        
        public XElement RngZeroOrMore(XElement parentElement)
        {
            return RngGenericElement(parentElement, "zeroOrMore");
        }

        public XElement RngStart(XElement parentElement)
        {
            return RngGenericElement(parentElement, "start");
        }

        public void HandleCardinality(XElement parentElement, IHasCardinality cardinalityElement)
        {
            HandleCardinality(parentElement, cardinalityElement.Lower, cardinalityElement.Upper);
        }

        public void HandleCardinality(XElement parentElement, uint lower, UnlimitedInt upper)
        {
            XElement cardinalityElement = null;
            if (lower == 1 && upper == 1)
            {
                return;
            }
            else if (lower == 0 && upper == 1)
            {
                // optional
                cardinalityElement = new XElement(RNG_NAMESPACE + "optional");
            }
            else if (lower == 0 && upper == UnlimitedInt.Infinity)
            {
                // zeroOrMore
                cardinalityElement = new XElement(RNG_NAMESPACE + "zeroOrMore");
            }
            else if (lower == 1 && upper == UnlimitedInt.Infinity)
            {
                // oneOrMore
                cardinalityElement = new XElement(RNG_NAMESPACE + "oneOrMore");
            }
            else
            {
                // nonDefaultCardinality
                // required occurences
                XElement prev = parentElement;
                XComment comment = new XComment(string.Format("Here follows expansion of occurence '{0}..{1}'.", lower, upper));
                prev.AddBeforeSelf(comment);
                for (int i = 0; i < lower; i++)
                {
                    XElement dupl = new XElement(parentElement);
                    prev.AddAfterSelf(dupl);
                    prev = dupl;
                }
                // optional occurences
                if (!upper.IsInfinity)
                {
                    for (int i = 0; i < (upper.Value - lower); i++)
                    {
                        XElement optional = new XElement(RNG_NAMESPACE + "optional");
                        XElement dupl = new XElement(parentElement);
                        optional.Add(dupl);
                        prev.AddAfterSelf(optional);
                        prev = optional;
                    }
                }
                else
                {
                    XElement optional = new XElement(RNG_NAMESPACE + "zeroOrMore");
                    XElement dupl = new XElement(parentElement);
                    optional.Add(dupl);
                    prev.AddAfterSelf(optional);
                    prev = optional;
                }

                parentElement.Remove();
            }
            if (cardinalityElement != null)
            {
                parentElement.ReplaceWith(cardinalityElement);
                cardinalityElement.Add(parentElement);
            }
        }

        public void AddComment(XElement parentElement, string comment)
        {
            XComment xComment = new XComment(comment);
            parentElement.Add(xComment);
        }

        XDocument doc;

        public void CreateInitialDeclarations(out XElement topElement)
        {
            doc = new XDocument(new XDeclaration("1.0", "utf-8", null));
            XElement rngSchema = this.RngGrammar(doc);
            topElement = rngSchema;
        }

        public XDocument GetFinalResult()
        {
            return doc;
        }

        public void AddAttributeWithValue(XElement element, string attributeName, string value)
        {
            XAttribute XElement = new XAttribute(attributeName, value ?? string.Empty);
            element.Add(XElement);
        }
    }
}