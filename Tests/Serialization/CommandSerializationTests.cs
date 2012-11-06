using System.Collections.Generic;
using System.Xml.Linq;
using Exolutio.Controller.Commands;
using Exolutio.Controller.Commands.Complex.PIM;
using Exolutio.Controller.Commands.Reflection;
using Exolutio.Model;
using NUnit.Framework;

namespace Exolutio.Tests.Serialization
{
    [TestFixture]
    public class CommandSerializationTests
    {
        [Test]
        public void TestSerialization()
        {
            Project sampleProject = TestUtils.CreateSampleProject();
            Controller.Controller c = new Controller.Controller(sampleProject);
            cmdDeletePIMAttribute command = new cmdDeletePIMAttribute(c);
            command.Set(sampleProject.SingleVersion.PIMSchema.PIMAttributes[0]);

            XDocument document = CommandSerializer.CreateEmptySerializationDocument();
            
            //ser.Serialize(command, rootElement);
            //ser.Serialize(command, rootElement);
            //ser.Serialize(command, rootElement);
            Assert.Inconclusive();
            //document.Save("TestCommandSerialization.xml");
        }

        [Test]
        public void TestDeserialization()
        {
            TestSerialization();

            XDocument scriptDocument = new XDocument();
            //scriptDocument.Load("TestCommandSerialization.xml");

            List<CommandBase> deserializeScript = CommandSerializer.DeserializeDocument(scriptDocument);

            Assert.Inconclusive();
        }


        [Test]
        public void TestSchemaReferences()
        {
            //Project sampleProject = TestUtils.CreateSampleProject();
            //PSMSchemaReference r1 = new PSMSchemaReference(sampleProject, sampleProject.SingleVersion.PSMSchemas[0]);
            //r1.Namespace = "refNamespace1";
            //r1.SchemaLocation = "reffile1";
            //r1.ReferenceType = PSMSchemaReference.EReferenceType.Import;
            //sampleProject.SingleVersion.PSMSchemas.First().PSMSchemaReferences.Add(r1);
            //PSMSchemaReference r2 = new PSMSchemaReference(sampleProject, sampleProject.SingleVersion.PSMSchemas[1]);
            //r2.Namespace = "refNamespace2";
            //r2.SchemaLocation = "reffile2";
            //r2.ReferenceType = PSMSchemaReference.EReferenceType.Import;
            //sampleProject.SingleVersion.PSMSchemas.First().PSMSchemaReferences.Add(r2);

            //ModelIntegrity.ModelConsistency.CheckProject(sampleProject);

            //ProjectSerializationManager m = new ProjectSerializationManager();
            //m.SaveProject(sampleProject, "TestSchemaReferences.xml");

            //TestUtils.LoadSaveAndCompare("TestSchemaReferences.xml");
        }
    }
}