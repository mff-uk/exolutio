using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using EvoX.Model.Versioning;
using EvoX.Model.ViewHelper;
using EvoX.SupportingClasses;
using EvoX.SupportingClasses.Annotations;

namespace EvoX.Model.Serialization
{
    public interface IEvoXSerializable
    {
        void Deserialize(XElement parentNode, SerializationContext context);

        void Serialize(XElement parentNode, SerializationContext context);

        Project Project { get; }
    }

    public static class IEvoXSerializableExt
    {
        public delegate T CreateComponentDelegate<T>(Project project) where T : IEvoXSerializable;

        public delegate void RegisterComponentDelegate<T>(T member) where T : IEvoXSerializable;

        #region deserialization 

        public static void DeserializeWrappedCollection<T>(this IEvoXSerializable component, [NotNull] string collectionNodeName, UndirectCollection<T> collection, CreateComponentDelegate<T> createAction, XElement parentNode, SerializationContext context, bool missingAsEmpty = false)
            where T : EvoXObject
        {
            XElement collectionNode = parentNode.Element(context.EvoXNS + collectionNodeName);

            if (collectionNode == null && missingAsEmpty)
            {
                return;
            }

            if (collectionNode == null)
            {
                context.Log.AddErrorFormat(SerializationLogMessages.Collection_node__0__not_found_in_node__1__,
                                     collectionNodeName, parentNode);
                return;
            }

            int? count = null;
            if (collectionNode.Attribute("Count") == null)
            {
                context.Log.AddWarningFormat(SerializationLogMessages.Collection_node__0__missing_attribute__Count__,
                                       collectionNode);
            }
            else
            {
                count = int.Parse(collectionNode.Attribute("Count").Value);
            }

            int subelementsCount = 0;
            foreach (XElement xmlElement in collectionNode.Elements())
            {
                T member = createAction(component.Project);
                member.Deserialize(xmlElement, context);
                collection.Add(member);
                subelementsCount++;
            }

            if (count.HasValue && count != subelementsCount)
            {
                context.Log.AddWarningFormat(SerializationLogMessages.InconsistentCount, count, collectionNode, subelementsCount);
            }
        }
        
        public static void DeserializeWrappedIDRefCollection<T>(this IEvoXSerializable component, [NotNull] string collectionNodeName,
            [NotNull] string idRefAttribute, UndirectCollection<T> idRefCollection,
            XElement parentNode, SerializationContext context) where T : EvoXObject
        {
            XElement collectionNode = parentNode.Element(context.EvoXNS + collectionNodeName);

            if (collectionNode == null)
            {
                context.Log.AddErrorFormat(SerializationLogMessages.Collection_node__0__not_found_in_node__1__,
                                     collectionNodeName, parentNode);
                return;
            }

            int? count = null;
            if (collectionNode.Attribute("Count") == null)
            {
                context.Log.AddWarningFormat(SerializationLogMessages.Collection_node__0__missing_attribute__Count__,
                                       collectionNode);
            }
            else
            {
                count = int.Parse(collectionNode.Attribute("Count").Value);
            }

            int subelementsCount = 0;
            foreach (XElement xmlElement in collectionNode.Elements())
            {
                Guid deserializedIdRef = component.DeserializeIDRef(idRefAttribute, xmlElement, context);
                idRefCollection.AddAsGuid(deserializedIdRef);
                subelementsCount++;
            }

            if (count.HasValue && count != subelementsCount)
            {
                context.Log.AddWarningFormat(SerializationLogMessages.InconsistentCount, count, collectionNode, subelementsCount);
            }
        }

        public static void DeserializeFromChildElement(this IEvoXSerializable component, [NotNull]string elementName, XElement parentNode, SerializationContext context)
        {
            if (String.IsNullOrEmpty(elementName))
            {
                throw new ArgumentNullException("elementName");
            }

            XElement serializedComponentNode = parentNode.Element(context.EvoXNS + elementName);
            if (serializedComponentNode == null)
            {
                context.Log.AddErrorFormat("Element '{0}' not found in node {1}.", elementName, parentNode);
                return;
            }
            
            component.Deserialize(serializedComponentNode, context);
        }

