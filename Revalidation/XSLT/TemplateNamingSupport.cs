using System;
using System.Collections.Generic;
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
                    string distinguisher = string.Empty;
                    if (!cm.ParentAssociation.IsNamed)
                    {
                        List<PSMContentModel> withSiblings = Model.ModelIterator.GetPSMChildren(cm.ParentAssociation.Parent).OfType<PSMContentModel>().ToList();
                        if (withSiblings.Count(s => s.Type == cm.Type && !s.IsNamed) > 1)
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
                            result = SuggestName(cm.ParentAssociation.Parent, false, false) + "-" + "SEQ" + (!String.IsNullOrEmpty(distinguisher) ? distinguisher : string.Empty);
                            break;
                        case PSMContentModelType.Choice:
                            result = SuggestName(cm.ParentAssociation.Parent, false, false) + "-" + "CH" + (!String.IsNullOrEmpty(distinguisher) ? distinguisher : string.Empty);
                            break;
                        case PSMContentModelType.Set:
                            result = SuggestName(cm.ParentAssociation.Parent, false, false) + "-" + "SET" + (!String.IsNullOrEmpty(distinguisher) ? distinguisher : string.Empty);
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
    }
}