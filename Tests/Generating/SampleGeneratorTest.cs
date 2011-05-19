using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Xml.Linq;
using EvoX.DataGenerator;
using EvoX.Model;
using EvoX.Model.PSM;
using EvoX.Model.Versioning;
using NUnit.Framework;

namespace Tests.CodeTests
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