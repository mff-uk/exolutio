using System;
using System.Collections.Generic;
using Exolutio.Model;
using Exolutio.Model.PSM;
using System.Linq;

namespace Exolutio.Revalidation.XSLT
{
    public class TemplateNamingSupport
    {
        public PSMSchema PSMSchema { get; set; }

        public void Initialize(PSMSchema psmSchema)
        {
            this.PSMSchema = psmSchema;
        }

        /// <summary>
        /// <para>
        /// Returns a name for a top level template for a given node in a PSM tree. 
        /// The name follows the tree structure, dashes are used as tree separators, 
        /// names of the nodes are used in each step (names of the associatiosn are ignored). 
        /// </para>
        /// <para>
        /// Names for nodes in the schema class tree start with "ROOT", other names start with "TOP"
        /// </para>
        /// </summary>
        /// <returns></returns>
        public string SuggestName(PSMComponent node, bool elementsTemplate, bool attributesTemplate)
        {
            string result;
            if (PSMSchema.TopClasses.Contains(node))
            {
                result = "TOP-" + node.Name; 
            }
            else if (PSMSchema.Roots.Contains(node))
            {
                result = "ROOT" + node.Name;
            }
            else
            {
                if (node is PSMContentModel)
                {
                    PSMContentModel cm = (PSMContentModel) node;
                    string distinguisher = String.Empty;
                    if (!cm.ParentAssociation.IsNamed)
                    {
                        List<PSMContentModel> withSiblings = ModelIterator.GetPSMChildren(cm.ParentAssociation.Parent).OfType<PSMContentModel>().ToList();
                        if (withSiblings.Count(s => s.Type == cm.Type) > 1)
                        {
                            distinguisher = (withSiblings.Where(c => c.Type == cm.Type).ToList().IndexOf(cm) + 1).ToString();
                        }
                    }
                    else
                    {
                        distinguisher = cm.ParentAssociation.Name;
                    }
                    switch (cm.Type)
                    {
                        case PSMContentModelType.Sequence:
                            result = SuggestName(cm.ParentAssociation.Parent, false, false) + "-" + "SEQ" + (!String.IsNullOrEmpty(distinguisher) ? distinguisher : String.Empty);
                            break;
                        case PSMContentModelType.Choice:
                            result = SuggestName(cm.ParentAssociation.Parent, false, false) + "-" + "CH" + (!String.IsNullOrEmpty(distinguisher) ? distinguisher : String.Empty);
                            break;
                        case PSMContentModelType.Set:
                            result = SuggestName(cm.ParentAssociation.Parent, false, false) + "-" + "SET" + (!String.IsNullOrEmpty(distinguisher) ? distinguisher : String.Empty);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
                else if (node is PSMClass)
                {
                    PSMClass psmClass = (PSMClass)node;
                    result = SuggestName(psmClass.ParentAssociation.Parent, false, false) + "-" + node.Name;
                }
                else if (node is PSMAttribute)
                {
                    PSMAttribute psmAttribute = (PSMAttribute)node;
                    result = SuggestName(psmAttribute.PSMClass, false, false) + "-" + psmAttribute.Name;
                }
                else
                {
                    throw new ArgumentOutOfRangeException();

                }
            }

            if (elementsTemplate)
            {
                return result + "-ELM";
            }
            if (attributesTemplate)
            {
                return result + "-ATT";
            }
            return result;
        }

        public string SuggestNameForInstanceGenerator(PSMComponent node, bool wrappingTemplate = false, bool attributesTemplate = false, bool elementsTemplate = false)
        {
            if (wrappingTemplate || node is PSMAttribute)
            {
                return SuggestName(node, false, false) + "-IG";
            }
            if (attributesTemplate)
            {
                return SuggestName(node, false, true) + "-IG";
            }
            if (elementsTemplate)
            {
                return SuggestName(node, true, false) + "-IG";
            }
            throw new InvalidOperationException();
        }

        public string SuggestNameForInstanceGenerator(PSMComponent node, Template callingTemplate, TemplateReference reference)
        {
            string result;
            if (callingTemplate.AttributesTemplate)
            {
                result = SuggestNameForInstanceGenerator(node, attributesTemplate: true);
            }
            else if (callingTemplate.ElementsTemplate)
            {
                if (reference.ReferencesGroupNode)
                {
                    result = SuggestNameForInstanceGenerator(node, elementsTemplate: true);
                }
                else
                {
                    result = SuggestNameForInstanceGenerator(node, wrappingTemplate: true);
                }
            }
            else
            {
                throw new InvalidOperationException();
            }
            return result;
        }

        public string GetWrapElementName(PSMComponent psmComponent)
        {
            if (psmComponent is PSMAttribute)
	            return psmComponent.Name;

			List<string> candidateNames = new List<string>();
	        var am = (PSMAssociationMember) psmComponent;
			if (am.ParentAssociation != null && am.ParentAssociation.IsNamed)
				candidateNames.Add(am.ParentAssociation.Name);
	        var c = (PSMClass) psmComponent; 
			if (c != null)
			{
				foreach (PSMAssociation nta in c.GetIncomingNonTreeAssociations())
				{
					if (nta.IsNamed)
						SupportingClasses.CollectionsExtensions.AddIfNotContained(candidateNames, nta.Name);
				}
			}

			// HACK neumim resit kdyz bude candidate names vic, to by se muselo vse prestrukturovat, protoze 
			//	 by jmeno wrapping nodu muselo byt nejak parametrem wrapovaciho volani...
			//   vic candidate names nastane pokud mi do tridy vede vic asociaci s ruznymi jmeny
			if (candidateNames.Count > 0)
				return candidateNames.First();
			else 
				throw new NotImplementedException();

        }
    }
}