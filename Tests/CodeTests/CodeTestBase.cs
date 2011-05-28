using System;
using Exolutio.Model;
using NUnit.Framework;
using System.Reflection;

namespace Tests.CodeTests
{
	public abstract class CodeTestBase
	{
		public Assembly ModelAssembly { get; set; }

		[TestFixtureSetUp]
		public virtual void InitializeFixture()
		{
			ModelAssembly = Assembly.GetAssembly(typeof(Component));
			
		}

		[TestFixtureTearDown]
		public virtual void FinalizeFixture()
		{
			
		}

	    public static void TestMethodPresent(Type type, string methodName)
        {
            MethodInfo method = type.GetMethod(methodName);

            Assert.IsNotNull(method, string.Format("Type '{0}' should override '{1}' method. ", type.Name, methodName));
            Assert.AreEqual(method.DeclaringType, type, string.Format("Type '{0}' should override '{1}' method. ", type.Name, methodName));
        }
	}
}