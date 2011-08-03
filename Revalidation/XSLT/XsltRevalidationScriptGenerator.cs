using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;
using Exolutio.Model;
using Exolutio.Model.PSM;
using Exolutio.Model.Versioning;
using Exolutio.Revalidation.Changes;
using Exolutio.SupportingClasses;
using Version = Exolutio.Model.Versioning.Version;

namespace Exolutio.Revalidation.XSLT
{
    public class XsltRevalidationScriptGenerator
    {
        public XsltRevalidationScriptGenerator()
        {

        }

        #region properties

        readonly Dictionary<PSMComponent, RevalidationNodeInfo> nodeInfos = new Dictionary<PSMComponent, RevalidationNodeInfo>();
        
        readonly List<PSMComponent> componentsRequireingInstanceGenerators = new List<PSMComponent>();

        public PSMSchema PSMSchemaOldVersion { get; private set; }

        public Version OldVersion { get { return PSMSchemaOldVersion.Version; } }

        public PSMSchema PSMSchemaNewVersion { get; private set; }

        public Version NewVersion { get { return PSMSchemaNewVersion.Version; } }

        public DetectedChangeInstancesSet DetectedChangeInstances { get; private set; }

        private GeneratorContext context;

        private TemplateNamingSupport namingSupport;

        public void Initialize(PSMSchema psmSchemaOldVersion, PSMSchema psmSchemaNewVersion, DetectedChangeInstancesSet changeInstances)
        {
            this.PSMSchemaNewVersion = psmSchemaNewVersion;
            this.PSMSchemaOldVersion = psmSchemaOldVersion;
            DetectedChangeInstances = changeInstances;
            nodeInfos.Clear();
            componentsRequireingInstanceGenerators.Clear();
        }

        #endregion 

