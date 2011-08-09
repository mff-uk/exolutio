using System.Linq;
using System.Xml.Linq;

namespace Exolutio.SupportingClasses.XML
{
    public static class XDocumentHelper
    {
        public static void RemoveComments(this XDocument xdocument)
        {
            foreach (XComment descendantNode in xdocument.DescendantNodes().OfType<XComment>().ToList())
            {
                descendantNode.Remove();
            }
        }
    }
}