using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Exolutio.SupportingClasses;

namespace Exolutio.Model.PSM.Grammar.XSDTranslation
{
    public class XsdSchemaGenerator
    {
        public PSMSchema PSMSchema { get; private set; }

        public Log Log { get; private set; }

        public NamingSupport NamingSupport { get; private set; }

        public void Initialize(PSMSchema psmSchema)
        {
            PSMSchema = psmSchema;
            nodeInfos.Clear();
            Log = new Log();
            NamingSupport = new NamingSupport {Log = Log};
        }

        public void GenerateXSDStructure()
        {
            foreach (PSMAssociationMember node in PSMSchema.PSMNodes)
            {
                if (node is PSMSchemaClass)
                    continue;

                XsdNodeTranslationInfo nodeInfo = new XsdNodeTranslationInfo();
                nodeInfo.Node = node;
                nodeInfos[node] = nodeInfo;

                if (node.DownCastSatisfies<PSMClass>(c => c.ParentAssociation != null && c.ParentAssociation.IsNamed))
                {
                    nodeInfo.ComplexTypeRequired = true;
                    nodeInfo.ComplexTypeName = NamingSupport.SuggestName(node, complexType: true);
                }

                if (node.DownCastSatisfies<PSMClass>(c => c.HasStructuralRepresentatives))
                {
                    nodeInfo.GroupsRequired = true; 
                }

                if (node.DownCastSatisfies<PSMClass>(c => c.ParentAssociation == null || !c.ParentAssociation.IsNamed))
                {
                    nodeInfo.GroupsRequired = true;
                }

                nodeInfo.DefinesAttributes = TestNodeDefinesAttributes(node);
                nodeInfo.DefinesElements = TestNodeDefinesElements(node);

                if (nodeInfo.DefinesElements && nodeInfo.GroupsRequired)
                {
                    nodeInfo.ContentGroupName = NamingSupport.SuggestName(node, contentGroup: true);
                }

                // find all structural representatives of a class
                if (node is PSMClass && nodeInfo.DefinesAttributes)
                {
                    IList<PSMClass> referencing = ((PSMClass)node).GetReferencingStructuralRepresentatives(true);
                    referencing.Add((PSMClass) node);

                    foreach (PSMClass psmClass in referencing)
                    {
                        if (psmClass.PropagatesToChoice())
                        {
                            nodeInfo.OptAttributeReference = true;
                            nodeInfo.OptAttributeGroupName = NamingSupport.SuggestName(node, optGroup: true);
                            Log.AddErrorFormat("Class '{0}' defines attributes which propagate to choice/set content model(s). Consider giving name to the parent association of the class or representing attributes of '{0}' as elements. ", node);
                        }
                        else
                        {
                            nodeInfo.NonOptAttributeReference = true;
                            nodeInfo.NonOptAttributeGroupName = NamingSupport.SuggestName(node, attributeGroup: true);
                        }
                    }
                }
            }
        }

        private bool TestNodeDefinesElements(PSMAssociationMember node)
        {
            if (node is PSMClass)
            {
                PSMClass psmClass = (PSMClass)node;
                if (psmClass.PSMAttributes.Any(a => a.Element))
                    return true;

                if (psmClass.IsStructuralRepresentative)
                {
                    if (TestNodeDefinesElements(psmClass.RepresentedClass))
                        return true;
                }
            }

            foreach (PSMAssociation childPsmAssociation in node.ChildPSMAssociations)
            {
                if (childPsmAssociation.IsNamed)
                    return true;

                if (!childPsmAssociation.IsNamed)
                {
                    if (TestNodeDefinesElements(childPsmAssociation.Child))
                        return true;
                }
            }

            return false;
        }

        private bool TestNodeDefinesAttributes(PSMAssociationMember node)
        {
            if (node is PSMClass)
            {
                PSMClass psmClass = (PSMClass)node;
                if (psmClass.PSMAttributes.Any(a => !a.Element))
                    return true;

                if (psmClass.IsStructuralRepresentative)
                {
                    if (TestNodeDefinesAttributes(psmClass.RepresentedClass))
                        return true;
                }
            }

            foreach (PSMAssociation childPsmAssociation in node.ChildPSMAssociations)
            {
                if (!childPsmAssociation.IsNamed)
                {
                    if (TestNodeDefinesAttributes(childPsmAssociation.Child))
                        return true; 
                }
            }

            return false;
        }

        Dictionary<PSMComponent, XsdNodeTranslationInfo> nodeInfos = new Dictionary<PSMComponent, XsdNodeTranslationInfo>(); 

