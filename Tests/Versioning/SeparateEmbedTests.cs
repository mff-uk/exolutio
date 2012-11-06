using Exolutio.Model;
using Exolutio.Model.Versioning;
using NUnit.Framework;

namespace Exolutio.Tests.Versioning
{
    [TestFixture]
    public class SeparateEmbedTests
    {
        [TestCase(true)]
        [TestCase(false)]
        public void TestSeparationSimple(bool keepGuids)
        {
            Project simpleSampleProject = TestUtils.CreateSimpleSampleProject3Versions();

            Project separatedProject = simpleSampleProject.VersionManager.SeparateVersion(simpleSampleProject.ProjectVersions[1], keepGuids);

            global::Exolutio.Tests.ModelIntegrity.ModelConsistency.CheckProject(separatedProject);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void TestSeparation(bool keepGuids)
        {
            Project sampleProject = TestUtils.CreateSampleProject3Versions();

            Project separatedProject = sampleProject.VersionManager.SeparateVersion(sampleProject.ProjectVersions[1], keepGuids);

            global::Exolutio.Tests.ModelIntegrity.ModelConsistency.CheckProject(separatedProject);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void TestEmbedSimple(bool createVersionLinks)
        {
            Project simpleSampleProject = TestUtils.CreateSimpleSampleProject3Versions();

            Project separatedProject = simpleSampleProject.VersionManager.SeparateVersion(simpleSampleProject.ProjectVersions[1], true);

            global::Exolutio.Tests.ModelIntegrity.ModelConsistency.CheckProject(separatedProject);
            
            Version separatedVersion = simpleSampleProject.VersionManager.Versions[1];
            if (!createVersionLinks)
                simpleSampleProject.VersionManager.DeleteVersion(separatedVersion);

            Version newVersion = new Version(simpleSampleProject);
            newVersion.Number = 100;
            newVersion.Label = "new";
            simpleSampleProject.VersionManager.EmbedVersion(separatedProject.SingleVersion, newVersion, separatedVersion.BranchedFrom, !createVersionLinks, createVersionLinks);
            global::Exolutio.Tests.ModelIntegrity.ModelConsistency.CheckProject(separatedProject);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void TestEmbed(bool createVersionLinks)
        {
            Project sampleProject = TestUtils.CreateSampleProject3Versions();

            Project separatedProject = sampleProject.VersionManager.SeparateVersion(sampleProject.ProjectVersions[1], true);

            global::Exolutio.Tests.ModelIntegrity.ModelConsistency.CheckProject(separatedProject);

            Version separatedVersion = sampleProject.VersionManager.Versions[1];
            if (!createVersionLinks)
                sampleProject.VersionManager.DeleteVersion(separatedVersion);

            Version newVersion = new Version(sampleProject);
            newVersion.Number = 100;
            newVersion.Label = "new";
            sampleProject.VersionManager.EmbedVersion(separatedProject.SingleVersion, newVersion, separatedVersion.BranchedFrom, !createVersionLinks, createVersionLinks);
            global::Exolutio.Tests.ModelIntegrity.ModelConsistency.CheckProject(separatedProject);
        }
    }
}