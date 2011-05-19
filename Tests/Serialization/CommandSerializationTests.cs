using System.Collections.Generic;
using System.Linq;
using System.Xml;
using EvoX.Controller;
using EvoX.Controller.Commands;
using EvoX.Controller.Commands.Complex.PIM;
using EvoX.Controller.Commands.Reflection;
using EvoX.Model;
using EvoX.Model.PSM;
using EvoX.Model.Serialization;
using NUnit.Framework;
using EvoX.Controller.Commands.Atomic.PIM;

namespace Tests.Serialization
{
    [TestFixture]
    public class CommandSerializationTests
    {
        [Test]
        public void TestSerialization()
        {
            Project sampleProject = TestUtils.CreateSampleProject();
            Controller c = new Controller(sampleProject);
            cmdDeletePIMAttribute command = new cmdDeletePIMAttribute(c);
            command.Set(sampleProject.SingleVersion.PIMSchema.PIMAttributes[0]);

            CommandSerializer ser = new CommandSerializer();
            XmlDocument document = ser.CreateEmptySerializationDocument();
            XmlElement rootElement = (XmlElement) document.ChildNodes[1];
            ser.Serialize(command, rootElement);
            ser.Serialize(command, rootElement);
            ser.Serialize(command, rootElement);

            document.Save("TestCommandSerialization.xml");
        }

        [Test]
        public void TestDeserialization()
        {
            TestSerialization();

            XmlDocument scriptDocument = new XmlDocument();
            scriptDocument.Load("TestCommandSerialization.xml");

            CommandSerializer ser = new CommandSerializer();
            List<CommandBase> deserializeScript = ser.DeserializeScript(scriptDocument);
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