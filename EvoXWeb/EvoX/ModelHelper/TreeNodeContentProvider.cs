using System;
using System.Text;
using EvoX.Model;
using EvoX.Model.PIM;
using EvoX.Model.PSM;
using EvoX.Web.IO;

namespace EvoX.Web.ModelHelper
{
    public class TreeNodeContentProvider: ModelWorkerWithInheritance<string, StringBuilder>
    {
        const string htmlBreak = "<br />";

        protected override string GetResult(StringBuilder context)
        {
            
            if (context.ToString(context.Length - 6, 6) == htmlBreak)
            {
                return context.ToString(0, context.Length - 6);
            }
            else
            {
                return context.ToString();
            }
        }

        protected override StringBuilder CreateStartingContext()
        {
            return new StringBuilder();
        }

        public override void ProcessPSMSchemaClass(PSMSchemaClass psmSchemaClass, ref StringBuilder context)
        {
            context.AppendFormat("<div id=\"w{0}\">", psmSchemaClass.ID);
            context.AppendFormat(URLHelper.GetHtmlAnchoringSpan(psmSchemaClass));
            context.AppendFormat("<div class=\"classBoundingBox classBoundingBoxPSM pimLessHeader\">");
            context.AppendFormat("<span class=\"componentName\">{0}</span> {1}", psmSchemaClass.Name, htmlBreak);
            base.ProcessPSMSchemaClass(psmSchemaClass, ref context);
            context.AppendFormat("</div>");
            context.AppendFormat("</div>");
        }

        public override void ProcessPSMClass(PSMClass psmClass, ref StringBuilder context)
        {
            if (psmClass.ParentAssociation != null && psmClass.ParentAssociation.IsNamed)
            {
                context.AppendFormat("&nbsp;&nbsp; <span title=\"Displays parent association name\">{0}</span>", psmClass.ParentAssociation.Name);
            }
            if (psmClass.ParentAssociation != null && psmClass.ParentAssociation.HasNondefaultCardinality())
            {
                context.AppendFormat("&nbsp;&nbsp;Cardinality: {0}", psmClass.ParentAssociation.CardinalityString);
            }
            context.AppendFormat(URLHelper.GetHtmlAnchoringSpan(psmClass));
            context.AppendFormat("<div id=\"w{0}\">", psmClass.ID);
            context.AppendFormat("<div class=\"classBoundingBox classBoundingBoxPSM\">");
            if (psmClass.IsStructuralRepresentative)
            {
                context.AppendFormat("<div class=\"classHeader representativeHeader\">");    
            }
            else if (psmClass.Interpretation != null)
            {
                context.AppendFormat("<div class=\"classHeader\">");    
            }
            else
            {
                context.AppendFormat("<div class=\"classHeader pimLessHeader\">");
            }
            if (psmClass.IsStructuralRepresentative)
            {
                context.AppendFormat("<div class=\"representedClassLink\">");
                string representsLink = URLHelper.GetHtmlAnchor(psmClass.RepresentedClass, psmClass.RepresentedClass.Name);
                context.AppendFormat(representsLink);
                context.AppendFormat("</div>");
                //context.AppendFormat("&nbsp;&nbsp;Represents: {0} {1}", representsLink, htmlBreak);
            }
            context.AppendFormat("<span class=\"componentName\">{0}</span> {1}", psmClass.Name, htmlBreak);
            context.AppendFormat("</div>");
            if (psmClass.IsStructuralRepresentative)
            {
                context.AppendFormat("<div class=\"classContent representativeContent\">");
            }
            else if (psmClass.Interpretation != null)
            {
                context.AppendFormat("<div class=\"classContent\">");
            }
            else
            {
                context.AppendFormat("<div class=\"classContent pimLessContent\">");
            }
            base.ProcessPSMClass(psmClass, ref context);
            if (psmClass.PSMAttributes.Count > 0)
            {
                context.AppendFormat("&nbsp;&nbsp;Attributes: {0}", htmlBreak);
                context.AppendFormat("<ul>");
                foreach (PSMAttribute psmAttribute in psmClass.PSMAttributes)
                {
                    ProcessPSMAttribute(psmAttribute, ref context);
                }
                context.AppendFormat("</ul>");
            }
            context.AppendFormat("</div>");
            context.AppendFormat("</div>");
            context.AppendFormat("</div>");
        }

