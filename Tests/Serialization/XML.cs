using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using Exolutio.Model;
using Exolutio.Model.Serialization;
using Exolutio.Model.Versioning;
using NUnit.Framework;
using Tests.CodeTests;

namespace Tests.Serialization
{
    [TestFixture]
    public class XML
    {
        [Test]
        public void TestProjectSave()
        {
            Project sampleProject = TestUtils.CreateSampleProject();

            ProjectSerializationManager m = new ProjectSerializationManager();
            m.SaveProject(sampleProject, "TestProjectSave.xml");

            CollectionAssert.IsEmpty(m.Log, "Log contains errors or warnings");
        }

        [Test]
        public void TestProjectLoad()
        {
            TestProjectSave();

            TestUtils.LoadSaveAndCompare("TestProjectSave.xml");
        }

        [Test]
        public void TestProjectSaveWithVersions()
        {
            Project sampleProject3Versions = TestUtils.CreateSampleProject3Versions();
            ModelIntegrity.ModelConsistency.CheckProject(sampleProject3Versions);

            ProjectSerializationManager m = new ProjectSerializationManager();
            m.SaveProject(sampleProject3Versions, "TestProjectSaveWithVersions.xml");
        }

        [Test]
        public void TestProjectSaveWithVersions2()
        {
            Project sampleProject3Versions = TestUtils.CreateSampleProject3Versions();

            sampleProject3Versions.VersionManager.DeleteVersion(sampleProject3Versions.VersionManager.Versions[1]);

            ProjectSerializationManager m = new ProjectSerializationManager();
            m.SaveProject(sampleProject3Versions, "TestProjectSaveWithVersions2.xml");
        }
        
        [Test]
        public void TestProjectLoadWithVersions()
        {
            TestProjectSaveWithVersions();
            TestUtils.LoadSaveAndCompare("TestProjectSaveWithVersions.xml");
        }
        
        [Test]
        public void TestProjectLoadWithVersions2()
        {
            TestProjectSaveWithVersions2();
            TestUtils.LoadSaveAndCompare("TestProjectSaveWithVersions2.xml");
        }

        [Test]
        public void TestVersioning()
        {
            Project sampleProject = TestUtils.CreateSampleProject3Versions();

            sampleProject.VersionManager.DeleteVersion(sampleProject.VersionManager.Versions[1]);

        } 
    }
}