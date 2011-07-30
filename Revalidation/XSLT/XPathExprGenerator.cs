using System.Collections.Generic;
using System.Xml.Linq;
using Exolutio.Model;
using Exolutio.Model.Versioning;
using Exolutio.Model.PSM;

namespace Exolutio.Revalidation.XSLT
{
    public class XPathExprGenerator
    {
        public static XPathExpr GetGroupMembers(GeneratorContext context, PSMComponent component, bool useCurrentInstanceVariable)
        {
            List<PSMComponent> groupMembers = new List<PSMComponent>();
            AddGroupMembersRecursive(component, ref groupMembers);

            List<XPathExpr> result = new List<XPathExpr>();

            foreach (PSMComponent psmComponent in groupMembers)
            {
                if (!psmComponent.ExistsInVersion(context.OldVersion))
                {
                    continue;
                }
                result.Add(context.GetRelativeXPath(psmComponent, useCurrentInstanceVariable));
            }

            return XPathExpr.ConcatWithOrOperator(result);
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
                    PSMAssociationMember associationMember = (PSMAssociationMember) component;
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

        public static XAttribute GetGroupDistinguisher(GeneratorContext context, PSMComponent component)
        {
            List<PSMComponent> groupMembers = new List<PSMComponent>();
            AddGroupMembersRecursive(component, ref groupMembers);
            List<PSMComponent> oldMembers = new List<PSMComponent>();
            AddGroupMembersRecursive(component.GetInVersion(context.OldVersion), ref oldMembers);

            List<XPathExpr> result = new List<XPathExpr>();

            foreach (PSMComponent psmComponent in oldMembers)
            {
                if (!psmComponent.ExistsInVersion(context.NewVersion))
                {
                    continue;
                }
                result.Add(context.GetRelativeXPath(psmComponent, false));
            }


            return new XAttribute("group-starting-with", result[0].ToString());
        }
    }
}