        public override void ProcessPSMAttribute(PSMAttribute psmAttribute, ref StringBuilder context)
        {
            if (psmAttribute.Interpretation != null)
                context.AppendFormat("<li id=\"w{0}\">", psmAttribute.ID);
            else
                context.AppendFormat("<li class=\"pimLessAttribute\" id=\"w{0}\">", psmAttribute.ID);
            context.AppendFormat(URLHelper.GetHtmlAnchoringSpan(psmAttribute));
            context.AppendFormat("<span class=\"componentName\">{0}</span>", psmAttribute.Element ? psmAttribute.Name : "@" + psmAttribute.Name);
            if (psmAttribute.HasNondefaultCardinality())
            {
                context.AppendFormat("&nbsp;&nbsp;Cardinality: {0}", psmAttribute.CardinalityString);    
            }
            if (psmAttribute.AttributeType != null)
            {
                context.AppendFormat("&nbsp;&nbsp;Type: {0}", psmAttribute.AttributeType);
            }
            if (!string.IsNullOrEmpty(psmAttribute.DefaultValue))
            {
                context.AppendFormat("&nbsp;&nbsp;Default value: {0}", psmAttribute.DefaultValue);
            }

            base.ProcessPSMAttribute(psmAttribute, ref context);
            context.AppendFormat("</li>");
        }

        public override void ProcessPSMAssociationMember(PSMAssociationMember psmAssociationMember, ref StringBuilder context)
        {
            base.ProcessPSMAssociationMember(psmAssociationMember, ref context);
        }

        public override void ProcessPSMContentModel(PSMContentModel psmContentModel, ref StringBuilder context)
        {
            if (psmContentModel.ParentAssociation != null && psmContentModel.ParentAssociation.IsNamed)
            {
                context.AppendFormat("&nbsp;&nbsp; <span title=\"Displays parent association name\">{0}</span>", psmContentModel.ParentAssociation.Name);
            }
            if (psmContentModel.ParentAssociation != null && psmContentModel.ParentAssociation.HasNondefaultCardinality())
            {
                context.AppendFormat("&nbsp;&nbsp;Cardinality: {0}", psmContentModel.ParentAssociation.CardinalityString);
            }
            context.AppendFormat(URLHelper.GetHtmlAnchoringSpan(psmContentModel));
            context.AppendFormat("<div id=\"w{0}\">", psmContentModel.ID);
            context.AppendFormat("<div class=\"classBoundingBox classBoundingBoxPSM pimLessHeader\">");
            switch (psmContentModel.Type)
            {
                case PSMContentModelType.Sequence:
                    context.AppendFormat("<span class=\"componentName\">&lt;sequence&gt;</span> {0}", htmlBreak);
                    break;
                case PSMContentModelType.Choice:
                    context.AppendFormat("<span class=\"componentName\">&lt;choice&gt;</span> {0}", htmlBreak);
                    break;
                case PSMContentModelType.Set:
                    context.AppendFormat("<span class=\"componentName\">&lt;set&gt;</span> {0}", htmlBreak);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            base.ProcessPSMContentModel(psmContentModel, ref context);
            context.AppendFormat("</div>");
            context.AppendFormat("</div>");
        }

        public override void ProcessPSMComponent(PSMComponent psmComponent, ref StringBuilder context)
        {
            base.ProcessPSMComponent(psmComponent, ref context);
            if (psmComponent.Interpretation != null)
            {
                string interpretationLink;
                if (psmComponent is PSMAttribute)
                {
                    context.Append(htmlBreak);
                    PIMAttribute attribute = (PIMAttribute) psmComponent.Interpretation;
                    interpretationLink = URLHelper.GetHtmlAnchor(attribute, string.Format("{0}.{1}", attribute.PIMClass.Name, attribute.Name), true);
                }
                else
                {
                    interpretationLink = URLHelper.GetHtmlAnchor(psmComponent.Interpretation, true);
                }
                context.AppendFormat("&nbsp;&nbsp;Interpretation: {0} {1}", interpretationLink, htmlBreak);
            }
        }
    }
}