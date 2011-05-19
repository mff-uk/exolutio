using System.Text;
using System.Xml;

namespace EvoX.SupportingClasses.XML
{
    public class XmlDocumentHelper
    {
        #if SILVERLIGHT
        #else 
        public string DocumentToString(XmlDocument document)
        {
            StringBuilder sb = new StringBuilder();
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.IndentChars = "  ";
            settings.NewLineChars = "\r\n";
            settings.NewLineHandling = NewLineHandling.Replace;
            XmlWriter writer = XmlWriter.Create(sb, settings);
            document.Save(writer);
            writer.Close();
            return sb.ToString();
        }
        #endif

    }
}