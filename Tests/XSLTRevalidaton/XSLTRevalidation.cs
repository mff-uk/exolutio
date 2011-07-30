using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;
using Exolutio.Controller.Commands;
using Exolutio.Model;
using Exolutio.Model.PSM;
using Exolutio.Model.Serialization;
using Exolutio.Revalidation;
using Exolutio.Revalidation.XSLT;
using NUnit.Framework;
using Tests.CodeTests;

namespace Tests.XSLTRevalidaton
{
    [TestFixture]
    public class XSLTRevalidation
    {
        private static string TEST_BASE_DIR
        {
            get
            {
                if (string.IsNullOrEmpty(ConfigurationManager.AppSettings["ExolutioRevalidationTestRootDirectory"]))
                    throw new ConfigurationErrorsException("Key ExolutioRevalidationTestRootDirectory not found in the configuration file");
                return ConfigurationManager.AppSettings["ExolutioRevalidationTestRootDirectory"];
            }
        }

        [TestFixtureSetUp]
        public void LookupTestDirectories()
        {
            DirectoryInfo d = new DirectoryInfo(TEST_BASE_DIR);

            List<DirectoryInfo> directories = new List<DirectoryInfo>();

            ScanDirectoryRecursive(d, ref directories);

            testDirectories = directories.Select(f => f.FullName).ToArray();
        }

        private void ScanDirectoryRecursive(DirectoryInfo dir, ref List<DirectoryInfo> directories)
        {
            DirectoryInfo[] subDirectories = dir.GetDirectories("*");
            int count = 0;
            if (subDirectories.Length > 0)
            {
                foreach (DirectoryInfo subDirectory in subDirectories)
                {
                    if (subDirectory.Name.ToUpper().Contains("SVN"))
                    {
                        continue;
                    }
                    count++;
                    ScanDirectoryRecursive(subDirectory, ref directories);
                }
            }

            if (count == 0)
            {
                directories.Add(dir);
            }
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

        private ProjectSerializationManager serializationManager = new ProjectSerializationManager();

        [Test, TestCaseSource("TestDirectories")]
        public void TestRevalidation(object directory)
        {
            DirectoryInfo testDir = new DirectoryInfo(directory.ToString());

            FileInfo[] inputFiles = testDir.GetFiles("*in.xml");
            FileInfo projectFile = testDir.GetFiles("*.eXo").FirstOrDefault();

            if (projectFile == null)
            {
                Assert.Fail("No project file found for test {0}", testDir.Name);
            }

            FileInfo[] stylesheetFiles = testDir.GetFiles("*.xslt");

            FileInfo referenceStylesheet = null;

            foreach (FileInfo stylesheetFile in stylesheetFiles)
            {
                if (stylesheetFile.Name.Contains("DesiredStylesheet") 
                    || stylesheetFile.Name.Contains("LastStylesheet") 
                    || stylesheetFile.Name.Contains("generated")
                    || stylesheetFile.Name.Contains("last-working"))
                {
                    continue;
                }

                referenceStylesheet = stylesheetFile;
            }

            Project project = serializationManager.LoadProject(projectFile);

            XsltRevalidationScriptGenerator generator = new XsltRevalidationScriptGenerator();
            ChangeDetector detector = new ChangeDetector();
            PSMSchema psmSchemaOldVersion = project.ProjectVersions[0].PSMSchemas.First();
            PSMSchema psmSchemaNewVersion = project.ProjectVersions[1].PSMSchemas.First();
            DetectedChangeInstancesSet detectedChangeInstancesSet = detector.DetectChanges(psmSchemaOldVersion, psmSchemaNewVersion);
            generator.Initialize(psmSchemaOldVersion, psmSchemaNewVersion, detectedChangeInstancesSet);
            generator.GenerateTemplateStructure();
            XDocument testGeneratedStylesheet = generator.GetRevalidationStylesheet();
            string testGeneratedStylesheetFile = testDir.FullName + "/" + testDir.Name + "-generated.xslt";
            testGeneratedStylesheet.Save(testGeneratedStylesheetFile);
            Console.WriteLine("Revalidation stylesheet generated.");
            Console.WriteLine();

            string generatedStylesheetText = File.ReadAllText(testGeneratedStylesheetFile);

            StringBuilder failMessage = new StringBuilder();
            int failCounts = 0;
            StringBuilder inconclusiveMessage = new StringBuilder();
            

            if (referenceStylesheet != null)
            {
                string referenceStylesheetText = File.ReadAllText(referenceStylesheet.FullName);
                
                if (generatedStylesheetText != referenceStylesheetText)
                {
                    string message = string.Format("Generated stylesheet {0} differs from reference stylesheet {1}.", referenceStylesheet.Name, testDir.Name + ".xslt");
                    Console.WriteLine(message);
                    inconclusiveMessage.AppendLine(message);
                }
                else
                {
                    string message = string.Format("Generated stylesheet {0} is identical to reference stylesheet {1}.", referenceStylesheet.Name, testDir.Name + ".xslt");
                    Console.WriteLine(message);
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
                string output = XsltProcessing.Transform(input, generatedStylesheetText, tmpDir);

                string genOutputFile = inputFile.FullName.Replace("in.xml", "out-generated.xml");
                File.WriteAllText(genOutputFile, output);
                Console.WriteLine("XSLT transformation succeeded, results written to {0}. ", genOutputFile);

                string refOutputFile = inputFile.FullName.Replace("in.xml", "out.xml");
                if (File.Exists(refOutputFile))
                {
                    string referenceOutput = File.ReadAllText(refOutputFile);
                    if (referenceOutput != output)
                    {
                        string message = string.Format("Result differs from reference output for file {0}", inputFile.Name);
                        Console.WriteLine(message);
                        failMessage.AppendLine(message);
                        failCounts++;
                    }
                    else
                    {
                        string message = string.Format("Output for file {0} is identical to the reference output. ", inputFile.Name);
                        Console.WriteLine(message);
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

            string lastWorkingStylesheetFile = testDir.FullName + "/" + testDir.Name + "-last-working.xslt";
            testGeneratedStylesheet.Save(lastWorkingStylesheetFile);

            Console.WriteLine("Test succeeded. ");

        }
    }
}