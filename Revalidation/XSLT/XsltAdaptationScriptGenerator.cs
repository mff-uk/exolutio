using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;
using Exolutio.Model;
using Exolutio.Model.OCL.AST;
using Exolutio.Model.PSM;
using Exolutio.Model.PSM.Grammar.SchematronTranslation;
using Exolutio.Model.Versioning;
using Exolutio.Revalidation.Changes;
using Exolutio.SupportingClasses;
using Version = Exolutio.Model.Versioning.Version;

namespace Exolutio.Revalidation.XSLT
{
    public class XsltAdaptationScriptGenerator
    {
        public XsltAdaptationScriptGenerator()
        {

        }

        #region properties

        readonly Dictionary<PSMComponent, AdaptationNodeInfo> nodeInfos = new Dictionary<PSMComponent, AdaptationNodeInfo>();

        readonly List<PSMComponent> componentsRequireingInstanceGenerators = new List<PSMComponent>();

        public PSMSchema PSMSchemaOldVersion { get; private set; }

        public Version OldVersion { get { return PSMSchemaOldVersion.Version; } }

        public PSMSchema PSMSchemaNewVersion { get; private set; }

        public Version NewVersion { get { return PSMSchemaNewVersion.Version; } }

        public Log<OclExpression> OclTranslationLog { get; private set; }

        public DetectedChangeInstancesSet DetectedChangeInstances { get; private set; }

        public bool SchemaAware
        {
            get { return schemaAware; }
            set
            {
                schemaAware = value; 
                if (expressionConverter != null && expressionConverter.Settings != null)
                    expressionConverter.Settings.SchemaAware = true;
            }
        }

        private GeneratorContext context;

        private TemplateNamingSupport namingSupport;

        private List<Template> templates = new List<Template>();

        public void Initialize(PSMSchema psmSchemaOldVersion, PSMSchema psmSchemaNewVersion, DetectedChangeInstancesSet changeInstances)
        {
            this.PSMSchemaNewVersion = psmSchemaNewVersion;
            this.PSMSchemaOldVersion = psmSchemaOldVersion;
            DetectedChangeInstances = changeInstances;
            nodeInfos.Clear();
            componentsRequireingInstanceGenerators.Clear();
            templates.Clear();
        }

        #endregion