        public void GenerateTemplateStructure()
        {
            context = new GeneratorContext() { OldVersion = OldVersion, NewVersion = NewVersion};
            namingSupport = new TemplateNamingSupport() {PSMSchema = PSMSchemaNewVersion};
            
            // pregenerate 
            foreach (PSMComponent psmComponent in DetectedChangeInstances.RedNodes)
            {
                RevalidationNodeInfo nodeInfo = new RevalidationNodeInfo(psmComponent);
                nodeInfos[psmComponent] = nodeInfo;
            }

            foreach (PSMComponent psmComponent in DetectedChangeInstances.RedNodes)
            {
                RevalidationNodeInfo nodeInfo = nodeInfos[psmComponent];

                context.CurrentNode = psmComponent;
                
                if (nodeInfo.ElementTemplateRequired || nodeInfo.AttributeTemplateRequired || psmComponent.DownCastSatisfies<PSMClass>(c => c.IsNamed))
                {
                    if (!(psmComponent is PSMAttribute))
                    {
                        Template processElementsTemplate = new Template();
                        processElementsTemplate.Name = namingSupport.SuggestName(psmComponent, true, false);
                        processElementsTemplate.ElementsTemplate = true; 
                        nodeInfo.ProcessElementsTemplate = processElementsTemplate;

                        Template processAttributesTemplate = new Template();
                        processAttributesTemplate.Name = namingSupport.SuggestName(psmComponent, false, true);
                        processAttributesTemplate.AttributesTemplate = true;
                        nodeInfo.ProcessAttributesTemplate = processAttributesTemplate;

                        IEnumerable<PSMComponent> childComponents = ModelIterator.GetPSMChildren(psmComponent, true);

                        foreach (PSMComponent childComponent in childComponents)
                        {
                            if (childComponent is PSMAttribute && !((PSMAttribute)childComponent).Element)
                            {
                                TemplateReference attributeReference = new TemplateReference
                                {
                                    ReferencedNode = childComponent,
                                    CallingNode = psmComponent,
                                    ReferencesRedNode = DetectedChangeInstances.RedNodes.Contains(childComponent),
                                    Lower = IHasCardinalityExt.GetLowerCardinality(childComponent),
                                    Upper = IHasCardinalityExt.GetUpperCardinality(childComponent),
                                    DeletionRequired = DetectedChangeInstances.ExistsCardinalityChangeRequiringDeletion(childComponent),
                                    CreationRequired = DetectedChangeInstances.ExistsCardinalityChangeRequiringCreation(childComponent),
                                    ReferencesAddedNode = DetectedChangeInstances.AddedNodes.Contains(childComponent),
                                    ReferencesGroupNode = DetectedChangeInstances.IsGroupNode(childComponent)
                                };
                                processAttributesTemplate.References.Add(attributeReference);
                                if (attributeReference.CreationRequired || attributeReference.ReferencesAddedNode)
                                    componentsRequireingInstanceGenerators.AddIfNotContained(childComponent);
                            }

                            TemplateReference reference = new TemplateReference
                            {
                                ReferencedNode = childComponent,
                                CallingNode = psmComponent,
                                ReferencesRedNode = DetectedChangeInstances.RedNodes.Contains(childComponent),
                                Lower = IHasCardinalityExt.GetLowerCardinality(childComponent),
                                Upper = IHasCardinalityExt.GetUpperCardinality(childComponent),
                                DeletionRequired = DetectedChangeInstances.ExistsCardinalityChangeRequiringDeletion(childComponent),
                                CreationRequired = DetectedChangeInstances.ExistsCardinalityChangeRequiringCreation(childComponent),
                                ReferencesAddedNode = DetectedChangeInstances.AddedNodes.Contains(childComponent),
                                ReferencesGroupNode = DetectedChangeInstances.IsGroupNode(childComponent)
                            };
                            if (reference.CreationRequired || (childComponent is PSMAttribute && reference.ReferencesAddedNode))
                                componentsRequireingInstanceGenerators.AddIfNotContained(childComponent);

                            if (childComponent is PSMAttribute && ((PSMAttribute)childComponent).Element)
                            {
                                processElementsTemplate.References.Add(reference);
                            }

                            if (childComponent is PSMAssociationMember)
                            {
                                if (nodeInfos.ContainsKey(childComponent))
                                {
                                    RevalidationNodeInfo childNodeInfo = nodeInfos[childComponent];
                                    if (childNodeInfo.AttributeTemplateRequired && 
                                        childComponent.DownCastSatisfies<PSMClass>(c => !c.ParentAssociation.IsNamed))
                                        processAttributesTemplate.References.Add(reference);
                                    if (childNodeInfo.ElementTemplateRequired ||
                                        childComponent.DownCastSatisfies<PSMClass>(c => c.ParentAssociation.IsNamed))
                                        processElementsTemplate.References.Add(reference);
                                }
                                else
                                {
                                    // TODO: can adding attribute calls be really left out??
                                    //processAttributesTemplate.References.Add(reference);
                                    processElementsTemplate.References.Add(reference);
                                }
                            }
                            
                        }

                        if (processAttributesTemplate.References.Count == 0)
                        {
                            nodeInfo.ProcessAttributesTemplate = null;
                        }

                        if (processElementsTemplate.References.Count == 0)
                        {
                            nodeInfo.ProcessElementsTemplate = null;
                        }
                    }

                    bool generateWrappingTemplate = true; 
                    /* not a root class 
                       and not a group node given with association given a name in the new version
                       and not an added attribute
                       and not a content model 
                       => node template (wrapping template)
                    */
                    if (psmComponent is PSMClass && PSMSchemaNewVersion.Roots.Contains((PSMClass)psmComponent))
                        generateWrappingTemplate = false;
                    if (DetectedChangeInstances.IsGroupNode(psmComponent) && psmComponent.DownCastSatisfies<PSMClass>(g => g.ParentAssociation == null || !g.ParentAssociation.IsNamed))
                        generateWrappingTemplate = false;
                    if (psmComponent.DownCastSatisfies<PSMAttribute>(a => DetectedChangeInstances.IsAddedNode(a)))
                        generateWrappingTemplate = false;
                    if (psmComponent is PSMContentModel)
                        generateWrappingTemplate = false; 

                    if (generateWrappingTemplate)
                    {
                        Template processNodeTemplate = new Template();
                        nodeInfo.ProcessNodeTemplate = processNodeTemplate;
                        processNodeTemplate.WrapNodeName = psmComponent is PSMAttribute ? psmComponent.Name : ((PSMAssociationMember)psmComponent).ParentAssociation.Name;

                        // added class
                        if (DetectedChangeInstances.IsAddedNode(psmComponent))
                        {
                            processNodeTemplate.Name = namingSupport.SuggestName(psmComponent, false, false);
                        }
                        // existing class
                        else 
                        {
                            if (!DetectedChangeInstances.IsGroupNode(psmComponent))
                            {
                                // use absolute path for template matches
                                XPathExpr absolutePath = new XPathExpr(psmComponent.GetInVersion(OldVersion).XPath);
                                processNodeTemplate.MatchList.Add(absolutePath);
                            }
                            else
                            {
                                processNodeTemplate.Name = namingSupport.SuggestName(psmComponent, false, false);
                                processNodeTemplate.GroupNodeTemplate = true; 
                            }
                        }
                    }
                }
            }
        }

        public XDocument GetRevalidationStylesheet()
        {
            XDocument doc = new XDocument(new XDeclaration("1.0", "utf-8", null));
            XElement xslStylesheet = doc.XslStylesheet("2.0");
            XElement outputElement = new XElement(XDocumentXsltExtensions.XSLT_NAMESPACE + "output");
            outputElement.Add(new XAttribute("method", "xml"));
            outputElement.Add(new XAttribute("indent", "yes"));
            xslStylesheet.Add(outputElement);
            IEnumerable<PSMComponent> orderedNodes = ModelIterator.OrderBFS(DetectedChangeInstances.RedNodes);
            foreach (PSMComponent psmComponent in orderedNodes)
            {
                context.CurrentNode = psmComponent;
                RevalidationNodeInfo nodeInfo = nodeInfos[psmComponent];
                if (nodeInfo.ProcessNodeTemplate != null)
                {
                    GenWrappingNodeTemplate(psmComponent, xslStylesheet);
                }
                if (nodeInfo.ProcessAttributesTemplate != null)
                {
                    GenProcessTemplate(psmComponent, nodeInfo.ProcessAttributesTemplate, xslStylesheet);
                }
                if (nodeInfo.ProcessElementsTemplate != null)
                {
                    GenProcessTemplate(psmComponent, nodeInfo.ProcessElementsTemplate, xslStylesheet);
                }
            }
            GenInstanceGenerators(xslStylesheet);
            GenXFormConvertorTemplateEtoA(xslStylesheet);
            GenXFormConvertorTemplateAtoE(xslStylesheet);
            GenBlueNodesTemplate(xslStylesheet);
            GenGreenNodesTemplate(xslStylesheet);

            return doc;
        }

