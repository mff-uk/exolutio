using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Exolutio.Controller.Commands;
using NUnit.Framework;

namespace Exolutio.Tests.CodeTests
{
    [TestFixture]
	public class CommandReportTest : CodeTestBase
	{
        [TestFixtureSetUp]
        public void LookupTestFiles()
        {
            Assembly controller = typeof (CommandBase).Assembly;

            string commandBaseDir = controller.CodeBase.Replace("file:///","");
            DirectoryInfo d = new DirectoryInfo(commandBaseDir);
            d = new DirectoryInfo(d.Parent.Parent.Parent.Parent.FullName + "\\Controller\\");
            Assert.IsTrue(Directory.Exists(d.FullName), "Command base dir not found");

            List<string> res = new List<string>();
            FileInfo[] files = d.GetFiles(@"*.cs", SearchOption.AllDirectories);

            foreach (FileInfo fileInfo in files)
            {
                if (File.ReadAllText(fileInfo.FullName).Contains("override void CommandOperation()"))
                {
                    res.Add(fileInfo.FullName);
                }
            }
            
            testFiles = res.ToArray();
        }

        public object[] testFiles;

        public object[] TestFiles
        {
            get
            {
                if (testFiles == null)
                {
                    LookupTestFiles();
                }
                return testFiles;
            }
        }

        [Test, TestCaseSource("TestFiles")]
        public void TestCommandResult(object fileName)
        {
            if (!File.Exists(fileName.ToString()))
            {
                Assert.Fail("File {0} does not exist." + fileName.ToString());
            }

            FileInfo file = new FileInfo(fileName.ToString());
            
            

            string fileText = System.IO.File.ReadAllText(file.FullName, System.Text.Encoding.ASCII);

            int pos = 0;
            while ((pos = fileText.IndexOf(" class ", pos)) > 0)
            {
                int methodDef = fileText.IndexOf("void CommandOperation()");

                int startMethodBody = fileText.IndexOf("{", methodDef) + 1;
                int p = startMethodBody;
                int openedBrackets = 1;

                while (openedBrackets != 0)
                {
                    if (fileText[p] == '}')
                        openedBrackets--;
                    if (fileText[p] == '{')
                        openedBrackets++;
                    p++;
                }

                if (!fileText.Substring(startMethodBody, p - startMethodBody).Contains("Report = "))
                {
                    string className = fileText.Substring(pos + " class ".Length,
                                                          fileText.IndexOfAny(new char[] {'\n', ':'}, pos) - pos -
                                                          " class ".Length);
                    if (!className.Contains("MacroCommand"))
                    {
                        Assert.Fail("Command report not set in {0}.", className);
                    }
                }
                pos++;
            }
        }
	}
}