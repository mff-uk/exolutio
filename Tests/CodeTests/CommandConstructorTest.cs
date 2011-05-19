using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using EvoX.Controller;
using EvoX.Controller.Commands;
using EvoX.Model;
using EvoX.Model.Versioning;
using EvoX.SupportingClasses.Reflection;
using NUnit.Framework;

namespace Tests.CodeTests
{
    [TestFixture]
	public class CommandConstructorTest: CodeTestBase
	{
		
		[Test]
		public void PublicCommandConstructorTest()
		{
            Assembly assembly = typeof(CommandBase).Assembly;
            List<Type> commandsTypes = assembly.GetTypesWithAttribute<PublicCommandAttribute>();

		    foreach (Type commandType in commandsTypes)
		    {
		        ConstructorInfo constructorInfo = commandType.GetConstructor(new Type[0]);
                Assert.IsNotNull(constructorInfo, "Public command {0} must declare public parameterless constructor. ", commandType.Name);
		    }
		}
	}
}