        public XDocument GetXsd()
        {
            XDocument doc = new XDocument(new XDeclaration("1.0", "utf-8", null));
            XElement xsdSchema = doc.XsdSchema();
            XComment comment =  new XComment(string.Format(" generated by eXolutio on {0} {1} from {2}/{3}. ", DateTime.Now.ToShortDateString(), DateTime.Now.ToShortTimeString(), PSMSchema.Project.Name, PSMSchema.Caption));
            xsdSchema.Add(comment);
            var orderedNodes = ModelIterator.OrderBFS(PSMSchema.PSMNodes.Cast<PSMComponent>().ToList());

            AddGlobalElements(xsdSchema);

            foreach (PSMAssociationMember node in orderedNodes)
            {
                if (node is PSMClass)
                {
                    TranslateClass(xsdSchema, (PSMClass)node);
                }
            }

            return doc;
        }

        private void AddGlobalElements(XElement parentElement)
        {
            foreach (PSMClass psmClass in PSMSchema.TopClasses)
            {
                if (psmClass.ParentAssociation.IsNamed)
                {
                    XElement globalElement = parentElement.XsdElement(psmClass.ParentAssociation.Name);
                    globalElement.TypeAttribute(nodeInfos[psmClass].ComplexTypeName);
                }
            }
        }
        
        private void TranslateClass(XElement parentElement, PSMClass psmClass)
        {
            XsdNodeTranslationInfo nodeInfo = nodeInfos[psmClass];

            if (nodeInfo.ComplexTypeRequired)
            {
                XElement complexType = parentElement.XsdComplexType(nodeInfo.ComplexTypeName);
                XElement complexTypeContent = !ContentIsSet(psmClass) ? complexType.XsdSequence() : complexType.XsdAll();

                if (ContentIsSet(psmClass) && CountContentForSet(psmClass) > 1)
                {
                    Log.AddErrorFormat("Class '{0}' has a content model of type 'set' in its content. Such classes can not have other content except this content model. ", psmClass);
                }

                if (nodeInfo.GroupsRequired)
                {
                    if (nodeInfo.DefinesAttributes)
                    {
                        complexTypeContent.XsdAttributeGroupRef(nodeInfo.NonOptAttributeGroupName);
                    }
                    if (nodeInfo.DefinesElements)
                    {
                        complexTypeContent.XsdGroupRef(nodeInfo.ContentGroupName);
                    }
                }
                else
                {
                    AddReferences(complexType, true, nodeInfo, false);
                    AddReferences(complexTypeContent, false, nodeInfo, false);
                }
            }

            if (nodeInfo.GroupsRequired)
            {
                if (nodeInfo.DefinesElements)
                {
                    XElement group = parentElement.XsdGroup(nodeInfo.ContentGroupName);
                    XElement groupSequence = group.XsdSequence();
                    AddReferences(groupSequence, false, nodeInfo, false);
                }

                if (nodeInfo.DefinesAttributes)
                {
                    if (nodeInfo.NonOptAttributeReference)
                    {
                        XElement attributeGroup = parentElement.XsdAttributeGroup(nodeInfo.NonOptAttributeGroupName);
                        AddReferences(attributeGroup, true, nodeInfo, false);
                    }

                    if (nodeInfo.OptAttributeReference)
                    {
                        XElement attributeGroup = parentElement.XsdAttributeGroup(nodeInfo.OptAttributeGroupName);
                        AddReferences(attributeGroup, true, nodeInfo, true);
                    }
                }
            }
        }

        private static bool ContentIsSet(PSMComponent component)
        {
            return component.DownCastSatisfies<PSMClass>(psmClass =>
                   psmClass.ChildPSMAssociations.Count > 0
                   && psmClass.ChildPSMAssociations.First().Child.DownCastSatisfies<PSMContentModel>(cm => cm.Type == PSMContentModelType.Set));
        }

        private int CountContentForSet(PSMClass psmClass)
        {
            return psmClass.PSMAttributes.Count(a => a.Element) + psmClass.ChildPSMAssociations.Count;
        }


