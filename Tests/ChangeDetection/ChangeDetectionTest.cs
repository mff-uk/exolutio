using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Exolutio.Model;
using Exolutio.Model.PIM;
using Exolutio.Model.PSM;
using Exolutio.Model.Serialization;
using Exolutio.Model.Versioning;
using Exolutio.Revalidation;
using NUnit.Framework;
using Version = Exolutio.Model.Versioning.Version;

namespace Tests.ChangeDetection
{
    [TestFixture]
    public class ChangeDetectionTest
    {
        [Test]
        public void AdbisExample()
        {
            ProjectSerializationManager s = new ProjectSerializationManager();
            DirectoryInfo pd = new DirectoryInfo(Environment.CurrentDirectory);
            while (!pd.GetDirectories().Any(d => d.FullName.EndsWith(@"Projects")))
                pd = pd.Parent;

            Project p = s.LoadProject(Path.Combine(pd.FullName, "Projects", "adbis-branched.eXo"));
            
            ChangeDetector detector = new ChangeDetector();
            DetectedChangesSet detectedChangesSet = detector.DetectChanges(p.ProjectVersions[0].PSMSchemas[0], p.ProjectVersions[1].PSMSchemas[0]);
        }
    }
}