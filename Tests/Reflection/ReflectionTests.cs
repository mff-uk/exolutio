using System;
using System.Collections.Generic;
using System.Reflection;
using Exolutio.Controller.Commands;
using Exolutio.SupportingClasses.Reflection;
using Exolutio.Tests.CodeTests;
using NUnit.Framework;

namespace Exolutio.Tests.Reflection
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