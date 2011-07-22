using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Xml.Linq;
using Exolutio.Model;
using Exolutio.SupportingClasses;
using Exolutio.View;
using Exolutio.View.Commands;
using System.Windows.Media;
using DataFormats = System.Windows.DataFormats;
using DragEventArgs = System.Windows.DragEventArgs;

namespace Exolutio.WPFClient
{
    public partial class MainWindow
    {
        private void OnRecentFile(FileInfo fileInfo)
        {
            ConfigurationManager.Configuration.AddToRecentFiles(fileInfo);
            ConfigurationManager.SaveConfiguration();
            ExolutioRibbon.FillRecent(ConfigurationManager.Configuration.RecentFiles, ConfigurationManager.Configuration.RecentDirectories);
        }

        private bool dockManagerLoaded = false;

        void dockManager_Loaded(object sender, RoutedEventArgs e)
        {
            dockManagerLoaded = true;

            if (Current.Project != null && Current.Project.ProjectFile != null &&
                File.Exists(Current.MainWindow.UserFileForProjectFile(Current.Project.ProjectFile.FullName)))
            {
                try
                {
                    LoadProjectLayout(Current.MainWindow.UserFileForProjectFile(Current.Project.ProjectFile.FullName));
                }
                catch
                {
                    
                }
            }
            else if (ConfigurationManager.HasStoredLayout)
            {
                try
                {
                    dockManager.RestoreLayout(ConfigurationManager.LayoutFilePath);
                }
                catch (Exception)
                {
                    try
                    {
                        Commands.StaticWPFClientCommands.ResetWindowLayout.Execute();
                    }
                    catch (Exception)
                    {

                    }
                }
            }

            dockManager.MainDocumentPane.Background = Brushes.Transparent;

            if (Environment.MachineName.Contains("TRUPIK"))
            {
                if (System.Windows.Forms.Screen.AllScreens.Length > 1)
                {
                    this.WindowState = WindowState.Normal;
                    this.Left = System.Windows.Forms.Screen.AllScreens[0].Bounds.X;
                    this.Top = System.Windows.Forms.Screen.AllScreens[0].Bounds.Y;
                    this.Width = System.Windows.Forms.Screen.AllScreens[0].Bounds.Width;
                    this.Height = System.Windows.Forms.Screen.AllScreens[0].Bounds.Height;
                }
            }
        }

        private void MainWindow_FileDropped(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                GuiCommands.OpenProjectCommand.Execute((string[])e.Data.GetData(DataFormats.FileDrop, true), noOpenFileDialog: true);
            }
        }

        private void OpenStartupProject()
        {
            //Project sampleProject = Tests.TestUtils.CreateSimpleSampleProject();
            if (!ConfigurationManager.Configuration.RecentFiles.IsEmpty() && ConfigurationManager.Configuration.RecentFiles.First().Exists)
            {
                GuiCommands.OpenProjectCommand.Execute(ConfigurationManager.Configuration.RecentFiles.First().FullName, true, true);
            }
            else
            {
                #if DEBUG
                Project sampleProject = Tests.TestUtils.CreateSampleProject();
                Current.Project = sampleProject;
                #else
                GuiCommands.NewProjectCommand.Execute();
                #endif
            }
        }

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            GuiCommands.CloseProjectCommand.Execute(e);
            if (!e.Cancel)
            {
                ConfigurationManager.SaveConfiguration();
                dockManager.SaveLayout(ConfigurationManager.LayoutFilePath);
            }
        }

        public void LoadProjectLayout(string filePath)
        {
            if (dockManagerLoaded)
            {
                XDocument d = XDocument.Load(filePath);
                List<Guid> diagramIds = new List<Guid>();
                ExtractDiagramIds((XElement) d.FirstNode, diagramIds);
                Diagram diagram = null;
                foreach (Guid diagramId in diagramIds)
                {
                    diagram = (Diagram) Current.Project.TranslateComponent(diagramId);
                    DiagramTabManager.ActivateDiagram(diagram, false);
                }
                dockManager.RestoreLayout(filePath);
                if (diagram != null)
                {
                    DiagramTabManager.ActivateDiagram(diagram, true);
                }
            }
        }

        private static void ExtractDiagramIds(XElement element, List<Guid> diagramIds)
        {
            if (element.Name == "DocumentContent")
            {
                XAttribute xAttribute = element.Attribute("Name");
                if (xAttribute != null)
                {
                    string strGuid = xAttribute.Value.Replace("_", "-").Substring(1);
                    diagramIds.Add(new Guid(strGuid));
                }
            }
            foreach (XElement child in element.Elements())
            {
                ExtractDiagramIds(child, diagramIds);
            }
        }

        public void SaveProjectLayout(string filePath)
        {
            try
            {
                dockManager.SaveLayout(filePath);
            }
            catch (Exception)
            {
            }
        }

        public string UserFileForProjectFile(string projectFilePath)
        {
            string fileNameWithoutExtension = System.IO.Path.GetFileNameWithoutExtension(projectFilePath);
            return Path.GetDirectoryName(projectFilePath) + "\\" + fileNameWithoutExtension + ".eXo.user";
        }
    }
}