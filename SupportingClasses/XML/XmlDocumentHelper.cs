using System.Diagnostics;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace Exolutio.SupportingClasses.XML
{
    public static class XmlDocumentHelper
    {
        #if SILVERLIGHT
        #else 
        public static string DocumentToString(XmlDocument document)
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

        public static string PrettyPrintXML(this XDocument document)
        {
            StringBuilder _tmp = new StringBuilder();
            XmlWriter writer = XmlWriter.Create(_tmp, new XmlWriterSettings { Indent = true, CheckCharacters = false, NewLineOnAttributes = false });
            Debug.Assert(writer != null);
            document.Save(writer);
            writer.Flush();
            writer.Close();
            _tmp.Replace("utf-16", "utf-8");
            return _tmp.ToString();
        }

    }
}