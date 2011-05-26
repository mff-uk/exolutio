using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Xml;
using System.Xml.Linq;
using EvoX.Controller.Commands;
using EvoX.Model;
using EvoX.Model.PIM;
using EvoX.SupportingClasses;
using EvoX.View;
using Component = EvoX.Model.Component;

#if SILVERLIGHT
using SilverFlow.Controls;
#endif

namespace EvoX.View
{
    public static class Current
    {
        private static Project project;
        public static Project Project
        {
            get { return project; }
            set
            {
                CurrentProjectChangedEventArgs e = new CurrentProjectChangedEventArgs(project, value);
                e.OldController = Controller;
                project = value;
                if (value != null)
                {
                    Controller = new Controller.Controller(e.NewProject);
                }
                else
                {
                    Controller = null;
                }
                e.NewController = Controller;
                InvokeProjectChanged(e);
                if (Project != null)
                {
                    ProjectVersion = Project.LatestVersion;
                }
            }
        }

        public static event EventHandler<CurrentProjectChangedEventArgs> ProjectChanged;

        public static void InvokeProjectChanged(CurrentProjectChangedEventArgs e)
        {
            var tmp = ProjectChanged;
            if (tmp != null)
            {
                tmp(null, e);
            }
        }

        private static ProjectVersion projectVersion;

        public static ProjectVersion ProjectVersion
        {
            get { return projectVersion; }
            set
            {
                if (projectVersion != value)
                {
                    CurrentProjectVersionChangedEventArgs e = new CurrentProjectVersionChangedEventArgs(projectVersion, value);
                    projectVersion = value;
                    InvokeProjectVersionChanged(e);
                }
            }
        }

        public static event EventHandler<CurrentProjectVersionChangedEventArgs> ProjectVersionChanged;

        public static void InvokeProjectVersionChanged(CurrentProjectVersionChangedEventArgs e)
        {
            EventHandler<CurrentProjectVersionChangedEventArgs> handler = ProjectVersionChanged;
            if (handler != null) handler(null, e);
        }

        private static Diagram activeDiagram;
        public static Diagram ActiveDiagram
        {
            get
            {
                if (Project == null || ProjectVersion == null)
                    return null;
                else 
                    return activeDiagram;
            }
            set
            {
                activeDiagram = value;
                if (ActiveDiagram != null && ActiveDiagram.ProjectVersion != ProjectVersion)
                {
                    ProjectVersion = ActiveDiagram.ProjectVersion;
                }
                InvokeActiveDiagramChanged();
            }
        }

        public static DiagramView ActiveDiagramView
        {
            get { return MainWindow.ActiveDiagramView; }
        }

        public static event Action ActiveDiagramChanged;

        public static void InvokeActiveDiagramChanged()
        {
            Action handler = ActiveDiagramChanged;
            if (handler != null) handler();
            InvokeSelectionChanged();
        }

        private static IMainWindow mainWindow;

        public static IMainWindow MainWindow
        {
            get { return mainWindow; }
            set
            {
                if (mainWindow != null)
                {
                    throw new EvoXException("MainWindow should be set only once.");
                }
                mainWindow = value;
                ProjectChanged += MainWindow.CurrentProjectChanged;
                ProjectVersionChanged += MainWindow.CurrentProjectVersionChanged;
                BusyStateChanged += MainWindow.BusyStateChanged;
            }
        }

        private static bool busyState;
        public static bool BusyState
        {
            get
            {
                return busyState;
            }
            set
            {
                busyState = value;
                OnBusyStateChanged(new BusyStateChangedEventArgs(busyState));
            }
        }

        public static Controller.Controller Controller { get; set; }

        public static event EventHandler<BusyStateChangedEventArgs> BusyStateChanged;

        public static void OnBusyStateChanged(BusyStateChangedEventArgs e)
        {
            var tmp = BusyStateChanged;
            if (tmp != null)
            {
                tmp(null, e);
            }
        }

        public static event Action SelectionChanged;

        public static void InvokeSelectionChanged()
        {
            Action handler = SelectionChanged;
            if (handler != null) handler();
        }

        public static event Action<FileInfo> RecentFile;

        public static void InvokeRecentFile(FileInfo file)
        {
            Action<FileInfo> handler = RecentFile;
            if (handler != null) handler(file);
        }

        public static event Action<Component> ComponentTouched;

        public static void InvokeComponentTouched(Component component)
        {
            Action<Component> handler = ComponentTouched;
            if (handler != null) handler(component);
        }
    }

    public interface IFilePresenter
    {
        void DisplaySampleFile(XDocument xmlDocument);
    }

    public interface IDiagramTabManager
    {
        /// <summary>
        /// Activates a diagram
        /// </summary>
        /// <param name="diagram">Diagram to be activated</param>
        void ActivateDiagram(Diagram diagram);

        /// <summary>
        /// Activates given diagram and selects given element on it
        /// </summary>
        /// <param name="diagram"></param>
        /// <param name="selectedComponent"></param>
        void ActivateDiagramWithElement(Diagram diagram, Component selectedComponent);

        void CloseActiveTab();

        IEnumerable<EvoXVersionedObject> AnotherOpenedVersions(EvoXVersionedObject item);
    }

    public interface IMainWindow
    {
        void CurrentProjectChanged(object sender, CurrentProjectChangedEventArgs e);
        void CurrentProjectVersionChanged(object sender, CurrentProjectVersionChangedEventArgs e);
        void BusyStateChanged(object sender, BusyStateChangedEventArgs e);
        Diagram ActiveDiagram { get; }
        DiagramView ActiveDiagramView { get; }
#if SILVERLIGHT
        FloatingWindowHost FloatingWindowHost { get; }
        void Current_DispatcherUnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e);
#else
        InputBindingCollection InputBindings { get; }
        CommandBindingCollection CommandBindings { get; }
#endif
        void Close();
        void CloseRibbonBackstage();
        void CloseProject();
        void FocusComponent(Diagram diagram, EvoX.Model.Component component);
        void DisplayReport(NestedCommandReport finalReport);
        IDiagramTabManager DiagramTabManager { get; }
        IFilePresenter FilePresenter { get; }
        bool CommandsDisabled { get; }
        void RefreshMenu();
        void FocusComponent(IEnumerable<PIMDiagram> pimDiagrams, PIMComponent component);
        void DisableCommands();
        void EnableCommands();
    }
}