        private void GenWrappingNodeTemplate(PSMComponent psmComponent, XElement xslStylesheet)
        {
            RevalidationNodeInfo nodeInfo = nodeInfos[psmComponent];
            Template processNodeTemplate = nodeInfo.ProcessNodeTemplate;
            XElement templateElement;
            xslStylesheet.Add(new XComment(psmComponent.ToString()));
            if (!processNodeTemplate.IsNamed)
            {
                templateElement = xslStylesheet.XslTemplate(processNodeTemplate.Match);
            }
            else
            {
                XDocumentXsltExtensions.TemplateParameter[] parameters = null;
                if (processNodeTemplate.GroupNodeTemplate)
                {
                    // group nodes have current instance parameter, even when they are for "existing" nodes
                    parameters = new[] { XDocumentXsltExtensions.CreateCurrentInstanceParameterDeclaration() };
                    processNodeTemplate.HasCurrentInstanceParameter = true; 
                }
                templateElement = xslStylesheet.XslNamedTemplate(processNodeTemplate.Name, parameters);
            }

            XElement contentElement = templateElement;
            
            // create wrapping node if needed
            if (!string.IsNullOrEmpty(processNodeTemplate.WrapNodeName))
            {
                XElement wrapNode = null;
                if (psmComponent is PSMAttribute && !((PSMAttribute)psmComponent).Element)
                {
                    wrapNode = contentElement.XslAttribute(processNodeTemplate.WrapNodeName);
                }
                else
                {
                    wrapNode = new XElement(processNodeTemplate.WrapNodeName);
                    contentElement.Add(wrapNode);
                }               
                
                contentElement = wrapNode;
            }

            // add value for attributes 
            if (psmComponent is PSMAttribute)
            {
                XPathExpr relativePath = context.GetRelativeXPath(psmComponent, false);
                contentElement.XslValueOf(relativePath);
            }

            // subtree processing (attributes)
            if (nodeInfo.ProcessAttributesTemplate != null)
            {
                XDocumentXsltExtensions.TemplateParameter[] parameters = null;
                if (!DetectedChangeInstances.IsAddedNode(psmComponent))
                {
                    XPathExpr currentInstanceCall = processNodeTemplate.GroupNodeTemplate ? new XPathExpr("$ci") : new XPathExpr("./(* | @*)");
                    parameters = new[] { XDocumentXsltExtensions.CreateCurrentInstanceParameterCall(currentInstanceCall) };
                }

                contentElement.XslCallTemplate(nodeInfo.ProcessAttributesTemplate.Name, parameters);
            }

            // subtree processing (elements)
            if (nodeInfo.ProcessElementsTemplate != null)
            {
                XDocumentXsltExtensions.TemplateParameter[] parameters = null;
                if (!DetectedChangeInstances.IsAddedNode(psmComponent))
                {
                    XPathExpr currentInstanceCall = processNodeTemplate.GroupNodeTemplate ? new XPathExpr("$ci") : new XPathExpr("./(* | @*)");
                    parameters = new[] { XDocumentXsltExtensions.CreateCurrentInstanceParameterCall(currentInstanceCall) };
                }

                contentElement.XslCallTemplate(nodeInfo.ProcessElementsTemplate.Name, parameters);
            }
            xslStylesheet.Add(new XComment("End of: " + psmComponent.ToString()));
        }

        private void GenProcessTemplate(PSMComponent psmComponent, Template processTemplate,  XElement xslStylesheet)
        {
            XDocumentXsltExtensions.TemplateParameter[] parameters = null;
            if (!DetectedChangeInstances.IsAddedNode(psmComponent) || DetectedChangeInstances.IsInlinedNode(psmComponent))
            {
                parameters = new[] { XDocumentXsltExtensions.CreateCurrentInstanceParameterDeclaration() };
                processTemplate.HasCurrentInstanceParameter = true; 
            }
            XElement templateElement = xslStylesheet.XslNamedTemplate(processTemplate.Name, parameters);
            if (psmComponent.DownCastSatisfies<PSMContentModel>(cm => cm.Type == PSMContentModelType.Choice))
            {
                GenChoiceTemplate(processTemplate, templateElement);
            }
            else
            {
                foreach (TemplateReference templateReference in processTemplate.References)
                {
                    CreateReference(processTemplate, templateElement, templateReference);
                }
            }
        }

        private void GenChoiceTemplate(Template processTemplate, XElement templateElement)
        {
            XElement xslChoose = templateElement.XslChoose();
            foreach (TemplateReference templateReference in processTemplate.References)
            {
                XPathExpr test = context.GetRelativeXPath(ExpandIfInlinedNode(templateReference), processTemplate.HasCurrentInstanceParameter);
                XElement xslWhen = xslChoose.XslWhen(test);
                CreateReference(processTemplate, xslWhen, templateReference);
            }
        }

