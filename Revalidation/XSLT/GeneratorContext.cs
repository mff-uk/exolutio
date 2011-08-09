using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;
using Exolutio.Model;
using Exolutio.Model.PSM;
using Exolutio.Model.Versioning;
using Version = Exolutio.Model.Versioning.Version;

namespace Exolutio.Revalidation.XSLT
{
    public class GeneratorContext
    {
        public bool CreatingMode { get; set; }

        public PSMComponent CurrentNode { get; set; }

        public Version OldVersion { get; set; }

        public Version NewVersion { get; set; }

        public XPathExpr GetRelativeXPath(PSMComponent node, bool useCurrentInstanceVariable, string currentInstanceVariableName = "ci")
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
                    result = new XPathExpr(string.Format("${3}[name() = '{0}']{1}{2}", nextStep, string.IsNullOrEmpty(afterNextStep) ? string.Empty : "/", afterNextStep, currentInstanceVariableName));
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

        public XPathExpr GetGroupMembers(PSMComponent component, bool useCurrentInstanceVariable)
        {
            List<PSMComponent> groupMembers = new List<PSMComponent>();
            AddGroupMembersRecursive(component, ref groupMembers);

            List<XPathExpr> result = new List<XPathExpr>();

            foreach (PSMComponent psmComponent in groupMembers)
            {
                if (!psmComponent.ExistsInVersion(this.OldVersion))
                {
                    continue;
                }
                result.Add(this.GetRelativeXPath(psmComponent, useCurrentInstanceVariable));
            }

            if (result.Count == 0)
            {
                return new XPathExpr("()");
            }
            else
            {
                return XPathExpr.ConcatWithOrOperator(result);
            }
        }

        private static void AddGroupMembersRecursive(PSMComponent referencedNode, ref List<PSMComponent> groupMembers)
        {
            foreach (PSMComponent component in ModelIterator.GetPSMChildren(referencedNode, true))
            {
                if (component is PSMAttribute)
                {
                    groupMembers.Add(component);
                }
                if (component is PSMAssociationMember)
                {
                    PSMAssociationMember associationMember = (PSMAssociationMember)component;
                    if (associationMember.ParentAssociation.IsNamed)
                    {
                        groupMembers.Add(associationMember);
                    }
                    else
                    {
                        AddGroupMembersRecursive(associationMember, ref groupMembers);
                    }
                }
            }
        }

        public XPathExpr GetGroupDistinguisher(PSMComponent component)
        {
            List<PSMComponent> groupMembers = new List<PSMComponent>();
            AddGroupMembersRecursive(component, ref groupMembers);
            List<PSMComponent> oldMembers = new List<PSMComponent>();
            AddGroupMembersRecursive(component.GetInVersion(this.OldVersion), ref oldMembers);

            List<XPathExpr> result = new List<XPathExpr>();

            foreach (PSMComponent psmComponent in oldMembers)
            {
                if (!psmComponent.ExistsInVersion(this.NewVersion))
                {
                    continue;
                }

                PSMComponent comp = psmComponent.GetFirstAncestorOrSelfExistingInVersion(this.OldVersion).GetInVersion(this.OldVersion);
                int lastIndexOf = comp.XPath.LastIndexOf("/");
                string expression = lastIndexOf > -1 ? comp.XPath.Substring(lastIndexOf + 1) : comp.XPath;
                result.Add(new XPathExpr(expression));
            }
            
            return result[0];
        }

        public XPathExpr GetGroupDistinguisher(IEnumerable<PSMComponent> expandedReference)
        {
            IEnumerable<XPathExpr> result = from component in expandedReference select GetGroupDistinguisher(component);
            return XPathExpr.ConcatWithOrOperator(result);
        }
    }
}