        /// <summary>
        /// Locates and reads reference to another component in the project
        /// </summary>
        /// <param name="component"></param>
        /// <param name="attributeName">name of the attribute containing the reference</param>
        /// <param name="parentNode">node where the attribute is looked up</param>
        /// <param name="context">deserialization context</param>
        /// <param name="optional">if set to <c>false</c> (default), error is written into the log when the attribute is not found</param>
        public static Guid DeserializeIDRef(this IEvoXSerializable component, string attributeName, XElement parentNode, SerializationContext context, bool optional = false)
        {
            if (parentNode.Attribute(attributeName) == null)
            {
                if (optional)
                {
                    return Guid.Empty;
                }
                else
                {
                    context.Log.AddErrorFormat("Attribute '{0}' not found in the node {1}.", attributeName, parentNode);
                    return Guid.Empty;
                }
            }
            return SerializationContext.DecodeGuid(parentNode.Attribute(attributeName).Value);
        }

        public static void DeserializeCardinality(this IHasCardinality component, XElement parentNode, SerializationContext context)
        {
            if (parentNode.Attribute("Lower") != null)
            {
                component.Lower = SerializationContext.DecodeUInt(parentNode.Attribute("Lower").Value);
            }
            else
            {
                component.Lower = 1;
            }

            if (parentNode.Attribute("Upper") != null)
            {
                component.Upper = SerializationContext.DecodeUnlimitedInt(parentNode.Attribute("Upper").Value);
            }
            else
            {
                component.Upper = 1;
            }
        }

        /// <summary>
        /// Returns ID from the value of Type attribute of <paramref name="parentNode"/>.
        /// </summary>
        public static Guid DeserializeAttributeType(this IEvoXSerializable component, XElement parentNode, SerializationContext context)
        {
            if (parentNode.Attribute("Type") == null)
            {
                return Guid.Empty;
            }
            return component.DeserializeIDRef("Type", parentNode, context);
        }

        public static string DeserializeSimpleValueFromElement(this IEvoXSerializable component, [NotNull]string elementName, XElement parentNode, SerializationContext context)
        {
            XElement element = parentNode.Element(context.EvoXNS + elementName);
            return element.Value;
        }

        public static string DeserializeSimpleValueFromCDATA(this IEvoXSerializable component, [NotNull]string elementName, XElement parentNode, SerializationContext context)
        {
            XElement element = parentNode.Element(context.EvoXNS + elementName);
            XCData cdata = (XCData) element.Nodes().First();
            return cdata.Value;
        }

        public static string DeserializeSimpleValueFromAttribute(this IEvoXSerializable component, [NotNull]string elementName, XElement parentNode, SerializationContext context)
        {
            XAttribute xmlAttribute = parentNode.Attribute(elementName);
            return xmlAttribute.Value;
        }

        public static void DeserializePointsCollection(this IEvoXSerializable component, ObservablePointCollection points, XElement parentNode, SerializationContext context)
        {
            XElement pointsElement = parentNode.Element(context.EvoXNS + "Points");

            foreach (XElement pointElement in pointsElement.Elements())
            {
                double x = double.Parse(component.DeserializeSimpleValueFromAttribute("X", pointElement, context));
                double y = double.Parse(component.DeserializeSimpleValueFromAttribute("Y", pointElement, context));
                points.Add(new rPoint(x, y));
            }
        }

        #endregion 

        #region serialization

        public static void SerializeIDRef(this IEvoXSerializable component, IEvoXSerializable referencedObject, string attributeName, XElement parentNode, SerializationContext context,
            bool? outputDisplayAttribute = null, string displayAttributeName = null)
        {
            XAttribute idRefAttribute = new XAttribute(attributeName, SerializationContext.EncodeValue(((EvoXObject)referencedObject).ID));
            parentNode.Add(idRefAttribute);

            if ((!outputDisplayAttribute.HasValue && context.OutputNamesWithIdReferences) ||
                outputDisplayAttribute == true)
            {
                if (string.IsNullOrEmpty(displayAttributeName))
                {
                    if (attributeName.Contains("ID"))
                    {
                        displayAttributeName = attributeName.Substring(0, attributeName.IndexOf("ID"));
                    }
                    else
                    {
                        displayAttributeName = "displayName";
                    }
                }
                XAttribute displayNameAttribute = new XAttribute(displayAttributeName, referencedObject.ToString().Replace("\"", "*"));
                parentNode.Add(displayNameAttribute);
            }
        }

        public static void SerializeCardinality(this IHasCardinality component, XElement parentNode, SerializationContext context)
        {
            if (component.Lower != 1)
            {
                XAttribute lowerAttribute = new XAttribute("Lower", SerializationContext.EncodeValue(component.Lower));
                parentNode.Add(lowerAttribute);
            }

            if (component.Upper != 1)
            {
                XAttribute upperAttribute = new XAttribute("Upper", SerializationContext.EncodeValue(component.Upper));
                parentNode.Add(upperAttribute);
            }
        }

