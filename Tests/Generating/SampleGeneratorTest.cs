using System.Xml.Linq;
using Exolutio.DataGenerator;
using Exolutio.Model;
using Exolutio.Model.PSM;
using NUnit.Framework;

namespace Exolutio.Tests.Generating
{
    [TestFixture]
	public class SampleGeneratorTest
	{
		[Test]
		public void TestSampleGenerator()
		{
		    Project project = TestUtils.CreateSampleProject();

            SampleDataGenerator generator = new SampleDataGenerator();
		    PSMSchema psmSchema = project.SingleVersion.PSMSchemas[0];
		    XDocument xmlDocument = generator.Translate(psmSchema);
            xmlDocument.Save("TestSampleGenerator.xml");
		}
	}
}