        public void GenerateTransformationStructure()
        {
            context = new GeneratorContext() { OldVersion = OldVersion, NewVersion = NewVersion };
            namingSupport = new TemplateNamingSupport() { PSMSchema = PSMSchemaNewVersion };

            // pregenerate 
            foreach (PSMComponent psmComponent in DetectedChangeInstances.RedNodes)
            {
                AdaptationNodeInfo nodeInfo = new AdaptationNodeInfo(psmComponent);
                nodeInfos[psmComponent] = nodeInfo;
            }

            foreach (KeyValuePair<PSMAssociation, OclExpression> kvp in
                DetectedChangeInstances.AssociationInitializations)
            {
                nodeInfos[kvp.Key.Child].InitializationExpression = kvp.Value;
            }

            foreach (KeyValuePair<PSMAttribute, OclExpression> kvp in
                DetectedChangeInstances.AttributeInitializations)
            {
                nodeInfos[kvp.Key].InitializationExpression = kvp.Value;
            }

            foreach (PSMComponent redNode in DetectedChangeInstances.RedNodes)
            {
                AdaptationNodeInfo nodeInfo = nodeInfos[redNode];

                context.CurrentNode = redNode;

                if (nodeInfo.ElementTemplateRequired || nodeInfo.AttributeTemplateRequired || redNode.DownCastSatisfies<PSMClass>(c => c.IsNamed))
                {
                    if (!(redNode is PSMAttribute))
                    {
                        Template processElementsTemplate = new Template();
                        templates.Add(processElementsTemplate);
                        processElementsTemplate.Name = namingSupport.SuggestName(redNode, true, false);
                        processElementsTemplate.ElementsTemplate = true;
                        processElementsTemplate.HasCurrentInstanceParameter |= DetectedChangeInstances.IsAddedNode(redNode);
                        nodeInfo.ProcessElementsTemplate = processElementsTemplate;


                        Template processAttributesTemplate = new Template();
                        templates.Add(processAttributesTemplate);
                        processAttributesTemplate.Name = namingSupport.SuggestName(redNode, elementsTemplate: false, attributesTemplate: true);
                        processAttributesTemplate.AttributesTemplate = true;
                        processAttributesTemplate.HasCurrentInstanceParameter |= DetectedChangeInstances.IsAddedNode(redNode);
                        nodeInfo.ProcessAttributesTemplate = processAttributesTemplate;

                        IEnumerable<PSMComponent> childComponents = ModelIterator.GetPSMChildren(redNode, returnAttributesForClass: true,
                            returnChildAssociationsForAssociationMembers: true);

                        foreach (PSMComponent _childComponent in childComponents)
                        {
                            PSMComponent childComponent = _childComponent;
                            PSMAssociation associationToChildComponent = childComponent as PSMAssociation;
                            if (associationToChildComponent != null)
                            {
                                childComponent = associationToChildComponent.Child;
                            }

                            if (childComponent is PSMAttribute && !((PSMAttribute)childComponent).Element)
                            {
                                TemplateReference attributeReference = new TemplateReference
                                {
                                    ReferencedNode = childComponent,
                                    CallingNode = redNode,
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
                                if (DetectedChangeInstances.AttributeInitializations.ContainsKey((PSMAttribute)childComponent))
                                {
                                    attributeReference.InitializationExpression =
                                        DetectedChangeInstances.AttributeInitializations[(PSMAttribute)childComponent];
                                }
                            }

                            TemplateReference reference = new TemplateReference
                            {
                                ReferencedNode = childComponent,
                                CallingNode = redNode,
                                ReferenceAssociation = associationToChildComponent,
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
                                if (DetectedChangeInstances.AttributeInitializations.ContainsKey((PSMAttribute)childComponent))
                                {
                                    reference.InitializationExpression =
                                        DetectedChangeInstances.AttributeInitializations[(PSMAttribute)childComponent];
                                }
                                processElementsTemplate.References.Add(reference);
                            }

                            if (childComponent is PSMAssociationMember)
                            {
                                if (associationToChildComponent != null &&
                                    DetectedChangeInstances.AssociationInitializations.ContainsKey(associationToChildComponent))
                                {
                                    reference.InitializationExpression =
                                        DetectedChangeInstances.AssociationInitializations[associationToChildComponent];
                                }

                                if (nodeInfos.ContainsKey(childComponent))
                                {
                                    AdaptationNodeInfo childNodeInfo = nodeInfos[childComponent];
                                    if (childNodeInfo.AttributeTemplateRequired &&
                                        childComponent.DownCastSatisfies<PSMAssociationMember>(c => !c.ParentAssociation.IsNamed || c is PSMContentModel))
                                        processAttributesTemplate.References.Add(reference);
                                    if (childNodeInfo.ElementTemplateRequired ||
                                        childComponent.DownCastSatisfies<PSMClass>(c => c.ParentAssociation.IsNamed))
                                        processElementsTemplate.References.Add(reference);
                                }
                                else
                                {
                                    bool attributeRequired;
                                    bool elementRequired;
                                    bool dummy;
                                    Debug.Assert(!reference.ReferencesRedNode);
                                    AdaptationNodeInfo.DetermineRequiredTemplates(reference.ReferencedNode, out attributeRequired, out elementRequired, out dummy);
                                    if (attributeRequired &&
                                        childComponent.DownCastSatisfies<PSMAssociationMember>(c => !c.ParentAssociation.IsNamed || c is PSMContentModel))
                                    {
                                        processAttributesTemplate.References.Add(reference);
                                    }
                                    if (elementRequired ||
                                        childComponent.DownCastSatisfies<PSMClass>(c => c.ParentAssociation.IsNamed))
                                    {
                                        processElementsTemplate.References.Add(reference);
                                    }
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
                    if (redNode is PSMClass && PSMSchemaNewVersion.Roots.Contains((PSMClass)redNode))
                        generateWrappingTemplate = false;
                    if (DetectedChangeInstances.IsGroupNode(redNode) && redNode.DownCastSatisfies<PSMClass>(g => g.ParentAssociation == null || !g.ParentAssociation.IsNamed))
                        generateWrappingTemplate = false;
                    if (redNode.DownCastSatisfies<PSMAttribute>(a => DetectedChangeInstances.IsAddedNode(a)))
                        generateWrappingTemplate = false;
                    if (redNode is PSMContentModel)
                        generateWrappingTemplate = false;

                    #region generate wrapping template
                    if (generateWrappingTemplate)
                    {
                        Template processNodeTemplate = new Template();
                        templates.Add(processNodeTemplate);
                        nodeInfo.ProcessNodeTemplate = processNodeTemplate;
                        processNodeTemplate.WrapElementName = namingSupport.GetWrapElementName(redNode);

                        // added class
                        if (DetectedChangeInstances.IsAddedNode(redNode))
                        {
                            processNodeTemplate.Name = namingSupport.SuggestName(redNode, false, false);
                            processNodeTemplate.HasCurrentInstanceParameter = true;
                        }
                        // existing class
                        else
                        {
                            if (!DetectedChangeInstances.IsGroupNode(redNode))
                            {
                                // use absolute path for template matches
                                XPathExpr absolutePath = new XPathExpr(redNode.GetInVersion(OldVersion).XPath);
                                processNodeTemplate.MatchList.Add(absolutePath);
                            }
                            else
                            {
                                processNodeTemplate.Name = namingSupport.SuggestName(redNode, false, false);
                                processNodeTemplate.GroupNodeTemplate = true;
                            }
                        }
                    }
                    #endregion
                }

                if (nodeInfo.ConstructorFunctionRequired)
                {
                    Template constructorFunction = new Template();
                    templates.Add(constructorFunction);
                    constructorFunction.Node = redNode;
                    constructorFunction.WrapElementName = namingSupport.GetWrapElementName(redNode);
                    constructorFunction.Name = namingSupport.SuggestNameForConstructor(redNode);
                    constructorFunction.ConstructorFunction = true;
                    nodeInfo.ConstructorFunction = constructorFunction;
                }
            }
        }

        public XDocument GetAdaptationTransformation()
        {
            XDocument doc = new XDocument(new XDeclaration("1.0", "utf-8", null));
            XElement xslStylesheet = doc.XslStylesheet("3.0");
            xslStylesheet.Add(new XAttribute(XNamespace.Xmlns + "xs", XDocumentXsltExtensions.XSD_NAMESPACE));
            xslStylesheet.Add(new XAttribute(XNamespace.Xmlns + "const", XDocumentXsltExtensions.EXOLUTIO_CONSTRUCTORS_NAMESPACE));
            foreach (KeyValuePair<string, XNamespace> kvp in XDocumentXsltExtensions.OCLX_NAMESPACES)
            {
                xslStylesheet.Add(new XAttribute(XNamespace.Xmlns + kvp.Key, kvp.Value));
            }

            if (Environment.MachineName.Contains("TRUPIK"))
            {
                xslStylesheet.XslImport(@"file:/D:\Programování\EvoXSVN\OclX\oclX-functional.xsl");
            }
            else
            {
                xslStylesheet.XslImport(@"oclX-functional.xsl");
            }

            XElement outputElement = new XElement(XDocumentXsltExtensions.XSLT_NAMESPACE + "output");
            outputElement.Add(new XAttribute("method", "xml"));
            outputElement.Add(new XAttribute("indent", "yes"));
            xslStylesheet.Add(outputElement);
            IEnumerable<PSMComponent> orderedRedNodes = ModelIterator.OrderBFS(DetectedChangeInstances.RedNodes);
            foreach (PSMComponent redNode in orderedRedNodes)
            {
                context.CurrentNode = redNode;
                AdaptationNodeInfo nodeInfo = nodeInfos[redNode];
                if (nodeInfo.ConstructorFunctionRequired)
                {
                    GenConstructorFunction(redNode, xslStylesheet);
                }
                if (nodeInfo.ProcessNodeTemplate != null)
                {
                    GenWrappingNodeTemplate(redNode, xslStylesheet);
                }
                if (nodeInfo.ProcessAttributesTemplate != null)
                {
                    GenProcessTemplate(redNode, nodeInfo.ProcessAttributesTemplate, xslStylesheet);
                }
                if (nodeInfo.ProcessElementsTemplate != null)
                {
                    GenProcessTemplate(redNode, nodeInfo.ProcessElementsTemplate, xslStylesheet);
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
            AdaptationNodeInfo nodeInfo = nodeInfos[psmComponent];
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
                if (processNodeTemplate.HasCurrentInstanceParameter || processNodeTemplate.GroupNodeTemplate)
                // i think only the first part of the condition should be here... 
                {
                    // group nodes have current instance parameter, even when they are for "existing" nodes
                    parameters = new[] { XDocumentXsltExtensions.CreateCurrentInstanceParameterDeclaration() };
                    processNodeTemplate.HasCurrentInstanceParameter = true;
                }
                templateElement = xslStylesheet.XslNamedTemplate(processNodeTemplate.Name, parameters);
            }

            XElement contentElement = templateElement;

            // create wrapping node if needed
            if (!string.IsNullOrEmpty(processNodeTemplate.WrapElementName))
            {
                XElement wrapNode = null;
                if (psmComponent is PSMAttribute && !((PSMAttribute)psmComponent).Element)
                {
                    wrapNode = contentElement.XslAttribute(processNodeTemplate.WrapElementName);
                }
                else
                {
                    wrapNode = new XElement(processNodeTemplate.WrapElementName);
                    contentElement.Add(wrapNode);
                }

                contentElement = wrapNode;
            }

            // add value for attributes 
            if (psmComponent is PSMAttribute)
            {
                // this is the only place where GetRelativeXPath is called with useCurrentInstance = false 
                // and it is very likely wrong or just not relevant
                XPathExpr relativePath = context.GetRelativeXPath(psmComponent, false);
                contentElement.XslValueOf(relativePath);
            }

            // subtree processing (attributes)
            if (nodeInfo.ProcessAttributesTemplate != null)
            {
                XDocumentXsltExtensions.TemplateParameter[] parameters = null;
                if (nodeInfo.ProcessElementsTemplate.HasCurrentInstanceParameter || !DetectedChangeInstances.IsAddedNode(psmComponent))
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
                if (nodeInfo.ProcessElementsTemplate.HasCurrentInstanceParameter || !DetectedChangeInstances.IsAddedNode(psmComponent))
                {
                    XPathExpr currentInstanceCall = processNodeTemplate.GroupNodeTemplate ? new XPathExpr("$ci") : new XPathExpr("./(* | @*)");
                    parameters = new[] { XDocumentXsltExtensions.CreateCurrentInstanceParameterCall(currentInstanceCall) };
                }

                contentElement.XslCallTemplate(nodeInfo.ProcessElementsTemplate.Name, parameters);
            }
            xslStylesheet.Add(new XComment("End of: " + psmComponent.ToString()));
        }

        private void GenProcessTemplate(PSMComponent psmComponent, Template processTemplate, XElement xslStylesheet)
        {
            XDocumentXsltExtensions.TemplateParameter[] parameters = null;
            // JM 30.6.2012: attempt to push $ci through to templates of added nodes 
            //if (!DetectedChangeInstances.IsAddedNode(psmComponent) || DetectedChangeInstances.IsInlinedNode(psmComponent))
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
                    GenReference(processTemplate, templateElement, templateReference);
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
                GenReference(processTemplate, xslWhen, templateReference);
            }
        }

        private void GenReference(Template processTemplate, XElement templateElement, TemplateReference templateReference)
        {
            GenReferenceWithConditionMethod singleReference;
            GenReferenceMethod cardinalityReference;
            templateElement.Add(new XComment(string.Format("ref {0}", templateReference.ReferencedNode)));
            if (templateReference.HasExplicitInitialization)
            {
                if (templateReference.ReferencedNode is PSMAttribute)
                    GenInitializationReferenceForPSMAttribute(templateElement, processTemplate, templateReference);
                else
                    GenInitializationReferenceForPSMAssociation(templateElement, processTemplate, templateReference);
            }
            else
            {
                if (/*templateReference.ReferencesRedNode && */ templateReference.ReferencesGroupNode)
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
            AdaptationNodeInfo calledNodeInfo;
            nodeInfos.TryGetValue(reference.ReferencedNode, out calledNodeInfo);

            if (DetectedChangeInstances.IsAddedNode(reference.ReferencedNode))
            {
                Debug.Assert(calledNodeInfo != null);
                if (calledNodeInfo.Node.DownCastSatisfies<PSMAttribute>(a => a.Lower > 0))
                {
                    IEnumerable<XDocumentXsltExtensions.TemplateParameter> parameters = ((PSMAttribute)reference.ReferencedNode).Lower > 1 ?
                        new[] { new XDocumentXsltExtensions.TemplateParameter("count", new XPathExpr(((PSMAttribute)reference.ReferencedNode).Lower.ToString())) } : null;
                    templateElement.XslCallTemplate(namingSupport.SuggestNameForInstanceGenerator(calledNodeInfo.Node), parameters);
                }
                else if (callingTemplate.ElementsTemplate)
                {
                    //templateElement.XslCallTemplate(calledNodeInfo.ProcessNodeTemplate.Name);    
                    // JM 30.6.2012: attempt to push $ci through to templates of added nodes 
                    IEnumerable<XDocumentXsltExtensions.TemplateParameter> parameters = callingTemplate.HasCurrentInstanceParameter ?
                        new[] { XDocumentXsltExtensions.CreateCurrentInstanceParameterCall(new XPathExpr("$ci")) } : null;
                    templateElement.XslCallTemplate(calledNodeInfo.ProcessNodeTemplate.Name, parameters);
                }
                else if (callingTemplate.AttributesTemplate)
                {
                    templateElement.XslCallTemplate(calledNodeInfo.ProcessAttributesTemplate.Name);
                }
            }
            else
            {
                IEnumerable<PSMComponent> expandedReference = ExpandIfInlinedNode(reference);
                XPathExpr relativeXPath = context.GetRelativeXPath(expandedReference, callingTemplate.HasCurrentInstanceParameter).AppendPredicate(condition);
                templateElement.XslApplyTemplates(relativeXPath);
            }
        }

        private void GenGroupNodeCardinalityReference(XElement templateElement, Template callingTemplate, TemplateReference reference)
        {
            //if (reference.ReferencesRedNode)
            {
                if (!DetectedChangeInstances.IsAddedNode(reference.ReferencedNode))
                {
                    // process existing
                    XElement xslForEachGroup = templateElement.XslForEachGroup(context.GetGroupMembers(reference.ReferencedNode, callingTemplate.HasCurrentInstanceParameter));
                    XAttribute groupDistinguisherAttribute = new XAttribute("group-starting-with", context.GetGroupDistinguisher(reference.ReferencedNode));
                    xslForEachGroup.Add(groupDistinguisherAttribute);
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
                            context.GetGroupDistinguisher(reference.ReferencedNode));
                    XPathExpr countExpr = new XPathExpr(string.Format("{0} - count({1})", reference.Lower, existing));
                    templateElement.XslCallTemplate(namingSupport.SuggestNameForInstanceGenerator(reference.ReferencedNode, callingTemplate, reference),
                        new XDocumentXsltExtensions.TemplateParameter("count", countExpr));
                }
            }
            //else
            //{

            //}
        }

        private void GenGroupNodeSingleReference(XElement callingElement, Template callingTemplate, TemplateReference reference, string condition = null)
        {
            if (reference.ReferencesRedNode)
            {
                AdaptationNodeInfo calledNodeInfo = nodeInfos[reference.ReferencedNode];

                XPathExpr ci = condition == null ? context.GetGroupMembers(reference.ReferencedNode, callingTemplate.HasCurrentInstanceParameter) : new XPathExpr(condition);

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
            else
            {
                // deep inline group, all it's members
                List<PSMComponent> inlinedGroup = AddChildComponentsTransitive(new[] { reference.ReferencedNode }, null,
                    c => DetectedChangeInstances.IsInlinedNode(c) && !DetectedChangeInstances.RedNodes.Contains(c));
                bool useCurrentInstance = callingTemplate.HasCurrentInstanceParameter;
                if (condition == "current-group()")
                {
                    callingElement.XslVariable(XDocumentXsltExtensions.ParamCurrentInstance, new XPathExpr("current-group()"));
                    useCurrentInstance = true;
                }
                foreach (PSMComponent psmComponent in inlinedGroup)
                {
                    bool attributeRequired;
                    bool elementRequired;
                    bool wrapTemplateRequired;
                    AdaptationNodeInfo.DetermineRequiredTemplates(psmComponent, out attributeRequired, out elementRequired, out wrapTemplateRequired);

                    if ((callingTemplate.ElementsTemplate && (wrapTemplateRequired || psmComponent.DownCastSatisfies<PSMAttribute>(a => a.Element))) ||
                        (callingTemplate.AttributesTemplate && (psmComponent.DownCastSatisfies<PSMAttribute>(a => !a.Element))))
                    {
                        XPathExpr relativeXPath = context.GetRelativeXPath(psmComponent, useCurrentInstance);
                        callingElement.XslApplyTemplates(relativeXPath);
                    }
                }
            }
        }

        private void GenCMCardinalityReference(XElement templateElement, Template callingTemplate, TemplateReference reference)
        {
            if (!DetectedChangeInstances.IsAddedNode(reference.ReferencedNode))
            {
                // process existing
                XElement xslForEachGroup = templateElement.XslForEachGroup(context.GetGroupMembers(reference.ReferencedNode, callingTemplate.HasCurrentInstanceParameter));
                xslForEachGroup.Add(context.GetGroupDistinguisher(ExpandIfInlinedNode(reference, 1)));
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
                        context.GetGroupDistinguisher(ExpandIfInlinedNode(reference, 1)));
                XPathExpr countExpr = new XPathExpr(string.Format("{0} - count({1})", reference.Lower, existing));
                templateElement.XslCallTemplate(namingSupport.SuggestNameForInstanceGenerator(reference.ReferencedNode, callingTemplate, reference),
                    new XDocumentXsltExtensions.TemplateParameter("count", countExpr));
            }
        }

        private void GenCMSingleReference(XElement callingElement, Template callingTemplate, TemplateReference reference, string condition = null)
        {
            AdaptationNodeInfo calledNodeInfo = nodeInfos[reference.ReferencedNode];

            XPathExpr ci = condition == null ? context.GetGroupMembers(reference.ReferencedNode, callingTemplate.HasCurrentInstanceParameter)
                : new XPathExpr(condition);

            if (reference.ReferencesRedNode)
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
            else
            {
                // deep inline group, all it's members
                List<PSMComponent> inlinedGroup = AddChildComponentsTransitive(new[] { reference.ReferencedNode }, null,
                    c => DetectedChangeInstances.IsInlinedNode(c) && !DetectedChangeInstances.RedNodes.Contains(c));
                bool useCurrentInstance = callingTemplate.HasCurrentInstanceParameter;
                if (condition == "current-group()")
                {
                    callingElement.XslVariable(XDocumentXsltExtensions.ParamCurrentInstance, new XPathExpr("current-group()"));
                    useCurrentInstance = true;
                }
                foreach (PSMComponent psmComponent in inlinedGroup)
                {
                    bool attributeRequired;
                    bool elementRequired;
                    bool wrapTemplateRequired;
                    AdaptationNodeInfo.DetermineRequiredTemplates(psmComponent, out attributeRequired, out elementRequired, out wrapTemplateRequired);

                    if ((callingTemplate.ElementsTemplate && (wrapTemplateRequired || psmComponent.DownCastSatisfies<PSMAttribute>(a => a.Element))) ||
                        (callingTemplate.AttributesTemplate && (psmComponent.DownCastSatisfies<PSMAttribute>(a => !a.Element))))
                    {
                        XPathExpr relativeXPath = context.GetRelativeXPath(psmComponent, useCurrentInstance);
                        callingElement.XslApplyTemplates(relativeXPath);
                    }
                }
            }
        }

        private IEnumerable<PSMComponent> ExpandIfInlinedNode(TemplateReference reference, int? maxRecursion = null)
        {
            List<PSMComponent> result = new List<PSMComponent>();
            if (reference.ReferencesInlinedNode)
            {
                ModelIterator.ExpandInlinedNode(reference.ReferencedNode, ref result, DetectedChangeInstances.IsInlinedNode, maxRecursion);
            }
            else
            {
                result.Add(reference.ReferencedNode);
            }
            return result;
        }

        #region OCL initialization

        private PSMOCLtoXPathConverterFunctional expressionConverter;
        private bool schemaAware;

        private void InitializeExpressionConverter()
        {
            expressionConverter = new PSMOCLtoXPathConverterFunctional();
            expressionConverter.Bridge = new Model.OCL.Bridge.PSMBridge(PSMSchemaNewVersion);
            expressionConverter.Settings = new TranslationSettings(true, true)
            {
                Evolution = true,
                OldVersionSchema = PSMSchemaOldVersion,
                GetRelativeXPathEvolutionCallback = context.GetRelativeXPathEvolutionCallback,
                SchemaAware = this.SchemaAware
            };
            expressionConverter.ConstructorFunctions.AddRange(templates.Where(t => t.ConstructorFunction).Select(t => new KeyValuePair<PSMComponent, string>(t.Node, t.Name)));
            OclTranslationLog = new Log<OclExpression>();
            expressionConverter.Log = OclTranslationLog;
        }

        private void GenInitializationReferenceForPSMAssociation(XElement templateElement, Template processTemplate, TemplateReference templateReference)
        {
            PSMAssociation association = templateReference.ReferenceAssociation;
            PSMAssociationMember associationMember = (PSMAssociationMember)templateReference.ReferencedNode;
            string varname = (association.IsNamed ? association.Name : associationMember.Name) + "-new-values";
            OclExpression initializationOclExpression = templateReference.InitializationExpression;
            if (expressionConverter == null)
            {
                InitializeExpressionConverter();
            }

            expressionConverter.EvolutionAssignmentStack.Clear();
            expressionConverter.EvolutionAssignmentStack.Push(new EvolutionAssignmentStackEntry() { PSMAssociation = association });
            expressionConverter.OclContext = initializationOclExpression.ConstraintContext;
            var xpathExpr = expressionConverter.TranslateExpression(initializationOclExpression);

            templateElement.XslVariable(varname, new XPathExpr(xpathExpr), "item()*");
            templateElement.XslSequence(new XPathExpr("$" + varname));
        }

        private void GenInitializationReferenceForPSMAttribute(XElement templateElement, Template processTemplate, TemplateReference templateReference)
        {
            PSMAttribute attribute = (PSMAttribute)templateReference.ReferencedNode;
            string varname = attribute.Name + "-new-values";
            OclExpression initializationOclExpression = templateReference.InitializationExpression;
            if (expressionConverter == null)
            {
                InitializeExpressionConverter();
            }

            expressionConverter.OclContext = initializationOclExpression.ConstraintContext;
            var xpathExpr = expressionConverter.TranslateExpression(initializationOclExpression);

            templateElement.XslVariable(varname, new XPathExpr(xpathExpr), "item()*");
            XElement xslForEach = templateElement.XslForEach(new XPathExpr("$" + varname));
            if (attribute.Element)
            {
                XElement outputLiteralElement = xslForEach.OutputLiteralElement(attribute.Name);
                outputLiteralElement.XslSequence(new XPathExpr("."));
            }
            else
            {
                xslForEach.XslAttribute(attribute.Name, "{ . }");
            }
        }

        private void GenConstructorFunction(PSMComponent redNode, XElement xslStylesheet)
        {
            AdaptationNodeInfo nodeInfo = nodeInfos[redNode];
            Template constructorFunction = nodeInfo.ConstructorFunction;

            List<XDocumentXsltExtensions.TemplateParameter> parameters =
                new List<XDocumentXsltExtensions.TemplateParameter>();
            parameters.Add(new XDocumentXsltExtensions.TemplateParameter("element-name", null) { Type = "xs:string" });

            IEnumerable<PSMComponent> childComponents = ModelIterator.GetPSMChildren(redNode, returnAttributesForClass: true,
                            returnChildAssociationsForAssociationMembers: true);

            // first attributes
            foreach (PSMAttribute psmAttribute in childComponents.Where(cp => cp.DownCastSatisfies<PSMAttribute>(a => !a.Element)))
            {
                XDocumentXsltExtensions.TemplateParameter parameter = new XDocumentXsltExtensions.TemplateParameter(psmAttribute.Name, null)
                    {
                        Type = psmAttribute.AttributeType.XSDTypeName
                    };
                parameters.Add(parameter);
            }

            // then elements
            foreach (PSMComponent _childComponent in childComponents.Where(cp => !cp.DownCastSatisfies<PSMAttribute>(a => !a.Element)))
            {
                PSMComponent childComponent = _childComponent;
                PSMAssociation associationToChildComponent = childComponent as PSMAssociation;
                if (associationToChildComponent != null)
                {
                    childComponent = associationToChildComponent.Child;
                }
                XDocumentXsltExtensions.TemplateParameter parameter = new XDocumentXsltExtensions.TemplateParameter(childComponent.Name, null)
                {
                    Type = "item()*"
                };
                parameters.Add(parameter);
            }

            XElement functionElement = xslStylesheet.XslFunction("const:" + constructorFunction.Name, "item()*", parameters.ToArray());
            XElement xslChoose = functionElement.XslChoose();
            XElement xslWhen = xslChoose.XslWhen(new XPathExpr("$element-name ne ''"));
            XElement xslElement = xslWhen.XslElement("{$element-name}");
            foreach (XDocumentXsltExtensions.TemplateParameter templateParameter in parameters.Skip(1))
            {
                xslElement.XslSequence(new XPathExpr("$" + templateParameter.Name));
            }
            XElement xslOtherwise = new XElement(xslElement); // copy content then rename
            xslOtherwise.Name = XDocumentXsltExtensions.XSLT_NAMESPACE + "otherwise";
            xslOtherwise.RemoveAttributes();
            xslChoose.Add(xslOtherwise);
        }

        #endregion

        #region instance generators

        private void GenInstanceGenerators(XElement xslStylesheet)
        {
            Predicate<PSMComponent> removeFromResult =
                c => c.DownCastSatisfies<PSMAttribute>(a => a.Lower == 1 && !componentsRequireingInstanceGenerators.Contains(a));
            List<PSMComponent> componentsRequireingInstanceGeneratorsTransitive =
                AddChildComponentsTransitive(componentsRequireingInstanceGenerators, removeFromResult);

            XDocumentXsltExtensions.TemplateParameter countParameter = new XDocumentXsltExtensions.TemplateParameter("count", null) { DefaultValue = new XPathExpr("1"), Type = "item()" };

            if (!componentsRequireingInstanceGeneratorsTransitive.IsEmpty())
            {
                xslStylesheet.Add(new XComment(" Instance generators "));
            }

            foreach (PSMComponent component in componentsRequireingInstanceGeneratorsTransitive)
            {
                bool attributeTemplateRequired, elementTemplateRequired, wrapTemplateRequired;
                AdaptationNodeInfo.DetermineRequiredTemplates(component, out attributeTemplateRequired, out elementTemplateRequired, out wrapTemplateRequired);

                if (wrapTemplateRequired)
                {
                    string name = namingSupport.SuggestNameForInstanceGenerator(component, wrappingTemplate: true);
                    XElement templateElement = xslStylesheet.XslNamedTemplate(name, countParameter);
                    XElement xslForEach = templateElement.XslForEach(new XPathExpr("1 to $count"));
                    XElement wrapNode = new XElement(((PSMClass)component).ParentAssociation.Name);
                    if (attributeTemplateRequired)
                    {
                        wrapNode.XslCallTemplate(namingSupport.SuggestNameForInstanceGenerator(component, attributesTemplate: true));
                    }
                    if (elementTemplateRequired)
                    {
                        wrapNode.XslCallTemplate(namingSupport.SuggestNameForInstanceGenerator(component, elementsTemplate: true));
                    }
                    xslForEach.Add(wrapNode);
                }

                if (attributeTemplateRequired)
                {
                    string name = namingSupport.SuggestNameForInstanceGenerator(component, attributesTemplate: true);
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
                            AdaptationNodeInfo.DetermineRequiredTemplates(child, out definesAttributes, out dummy, out dummy);
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
                            AdaptationNodeInfo.DetermineRequiredTemplates(child, out dummy, out definesElements, out dummy);
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

        private List<PSMComponent> AddChildComponentsTransitive(IEnumerable<PSMComponent> psmComponents,
            Predicate<PSMComponent> removeFromResult = null,
            Predicate<PSMComponent> continueTransitive = null)
        {
            List<PSMComponent> result = new List<PSMComponent>();
            Queue<PSMComponent> toDo = new Queue<PSMComponent>();
            toDo.EnqueueRange(psmComponents);

            while (!toDo.IsEmpty())
            {
                PSMComponent component = toDo.Dequeue();
                result.Add(component);
                if (continueTransitive == null || continueTransitive(component))
                {
                    foreach (PSMComponent child in ModelIterator.GetPSMChildren(component, true))
                    {
                        toDo.EnqueueIfNotContained(child);
                    }
                }
            }

            // (!(component is PSMAttribute) || (((PSMAttribute)component).Lower > 1 || componentsRequireingInstanceGenerators.Contains(component)))
            if (removeFromResult != null)
            {
                result.RemoveAll(removeFromResult);
            }

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
                XElement xslTemplate = xslStylesheet.XslTemplate(XPathExpr.ConcatWithPipeOperator(paths));
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
                XElement xslTemplate = xslStylesheet.XslTemplate(XPathExpr.ConcatWithPipeOperator(paths));
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
                XElement xslTemplate = xslStylesheet.XslTemplate(XPathExpr.ConcatWithPipeOperator(paths));
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
                XElement xslTemplate = xslStylesheet.XslTemplate(XPathExpr.ConcatWithPipeOperator(paths));
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
}