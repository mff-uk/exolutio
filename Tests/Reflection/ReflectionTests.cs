using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using Exolutio.Controller.Commands;
using Exolutio.Controller.Commands.Reflection;
using Exolutio.Model;
using Exolutio.Model.Versioning;
using Exolutio.SupportingClasses.Reflection;
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