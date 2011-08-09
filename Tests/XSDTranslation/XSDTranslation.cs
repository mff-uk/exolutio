using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Exolutio.Controller.Commands;
using Exolutio.Model;
using Exolutio.Model.PSM;
using Exolutio.Model.PSM.Grammar.XSDTranslation;
using Exolutio.Model.Serialization;
using Exolutio.Revalidation;
using Exolutio.Revalidation.XSLT;
using Exolutio.SupportingClasses.XML;
using NUnit.Framework;
using Tests.CodeTests;

namespace Tests.XSDTranslation
{
    [TestFixture]
    public class XSDTranslation
    {
        private static string TEST_BASE_DIR
        {
            get
            {
                if (string.IsNullOrEmpty(ConfigurationManager.AppSettings["ExolutioTranslationTestRootDirectory"]))
                    throw new ConfigurationErrorsException("Key ExolutioTranslationTestRootDirectory not found in the configuration file");
                return ConfigurationManager.AppSettings["ExolutioTranslationTestRootDirectory"];
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
        public void TestXSDTranslation(object directory)
        {
            DirectoryInfo testDir = new DirectoryInfo(directory.ToString());

            FileInfo projectFile = testDir.GetFiles("*.eXo").FirstOrDefault();

            if (projectFile == null)
            {
                Assert.Fail("No project file found for test {0}", testDir.Name);
            }

            FileInfo[] xsdFiles = testDir.GetFiles("*.xsd");

            Project project = serializationManager.LoadProject(projectFile);
            
            StringBuilder failMessage = new StringBuilder();
            StringBuilder inconclusiveMessage = new StringBuilder();

            foreach (PSMSchema psmSchema in project.LatestVersion.PSMSchemas)
            {
                FileInfo referenceXSD = null;

                #region find referenc schema

                foreach (FileInfo xsdFile in xsdFiles)
                {
                    if (xsdFile.Name.Contains("DesiredSchema") || xsdFile.Name.Contains("LastSchema") ||
                        xsdFile.Name.Contains("generated"))
                    {
                        continue;
                    }

                    if (project.LatestVersion.PSMSchemas.Count == 1)
                    {
                        referenceXSD = xsdFile;
                    }
                    else
                    {
                        if (xsdFile.Name == psmSchema.Caption)
                        {
                            referenceXSD = xsdFile;
                        }
                    }
                }

                #endregion

                XsdSchemaGenerator generator = new XsdSchemaGenerator();
                generator.Initialize(psmSchema);
                generator.GenerateXSDStructure();

                XDocument testGeneratedXSD = generator.GetXsd();

                // remove comments
                testGeneratedXSD.RemoveComments();

                string testGeneratedXSDfile = testDir.FullName + "/" + testDir.Name + "-generated.xsd";
                using (XmlWriter w = XmlWriter.Create(testGeneratedXSDfile, new XmlWriterSettings() {NewLineChars = "\n", Indent = true, IndentChars = "  "}))
                {
                    testGeneratedXSD.Save(w);    
                }
                

                Console.WriteLine("XSD generated.");
                Console.WriteLine();

                string generatedXSDText = File.ReadAllText(testGeneratedXSDfile).Replace("utf-16", "utf-8");
                
                if (referenceXSD != null)
                {
                    XDocument refXSDDocument = XDocument.Load(referenceXSD.FullName);
                    refXSDDocument.RemoveComments();
                    StringBuilder refBuilder = new StringBuilder();
                    using (XmlWriter w = XmlWriter.Create(refBuilder, new XmlWriterSettings() { NewLineChars = "\n", Indent = true, IndentChars = "  " }))
                    {
                        refXSDDocument.Save(w);
                    }

                    string referenceXSDText = refBuilder.ToString().Replace("utf-16", "utf-8");

                    if (generatedXSDText != referenceXSDText)
                    {
                        string message = string.Format("Generated xsd {0} differs from reference xsd {1}.", Path.GetFileName(testGeneratedXSDfile), referenceXSD.Name);
                        Console.WriteLine(message);
                        failMessage.AppendLine(message);
                    }
                    else
                    {
                        string message = string.Format("Generated xsd {0} is identical to reference xsd {1}.", Path.GetFileName(testGeneratedXSDfile), referenceXSD.Name);
                        Console.WriteLine(message);
                    }
                }
                else
                {
                    string message = string.Format("No reference xsd available. ");
                    inconclusiveMessage.AppendLine(message);
                    Console.WriteLine(message);
                }
                Console.WriteLine();
            }

            if (failMessage.Length > 0)
            {
                Assert.Fail(failMessage.ToString());
            }

            if (inconclusiveMessage.Length > 0)
            {
                Assert.Inconclusive(inconclusiveMessage.ToString());
            }
            
            Console.WriteLine("Test succeeded. ");
        }

    }
}