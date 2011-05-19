using System.Configuration;
using System.IO;
using EvoX.Model;
using EvoX.Model.Serialization;
using NUnit.Framework;
using System.Collections.Generic;

namespace Tests.EvoXExportTests
{
    [TestFixture]
    public class EvoXExportTest
    {
        private static string TEST_BASE_DIR
        {
            get
            {
                if (string.IsNullOrEmpty(ConfigurationManager.AppSettings["EvoXExportTestFilesDirectory"]))
                    throw new ConfigurationErrorsException("Key EvolutionTestFilesDirectory not found in the configuration file");
                return ConfigurationManager.AppSettings["EvoXExportTestFilesDirectory"];
            }

        }

        [TestFixtureSetUp]
        public void LookupTestFiles()
        {
            DirectoryInfo d = new DirectoryInfo(TEST_BASE_DIR);
            Assert.IsTrue(Directory.Exists(d.FullName), "Test base dir not found");

            List<object> ls = new List<object>();

            foreach (FileInfo fileInfo in d.GetFiles("*.EvoX"))
            {
                ls.Add(fileInfo.FullName);
            }

            testFiles = ls.ToArray();
        }

        public object[] testFiles;

        public object[] TestFiles
        {
            get
            {
                if (testFiles == null)
                {
                    LookupTestFiles();
                }
                return testFiles;
            }
        }

        EvoX.Model.Serialization.ProjectSerializationManager projectSerializationManager = new ProjectSerializationManager();

        [Test, TestCaseSource("TestFiles")]
        public void Test(string filename)
        {
            Project loadProject = projectSerializationManager.LoadProject(filename);
            
            ModelIntegrity.ModelConsistency.CheckProject(loadProject);
        }
    }
}