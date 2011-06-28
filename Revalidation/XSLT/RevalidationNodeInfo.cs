using System;
using System.Collections.Generic;
using Exolutio.Model;
using Exolutio.Model.PSM;

namespace Exolutio.Revalidation.XSLT
{
    public class RevalidationNodeInfo
    {
        public RevalidationNodeInfo(PSMComponent psmComponent)
        {
            Node = psmComponent;

            DetermineRequiredTemplates();
        }

        private void DetermineRequiredTemplates()
        {
            /*
             * Attribute template is required
             * for class: 
             *  1) when it has attributes 
             *  2) when it has child class connected via association 
             *    not having and the child class requires attributes 
             * for content model: 
             *  2)
             */
        }

        public PSMComponent Node { get; set; }

        public bool AttributeTemplateRequired { get; private set; }
        
        public bool ElementTemplateRequired { get; private set; }

        public Template ProcessAttributesTemplate { get; set; }

        public Template ProcessElementsTemplate { get; set; }

        public Template CreateAttributesTemplate { get; set; }

        public Template CreateElementsTemplate { get; set; }
    }

    public class Template
    {
        public PSMComponent Node { get; set; }
        
        private readonly List<TemplateReference> references = new List<TemplateReference>();
        
        public string Name { get; set; }

        public IEnumerable<XPathExpr> Match { get; set; }

        public void SetMatch(XPathExpr match)
        {
            Match = new XPathExpr[] { match };
        }

        /// <summary>
        /// Empty for <see cref="Node"/>s of type PSMAttribute.
        /// </summary>
        public List<TemplateReference> References { get { return references; } }
    }

    public class TemplateReference: IHasCardinality
    {
        public bool Single { get { return upper <= 1; } }
        
        private uint lower = 1;

        private UnlimitedInt upper = 1;
        
        public uint Lower
        {
            get { return lower; }
            set { lower = value; }
        }

        public UnlimitedInt Upper
        {
            get { return upper; }
            set { upper = value; }
        }

        public string CardinalityString
        {
            get { return this.GetCardinalityString(); }
        }
    }
}