        private void AddReferences(XElement parentElement, bool attributeReferences, XsdNodeTranslationInfo nodeInfo, bool inChoice)
        {
            #region SR
            if (nodeInfo.Node is PSMClass)
            {
                PSMClass psmClass = (PSMClass) nodeInfo.Node;
                if (psmClass.IsStructuralRepresentative)
                {
                    XsdNodeTranslationInfo srInfo = nodeInfos[psmClass.RepresentedClass];
                    if (srInfo.DefinesElements && !attributeReferences)
                    {
                        parentElement.XsdGroupRef(srInfo.ContentGroupName);
                    }
                    if (srInfo.DefinesAttributes && attributeReferences)
                    {
                        parentElement.XsdAttributeGroupRef(inChoice? srInfo.OptAttributeGroupName : srInfo.NonOptAttributeGroupName);
                    }
                }
            }

            #endregion 

            #region attributes

            if (nodeInfo.Node is PSMClass)
            {
                PSMClass psmClass = (PSMClass) nodeInfo.Node;

                foreach (PSMAttribute psmAttribute in psmClass.PSMAttributes)
                {
                    string attributeName = NamingSupport.SuggestName(psmAttribute, attribute: true);
                    if (psmAttribute.Element && !attributeReferences)
                    {
                        parentElement.XsdAttributeAsElement(attributeName, psmAttribute);                        
                    }
                    if (!psmAttribute.Element && attributeReferences)
                    {
                        parentElement.XsdAttribute(attributeName, psmAttribute, inChoice);
                    }
                    if (psmAttribute.AttributeType == null)
                    {
                        Log.AddWarningFormat("Type of attribute '{0}' is not specified. ", psmAttribute);
                    }
                }
            }

            #endregion

            #region associations

            if (nodeInfo.Node is PSMAssociationMember)
            {
                PSMAssociationMember psmAssociationMember = (PSMAssociationMember) nodeInfo.Node;
                // associations 
                XElement element = parentElement;
                foreach (PSMAssociation psmAssociation in psmAssociationMember.ChildPSMAssociations)
                {
                    XsdNodeTranslationInfo childInfo = nodeInfos[psmAssociation.Child];
                    if (!attributeReferences)
                    {
                        element = parentElement;
                        if (psmAssociation.IsNamed && childInfo.ComplexTypeRequired)
                        {
                            XElement wrappingLiteral = element.XsdElement(psmAssociation.Name);                            
                            if (childInfo.ComplexTypeRequired)
                            {
                                wrappingLiteral.TypeAttribute(childInfo.ComplexTypeName);
                            }                            
                            element = wrappingLiteral;
                        }

                        if (!psmAssociation.IsNamed && childInfo.GroupsRequired && childInfo.DefinesElements)                        
                        {
                            element = element.XsdGroupRef(childInfo.ContentGroupName);
                        }
                        
                        if (childInfo.Node is PSMContentModel)
                        {
                            /* 
                             * association name is ignored in this case,
                             * normalized schemas do not have content models 
                             * with named parent associations
                             */
                            if (psmAssociation.IsNamed)
                            {
                                Log.AddErrorFormat("Name of '{0}' is ignored. Associations leading to content models should not have names (this violates rules for normalized schemas).", psmAssociation);
                            }
                            switch (((PSMContentModel) childInfo.Node).Type)
                            {
                                case PSMContentModelType.Sequence:
                                    element = element.XsdSequence();
                                    break;
                                case PSMContentModelType.Choice:
                                    element = element.XsdChoice();
                                    break;
                                case PSMContentModelType.Set:
                                    // do nothing
                                    break;
                            }
                            AddReferences(element, false, childInfo, false);
                        }

                        if (element != parentElement || ContentIsSet(nodeInfo.Node))
                        {
                            element.CardinalityAttributes(psmAssociation);
                        }
                    }
                    else
                    {
                        if (childInfo.GroupsRequired && childInfo.DefinesAttributes)
                        {
                            parentElement.XsdAttributeGroupRef(inChoice ? childInfo.OptAttributeGroupName : childInfo.NonOptAttributeGroupName);
                        }
                        if (childInfo.Node is PSMContentModel)
                        {
                            AddReferences(element, true, childInfo, ((PSMContentModel)childInfo.Node).Type.IsAmong(PSMContentModelType.Choice, PSMContentModelType.Set));
                        }
                    }
                }
            }

            #endregion
        }
    }


    public class XsdNodeTranslationInfo
    {
        public PSMComponent Node { get; set; }

        /// <summary>
        /// Class node translates to complex type
        /// when its parent association has a name.
        /// </summary>
        public bool ComplexTypeRequired { get; set; }

        /// <summary>
        /// Groups (XSD attribute and/or content groups) are 
        /// required when a class is referenced from a structural
        /// representative. 
        /// </summary>
        public bool GroupsRequired { get; set; }
        
        /// <summary>
        /// <para>
        /// Attributes defined by the node are propagated (transitively)
        /// upwards to a content model choice node. 
        /// </para>
        /// <para>
        /// Both <see cref="OptAttributeReference"/> and <see cref="NonOptAttributeReference"/>
        /// can be true (caused by a reference from a structural representative)
        /// </para>
        /// </summary>
        public bool NonOptAttributeReference { get; set; }

        /// <summary>
        /// <para>
        /// Attributes defined by the node are propagated (transitively)
        /// upwards to a node which is not a content model choice node. 
        /// </para>
        /// <para>
        /// Both <see cref="OptAttributeReference"/> and <see cref="NonOptAttributeReference"/>
        /// can be true (caused by a reference from a structural representative)
        /// </para>
        /// </summary>
        public bool OptAttributeReference { get; set; }

        public string ComplexTypeName { get; set; }

        public string ContentGroupName { get; set; }

        public string NonOptAttributeGroupName { get; set; }

        public string OptAttributeGroupName { get; set; }

        public bool DefinesElements { get; set; }

        public bool DefinesAttributes { get; set; }
    }
}