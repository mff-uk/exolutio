using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml;
using System.Xml.Linq;
using Exolutio.Controller;
using Exolutio.Controller.Commands;
using Exolutio.Model;
using Exolutio.Model.OCL;
using Exolutio.Model.PIM;
using Exolutio.Model.PSM;
using Exolutio.SupportingClasses;
using Exolutio.View;
using Exolutio.View.Commands.Grammar;
using Component = Exolutio.Model.Component;
using System.Windows.Media;

namespace Exolutio.View
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
                if (Controller != null)
                {
                    e.OldController = Controller;
                    Controller.ExecutedCommand -= Current_Controller_ExecutedCommand;
                }
                
                project = value;
                if (value != null)
                {
                    Controller = new Controller.Controller(e.NewProject);
                    Controller.ExecutedCommand += Current_Controller_ExecutedCommand;
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
                ActiveOCLScript = (ActiveDiagram != null && ActiveDiagram.Schema.OCLScripts.Count > 0) 
                    ?  ActiveDiagram.Schema.OCLScripts[0] : null;
            }
        }

        public static event Action ActiveDiagramChanged;

        public static void InvokeActiveDiagramChanged()
        {
            Action handler = ActiveDiagramChanged;
            if (handler != null) handler();
            InvokeSelectionChanged();
        }

        private static OCLScript activeOCLScript;
        public static OCLScript ActiveOCLScript
        {
            get
            {
                if (Project == null || ProjectVersion == null)
                    return null;
                else
                    return activeOCLScript;
            }
            set
            {
                if (value != null && value.Schema != ActiveDiagram.Schema)
                    activeOCLScript = null;
                else 
                    activeOCLScript = value;
                InvokeActiveOCLScriptChanged();
            }
        }

        public static event Action ActiveOCLScriptChanged;

        public static void InvokeActiveOCLScriptChanged()
        {
            Action handler = ActiveOCLScriptChanged;
            if (handler != null) handler();
        }

        public static DiagramView ActiveDiagramView
        {
            get { return MainWindow.ActiveDiagramView; }
        }

        private static IMainWindow mainWindow;

        public static IMainWindow MainWindow
        {
            get { return mainWindow; }
            set
            {
                if (mainWindow != null)
                {
                    throw new ExolutioException("MainWindow should be set only once.");
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

        //public static event Action<IEnumerable<Component>> SelectComponents;

        private static void Current_Controller_ExecutedCommand(CommandBase command, bool ispartofmacro, CommandBase macrocommand, bool isUndo, bool isRedo)
        {
            InvokeExecutedCommand(command, ispartofmacro, macrocommand, isUndo, isRedo);   
        }
        
        /// <summary>
        /// This event is fired each time a command is executed in the current controller (current project). 
        /// Bind to this event with caution, the binding survives change of project/controller (e.g. when 
        /// new project is opened).
        /// </summary>
        public static event CommandEventHandler ExecutedCommand;

        public static void InvokeExecutedCommand(CommandBase command, bool isPartOfMacro, CommandBase macroCommand, bool isUndo, bool isRedo)
        {
            CommandEventHandler handler = ExecutedCommand;
            if (handler != null) handler(command, isPartOfMacro, macroCommand, isUndo, isRedo);
        }
    }

    public interface IFilePresenter
    {
        IFilePresenterTab DisplayFile(XDocument xmlDocument, EDisplayedFileType fileType, string fileName = null, ILog log = null, PSMSchema validationSchema = null, PSMSchema sourcePSMSchema = null, FilePresenterButtonInfo[] additionalActions = null, object tag = null);
    }

    public class FilePresenterButtonInfo
    {
        public ImageSource Icon { get; set; }

        public string Text { get; set; }

        public bool ToggleButton { get; set; }

        public bool RadioToggleButton { get; set; }

        public bool IsToggled { get; set; }

        public string ButtonName { get; set; }

        public UpdateFileContent UpdateFileContentAction;
    }

    public interface IFilePresenterTab
    {
        string FileName { get; set; }
        PSMSchema ValidationSchema { get; set; }
        string GetDocumentText();
        void SetDocumentText(string text);
        Action<IFilePresenterTab> RefreshCallback { get; set; }
        PSMSchema SourcePSMSchema { get; set; }
        void ReDisplayFile(XDocument xmlDocument, EDisplayedFileType fileType, string fileName = null, ILog log = null, PSMSchema validationSchema = null, PSMSchema sourcePSMSchema = null, FilePresenterButtonInfo[] additionalActions = null);
        void ReDisplayFile(string document, EDisplayedFileType fileType, string fileName = null, ILog log = null, PSMSchema validationSchema = null, PSMSchema sourcePSMSchema = null, FilePresenterButtonInfo[] additionalActions = null);
        IEnumerable<FilePresenterButtonInfo> FilePresenterButtons { get; }
        object Tag { get; }
        void DisplayAdditionalControl(ContentControl contentControl, string tabHeader);
    }

    public delegate void UpdateFileContent(IFilePresenterTab fileTab);

    public interface IDiagramTabManager
    {
        UIElement TopElement { get; }
        /// <summary>
        /// Activates a diagram
        /// </summary>
        /// <param name="diagram">Diagram to be activated</param>
        void ActivateDiagram(Diagram diagram);

        /// <summary>
        /// Activates given diagram and selects given element on it
        /// </summary>
        void ActivateDiagramWithElement(Diagram diagram, Component selectedComponent, bool activateDiagramTab = true);

        void CloseActiveTab();

        IEnumerable<ExolutioVersionedObject> AnotherOpenedVersions(ExolutioVersionedObject item);
        DiagramView GetOpenedDiagramView(Diagram diagram);
        IList<DiagramView> GetTopDiagramViews();
        IList<DiagramView> GetOpenedDiagramViews();
    }

    public interface IMainWindow
    {
        void CurrentProjectChanged(object sender, CurrentProjectChangedEventArgs e);
        void CurrentProjectVersionChanged(object sender, CurrentProjectVersionChangedEventArgs e);
        void BusyStateChanged(object sender, BusyStateChangedEventArgs e);
        Diagram ActiveDiagram { get; }
        DiagramView ActiveDiagramView { get; }
#if SILVERLIGHT
        void Current_DispatcherUnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e);
#else
        InputBindingCollection InputBindings { get; }
        CommandBindingCollection CommandBindings { get; }
        void LoadProjectLayout(string filePath);
        void SaveProjectLayout(string filePath);
        string UserFileForProjectFile(string projectFilePath);
#endif
        void Close();
        void CloseRibbonBackstage();
        void CloseProject();
        void DisplayReport(CommandReportBase report, bool showEvenIfNotVisible);
        void DisplayLog(Log log, bool showEvenIfNotVisible);
        IDiagramTabManager DiagramTabManager { get; }
        IFilePresenter FilePresenter { get; }
        bool CommandsDisabled { get; }
        void RefreshMenu();
        void FocusComponent(Diagram diagram, Component component, bool activateDiagramTab = true);
        void FocusComponent(IEnumerable<PIMDiagram> pimDiagrams, PIMComponent component, bool activateDiagramTab = true);
        void FocusComponent(Component component, bool activateDiagramTab = true);
        void DisableCommands();
        void EnableCommands();
    }
}