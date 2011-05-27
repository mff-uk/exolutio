using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using AvalonDock;
using EvoX.Controller.Commands;
using EvoX.View;
using EvoX.View.Commands;
using EvoX.Model;
using EvoX.Model.PIM;
using EvoX.Model.PSM;
using EvoX.ResourceLibrary;
using EvoX.SupportingClasses;
using EvoX.ViewToolkit;
using Component = EvoX.Model.Component;

namespace EvoX.WPFClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : IMainWindow
    {
        public DiagramTabManager DiagramTabManager { get; private set; }

        IDiagramTabManager IMainWindow.DiagramTabManager { get { return DiagramTabManager; } }

        public IFilePresenter FilePresenter { get { return DiagramTabManager; } }
        
        public MainWindow()
        {
            Current.MainWindow = this;
            EvoXGuiCommands.Init(this);
            InitializeComponent();
            this.Icon = EvoXResourceNames.GetResourceImageSource(EvoXResourceNames.EvoXIcon);

            DiagramTabManager = new DiagramTabManager(this);
            ConfigurationManager.LoadConfiguration();
            
            this.Loaded += MainWindow_Loaded;
            dockManager.Loaded += dockManager_Loaded;
            Current.ActiveDiagramChanged += OCLEditor.LoadScriptsForActiveSchema;
            Current.RecentFile += OnRecentFile;

            EvoXRibbon.FillRecent(ConfigurationManager.Configuration.RecentFiles, ConfigurationManager.Configuration.RecentDirectories);

            //Project sampleProject = Tests.TestUtils.CreateSimpleSampleProject();
            if (!ConfigurationManager.Configuration.RecentFiles.IsEmpty() && ConfigurationManager.Configuration.RecentFiles.First().Exists)
            {
                EvoXGuiCommands.OpenProjectCommand.Execute(ConfigurationManager.Configuration.RecentFiles.First().FullName, true, true);
            }
            else
            {
#if DEBUG
                Project sampleProject = Tests.TestUtils.CreateSampleProject();
                Current.Project = sampleProject;
#else
                EvoXGuiCommands.NewProjectCommand.Execute();
#endif
            }
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
        }

        protected void MainWindow_Loaded(object sender, EventArgs e)
        {
            Application.Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;
        }

        private void OnRecentFile(FileInfo fileInfo)
        {
            ConfigurationManager.Configuration.AddToRecentFiles(fileInfo);
            ConfigurationManager.SaveConfiguration();
            EvoXRibbon.FillRecent(ConfigurationManager.Configuration.RecentFiles, ConfigurationManager.Configuration.RecentDirectories);
        }


        public Diagram ActiveDiagram
        {
            get { return DiagramTabManager != null ? DiagramTabManager.ActiveDiagram : null; }
        }

        public DiagramView ActiveDiagramView
        {
            get { return DiagramTabManager != null ? DiagramTabManager.ActiveDiagramView : null; }
        }

        public void CloseRibbonBackstage()
        {
            EvoXRibbon.BackStage.IsOpen = false;
        }

        private void MainWindow_FileDropped(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                EvoXGuiCommands.OpenProjectCommand.Execute((string[])e.Data.GetData(DataFormats.FileDrop, true), noOpenFileDialog: true);
            }
        }

        public void BusyStateChanged(object sender, BusyStateChangedEventArgs e)
        {
            this.Cursor = e.IsBusy ? Cursors.Wait : Cursors.Arrow;
        }

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            EvoXGuiCommands.CloseProjectCommand.Execute(e);
            if (!e.Cancel)
            {
                ConfigurationManager.SaveConfiguration();
                dockManager.SaveLayout(ConfigurationManager.LayoutFilePath);
            }
        }

        public void CurrentProjectChanged(object sender, CurrentProjectChangedEventArgs e)
        {
            CloseProject();
            if (e.NewProject != null)
            {
                BindProject(e.NewProject);
                if (Current.Project.LatestVersion.PIMDiagrams.Count > 0)
                    DiagramTabManager.ActivateDiagram(Current.Project.LatestVersion.PIMDiagrams[0]);
                if (Current.Project.LatestVersion != null)
                {
                    DiagramTabManager.OpenTabsForProjectVersion(Current.Project.LatestVersion);
                }
            }
            Current.ExecutedCommand += ReportDisplay.ExecutedCommand;
        }

        public void CurrentProjectVersionChanged(object sender, CurrentProjectVersionChangedEventArgs e)
        {
            UnBindProjectVersion(e.OldProjectVersion);
            if (e.NewProjectVersion != null)
            {
                BindProjectVersion(e.NewProjectVersion);
            }
        }

        private void BindProject(Project newProject)
        {
            if (newProject.ProjectFile != null)
            {
                ConfigurationManager.Configuration.AddToRecentFiles(newProject.ProjectFile);
            }
            if (newProject != null)
            {
                ProjectView.BindToProject(newProject);
            }
        }

        private void UnBindProject(Project unloadedProject)
        {
            if (unloadedProject != null)
            {
                ProjectView.UnbindFromProject(unloadedProject);
            }
        }

        private void BindProjectVersion(ProjectVersion projectVersion)
        {
            if (projectVersion.PIMDiagrams.Count == 0)
            {
                PIMDiagram pimDiagram = new PIMDiagram(projectVersion.Project);
                projectVersion.PIMDiagrams.Add(pimDiagram);
                pimDiagram.LoadSchemaToDiagram(projectVersion.PIMSchema);
            }

            if (projectVersion.PSMDiagrams.Count == 0)
            {
                foreach (PSMSchema psmSchema in projectVersion.PSMSchemas)
                {
                    PSMDiagram psmDiagram = new PSMDiagram(projectVersion.Project);
                    projectVersion.PSMDiagrams.Add(psmDiagram);
                    psmDiagram.LoadSchemaToDiagram(psmSchema);
                }
            }

            DiagramTabManager.BindToProjectVersion(projectVersion);
            if (DiagramTabManager.ActiveDiagram == null)
            {
                DiagramTabManager.OpenTabsForProjectVersion(Current.Project.LatestVersion);
            }
            navigatorTab.PIMModelTreeView.BindToProjectVersion(projectVersion);

            RefreshMenu();
        }

        private void UnBindProjectVersion(ProjectVersion unloadedProjectVersion)
        {
            DiagramTabManager.UnBindFromProjectVersion(unloadedProjectVersion);
            navigatorTab.PIMModelTreeView.UnBindFromProjectVersion(unloadedProjectVersion);
            RefreshMenu();
        }

        public void RefreshMenu()
        {
            
        }

        public void CloseProject()
        {
            UnBindProjectVersion(Current.ProjectVersion);
            UnBindProject(Current.Project);
            DiagramTabManager.CloseAllTabs();
            EvoXRibbon.versionGallery.ItemsSource = null;
        }

        static void Current_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            ExceptionWindow w = new ExceptionWindow(e.Exception);
            if (w.ShowDialog() == true)
            {
                e.Handled = true;
                Application.Current.Shutdown();
            }
            else
            {
                e.Handled = true;
            }
        }


        public void FocusComponent(Diagram diagram, Component component)
        {
            Component diagramComponent = component;
            Component subComponent = null;
            PIMAttribute pimAttribute = component as PIMAttribute;
            if (pimAttribute != null)
            {
                subComponent = pimAttribute;
                diagramComponent = ((PIMAttribute)component).PIMClass;
            }
            PSMAttribute psmAttribute = component as PSMAttribute;
            if (psmAttribute != null)
            {
                subComponent = psmAttribute;
                diagramComponent = ((PSMAttribute)component).PSMClass;
            }
            DiagramTabManager.ActivateDiagramWithElement(diagram, diagramComponent);
            if (subComponent != null)
            {
                Current.InvokeSelectComponents(new[] { subComponent });
            }
        }

        public void FocusComponent(IEnumerable<PIMDiagram> pimDiagrams, PIMComponent component)
        {
            if (pimDiagrams.Count() == 1)
            {
                DiagramTabManager.ActivateDiagramWithElement(pimDiagrams.First(), component);
            }
            else
            {
                throw new NotImplementedException("Focus component not implemented for the case where the component is present in zero or more than one diagram.");
            }
        }

        public void FocusComponent(Component component)
        {
            Diagram diagram = ModelIterator.GetDiagramForComponent(component);
            Current.MainWindow.FocusComponent(diagram, component);
        }

        public void DisplayReport(NestedCommandReport finalReport)
        {
            if (ReportDisplay.IsVisible)
            {
                ReportDisplay.DisplayedReport = finalReport;
                ReportDisplay.Update();
            }
        }

        public bool CommandsDisabled { get; private set; }

        public void DisableCommands()
        {
            EvoXRibbon.IsEnabled = false;
            CommandsDisabled = true; 
        }

        public void EnableCommands()
        {
            EvoXRibbon.IsEnabled = true;
            CommandsDisabled = false; 
        }
    }
}