        private void CreateReference(Template processTemplate, XElement templateElement, TemplateReference templateReference)
        {
            GenReferenceWithConditionMethod singleReference;
            GenReferenceMethod cardinalityReference;

            if (templateReference.ReferencesRedNode && templateReference.ReferencesGroupNode)
            {
                singleReference = GenGroupNodeSingleReference;
                cardinalityReference = GenGroupNodeCardinalityReference;
            }
            else if (templateReference.ReferencesRedNode && templateReference.ReferencesContentModel)
            {
                singleReference = GenCMSingleReference;
                cardinalityReference = GenCMCardinalityReference;
            }
            else
            {
                singleReference = GenSingleReference;
                cardinalityReference = GenCardinalityReference;
            }

            if (templateReference.Lower > 1 ||
                DetectedChangeInstances.ExistsCardinalityChange(templateReference.ReferencedNode)
                || (templateReference.ReferencesInlinedNode && templateReference.ReferencedNode.ExistsInVersion(OldVersion)
                    && IHasCardinalityExt.GetUpperCardinality(templateReference.ReferencedNode.GetInVersion(OldVersion)) > 1))
            {
                cardinalityReference(templateElement, processTemplate, templateReference);
            }
            else
            {
                singleReference(templateElement, processTemplate, templateReference);
            }
        }

        private void GenCardinalityReference(XElement templateElement, Template callingTemplate, TemplateReference reference)
        {
            // process existing
            if (!DetectedChangeInstances.IsAddedNode(reference.ReferencedNode))
            {
                if (!reference.DeletionRequired)
                {
                    GenSingleReference(templateElement, callingTemplate, reference);
                }
                else
                {
                    GenSingleReference(templateElement, callingTemplate, reference, string.Format("position() le {0}", reference.Upper));
                }
            }

            // added node 
            if (DetectedChangeInstances.IsAddedNode(reference.ReferencedNode))
            {
                if (reference.Lower == 1)
                {
                    GenSingleReference(templateElement, callingTemplate, reference);
                }
                if (reference.Lower > 1)
                {
                    XElement xslForEach = templateElement.XslForEach(new XPathExpr("1 to {0}", reference.Lower));
                    GenSingleReference(xslForEach, callingTemplate, reference);
                }
            }

            // create new with instance generator
            if (reference.CreationRequired)
            {
                Debug.Assert(!DetectedChangeInstances.IsAddedNode(reference.ReferencedNode));
                XPathExpr existing = context.GetRelativeXPath(reference.ReferencedNode, !DetectedChangeInstances.IsAddedNode(reference.CallingNode));
                XPathExpr countExpr = new XPathExpr(string.Format("{0} - count({1})", reference.Lower, existing));
                templateElement.XslCallTemplate(namingSupport.SuggestNameForInstanceGenerator(reference.ReferencedNode, callingTemplate, reference), 
                                                new XDocumentXsltExtensions.TemplateParameter("count", countExpr));
            }
        }

        private void GenSingleReference(XElement templateElement, Template callingTemplate, TemplateReference reference, string condition = null)
        {
            RevalidationNodeInfo calledNodeInfo;
            nodeInfos.TryGetValue(reference.ReferencedNode, out calledNodeInfo);

            if (DetectedChangeInstances.IsAddedNode(reference.ReferencedNode))
            {
                Debug.Assert(calledNodeInfo != null);
                if (calledNodeInfo.Node.DownCastSatisfies<PSMAttribute>(a => a.Lower > 0))
                {
                    IEnumerable<XDocumentXsltExtensions.TemplateParameter> parameters = ((PSMAttribute)reference.ReferencedNode).Lower > 1 ? 
                        new[] { new XDocumentXsltExtensions.TemplateParameter("count", new XPathExpr(((PSMAttribute)reference.ReferencedNode).Lower.ToString()))} : null;
                    templateElement.XslCallTemplate(namingSupport.SuggestNameForInstanceGenerator(calledNodeInfo.Node), parameters);    
                }
                else if (callingTemplate.ElementsTemplate)
                {
                    templateElement.XslCallTemplate(calledNodeInfo.ProcessNodeTemplate.Name);    
                }
                else if (callingTemplate.AttributesTemplate)
                {
                    templateElement.XslCallTemplate(calledNodeInfo.ProcessAttributesTemplate.Name);    
                }
            }
            else
            {
                IEnumerable<PSMComponent> expandedReference = ExpandIfInlinedNode(reference);
                XPathExpr relativeXPath = context.GetRelativeXPath(expandedReference, !DetectedChangeInstances.IsAddedNode(reference.CallingNode)).AppendPredicate(condition);
                templateElement.XslApplyTemplates(relativeXPath);                
            }
        }

