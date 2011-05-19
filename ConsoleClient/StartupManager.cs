//#define testingconsole

using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Net.Mime;
using System.Text;
using System.Text.RegularExpressions;
using CommandLineParser.Arguments;
using CommandLineParser.Validation;
using EvoX.Model;

namespace EvoX.WPFClient
{
    /// <summary>
    /// Starts the application, handles command line parameters
    /// </summary>
    [ArgumentGroupCertification("list,help,project", EArgumentGroupCondition.AtLeastOneUsed)]
    public class StartupManager
    {
        [FileArgument('p', "project", FileMustExist = true, Description = "Input EvoX project filePath", FullDescription = "ARG_INPUT")]
        public FileInfo InputFile { get; set; }

        [DirectoryArgument('o', "outputDir", DirectoryMustExist = true, Description = "Output directory", FullDescription = "ARG_OUTPUTDIR")]
        public DirectoryInfo OutputDir { get; set; }

        [DirectoryArgument('n', "outputDirPng", DirectoryMustExist = true, Description = "PNG output directory ", FullDescription = "ARG_OUTPUTDIRPNG")]
        public DirectoryInfo OutputDirPng { get; set; }

        [DirectoryArgument('g', "outputDirXsd", DirectoryMustExist = true, Description = "XSD output directory", FullDescription = "ARG_OUTPUTDIRXSD")]
        public DirectoryInfo OutputDirXsd { get; set; }

        [SwitchArgument('i', "noImages", false, Description = "Flag, do not export diagrams as images", FullDescription = "ARG_NOIMAGES")]
        public bool ExcludeImages { get; set; }

        [SwitchArgument('s', "noSchemas", false, Description = "Flag, export PSM diagrams as schemas", FullDescription = "ARG_NOSCHEMAS")]
        public bool ExcludeSchemas { get; set; }

        [ValueArgument(typeof(string), 'd', "diagram", AllowMultiple = true, Description = "Identification of the diagrams to export", FullDescription = "ARG_DIAGRAM")]
        public string[] DiagramSpecifications { get; set; }

        [SwitchArgument('l', "list", false, Description = "Do nothing, only list diagrams in the project", FullDescription = "ARG_LIST")]
        public bool List { get; set; }

        [SwitchArgument('h', "help", false, Description = "Usage info", FullDescription = "ARG_HELP")]
        public bool Help { get; set; }

        /// <summary>
        /// Processes the command line.
        /// </summary>
        /// <param name="args">Command line arguments</param>
        private void ProcessCommandLine(string[] args)
        {
            CommandLineParser.CommandLineParser parser = new CommandLineParser.CommandLineParser();
            parser.ShowUsageOnEmptyCommandline = false;
            parser.ExtractArgumentAttributes(this);
            //parser.FillDescFromResource(new CommandlineArguments());

#if testingconsole
			args = new string[]
			       	{
			       		"--project",
			       		@"D:\Programování\EvoX\Test\ABCCompany.EvoX",
						//"--outputDir",
                        //@"D:\Programování\EvoX\Test\cmdoutput\",
						//"-d",
                        //"0;1",
                        // "-d",
                        //"2(filename2)",
                        //"-d",
                        //"TransportDetailFormat(filename5)[I]"
						//"1-3(filename1,filename2,filename3)[I]"
						//"3"
			       	};
			//args = new string[] {"-i", "asdf"};
#endif
            try
            {
                parser.ParseCommandLine(args);
            }
            catch (CommandLineParser.Exceptions.CommandLineException e)
            {
                Console.WriteLine("Error in command line parameters: ");
                Console.WriteLine(e.Message);
                Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine("Correct usage: ");
                parser.ShowUsage();
                ShutDown();
                return;
            }

            #region List and Help

            if (Help)
            {
                HelpAndShutdown(parser);
                return;
            }

            // povolena kombinace list + project
            if (List)
            {
                ListAndShutdown();
                return;
            }

            #endregion
        }
        
        /// <summary>
        /// Prints help to console and shuts down.
        /// </summary>
        /// <param name="parser">The parser.</param>
        private void HelpAndShutdown(CommandLineParser.CommandLineParser parser)
        {
            parser.ShowUsage();
            ShutDown();
        }

        /// <summary>
        /// Lists the diagrams in input projects and shuts down.
        /// </summary>
        private void ListAndShutdown()
        {
            throw new NotImplementedException("Member StartupManager.ListAndShutdown not implemented.");
            //if (InputFile == null)
            //{
            //    Console.WriteLine("Error > input project was not specified");
            //}
            //else
            //{
            //    CreateHiddenMainWindow();
            //    ProjectVersions project = mainWindow.OpenProject(InputFile.FullName);
            //    Console.WriteLine("Diagrams in project {0}", project.Caption);
            //    for (int i = 0; i < project.Diagrams.Count; i++)
            //    {
            //        Diagram diagram = project.Diagrams[i];
            //        Console.WriteLine("  {2}: {0} ({1})", diagram.Caption, diagram is PIMDiagram ? "PIM" : "PSM", i);
            //    }
            //    Console.WriteLine("Total: {0} diagrams.", project.Diagrams.Count);
            //}
            //ShutDown();
        }

        /// <summary>
        /// Shuts down the application. 
        /// </summary>
        private void ShutDown()
        {
            
        }
    }
}
