using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Exolutio.Model;
using Exolutio.Model.PSM;
using Exolutio.Model.Serialization;
using Exolutio.Revalidation;
using Exolutio.Revalidation.XSLT;
using Exolutio.SupportingClasses.XML;
using Microsoft.XmlDiffPatch;
using NUnit.Framework;

namespace Exolutio.Tests.OCLAdaptation
{
    [TestFixture]
    public class OCLAdaptation
    {
        private static string TEST_BASE_DIR
        {
            get
            {
                if (string.IsNullOrEmpty(ConfigurationManager.AppSettings["ExolutioAdaptationTestRootDirectory"]))
                    throw new ConfigurationErrorsException("Key ExolutioAdaptationTestRootDirectory not found in the configuration file");
                return ConfigurationManager.AppSettings["ExolutioAdaptationTestRootDirectory"];
            }
        }

        [TestFixtureSetUp]
        public void LookupTestDirectories()
        {
            DirectoryInfo d = new DirectoryInfo(TEST_BASE_DIR);

            List<DirectoryInfo> directories = new List<DirectoryInfo>();

            TestUtils.ScanDirectoryRecursive(d, ref directories);

// ReSharper disable CoVariantArrayConversion
            testDirectories = directories.Select(f => f.FullName).ToArray();
// ReSharper restore CoVariantArrayConversion
        }

        public object[] testDirectories;

        public object[] TestDirectories
        {
            get
            {
                if (testDirectories == null)
                {
                    LookupTestDirectories();
                }
                return testDirectories;
            }
        }

        private readonly ProjectSerializationManager serializationManager 
            = new ProjectSerializationManager();