        private void GenGroupNodeCardinalityReference(XElement templateElement, Template callingTemplate, TemplateReference reference)
        {
            if (!DetectedChangeInstances.IsAddedNode(reference.ReferencedNode))
            {
                // process existing
                XElement xslForEachGroup = templateElement.XslForEachGroup(XPathExprGenerator.GetGroupMembers(context, reference.ReferencedNode, callingTemplate.HasCurrentInstanceParameter));
                xslForEachGroup.Add(XPathExprGenerator.GetGroupDistinguisher(context, reference.ReferencedNode));
                if (!reference.DeletionRequired)
                {
                    GenGroupNodeSingleReference(xslForEachGroup, callingTemplate, reference, "current-group()");
                }
                else
                {
                    XElement xslIf = xslForEachGroup.XslIf(new XPathExpr(string.Format("position() le {0}", reference.Upper)));
                    GenGroupNodeSingleReference(xslIf, callingTemplate, reference, "current-group()");
                }
            }
            //added node 
            if (DetectedChangeInstances.IsAddedNode(reference.ReferencedNode))
            {
                if (reference.Lower == 1)
                {
                    GenGroupNodeSingleReference(templateElement, callingTemplate, reference);
                }
                if (reference.Lower > 1)
                {
                    XElement xslForEach = templateElement.XslForEach(new XPathExpr("1 to {0}", reference.Lower));
                    GenGroupNodeSingleReference(xslForEach, callingTemplate, reference);
                }
            }
            // create new with instance generator
            if (reference.CreationRequired)
            {
                XPathExpr existing = context.GetRelativeXPath(reference.ReferencedNode, callingTemplate.HasCurrentInstanceParameter).Append(
                        XPathExprGenerator.GetGroupDistinguisher(context, reference.ReferencedNode).Value);
                XPathExpr countExpr = new XPathExpr(string.Format("{0} - count({1})", reference.Lower, existing));
                templateElement.XslCallTemplate(namingSupport.SuggestNameForInstanceGenerator(reference.ReferencedNode, callingTemplate, reference), 
                    new XDocumentXsltExtensions.TemplateParameter("count", countExpr));
            }
        }

        private void GenGroupNodeSingleReference(XElement callingElement, Template callingTemplate, TemplateReference reference, string condition = null)
        {            
            RevalidationNodeInfo calledNodeInfo = nodeInfos[reference.ReferencedNode];

            XPathExpr ci = condition == null ? XPathExprGenerator.GetGroupMembers(context, reference.ReferencedNode, callingTemplate.HasCurrentInstanceParameter) 
                : new XPathExpr(condition);

            if (calledNodeInfo.ProcessNodeTemplate != null)
            {
                callingElement.XslCallTemplate(calledNodeInfo.ProcessNodeTemplate.Name, XDocumentXsltExtensions.CreateCurrentInstanceParameterCall(ci));
            }
            else
            {
                if (callingTemplate.AttributesTemplate && calledNodeInfo.ProcessAttributesTemplate != null)
                {
                    callingElement.XslCallTemplate(calledNodeInfo.ProcessAttributesTemplate.Name, XDocumentXsltExtensions.CreateCurrentInstanceParameterCall(ci));
                }
                if (callingTemplate.ElementsTemplate && calledNodeInfo.ProcessElementsTemplate != null)
                {
                    callingElement.XslCallTemplate(calledNodeInfo.ProcessElementsTemplate.Name, XDocumentXsltExtensions.CreateCurrentInstanceParameterCall(ci));
                }
            }
        }

        private void GenCMCardinalityReference(XElement templateElement, Template callingTemplate, TemplateReference reference)
        {
            if (!DetectedChangeInstances.IsAddedNode(reference.ReferencedNode))
            {
                // process existing
                XElement xslForEachGroup = templateElement.XslForEachGroup(XPathExprGenerator.GetGroupMembers(context, reference.ReferencedNode, callingTemplate.HasCurrentInstanceParameter));
                xslForEachGroup.Add(XPathExprGenerator.GetGroupDistinguisher(context, reference.ReferencedNode));
                if (!reference.DeletionRequired)
                {
                    GenCMSingleReference(xslForEachGroup, callingTemplate, reference, "current-group()");
                }
                else
                {
                    XElement xslIf = xslForEachGroup.XslIf(new XPathExpr(string.Format("position() le {0}", reference.Upper)));
                    GenCMSingleReference(xslIf, callingTemplate, reference, "current-group()");
                }
            }
            //added node 
            if (DetectedChangeInstances.IsAddedNode(reference.ReferencedNode))
            {
                if (reference.Lower == 1)
                {
                    GenCMSingleReference(templateElement, callingTemplate, reference);
                }
                if (reference.Lower > 1)
                {
                    XElement xslForEach = templateElement.XslForEach(new XPathExpr("1 to {0}", reference.Lower));
                    GenCMSingleReference(xslForEach, callingTemplate, reference);
                }
            }
            // create new with instance generator
            if (reference.CreationRequired)
            {
                XPathExpr existing = context.GetRelativeXPath(reference.ReferencedNode, callingTemplate.HasCurrentInstanceParameter).Append(
                        XPathExprGenerator.GetGroupDistinguisher(context, reference.ReferencedNode).Value);
                XPathExpr countExpr = new XPathExpr(string.Format("{0} - count({1})", reference.Lower, existing));
                templateElement.XslCallTemplate(namingSupport.SuggestNameForInstanceGenerator(reference.ReferencedNode, callingTemplate, reference),
                    new XDocumentXsltExtensions.TemplateParameter("count", countExpr));
            }
        }

        private void GenCMSingleReference(XElement callingElement, Template callingTemplate, TemplateReference reference, string condition = null)
        {
            
        }

        private IEnumerable<PSMComponent> ExpandIfInlinedNode(TemplateReference reference)
        {
            List<PSMComponent> result = new List<PSMComponent>(); 
            if (reference.ReferencesInlinedNode)
            {
                ModelIterator.ExpandInlinedNode(reference.ReferencedNode, ref result, DetectedChangeInstances.IsInlinedNode);
            }
            else
            {
                result.Add(reference.ReferencedNode);
            }
            return result;
        }

