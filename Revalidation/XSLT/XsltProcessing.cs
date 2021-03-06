#define SAXON_XSLT
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Xsl;
using Exolutio.Model.PSM;
using Saxon.Api;
using Exolutio.SupportingClasses.XML;
using net.sf.saxon.lib;
using Exolutio.Model.PSM.Grammar.XSDTranslation;

namespace Exolutio.Revalidation.XSLT
{
    public static class XsltProcessing
    {
        public static string Transform(string document, string xslt, string tmpDir, PSMSchema psmSchema = null, bool schemaAware = false)
        {
#if SAXON_XSLT
            return TransformSAXON(document, xslt, tmpDir, psmSchema, schemaAware);
#else
            return TransformNET(document, xslt, tmpDir);
#endif
        }

        public static string TransformSAXON(string documentPath, string xsltPath, bool schemaAware)
        {
            Processor processor = new Processor();
            XsltCompiler xsltCompiler = processor.NewXsltCompiler();
            xsltCompiler.Processor.SetProperty(FeatureKeys.GENERATE_BYTE_CODE, "false");
            XsltExecutable xsltExecutable = xsltCompiler.Compile(new Uri("file://" + xsltPath));
            XsltTransformer xsltTransformer = xsltExecutable.Load();
            if (schemaAware
                //!string.IsNullOrEmpty(xsdPath)
                )
            {
                xsltTransformer.SchemaValidationMode = SchemaValidationMode.Strict;
            }

            using (FileStream fileStream = File.OpenRead(documentPath))
            {
                xsltTransformer.SetInputStream(fileStream, new Uri(@"file://" + documentPath));
                XdmDestination destination = new XdmDestination();
                xsltTransformer.Run(destination);
                StringBuilder outputBuilder = new StringBuilder();
                outputBuilder.Append(destination.XdmNode.OuterXml);
                return outputBuilder.ToString();
            }
        }

        private static string TransformSAXON(string document, string xslt, string tmpDir, PSMSchema psmSchema = null, bool schemaAware = false)
        {
            Processor processor = new Processor();
            XsltCompiler xsltCompiler = processor.NewXsltCompiler();
            //xsltCompiler.SchemaAware = true; 
            xsltCompiler.Processor.SetProperty(FeatureKeys.GENERATE_BYTE_CODE, "false");
            
            if (!Directory.Exists(tmpDir))
            {
                tmpDir = Path.GetTempPath();
            }

            using (StringReader sr = new StringReader(xslt))
            {
                XsltExecutable xsltExecutable = xsltCompiler.Compile(sr);
                
                string text;

                if (!schemaAware)
                {
                    int si = document.IndexOf("xmlns:xsi=\"");
                    int ei = document.IndexOf("\"", si + "xmlns:xsi=\"".Length) + 1;
                    text = si != -1 ? document.Remove(si, ei - si) : document;
                    si = text.IndexOf("xmlns=\"");
                    ei = text.IndexOf("\"", si + "xmlns=\"".Length) + 1;
                    string xmlns = si != -1 ? text.Substring(si, ei - si) : string.Empty;
                    text = si != -1 ? text.Remove(si, ei - si) : text;
                }
                else
                {
                    text = document;
                }

                string tmpDoc = tmpDir + "tmp.xml";

                if (schemaAware)
                {
                    XsdSchemaGenerator xsdGen = new XsdSchemaGenerator();
                    xsdGen.Initialize(psmSchema);
                    xsdGen.GenerateXSDStructure();
                    XDocument schema = xsdGen.GetXsd();
                    string schemaLoc = tmpDir + "LastSchema.xsd";
                    schema.Save(schemaLoc);
                }

                File.WriteAllText(tmpDoc, text.Replace("utf-16", "utf-8"), Encoding.UTF8);


                StringBuilder outputBuilder = new StringBuilder();
                //XmlWriterSettings outputWriterSettings = new XmlWriterSettings { Indent = true, CheckCharacters = false, NewLineOnAttributes = true };
                //XmlWriter outputWriter = XmlWriter.Create(outputBuilder, outputWriterSettings);
                //Debug.Assert(outputWriter != null);
                XsltArgumentList xsltArgumentList = new XsltArgumentList();                
                FileStream fs = null;
                try
                {
                    XsltTransformer xsltTransformer = xsltExecutable.Load();
                    if (schemaAware)
                        xsltTransformer.SchemaValidationMode = SchemaValidationMode.Strict;
                    
                    fs = new FileStream(tmpDoc, FileMode.Open);
                    xsltTransformer.SetInputStream(fs, new Uri(@"file://" + tmpDoc));
                    XdmDestination destination = new XdmDestination();
                    xsltTransformer.Run(destination);

                    outputBuilder.Append(destination.XdmNode.OuterXml);
                    //outputWriter.Flush();
                    int pos1 = outputBuilder.ToString().IndexOf(">");
                    int pos2 = outputBuilder.ToString().IndexOf("/>");
                    int pos;
                    if (pos1 == -1)
                        pos = pos2;
                    else if (pos2 == -1)
                        pos = pos1;
                    else
                        pos = Math.Min(pos1, pos2);
                    //outputBuilder.Insert(pos,
                    //                     Environment.NewLine + " xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" " + xmlns);

                    StringReader outputReader = new StringReader(outputBuilder.ToString());
                    XDocument d = XDocument.Load(outputReader);
                    outputReader.Close();
                    return d.PrettyPrintXML();
                }
                finally
                {
                    if (fs != null)
                        fs.Close();
                }
            }
        }

