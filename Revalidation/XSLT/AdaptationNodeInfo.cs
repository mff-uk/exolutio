using System;
using System.Linq;
using System.Collections.Generic;
using Exolutio.Model;
using Exolutio.Model.PSM;
using Exolutio.Model.OCL.AST;
using Exolutio.SupportingClasses;

namespace Exolutio.Revalidation.XSLT
{
    public class AdaptationNodeInfo
    {
        public AdaptationNodeInfo(PSMComponent psmComponent)
        {
            Node = psmComponent;

            bool attributeRequired;
            bool elementRequired;
            bool wrapTemplateRequired;
            DetermineRequiredTemplates(Node, out attributeRequired, out elementRequired, out wrapTemplateRequired);
            AttributeTemplateRequired = attributeRequired;
            ElementTemplateRequired = elementRequired;
        }

        public PSMComponent Node { get; set; }

        public bool AttributeTemplateRequired { get; private set; }

        public bool ElementTemplateRequired { get; private set; }

        public Template ProcessAttributesTemplate { get; set; }

        //public Template ProcessAttributesTemplateBodyX { get; set; }

        public Template ProcessNodeTemplate { get; set; }

        public Template ProcessElementsTemplate { get; set; }

        //public Template CreateAttributesTemplate { get; set; }

        public Template CreateElementsTemplate { get; set; }

        public OclExpression InitializationExpression { get; set; }

        public static void DetermineRequiredTemplates(PSMComponent node, out bool attributeRequired, out bool elementRequired, out bool wrapTemplateRequired)
        {
            wrapTemplateRequired = node.DownCastSatisfies<PSMClass>(c => c.ParentAssociation != null && c.ParentAssociation.IsNamed);

            /*
             * Attribute template is required
             * for class: 
             *  1) when it has attributes 
             *  2) when it has child class connected via association 
             *     not having name and the child class requires attributes 
             *  3) when an attribute was removed from the class, or attributes 
             *     xform has changed 
             * for content model: 
             *  2)
             *  3)
             */

            bool _aReq = false;
            bool _eReq = false;

            if (node is PSMClass)
            {
                PSMClass psmClass = (PSMClass)node;

                // examine represented class
                if (psmClass.IsStructuralRepresentative)
                {
                    bool repA, repE;
                    DetermineRequiredTemplates(psmClass.RepresentedClass, out repA, out repE, out wrapTemplateRequired);
                    if (repA)
                        _aReq = true;
                    if (repE)
                        _aReq = true;
                }

                if (psmClass.PSMAttributes.Any(a => !a.Element))
                {
                    _aReq = true;
                }

                if (psmClass.PSMAttributes.Any(a => a.Element))
                {
                    _eReq = true;
                }
            }

            if (!_aReq)
            {
                if (node is PSMAttribute)
                {
                    if (!((PSMAttribute)node).Element)
                        _aReq = true;
                }
                else
                {
                    bool dummy;
                    foreach (PSMAssociation psmAssociation in (((PSMAssociationMember)node).ChildPSMAssociations).Where(a => !a.IsNamed))
                    {
                        DetermineRequiredTemplates(psmAssociation.Child, out _aReq, out dummy, out wrapTemplateRequired);
                        if (_aReq)
                        {
                            break;
                        }
                    }
                }
            }

            if (!_eReq)
            {
                if (node is PSMAttribute)
                {
                    if (((PSMAttribute)node).Element)
                        _eReq = true;
                }
                else
                {
                    bool dummy;
                    foreach (PSMAssociation psmAssociation in (((PSMAssociationMember)node).ChildPSMAssociations))
                    {
                        if (psmAssociation.IsNamed)
                        {
                            _eReq = true;
                            break;
                        }
                        DetermineRequiredTemplates(psmAssociation.Child, out dummy, out _eReq, out wrapTemplateRequired);
                        if (_eReq)
                        {
                            break;
                        }
                    }
                }
            }

            attributeRequired = _aReq;
            elementRequired = _eReq;
        }
    }

    public class Template
    {
        public PSMComponent Node { get; set; }

        private readonly List<TemplateReference> references = new List<TemplateReference>();

        public string Name { get; set; }

        public string WrapElementName { get; set; }

        public IList<XPathExpr> matchList = new ExolutioList<XPathExpr>();

        public IList<XPathExpr> MatchList { get { return matchList; } }

        public XPathExpr Match { get { return XPathExpr.ConcatWithPipeOperator(MatchList); } }

        public bool IsNamed { get { return !string.IsNullOrEmpty(Name); } }

        public bool ElementsTemplate { get; set; }

        public bool AttributesTemplate { get; set; }

        public bool GroupNodeTemplate { get; set; }

        public bool ConstructorFunction { get; set; }

        public bool HasCurrentInstanceParameter { get; set; }

        /// <summary>
        /// Empty for <see cref="Node"/>s of type PSMAttribute.
        /// </summary>
        public List<TemplateReference> References { get { return references; } }
    }

    public class TemplateReference : IHasCardinality
    {
        public PSMComponent CallingNode { set; get; }

        public PSMComponent ReferencedNode { set; get; }

        public PSMAssociation ReferenceAssociation { get; set; }

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

        public bool CreationRequired { get; set; }

        public bool DeletionRequired { get; set; }

        public bool ReferencesRedNode { get; set; }

        public bool ReferencesAddedNode { get; set; }

        public bool ReferencesGroupNode { get; set; }

        public bool ReferencesInlinedNode { get { return ReferencesGroupNode || ReferencesContentModel; } }

        public bool ReferencesContentModel
        {
            get { return (ReferencedNode is PSMContentModel); }
        }

        public OclExpression InitializationExpression { get; set; }

        public bool HasExplicitInitialization { get { return InitializationExpression != null; } }

        public string CardinalityString
        {
            get { return this.GetCardinalityString(); }
        }

        public bool WithElementName { get { return !string.IsNullOrEmpty(ElementName); } }

        public string ElementName { get; set; }
    }
}