        #region instance generators

        private void GenInstanceGenerators(XElement xslStylesheet)
        {
            List<PSMComponent> componentsRequireingInstanceGeneratorsTransitive =
                AddComponentsReferencedFromInstanceGenerators(componentsRequireingInstanceGenerators);

            XDocumentXsltExtensions.TemplateParameter countParameter = new XDocumentXsltExtensions.TemplateParameter("count", null) { DefaultValue = new XPathExpr("1"), Type="item()" };

            if (!componentsRequireingInstanceGeneratorsTransitive.IsEmpty())
            {
                xslStylesheet.Add(new XComment(" Instance generators "));
            }

            foreach (PSMComponent component in componentsRequireingInstanceGeneratorsTransitive)
            {
                bool attributeTemplateRequired, elementTemplateRequired, wrapTemplateRequired;
                RevalidationNodeInfo.DetermineRequiredTemplates(component, out attributeTemplateRequired, out elementTemplateRequired, out wrapTemplateRequired);
                
                if (wrapTemplateRequired)
                {
                    string name = namingSupport.SuggestNameForInstanceGenerator(component, wrappingTemplate:true);
                    XElement templateElement = xslStylesheet.XslNamedTemplate(name, countParameter);
                    XElement xslForEach = templateElement.XslForEach(new XPathExpr("1 to $count"));                    
                    XElement wrapNode = new XElement(((PSMClass)component).ParentAssociation.Name);
                    if (attributeTemplateRequired)
                    {
                        wrapNode.XslCallTemplate(namingSupport.SuggestNameForInstanceGenerator(component, attributesTemplate:true));
                    }
                    if (elementTemplateRequired)
                    {
                        wrapNode.XslCallTemplate(namingSupport.SuggestNameForInstanceGenerator(component, elementsTemplate:true));
                    }
                    xslForEach.Add(wrapNode);
                }
                
                if (attributeTemplateRequired)
                {
                    string name = namingSupport.SuggestNameForInstanceGenerator(component, attributesTemplate:true);
                    XElement templateElement = xslStylesheet.XslNamedTemplate(name, countParameter);
                    XElement xslForEach = templateElement.XslForEach(new XPathExpr("1 to $count"));
                    if (component is PSMAttribute)
                    {
                        XElement attribute = xslForEach.XslAttribute(component.Name);
                        attribute.Add(new XText(component.Name));
                        attribute.XslValueOf(new XPathExpr("current()"));                        
                    }
                    else
                    {
                        List<PSMComponent> children = ModelIterator.GetPSMChildren(component, true).ToList();
                        foreach (PSMComponent child in children)
                        {
                            bool dummy, definesAttributes;
                            RevalidationNodeInfo.DetermineRequiredTemplates(child, out definesAttributes, out dummy, out dummy);
                            if (definesAttributes)
                            {
                                if (componentsRequireingInstanceGeneratorsTransitive.Contains(child))
                                {
                                    xslForEach.XslCallTemplate(namingSupport.SuggestNameForInstanceGenerator(child, attributesTemplate: true));
                                }
                                else
                                {
                                    Debug.Assert(child is PSMAttribute);
                                    XElement attribute = xslForEach.XslAttribute(child.Name);
                                    attribute.Add(new XText(child.Name));                                    
                                }
                            }
                        }
                    }
                }

                if (elementTemplateRequired)
                {
                    string name = namingSupport.SuggestNameForInstanceGenerator(component, elementsTemplate: true);
                    XElement templateElement = xslStylesheet.XslNamedTemplate(name, countParameter);
                    XElement xslForEach = templateElement.XslForEach(new XPathExpr("1 to $count"));
                    if (component is PSMAttribute)
                    {
                        XElement wrapNode = new XElement(component.Name);
                        wrapNode.Add(new XText(component.Name));
                        wrapNode.XslValueOf(new XPathExpr("current()"));
                        xslForEach.Add(wrapNode);
                    }
                    else
                    {
                        List<PSMComponent> children = ModelIterator.GetPSMChildren(component, true).ToList();
                        foreach (PSMComponent child in children)
                        {
                            bool dummy, definesElements;
                            RevalidationNodeInfo.DetermineRequiredTemplates(child, out dummy, out definesElements, out dummy);
                            if (definesElements || (child is PSMClass && ((PSMClass)child).ParentAssociation.IsNamed))
                            {
                                if (componentsRequireingInstanceGeneratorsTransitive.Contains(child))
                                {
                                    if (child is PSMClass && ((PSMClass)child).ParentAssociation.IsNamed)
                                    {
                                        xslForEach.XslCallTemplate(namingSupport.SuggestNameForInstanceGenerator(child, wrappingTemplate: true));
                                    }
                                    else
                                    {
                                        xslForEach.XslCallTemplate(namingSupport.SuggestNameForInstanceGenerator(child, elementsTemplate: true));                                        
                                    }
                                }
                                else
                                {
                                    Debug.Assert(child is PSMAttribute);
                                    XElement wrapNode = new XElement(child.Name);
                                    wrapNode.Add(new XText(child.Name));                                    
                                    xslForEach.Add(wrapNode);
                                }
                            }
                        }                        
                    }
                }
            }
        }

