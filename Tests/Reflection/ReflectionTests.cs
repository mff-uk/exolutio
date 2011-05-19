using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using EvoX.Controller.Commands;
using EvoX.Controller.Commands.Reflection;
using EvoX.Model;
using EvoX.Model.Versioning;
using EvoX.SupportingClasses.Reflection;
using NUnit.Framework;

namespace Tests.CodeTests
{
    [TestFixture]
	public class ReflectionTests : CodeTestBase
	{
        [Test]
		public void TestOperationParameters()
        {
            Assembly assembly = typeof(CommandBase).Assembly;
            List<Type> commandsTypes = assembly.GetTypesWithAttribute<PublicCommandAttribute>();

            foreach (Type type in commandsTypes)
            {
                
            }            
        }
	}
}