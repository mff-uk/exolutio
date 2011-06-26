using System.Collections.Generic;
using Exolutio.Model;
using Exolutio.Model.PSM;

namespace Exolutio.Revalidation.XSLT
{
    public struct RevalidationNodeInfo
    {
        public PSMComponent Node { get; set; }

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