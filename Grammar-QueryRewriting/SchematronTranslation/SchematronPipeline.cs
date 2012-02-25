using System.Diagnostics;
using System.IO;

namespace Exolutio.Model.PSM.Grammar.SchematronTranslation
{
    public class SchematronPipelineWithSaxonTransform
    {
        public string SaxonTransformExecutablePath { get; set; }

        public string IsoSchematronTemplatesPath { get; set; }

        public void Process(string inputSchematronSchemaPath, string outputDirectory, string documentPath = null)
        {
            SchematronPipeline pipeline = new SchematronPipeline();
            pipeline.XSLTProcessorCommand = SaxonTransformExecutablePath;
            pipeline.XSLTProcessorCommandArgs = null;
            pipeline.IsoSchematronTemplatesPath = IsoSchematronTemplatesPath;
            pipeline.TransformationArgsFormat = "-s:{0} -o:{1} -xsl:{2}{3}";
            pipeline.Process(inputSchematronSchemaPath, outputDirectory, documentPath);
        }
    }

    public class SchematronPipeline
    {
        public string XSLTProcessorCommand { get; set; }

        public string XSLTProcessorCommandArgs { get; set; }

        public string TransformationArgsFormat { get; set; }

        public string IsoSchematronTemplatesPath { get; set; }

        public void Process(string inputSchematronSchemaPath, string outputDirectory, string documentPath = null)
        {
            //XSLT -input=xxx.sch  -output=xxx1.sch  -stylesheet=iso_dsdl_include.xsl
            //XSLT -input=xxx1.sch  -output=xxx2.sch  -stylesheet=iso_abstract_expand.xsl
            //XSLT -input=xxx2.sch  -output=xxx.xsl  -stylesheet=iso_svrl.xsl
            //XSLT -input=document.xml  -output=xxx-document.svrl  -stylesheet=xxx.xsl  

            string iso_dsdl_include = Path.Combine(IsoSchematronTemplatesPath, "iso_dsdl_include.xsl");
            string iso_abstract_expand = Path.Combine(IsoSchematronTemplatesPath, "iso_abstract_expand.xsl");
            string iso_svrl = Path.Combine(IsoSchematronTemplatesPath, "iso_svrl.xsl");
            

            string inputWithoutExtension = Path.GetFileNameWithoutExtension(inputSchematronSchemaPath);
            Debug.Assert(inputWithoutExtension != null);

            string output1sch = Path.Combine(outputDirectory, inputWithoutExtension + "1.sch");
            string output2sch = Path.Combine(outputDirectory, inputWithoutExtension + "2.sch");
            string output3xsl = Path.Combine(outputDirectory, inputWithoutExtension + "3.xsl");
            // validation result
            string output4svrl = Path.Combine(outputDirectory, inputWithoutExtension + "4.svrl");

            string commandLine1 = string.Format(TransformationArgsFormat, inputSchematronSchemaPath,
                                                output1sch, iso_dsdl_include, null);

            string commandLine2 = string.Format(TransformationArgsFormat, output1sch,
                                                output2sch, iso_abstract_expand, null);

            string commandLine3 = string.Format(TransformationArgsFormat, output2sch, output3xsl, iso_svrl, "allow-foreign=true");
            
            string commandLine4 = string.Format(TransformationArgsFormat, documentPath,
                                                    output4svrl, output3xsl, null);

            string s = string.Empty; 
            foreach (string commandLine in new string[] { commandLine1, commandLine2, commandLine3, commandLine4 })
            {
                
                if (commandLine != commandLine4 || !string.IsNullOrEmpty(documentPath))
                {
                    s += string.Format("{0} {1} {2} {3}", XSLTProcessorCommand, XSLTProcessorCommandArgs, commandLine, System.Environment.NewLine);
                    Process p = new Process();
                    p.StartInfo.FileName = XSLTProcessorCommand;
                    p.StartInfo.Arguments = string.Format("{0} {1}", XSLTProcessorCommandArgs, commandLine);
                    p.StartInfo.UseShellExecute = false;
                    p.StartInfo.RedirectStandardOutput = true;
                    p.Start();
                    string output = p.StandardOutput.ReadToEnd();
                    p.WaitForExit();
                }
            }
            File.WriteAllText(Path.Combine(outputDirectory, inputWithoutExtension + ".bat"), s);
        }
    }
}