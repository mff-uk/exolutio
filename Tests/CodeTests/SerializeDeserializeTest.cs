using System;
using System.Reflection;
using EvoX.Model;
using EvoX.Model.PSM;
using EvoX.Model.Serialization;
using EvoX.Model.Versioning;
using EvoX.SupportingClasses;
using NUnit.Framework;

namespace Tests.CodeTests
{
    [TestFixture]
    public class SerializeDeserializeTest: CodeTestBase
    {
        [Test]
        public void TestSerializeDeserializeOverride()
        {
            Type serializableType = typeof(IEvoXSerializable);
            foreach (Type type in ModelAssembly.GetTypes())
            {
                if (type.IsAmong(new Type[] { typeof(PSMSchemaClass), typeof(EvoXVersionedObjectNotAPartOfSchema) }))
                {
                    continue;
                }

                if (type.IsClass && !type.IsAbstract && serializableType.IsAssignableFrom(type))
                {
                    TestMethodPresent(type, "Serialize");
                    TestMethodPresent(type, "Deserialize");
                }
            }
        }

        
    }
}