        public static string TransformNET(string document, string xslt, string tmpDir)
        {
            XslCompiledTransform t = new XslCompiledTransform(true);
            //StringReader xsltStringReader = new StringReader(tbXslt.Text);

            // XmlReader stylesheetReader = XmlReader.Create(xsltStringReader);
            XsltSettings settings = new XsltSettings();
            const XmlResolver stylesheetResolver = null;
            //t.Load(stylesheetReader, settings, stylesheetResolver);
            if (!Directory.Exists(tmpDir))
            {
                tmpDir = Path.GetTempPath();
            }

            string tmpFile = tmpDir + "tmp.xslt";
            File.WriteAllText(tmpFile, xslt);

            t.Load(@"file://" + tmpFile, settings, stylesheetResolver);

            // HACK: EVOLUTION strip namespace and schema instance
            int si = document.IndexOf("xmlns:xsi=\"");
            int ei = document.IndexOf("\"", si + "xmlns:xsi=\"".Length) + 1;
            string text = document.Remove(si, ei - si);
            si = text.IndexOf("xmlns=\"");
            ei = text.IndexOf("\"", si + "xmlns=\"".Length) + 1;
            text = text.Remove(si, ei - si);
            StringReader documentStringReader = new StringReader(text);
            XmlReader documentReader = XmlReader.Create(documentStringReader);

            StringBuilder outputBuilder = new StringBuilder();
            XmlWriterSettings outputWriterSettings = new XmlWriterSettings
                                                         {
                                                             Indent = true,
                                                             CheckCharacters = false,
                                                             NewLineOnAttributes = true
                                                         };
            XmlWriter outputWriter = XmlWriter.Create(outputBuilder, outputWriterSettings);
            Debug.Assert(outputWriter != null);
            XsltArgumentList xsltArgumentList = new XsltArgumentList();

            try
            {
                DoTransform(t, outputWriter, documentReader, xsltArgumentList);
                outputWriter.Flush();
                outputBuilder.Insert(outputBuilder.ToString().IndexOf(">", outputBuilder.ToString().IndexOf("?>") + 2),
                                     Environment.NewLine +
                                     "xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://kocour.ms.mff.cuni.cz/xcase/company/\"");
                 
                StringReader outputReader = new StringReader(outputBuilder.ToString());
                XDocument d = XDocument.Load(outputReader);
                outputReader.Close();
                return d.PrettyPrintXML();
            }
            finally
            {
                outputWriter.Close();

                //stylesheetReader.Close();
                //xsltStringReader.Close();
                //xsltStringReader.Dispose();

                documentReader.Close();
                documentStringReader.Close();
                documentStringReader.Dispose();
            }
        }

        private static void DoTransform(XslCompiledTransform t, XmlWriter outputWriter, XmlReader documentReader, XsltArgumentList xsltArgumentList)
        {
            t.Transform(documentReader, xsltArgumentList, outputWriter);
        }
    }
}