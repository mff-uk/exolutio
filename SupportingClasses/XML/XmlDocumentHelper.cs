using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Microsoft.XmlDiffPatch;

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

        public static bool DocumentCompare(string document1, 
            string document2, 
            XmlDiffOptions diffOptions = XmlDiffOptions.IgnoreComments)
        {
            XmlDiff xmlDiff = new XmlDiff(diffOptions);
            bool bIdentical = xmlDiff.Compare(document1, document2, false);
            return bIdentical;
        }

        public static bool DocumentCompare(string document1,
            string document2, out string diff,
            XmlDiffOptions diffOptions = XmlDiffOptions.IgnoreComments)
        {
            XmlDiff xmlDiff = new XmlDiff(diffOptions);
            StringBuilder sb = new StringBuilder();
            bool bIdentical;
            using (StringWriter sw = new StringWriter(sb))
            {
                using (XmlTextWriter tw = new XmlTextWriter(sw))
                {
                    bIdentical = xmlDiff.Compare(document1, document2, false, tw);
                }
            }

            diff = sb.ToString();
            return bIdentical;
        }

        public static void SaveInUtf8(this XmlDocument document, string fileName)
        {
            XmlWriterSettings set = new XmlWriterSettings();
            set.Encoding = Encoding.UTF8;
            using (FileStream fs = new FileStream(fileName, FileMode.Create, FileAccess.Write))
            {
                using (XmlWriter w = XmlWriter.Create(fs, set))
                {
                    document.Save(w);
                }
            }
        }

        public static void SaveInUtf8(string contents, string fileName)
        {
            XmlDocument d = new XmlDocument();
            d.LoadXml(contents);
            d.SaveInUtf8(fileName);
        }

        public static void SaveInUtf8(this XDocument document, string fileName)
        {
            XmlWriterSettings set = new XmlWriterSettings();
            set.Encoding = Encoding.UTF8;
            using (FileStream fs = new FileStream(fileName, FileMode.Create, FileAccess.Write))
            {
                using (XmlWriter w = XmlWriter.Create(fs, set))
                {
                    document.Save(w);
                }
            }
        }
    }
}