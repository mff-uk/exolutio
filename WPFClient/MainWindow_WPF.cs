using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using Exolutio.SupportingClasses;
using Exolutio.View.Commands;
using System.Windows.Media;

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

        void dockManager_Loaded(object sender, RoutedEventArgs e)
        {
            if (ConfigurationManager.HasStoredLayout)
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

    }
}