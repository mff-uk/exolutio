using System;
using System.Reflection;
using Exolutio.Model;
using Exolutio.Model.PSM;
using Exolutio.Model.Serialization;
using Exolutio.Model.Versioning;
using Exolutio.SupportingClasses;
using NUnit.Framework;

namespace Tests.CodeTests
{
    [TestFixture]
    public class SerializeDeserializeTest: CodeTestBase
    {
        [Test]
        public void TestSerializeDeserializeOverride()
        {
            Type serializableType = typeof(IExolutioSerializable);
            foreach (Type type in ModelAssembly.GetTypes())
            {
                if (type.IsAmong(new Type[] { typeof(PSMSchemaClass), typeof(ExolutioVersionedObjectNotAPartOfSchema) }))
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