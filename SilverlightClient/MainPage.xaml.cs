using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using EvoX.Controller.Commands;
using EvoX.Model.PIM;
using EvoX.Model.PSM;
using EvoX.Model.Serialization;
using EvoX.View;
using EvoX.View.Commands;
using SilverFlow.Controls;
using SilverlightClient.W;
using Tests;
using EvoX.Model;

namespace SilverlightClient
{
    public partial class MainPage : IMainWindow
    {
        public MainPage()
        {
            Current.MainWindow = this;
            EvoXGuiCommands.Init(this);
            InitializeComponent();

            Ribbon.bOpen.Command = EvoXGuiCommands.OpenProjectCommand;
            Ribbon.bNew.Command = EvoXGuiCommands.NewProjectCommand;
            Ribbon.bOpenWeb.Command = EvoXGuiCommands.OpenWebProjectCommand;
            Ribbon.bSaveToClient.Command = EvoXGuiCommands.SaveAsProjectCommand;
            Ribbon.bUndo.Command = EvoXGuiCommands.UndoCommand;
            Ribbon.bRedo.Command = EvoXGuiCommands.RedoCommand;
            Ribbon.bNormalize.Command = EvoXGuiCommands.NormalizeSchemaCommandCommand;
            Ribbon.bVerifyNormalized.Command = EvoXGuiCommands.TestNormalizationCommand;
            Ribbon.bGenerateGrammar.Command = EvoXGuiCommands.GenerateGrammarCommand;
            Ribbon.bHelp.Command = EvoXGuiCommands.HelpCommand;
            Ribbon.bPIMClass.Command = EvoXGuiCommands.AddPIMClassCommand;
            Ribbon.bPIMAssociation.Command = EvoXGuiCommands.AddPIMAssociationCommand;
            Ribbon.bPIMAttribute.Command = EvoXGuiCommands.AddPIMAttributeCommand;
            Ribbon.bPSMClass.Command = EvoXGuiCommands.AddPSMClassCommand;
            Ribbon.bPSMAssociation.Command = EvoXGuiCommands.AddPSMAssociationCommand;
            Ribbon.bPSMAttribute.Command = EvoXGuiCommands.AddPSMAttributeCommand;
            Ribbon.bPSMContentModel.Command = EvoXGuiCommands.AddPSMContentModel;
            
            EvoXGuiCommands.TestNormalizationCommand.ReportDisplay = this.ReportDisplay;
            EvoXGuiCommands.OpenWebProjectCommand.OpenServerWebProject = ServerCommunication.LoadWebFile;
            EvoXGuiCommands.OpenWebProjectCommand.GetServerProjectList = ServerCommunication.GetServerProjects;
            
            DiagramTabManager = new DiagramTabManager(this);
            ServerCommunication.ServerProjectListLoaded += ServerCommunication_ServerProjectListLoaded;
            ServerCommunication.GetServerProjects();
            button1_Click(null, null);
        }

        void ServerCommunication_ServerProjectListLoaded(object sender, EvoXServices.GetProjectFilesCompletedEventArgs e)
        {
            try
            {
                EvoXGuiCommands.OpenWebProjectCommand.ServerProjectList = e.Result;
            }
            catch 
            {
                
            }
        }

        public DiagramTabManager DiagramTabManager { get; private set; }

        IDiagramTabManager IMainWindow.DiagramTabManager
        {
            get { return DiagramTabManager; }
        }

        public IFilePresenter FilePresenter
        {
            get { return DiagramTabManager; }
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            ServerCommunication.LoadWebFile("web-project.evox");
        }

        public void OnActiveDiagramChanged(DiagramView diagramView)
        {
            Current.InvokeActiveDiagramChanged();
            Ribbon.PIMMode = diagramView != null && diagramView.Diagram is PIMDiagram;
        }

        private void LoadProject(Project newProject)
        {
            if (newProject.LatestVersion.PIMDiagrams.Count == 0)
            {
                PIMDiagram pimDiagram = new PIMDiagram(newProject);
                newProject.SingleVersion.PIMDiagrams.Add(pimDiagram);
                pimDiagram.LoadSchemaToDiagram(newProject.SingleVersion.PIMSchema);
            }

            if (newProject.LatestVersion.PSMDiagrams.Count == 0)
            {
                foreach (PSMSchema psmSchema in newProject.LatestVersion.PSMSchemas)
                {
                    PSMDiagram psmDiagram = new PSMDiagram(newProject);
                    newProject.SingleVersion.PSMDiagrams.Add(psmDiagram);
                    psmDiagram.LoadSchemaToDiagram(psmSchema);
                }
            }

            DiagramTabManager.BindToCurrentProject();
            DiagramTabManager.OpenTabsForCurrentProject();

        }

        public void CurrentProjectChanged(object sender, CurrentProjectChangedEventArgs e)
        {
            CloseProject();
            if (e.NewProject != null)
            {
                LoadProject(e.NewProject);
                if (Current.Project.LatestVersion.PIMDiagrams.Count > 0)
                    DiagramTabManager.ActivateDiagram(Current.Project.LatestVersion.PIMDiagrams[0]);
            }
            if (Current.Controller != null)
            {
                Current.Controller.ExecutedCommand += ReportDisplay.ExecutedCommand;
            }
            if (e.OldProject != null)
            {
                ProjectView.UnbindFromProject(e.OldProject);
            }
            if (e.NewProject != null)
            {
                ProjectView.BindToProject(e.NewProject);
            }

        }

        public void CurrentProjectVersionChanged(object sender, CurrentProjectVersionChangedEventArgs e)
        {
            
        }

        public void RefreshMenu()
        {
            
        }

        public void BusyStateChanged(object sender, BusyStateChangedEventArgs e)
        {
            this.Cursor = e.IsBusy ? Cursors.Wait : Cursors.Arrow;
        }

        public Diagram ActiveDiagram
        {
            get { return DiagramTabManager != null ? DiagramTabManager.ActiveDiagram : null; }
        }

        public DiagramView ActiveDiagramView
        {
            get { return DiagramTabManager != null ? DiagramTabManager.ActiveDiagramView : null; }
        }

        FloatingWindowHost IMainWindow.FloatingWindowHost
        {
            get { return floatingWindowHost; }
        }

        public void Close()
        {
            
        }

        public void CloseRibbonBackstage()
        {
            
        }

        public void CloseProject()
        {
            DiagramTabManager.CloseAllTabs();
        }

        public void FocusComponent(Diagram component, Component pimComponent)
        {
            DiagramTabManager.ActivateDiagramWithElement(component, pimComponent);    
        }

        public void DisplayReport(NestedCommandReport finalReport)
        {
            ReportDisplay.DisplayedReport = finalReport;
            ReportDisplay.Update();
        }

        private void floatingWindowHost_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            DockManager.Width = floatingWindowHost.ActualWidth - 0;
            DockManager.Height = floatingWindowHost.ActualHeight - 0;
        }

        public void Current_DispatcherUnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is LayoutCycleException)
            {
                // seems this can be ignored 
                return;
            }
            ExceptionWindow w = new ExceptionWindow(e.ExceptionObject);
            floatingWindowHost.Add(w);
            w.ShowModal();
        }
    }
}
