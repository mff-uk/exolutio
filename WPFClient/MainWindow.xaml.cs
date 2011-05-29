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
using Exolutio.Controller.Commands;
using Exolutio.View;
using Exolutio.View.Commands;
using Exolutio.Model;
using Exolutio.Model.PIM;
using Exolutio.Model.PSM;
using Exolutio.ResourceLibrary;
using Exolutio.SupportingClasses;
using Exolutio.ViewToolkit;
using Exolutio.WPFClient.Converters;
using Component = Exolutio.Model.Component;

namespace Exolutio.WPFClient
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
            GuiCommands.Init(this);
            InitializeComponent();
            this.Icon = ExolutioResourceNames.GetResourceImageSource(ExolutioResourceNames.ExolutioIcon);

            DiagramTabManager = new DiagramTabManager(this);
            ConfigurationManager.LoadConfiguration();
            
            this.Loaded += MainWindow_Loaded;
            dockManager.Loaded += dockManager_Loaded;
            Current.ActiveDiagramChanged += OCLEditor.LoadScriptsForActiveSchema;
            Current.RecentFile += OnRecentFile;
            ExolutioRibbon.FillRecent(ConfigurationManager.Configuration.RecentFiles, ConfigurationManager.Configuration.RecentDirectories);
            
            Application.Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;
        }

        protected void MainWindow_Loaded(object sender, EventArgs e)
        {
            OpenStartupProject();            
        }

        public Diagram ActiveDiagram
        {
            get { return DiagramTabManager != null ? DiagramTabManager.ActiveDiagram : null; }
        }

        public DiagramView ActiveDiagramView
        {
            get { return DiagramTabManager != null ? DiagramTabManager.ActiveDiagramView : null; }
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
            if (newProject != null)
            {
                ProjectView.BindToProject(newProject);
                #region title binding
                Binding titleBinding = new Binding();
                titleBinding.Mode = BindingMode.OneWay;
                titleBinding.Converter = new MainWindowTitleConverter();
                titleBinding.Source = newProject;
                this.SetBinding(TitleProperty, titleBinding);
                #endregion 
                if (newProject.ProjectFile != null)
                {
                    ConfigurationManager.Configuration.AddToRecentFiles(newProject.ProjectFile);
                }
            }
        }

        private void UnBindProject(Project unloadedProject)
        {
            if (unloadedProject != null)
            {
                ProjectView.UnbindFromProject(unloadedProject);
                BindingOperations.ClearBinding(this, TitleProperty);
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

        public void BusyStateChanged(object sender, BusyStateChangedEventArgs e)
        {
            this.Cursor = e.IsBusy ? Cursors.Wait : Cursors.Arrow;
        }

        public void CloseRibbonBackstage()
        {
            ExolutioRibbon.BackStage.IsOpen = false;
        }


        public void CloseProject()
        {
            UnBindProjectVersion(Current.ProjectVersion);
            UnBindProject(Current.Project);
            DiagramTabManager.CloseAllTabs();
            ExolutioRibbon.versionGallery.ItemsSource = null;
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

        #region focus 

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

        #endregion 

        public void DisplayReport(NestedCommandReport finalReport)
        {
            if (ReportDisplay.IsVisible)
            {
                ReportDisplay.DisplayedReport = finalReport;
                ReportDisplay.Update();
            }
        }

        #region enable/disable commands

        public bool CommandsDisabled { get; private set; }

        public void DisableCommands()
        {
            ExolutioRibbon.IsEnabled = false;
            CommandsDisabled = true; 
        }

        public void EnableCommands()
        {
            ExolutioRibbon.IsEnabled = true;
            CommandsDisabled = false;
        }

        #endregion
    }
}
