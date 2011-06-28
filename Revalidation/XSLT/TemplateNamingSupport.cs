using System;
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
        /// <param name="node"></param>
        /// <returns></returns>
        public string SuggestName(PSMAssociationMember node)
        {
            if (PSMSchema.TopClasses.Contains(node))
            {
                return "TOP-" + node.Name; 
            }
            else if (PSMSchema.Roots.Contains(node))
            {
                return "ROOT" + node.Name;
            }
            else
            {
                if (node is PSMContentModel)
                {
                    PSMContentModel cm = (PSMContentModel) node;
                    switch (cm.Type)
                    {
                        case PSMContentModelType.Sequence:
                            return SuggestName(node.ParentAssociation.Parent) + "-" + "SEQ";
                        case PSMContentModelType.Choice:
                            return SuggestName(node.ParentAssociation.Parent) + "-" + "CH";
                        case PSMContentModelType.Set:
                            return SuggestName(node.ParentAssociation.Parent) + "-" + "SET";
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
                else
                {
                    return SuggestName(node.ParentAssociation.Parent) + "-" + node.Name;
                }
            }
        }
    }
}