        private List<PSMComponent> AddComponentsReferencedFromInstanceGenerators(IEnumerable<PSMComponent> psmComponents)
        {
            List<PSMComponent> result = new List<PSMComponent>();
            Queue<PSMComponent> toDo = new Queue<PSMComponent>();
            toDo.EnqueueRange(psmComponents);

            while (!toDo.IsEmpty())
            {
                PSMComponent component = toDo.Dequeue();
                result.Add(component);
                foreach (PSMComponent child in ModelIterator.GetPSMChildren(component, true))
                {
                    toDo.EnqueueIfNotContained(child);
                }

            }

            // (!(component is PSMAttribute) || (((PSMAttribute)component).Lower > 1 || componentsRequireingInstanceGenerators.Contains(component)))
            result.RemoveAll(c => c.DownCastSatisfies<PSMAttribute>(a => a.Lower == 1 && !psmComponents.Contains(a)));

            return result;
        }

        #endregion

        #region green and blue templates 

        private void GenXFormConvertorTemplateEtoA(XElement xslStylesheet)
        {
            List<XPathExpr> paths = new List<XPathExpr>();
            foreach (PSMAttribute psmAttribute in DetectedChangeInstances.ConvertedAttributesEA)
            {
                if (DetectedChangeInstances.GreenNodes.Contains(psmAttribute))
                {
                    paths.Add(new XPathExpr(psmAttribute.GetInVersion(OldVersion).XPath.Substring(Math.Max(0, psmAttribute.GetInVersion(OldVersion).XPath.LastIndexOf('/') + 1))));
                }
            }

            if (paths.Count > 0)
            {
                xslStylesheet.Add(new XComment("Element to attribute conversion template"));
                XElement xslTemplate = xslStylesheet.XslTemplate(XPathExpr.ConcatWithOrOperator(paths));
                xslTemplate.AddAttributeWithValue("priority", "0");
                XElement xslAttribute = xslTemplate.XslAttribute("{name()}");
                xslAttribute.XslValueOf(new XPathExpr("."));
            }
        }

        private void GenXFormConvertorTemplateAtoE(XElement xslStylesheet)
        {
            List<XPathExpr> paths = new List<XPathExpr>();
            foreach (PSMAttribute psmAttribute in DetectedChangeInstances.ConvertedAttributesAE)
            {
                if (DetectedChangeInstances.GreenNodes.Contains(psmAttribute))
                {
                    paths.Add(new XPathExpr(psmAttribute.GetInVersion(OldVersion).XPath.Substring(Math.Max(0, psmAttribute.GetInVersion(OldVersion).XPath.LastIndexOf('/') + 1))));
                }
            }

            if (paths.Count > 0)
            {
                xslStylesheet.Add(new XComment("Attribute to element conversion template"));
                XElement xslTemplate = xslStylesheet.XslTemplate(XPathExpr.ConcatWithOrOperator(paths));
                xslTemplate.AddAttributeWithValue("priority", "0");
                XElement xslElement = xslTemplate.XslElement("{name()}");
                xslElement.XslValueOf(new XPathExpr("."));
            }
        }

        private void GenGreenNodesTemplate(XElement xslStylesheet)
        {
            List<XPathExpr> paths = new List<XPathExpr>(); 
            foreach (PSMComponent psmComponent in DetectedChangeInstances.GreenNodes)
            {
                if (psmComponent.DownCastSatisfies<PSMClass>(psmClass => psmClass.ParentAssociation != null && psmClass.ParentAssociation.IsNamed)
                    || psmComponent.DownCastSatisfies<PSMAttribute>(psmAttribute => !DetectedChangeInstances.ConvertedAttributesAE.Contains(psmAttribute) 
                        && !DetectedChangeInstances.ConvertedAttributesEA.Contains(psmAttribute)))
                {
                    paths.Add(new XPathExpr(psmComponent.GetInVersion(OldVersion).XPath.Substring(Math.Max(0, psmComponent.GetInVersion(OldVersion).XPath.LastIndexOf('/') + 1))));
                }
            }

            if (paths.Count > 0)
            {
                xslStylesheet.Add(new XComment("Green nodes template"));
                XElement xslTemplate = xslStylesheet.XslTemplate(XPathExpr.ConcatWithOrOperator(paths));
                xslTemplate.AddAttributeWithValue("priority", "0");
                xslTemplate.XslCopyOf(new XPathExpr("."));
            }
            else
            {
                xslStylesheet.Add(new XComment("No blue nodes"));
            }
        }

        private void GenBlueNodesTemplate(XElement xslStylesheet)
        {
            List<XPathExpr> paths = new List<XPathExpr>(); 
            foreach (PSMComponent psmComponent in DetectedChangeInstances.BlueNodes)
            {
                if (psmComponent.DownCastSatisfies<PSMClass>(psmClass => psmClass.ParentAssociation != null && psmClass.ParentAssociation.IsNamed)
                    || psmComponent is PSMAttribute)
                {
                    paths.Add(new XPathExpr(psmComponent.GetInVersion(OldVersion).XPath.Substring(Math.Max(0, psmComponent.GetInVersion(OldVersion).XPath.LastIndexOf('/') + 1))));
                }
            }

            if (paths.Count > 0)
            {
                xslStylesheet.Add(new XComment("Blue nodes template"));
                XElement xslTemplate = xslStylesheet.XslTemplate(XPathExpr.ConcatWithOrOperator(paths));
                xslTemplate.AddAttributeWithValue("priority", "0");
                XElement xslCopy = xslTemplate.XslCopy();
                xslCopy.XslCopyOf(new XPathExpr("@*"));
                xslCopy.XslApplyTemplates(new XPathExpr("*"));
            }
            else
            {
                xslStylesheet.Add(new XComment("No blue nodes"));
            }
        }

