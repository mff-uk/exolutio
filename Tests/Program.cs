using System;
using Tests.CodeTests;

namespace Tests
{
	public class Program
	{
		public static void Main()
		{
            CommandReportTest t = new CommandReportTest();


            foreach (object testFile in t.TestFiles)
            {
                try
                {
                    t.TestCommandResult(testFile);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
		}
	}
}