        public static void SerializeAttributeType(this IEvoXSerializable component, AttributeType attributeType, XElement parentNode, SerializationContext context)
        {
            component.SerializeIDRef(attributeType, "Type", parentNode, context);
        }

        public static void WrapAndSerializeCollection<T>(this IEvoXSerializable component, [NotNull] string collectionElementName, [NotNull] string elementName, ICollection<T> wrappedCollection, XElement parentNode, SerializationContext context, bool skipEmpty = false)
            where T : IEvoXSerializable
        {
            if (String.IsNullOrEmpty(elementName))
            {
                throw new ArgumentNullException("elementName");
            }

            if (string.IsNullOrEmpty(collectionElementName))
            {
                throw new ArgumentNullException("collectionElementName");
            }

            if (skipEmpty && (wrappedCollection == null || wrappedCollection.IsEmpty()))
                return;

            XElement collectionWrappingElement = new XElement(context.EvoXNS + collectionElementName);
            parentNode.Add(collectionWrappingElement);

            XAttribute countAttribute = new XAttribute("Count", SerializationContext.EncodeValue(wrappedCollection.Count));
            collectionWrappingElement.Add(countAttribute);

            foreach (T item in wrappedCollection)
            {
                component.SerializeToChildElement(elementName, item, collectionWrappingElement, context);
            }
        }

        public static void WrapAndSerializeIDRefCollection<T>(this IEvoXSerializable component, [NotNull] string collectionElementName, [NotNull] string elementName, [NotNull] string idRefAttribute, ICollection<T> wrappedCollection, XElement parentNode, SerializationContext context)
            where T : IEvoXSerializable
        {
            if (String.IsNullOrEmpty(elementName))
            {
                throw new ArgumentNullException("elementName");
            }

            if (string.IsNullOrEmpty(collectionElementName))
            {
                throw new ArgumentNullException("collectionElementName");
            }

            XElement collectionWrappingElement = new XElement(context.EvoXNS + collectionElementName);
            parentNode.Add(collectionWrappingElement);

            XAttribute countAttribute = new XAttribute("Count", SerializationContext.EncodeValue(wrappedCollection.Count));
            collectionWrappingElement.Add(countAttribute);

            foreach (T childPsmAssociation in wrappedCollection)
            {
                XElement memberElement = new XElement(context.EvoXNS + elementName);
                component.SerializeIDRef(childPsmAssociation, idRefAttribute, memberElement, context);
                collectionWrappingElement.Add(memberElement);
            }
        }

        public static XElement SerializeToChildElement(this IEvoXSerializable component, [NotNull]string elementName, IEvoXSerializable wrappedComponent, XElement parentNode, SerializationContext context)
        {
            if (String.IsNullOrEmpty(elementName))
            {
                throw new ArgumentNullException("elementName");
            }

            XElement element = new XElement(context.EvoXNS + elementName);
            wrappedComponent.Serialize(element, context);
            parentNode.Add(element);
            return element;
        }

        public static void SerializePointsCollection(this IEvoXSerializable component, IEnumerable<rPoint> points, XElement parentNode, SerializationContext context)
        {
            XElement pointsElement = new XElement(context.EvoXNS + "Points");
            parentNode.Add(pointsElement);
            foreach (rPoint rPoint in points)
            {
                XElement pointElement = new XElement(context.EvoXNS + "Point");
                pointsElement.Add(pointElement);
                component.SerializeSimpleValueToAttribute("X", Math.Floor(rPoint.X), pointElement, context);
                component.SerializeSimpleValueToAttribute("Y", Math.Floor(rPoint.Y), pointElement, context);
            }
        }

        public static void SerializeSimpleValueToElement(this IEvoXSerializable component, [NotNull] string elementName, object value, XElement parentNode, SerializationContext context)
        {
            XElement xmlElement = new XElement(context.EvoXNS + elementName);
            XText valueText = new XText(value.ToString());
            xmlElement.Add(valueText);
            parentNode.Add(xmlElement);
        }

        public static void SerializeSimpleValueToCDATA(this IEvoXSerializable component, [NotNull] string elementName, object value, XElement parentNode, SerializationContext context)
        {
            XElement xmlElement = new XElement(context.EvoXNS + elementName);
            XCData xcData = new XCData(value.ToString());
            xmlElement.Add(xcData);
            parentNode.Add(xmlElement);
        }

        public static void SerializeSimpleValueToAttribute(this IEvoXSerializable component, [NotNull] string attributeName, object value, XElement parentNode, SerializationContext context)
        {
            XAttribute xmlAttribute = new XAttribute(attributeName, value.ToString());
            parentNode.Add(xmlAttribute);
        }
        #endregion
    }
}