        #endregion

        
    }

    public delegate void GenReferenceMethod(XElement templateElement, Template callingTemplate, TemplateReference reference);
    public delegate void GenReferenceWithConditionMethod(XElement templateElement, Template callingTemplate, TemplateReference reference, string condition = null);

    public class GeneratorContext
    {
        public bool CreatingMode { get; set; }

        public PSMComponent CurrentNode { get; set; }

        public Version OldVersion { get; set; }

        public Version NewVersion { get; set; }

        public XPathExpr GetRelativeXPath(PSMComponent node, bool useCurrentInstanceVariable)
        {
            PSMComponent closestExistingCurrentNode = CurrentNode.GetFirstAncestorOrSelfExistingInVersion(OldVersion).GetInVersion(OldVersion);
            PSMComponent closestExistingTargetNode = node.GetFirstAncestorOrSelfExistingInVersion(OldVersion).GetInVersion(OldVersion);
            
            if (closestExistingTargetNode == null)
            {
                throw new InvalidOperationException();
            }

            if (closestExistingCurrentNode == null)
            {
                return new XPathExpr(closestExistingTargetNode.XPath);
            }

            XPathExpr currentNodeAbsoluteXPath = new XPathExpr(closestExistingCurrentNode.GetInVersion(OldVersion).XPath);
            // /A/R
            XPathExpr targetNodeAbsoluteXPath = new XPathExpr(closestExistingTargetNode.GetInVersion(OldVersion).XPath);
            // /A/R/B/z
                     
            if (targetNodeAbsoluteXPath.HasPrefix(currentNodeAbsoluteXPath) && targetNodeAbsoluteXPath != currentNodeAbsoluteXPath)
            {
                string withoutPrefix = targetNodeAbsoluteXPath.ToString().Substring(currentNodeAbsoluteXPath.ToString().Length);
                while (withoutPrefix[0].IsAmong('/', '@'))
                    withoutPrefix = withoutPrefix.Substring(1);

                string nextStep;
                if (withoutPrefix.IndexOf('/') == -1)
                {
                    nextStep = withoutPrefix.Substring(0, withoutPrefix.Length);
                }
                else
                {
                    nextStep = withoutPrefix.Substring(0, withoutPrefix.IndexOf('/'));
                }

                string afterNextStep = withoutPrefix.Substring(nextStep.Length);

                XPathExpr result;

                if (useCurrentInstanceVariable)
                {
                    result = new XPathExpr(string.Format("$ci[name() = '{0}']{1}{2}", nextStep, string.IsNullOrEmpty(afterNextStep) ? string.Empty : "/", afterNextStep));
                }
                else
                {
                    result = new XPathExpr(string.Format("{0}{1}{2}", nextStep, string.IsNullOrEmpty(afterNextStep) ? string.Empty : "/", afterNextStep));
                }

                return result;
            }
            else if (targetNodeAbsoluteXPath == currentNodeAbsoluteXPath)
            {
                return new XPathExpr(".");
            }
            else
            {
                // find the uppermost common node
                PSMAssociationMember nearestCommonAncestor = closestExistingCurrentNode.GetNearestCommonAncestor(closestExistingTargetNode);
                if (nearestCommonAncestor != null)
                {
                    string commonPath = nearestCommonAncestor.XPath;
                    Debug.Assert(targetNodeAbsoluteXPath.HasPrefix(new XPathExpr(commonPath)));
                    Debug.Assert(currentNodeAbsoluteXPath.HasPrefix(new XPathExpr(commonPath)));
                    string fromCommonToTarget = targetNodeAbsoluteXPath.ToString().Replace(commonPath, string.Empty);
                    string fromCommonToCurrent = currentNodeAbsoluteXPath.ToString().Replace(commonPath, string.Empty);
                    string stepsUp = string.Empty;

                    foreach (char c in fromCommonToCurrent)
                    {
                        if (c == '/')
                            stepsUp += "../";
                    }

                    if (fromCommonToTarget.StartsWith("/"))
                    {
                        fromCommonToTarget = fromCommonToTarget.Substring(1);
                    }

                    return new XPathExpr(string.Format("{0}{1}", stepsUp, fromCommonToTarget));
                }
                else
                {
                    return new XPathExpr(closestExistingTargetNode.XPath);
                }
            }
        }

        public XPathExpr GetRelativeXPath(IEnumerable<PSMComponent> expandedReference, bool useCurrentInstanceVariable)
        {
            IEnumerable<XPathExpr> result = from component in expandedReference select GetRelativeXPath(component, useCurrentInstanceVariable);
            return XPathExpr.ConcatWithOrOperator(result);
        }
    }
}