        [Test, TestCaseSource("TestDirectories")]
        public void TestOCLAdaptation(object directory)
        {
            DirectoryInfo testDir = new DirectoryInfo(directory.ToString());

            try
            {
                FileInfo[] inputFiles = testDir.GetFiles("*in.xml");
                FileInfo projectFile = testDir.GetFiles("*.eXo").FirstOrDefault();

                if (projectFile == null)
                {
                    Assert.Fail("No project file found for test {0}", testDir.Name);
                }

                FileInfo[] stylesheetFiles = testDir.GetFiles("*.xslt");

                FileInfo referenceStylesheetFileInfo = null;

                foreach (FileInfo stylesheetFile in stylesheetFiles)
                {
                    if (stylesheetFile.Name.Contains("DesiredStylesheet") 
                        || stylesheetFile.Name.Contains("LastStylesheet") 
                        || stylesheetFile.Name.Contains("generated")
                        || stylesheetFile.Name.Contains("last-working"))
                    {
                        continue;
                    }

                    referenceStylesheetFileInfo = stylesheetFile;
                }

                Project project = serializationManager.LoadProject(projectFile);

                XsltAdaptationScriptGenerator generator = new XsltAdaptationScriptGenerator();
                ChangeDetector detector = new ChangeDetector();
                PSMSchema psmSchemaOldVersion = project.ProjectVersions[0].PSMSchemas.First();
                PSMSchema psmSchemaNewVersion = project.ProjectVersions[1].PSMSchemas.First();
                DetectedChangeInstancesSet detectedChangeInstancesSet = detector.DetectChanges(psmSchemaOldVersion, psmSchemaNewVersion);
                generator.Initialize(psmSchemaOldVersion, psmSchemaNewVersion, detectedChangeInstancesSet);
                generator.SchemaAware = true;
                generator.GenerateTransformationStructure();
                XDocument testGeneratedStylesheet = generator.GetAdaptationTransformation();
                string testGeneratedStylesheetFileName = testDir.FullName + "/" + testDir.Name + "-generated.xslt";
                testGeneratedStylesheet.SaveInUtf8(testGeneratedStylesheetFileName);
                Console.WriteLine("Revalidation stylesheet generated.");
                Console.WriteLine();

                string generatedStylesheetText = File.ReadAllText(testGeneratedStylesheetFileName);

                StringBuilder failMessage = new StringBuilder();
                int failCounts = 0;
                StringBuilder inconclusiveMessage = new StringBuilder();
            

                if (referenceStylesheetFileInfo != null)
                {
                    //XDocument xdRefStylesheet = XDocument.Load(referenceStylesheet.FullName);
                    //xdRefStylesheet.RemoveComments();
                    //string  referenceStylesheetText = xdRefStylesheet.ToString();
                    //XDocument xdGenStylesheet = XDocument.Load(testGeneratedStylesheetFile);
                    //xdGenStylesheet.RemoveComments();
                    //generatedStylesheetText = xdGenStylesheet.ToString();
                    string diff;
                    if (XmlDocumentHelper.DocumentCompare(testGeneratedStylesheetFileName, referenceStylesheetFileInfo.FullName, out diff,
                        XmlDiffOptions.IgnoreComments | XmlDiffOptions.IgnoreWhitespace))
                    {
                        string message = string.Format("Generated stylesheet for {1} is identical to reference stylesheet {0}.", referenceStylesheetFileInfo.Name, testDir.Name);
                        Console.WriteLine(message);
                    }
                    else
                    {
                        string message = string.Format("Generated stylesheet for {1} differs from reference stylesheet {0}.", referenceStylesheetFileInfo.Name, testDir.Name);
                        Console.WriteLine(message);
                        inconclusiveMessage.AppendLine(message);
                    }
                }
                else
                {
                    string message = string.Format("No reference stylesheet available. ");
                    Console.WriteLine(message);
                }
                Console.WriteLine();

                string tmpDir = Path.GetTempPath();

                foreach (FileInfo inputFile in inputFiles)
                {
                    Console.WriteLine("Testing revalidation of file {0}.", inputFile.Name);

                    string input = File.ReadAllText(inputFile.FullName);
                    string output = XsltProcessing.TransformSAXON(inputFile.FullName, testGeneratedStylesheetFileName, true);

                    string genOutputFile = inputFile.FullName.Replace("in.xml", "out-generated.xml");
                    File.WriteAllText(genOutputFile, output);
                    Console.WriteLine("XSLT transformation succeeded, results written to {0}. ", genOutputFile);

                    string refOutputFile = inputFile.FullName.Replace("in.xml", "out.xml");
                    if (File.Exists(refOutputFile))
                    {
                        bool bIdentical = XmlDocumentHelper.DocumentCompare(refOutputFile, genOutputFile, XmlDiffOptions.IgnoreXmlDecl | XmlDiffOptions.IgnoreComments);
                        if (bIdentical)
                        {
                            string message = string.Format("Output for file {0} is identical to the reference output. ", inputFile.Name);
                            Console.WriteLine(message);
                        }
                        else
                        {
                            string message = string.Format("Result differs from reference output for file {0}", inputFile.Name);
                            Console.WriteLine(message);
                            failMessage.AppendLine(message);
                            failCounts++;
                        }
                    }
                    else
                    {
                        string message = string.Format("Reference output not found for file {0}", inputFile.Name);
                        Console.WriteLine(message);
                        inconclusiveMessage.AppendLine(message);
                    }
                }

                if (inputFiles.Length == 0)
                {
                    string message = string.Format("No input files for test {0}", testDir.Name);
                    Console.WriteLine(message);
                    inconclusiveMessage.AppendLine(message);
                }

                if (failCounts > 0)
                {
                    string finalMessage = string.Format("Failed for {0}/{1} test inputs. \r\n", failCounts, inputFiles.Length);
                    Console.WriteLine(finalMessage);
                    failMessage.Insert(0, finalMessage);
                    Assert.Fail(failMessage.ToString());
                }

                if (inconclusiveMessage.Length > 0)
                {
                    Assert.Inconclusive(inconclusiveMessage.ToString());
                }

                if (failCounts == 0)
                {
                    string lastWorkingStylesheetFile = testDir.FullName + "/" + testDir.Name + "-last-working.xslt";
                    testGeneratedStylesheet.SaveInUtf8(lastWorkingStylesheetFile);
                }

                Console.WriteLine("Test succeeded. ");

            }
            catch (Exception exception)
            {
            }
        }
    }
}