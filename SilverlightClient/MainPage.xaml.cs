using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Exolutio.Controller.Commands;
using Exolutio.Model.PIM;
using Exolutio.Model.PSM;
using Exolutio.Model.Serialization;
using Exolutio.SupportingClasses;
using Exolutio.View;
using Exolutio.View.Commands;
using SilverlightClient.ExolutioService;
using SilverlightClient.W;
using Tests;
using Exolutio.Model;

namespace SilverlightClient
{
    public partial class MainPage : IMainWindow
    {
        public DiagramTabManager DiagramTabManager { get; private set; }

        IDiagramTabManager IMainWindow.DiagramTabManager { get { return DiagramTabManager; } }

        public IFilePresenter FilePresenter { get { return DiagramTabManager; } }

        public MainPage()
        {
            Current.MainWindow = this;
            GuiCommands.Init(this);
            InitializeComponent();
            InitializeRibbon();

            DiagramTabManager = new DiagramTabManager(this);
            
            
            this.Loaded += MainWindow_Loaded;
            //Current.ActiveDiagramChanged += Current_ActiveDiagramChanged;

            #if DEBUG
            #else
            ServerCommunication.ServerProjectListLoaded += ServerCommunication_ServerProjectListLoaded;
            ServerCommunication.GetServerProjects();
            #endif
        }

        

        protected void MainWindow_Loaded(object sender, RoutedEventArgs e)
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
                if (e.NewProject.ProjectFile != null && e.NewProject.ProjectFile.Exists &&
                    File.Exists(UserFileForProjectFile(e.NewProject.ProjectFile.FullName)))
                {
                    LoadProjectLayout(UserFileForProjectFile(e.NewProject.ProjectFile.FullName));
                }
                else
                {
                    if (Current.Project.LatestVersion.PIMDiagrams.Count > 0)
                    {
                        DiagramTabManager.ActivateDiagram(Current.Project.LatestVersion.PIMDiagrams[0]);
                    }
                    if (Current.Project.LatestVersion != null)
                    {
                        DiagramTabManager.OpenTabsForProjectVersion(Current.Project.LatestVersion);
                    }    
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
                //Binding titleBinding = new Binding();
                //titleBinding.Mode = BindingMode.OneWay;
                //titleBinding.Converter = new MainWindowTitleConverter();
                //titleBinding.Source = newProject;
                //this.SetBinding(TitleProperty, titleBinding);
                #endregion
                if (newProject.ProjectFile != null)
                {
                    //ConfigurationManager.Configuration.AddToRecentFiles(newProject.ProjectFile);
                }
            }
        }

        private void UnBindProject(Project unloadedProject)
        {
            if (unloadedProject != null)
            {
                ProjectView.UnbindFromProject(unloadedProject);
                //BindingOperations.ClearBinding(this, TitleProperty);
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
            //navigatorTab.PIMModelTreeView.BindToProjectVersion(projectVersion);

            RefreshMenu();
        }

        private void UnBindProjectVersion(ProjectVersion unloadedProjectVersion)
        {
            DiagramTabManager.UnBindFromProjectVersion(unloadedProjectVersion);
            //navigatorTab.PIMModelTreeView.UnBindFromProjectVersion(unloadedProjectVersion);
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
            
        }

        public void CloseProject()
        {
            UnBindProjectVersion(Current.ProjectVersion);
            UnBindProject(Current.Project);
            DiagramTabManager.CloseAllTabs();
        }
        

        public void Current_DispatcherUnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is LayoutCycleException)
            {
                // seems this can be ignored 
                return;
            }
            ExceptionWindow w = new ExceptionWindow(e.ExceptionObject);
            w.ShowDialog();
        }

        #region focus

        public void FocusComponent(Diagram diagram, Component component, bool activateDiagramTab = true)
        {
            Component diagramComponent = component;
            DiagramTabManager.ActivateDiagramWithElement(diagram, diagramComponent, activateDiagramTab);
        }

        public void FocusComponent(IEnumerable<PIMDiagram> pimDiagrams, PIMComponent component, bool activateDiagramTab = true)
        {
            if (pimDiagrams.Count() == 1)
            {
                DiagramTabManager.ActivateDiagramWithElement(pimDiagrams.First(), component, activateDiagramTab);
            }
            else
            {
                throw new NotImplementedException("Focus component not implemented for the case where the component is present in zero or more than one diagram.");
            }
        }

        public void FocusComponent(Component component, bool activateDiagramTab = true)
        {
            Diagram diagram = ModelIterator.GetDiagramForComponent(component);
            Current.MainWindow.FocusComponent(diagram, component, activateDiagramTab);
        }
        
        #endregion

        public void DisplayReport(CommandReportBase report, bool showEvenIfNotVisible)
        {
            if (ReportDisplay.IsVisible || showEvenIfNotVisible)
            {
                ReportDisplay.DisplayReport(report);
            }
        }
        public void DisplayLog(Log log, bool showEvenIfNotVisible)
        {
            if (ReportDisplay.IsVisible || showEvenIfNotVisible)
            {
                ReportDisplay.DisplayLog(log);
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
