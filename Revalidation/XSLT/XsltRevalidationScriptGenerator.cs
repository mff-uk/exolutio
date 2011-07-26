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
        }

        #endregion 

        public void GenerateTemplateStructure()
        {
            context = new GeneratorContext() { OldVersion = OldVersion, NewVersion = NewVersion};
            namingSupport = new TemplateNamingSupport() {PSMSchema = PSMSchemaNewVersion};

            foreach (PSMComponent psmComponent in DetectedChangeInstances.RedNodes)
            {
                RevalidationNodeInfo nodeInfo = new RevalidationNodeInfo(psmComponent);
                nodeInfos[psmComponent] = nodeInfo;

                context.CurrentNode = psmComponent;
                
                if (nodeInfo.ElementTemplateRequired || nodeInfo.AttributeTemplateRequired)
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

                            if (childComponent is PSMAttribute && ((PSMAttribute)childComponent).Element)
                            {
                                processElementsTemplate.References.Add(reference);
                            }

                            if (childComponent is PSMAssociationMember)
                            {
                                // TODO: !!!!! temporary - this prevents from calling attributes of nested classes without named associations
                                //processAttributesTemplate.References.Add(reference);
                                processElementsTemplate.References.Add(reference);
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

                    // not a root class => node template (wrapping template)
                    if (!(psmComponent is PSMClass && PSMSchemaNewVersion.Roots.Contains((PSMClass)psmComponent)))
                    {
                        // TODO: unnamed associations - group clases
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

                if (nodeInfo.ElementTemplateRequired)
                {
                    if (nodeInfo.ProcessNodeTemplate != null)
                    {
                        GenNodeTemplate(psmComponent, xslStylesheet);
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
            }

            GenXFormConvertorTemplateEtoA(xslStylesheet);
            GenXFormConvertorTemplateAtoE(xslStylesheet);
            GenBlueNodesTemplate(xslStylesheet);
            GenGreenNodesTemplate(xslStylesheet);

            return doc;
        }

        private void GenNodeTemplate(PSMComponent psmComponent, XElement xslStylesheet)
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
                    parameters = new XDocumentXsltExtensions.TemplateParameter[] { XDocumentXsltExtensions.CreateCurrentInstanceParameterDeclaration() };
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
                if (DetectedChangeInstances.IsAddedNode(psmComponent))
                {
                    contentElement.Add(new XText("###"));
                }
                else
                {
                    XPathExpr relativePath = context.GetRelativeXPath(psmComponent, false);
                    contentElement.XslValueOf(relativePath);
                }
            }

            // subtree processing (attributes)
            if (nodeInfo.ProcessAttributesTemplate != null)
            {
                XDocumentXsltExtensions.TemplateParameter[] parameters = null;
                if (!DetectedChangeInstances.IsAddedNode(psmComponent))
                {
                    XPathExpr currentInstanceCall = processNodeTemplate.GroupNodeTemplate ? new XPathExpr("$ci") : new XPathExpr("./(* | @*)");
                    parameters = new XDocumentXsltExtensions.TemplateParameter[] { XDocumentXsltExtensions.CreateCurrentInstanceParameterCall(currentInstanceCall) };
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
                    parameters = new XDocumentXsltExtensions.TemplateParameter[] { XDocumentXsltExtensions.CreateCurrentInstanceParameterCall(currentInstanceCall) };
                }

                contentElement.XslCallTemplate(nodeInfo.ProcessElementsTemplate.Name, parameters);
            }
            xslStylesheet.Add(new XComment("End of: " + psmComponent.ToString()));
        }

        private void GenProcessTemplate(PSMComponent psmComponent, Template processTemplate,  XElement xslStylesheet)
        {
            XDocumentXsltExtensions.TemplateParameter[] parameters = null;
            if (!DetectedChangeInstances.IsAddedNode(psmComponent))
            {
                parameters = new XDocumentXsltExtensions.TemplateParameter[] { XDocumentXsltExtensions.CreateCurrentInstanceParameterDeclaration() };
            }
            XElement templateElement = xslStylesheet.XslNamedTemplate(processTemplate.Name, parameters);
            foreach (TemplateReference templateReference in processTemplate.References)
            {
                if (templateReference.ReferencesRedNode && templateReference.ReferencesGroupNode)
                {
                    GenGroupNodeCardinalityReference(templateElement, processTemplate, templateReference);
                }
                else if (templateReference.Lower > 1 || DetectedChangeInstances.ExistsCardinalityChange(psmComponent))
                {
                    GenCardinalityReference(templateElement, processTemplate, templateReference);
                }
                else
                {
                    GenSingleReference(templateElement, processTemplate, templateReference, null);
                }
            }
        }

        private void GenGroupNodeCardinalityReference(XElement templateElement, Template callingTemplate, TemplateReference reference)
        {
            bool useCurrentInstanceVariable = !DetectedChangeInstances.IsAddedNode(reference.CallingNode);

            if (!DetectedChangeInstances.IsAddedNode(reference.ReferencedNode))
            {
                // process existing
                XElement xslForEachGroup = templateElement.XslForEachGroup(XPathExprGenerator.GetGroupMembers(context, reference.ReferencedNode, useCurrentInstanceVariable));
                xslForEachGroup.Add(XPathExprGenerator.GetGroupDistinguisher(context, reference.ReferencedNode));
                if (!reference.DeletionRequired)
                {
                    GenGroupNodeSingleReference(xslForEachGroup, callingTemplate, reference);
                }
                else
                {
                    XElement xslIf = xslForEachGroup.XslIf(new XPathExpr(string.Format("position() leq {0}", reference.Upper)));
                    GenGroupNodeSingleReference(xslIf, callingTemplate, reference);
                }
            }
            if (DetectedChangeInstances.IsAddedNode(reference.ReferencedNode) || reference.CreationRequired)
            {
                XPathExpr countExpr;
                if (!DetectedChangeInstances.IsAddedNode(reference.ReferencedNode))
                {
                    XPathExpr existing = context.GetRelativeXPath(reference.ReferencedNode, useCurrentInstanceVariable).Append(
                        "/" + XPathExprGenerator.GetGroupDistinguisher(context, reference.ReferencedNode).Value);
                    
                    countExpr = new XPathExpr(string.Format("{0} - count({1})", reference.Lower, existing));
                }
                else
                {
                    countExpr = new XPathExpr(string.Format("{0}", reference.Lower));
                }

                // instance generator
                templateElement.XslCallTemplate(namingSupport.SuggestName(reference.ReferencedNode, false, false) + "-IG", new XDocumentXsltExtensions.TemplateParameter("count", countExpr));
            }
        }

        private void GenGroupNodeSingleReference(XElement callingElement, Template callingTemplate, TemplateReference reference)
        {
            RevalidationNodeInfo calledNodeInfo = nodeInfos[reference.ReferencedNode];
            callingElement.XslCallTemplate(calledNodeInfo.ProcessNodeTemplate.Name, XDocumentXsltExtensions.CreateCurrentInstanceParameterCall("current-group()"));    
        }

        private void GenCardinalityReference(XElement templateElement, Template callingTemplate, TemplateReference reference)
        {
            if (!DetectedChangeInstances.IsAddedNode(reference.ReferencedNode))
            {
                if (!reference.DeletionRequired)
                {
                    GenSingleReference(templateElement, callingTemplate, reference, null);
                }
                else
                {
                    GenSingleReference(templateElement, callingTemplate, reference, string.Format("position() leq {0}", reference.Upper));
                }
            }

            if (DetectedChangeInstances.IsAddedNode(reference.ReferencedNode) || reference.CreationRequired)
            {
                XPathExpr countExpr;
                if (!DetectedChangeInstances.IsAddedNode(reference.ReferencedNode))
                {
                    XPathExpr existing = context.GetRelativeXPath(reference.ReferencedNode, !DetectedChangeInstances.IsAddedNode(reference.CallingNode));
                    countExpr = new XPathExpr(string.Format("{0} - count({1})", reference.Lower, existing));
                }
                else
                {
                    countExpr = new XPathExpr(string.Format("{0}", reference.Lower));
                }

                // instance generator
                templateElement.XslCallTemplate(namingSupport.SuggestName(reference.ReferencedNode, false, false) + "-IG", new XDocumentXsltExtensions.TemplateParameter("count", countExpr));
            }
        }

        private void GenSingleReference(XElement templateElement, Template callingTemplate, TemplateReference reference, string condition)
        {
            RevalidationNodeInfo calledNodeInfo = nodeInfos.ContainsKey(reference.ReferencedNode) ? nodeInfos[reference.ReferencedNode] : null;

            // TODO: tests whether attributes/elements templates are required in called template

            if (DetectedChangeInstances.IsAddedNode(reference.ReferencedNode))
            {
                Debug.Assert(calledNodeInfo != null);
                if (callingTemplate.ElementsTemplate || calledNodeInfo.Node is PSMAttribute)
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
                // 18.7. Condition removed, xform changes are handled separately, hopefully, <xsl:attribute will not appear as a refernce anymore (it can be in top level template probably) 
                XPathExpr relativeXPath = context.GetRelativeXPath(reference.ReferencedNode, !DetectedChangeInstances.IsAddedNode(reference.CallingNode)).AppendPredicate(condition);
                //if (reference.ReferencedNode is PSMAttribute && reference.ReferencedNode.ExistsInVersion(OldVersion)
                //    && !((PSMAttribute)reference.ReferencedNode).Element
                //    && ((PSMAttribute)reference.ReferencedNode.GetInVersion(OldVersion)).Element)
                //{
                //    // attribute xform changed from 'e' to 'a' -> must create xml attribute explicitly
                //    XElement xslAttribute = templateElement.XslAttribute(reference.ReferencedNode.Name);
                //    xslAttribute.XslValueOf(relativeXPath);
                //}
                //else
                {                    
                    templateElement.XslApplyTemplates(relativeXPath);
                }
            }
        }

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
    }
}