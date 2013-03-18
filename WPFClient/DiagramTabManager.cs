using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Xml.Linq;
using AvalonDock;
using Exolutio.Model;
using Exolutio.Model.OCL;
using Exolutio.Model.PIM;
using Exolutio.Model.PSM;
using Exolutio.SupportingClasses;
using Exolutio.View;
using System.Windows.Media;

namespace Exolutio.WPFClient
{
    public partial class DiagramTabManager : IFilePresenter, IDiagramTabManager
    {
        public MainWindow MainWindow { get; private set; }

        public DockingManager DockManager { get { return MainWindow.dockManager; } }

        public UIElement TopElement
        {
            get { return DockManager; }
        }

        public DiagramView ActiveDiagramView
        {
            get
            {
                return DockManager.ActiveDocument != null && DockManager.ActiveDocument is DiagramTab ? ((DiagramTab)DockManager.ActiveDocument).DiagramView : null;
            }
        }

        public Diagram ActiveDiagram
        {
            get
            {
                return ActiveDiagramView != null ? ActiveDiagramView.Diagram : null;
            }
        }

        public DiagramTabManager(MainWindow mainWindow)
        {
            MainWindow = mainWindow;

            DockManager.ActiveDocumentChanged += DockManager_ActiveTabChanged;
            DockManager.MainDocumentPane.Background = Brushes.Transparent;
            Current.ActiveDiagramChanged += Current_ActiveDiagramChanged;
        }

        void Current_ActiveDiagramChanged()
        {
            if (ActiveDiagram is PIMDiagram)
            {
                MainWindow.ExolutioRibbon.pimGroup.Visibility = System.Windows.Visibility.Visible;
                if (MainWindow.ExolutioRibbon.tabPSM.IsSelected
                    || MainWindow.ExolutioRibbon.tabTranslation.IsSelected) 
                    MainWindow.ExolutioRibbon.tabPIM.IsSelected = true;
                MainWindow.ExolutioRibbon.psmGroup.Visibility = System.Windows.Visibility.Collapsed;
            }
            else if (ActiveDiagram is PSMDiagram)
            {
                MainWindow.ExolutioRibbon.psmGroup.Visibility = System.Windows.Visibility.Visible;
                if (MainWindow.ExolutioRibbon.tabPIM.IsSelected) MainWindow.ExolutioRibbon.tabPSM.IsSelected = true;
                MainWindow.ExolutioRibbon.pimGroup.Visibility = System.Windows.Visibility.Collapsed;
            }
            else
            {
                MainWindow.ExolutioRibbon.pimGroup.Visibility = System.Windows.Visibility.Collapsed;
                MainWindow.ExolutioRibbon.psmGroup.Visibility = System.Windows.Visibility.Collapsed;
            }

            ActivateDiagram(Current.ActiveDiagram);
        }

        /// <summary>
        /// Activates a diagram
        /// </summary>
        /// <param name="diagram">Diagram to be activated</param>
        /// <param name="activateDiagramTab"></param>
        public DiagramTab ActivateDiagram(Diagram diagram, bool activateDiagramTab = true)
        {
            if (ActiveDiagram != diagram && diagram != null)
            {
                DiagramTab tab = FindTab(diagram);
                if (tab == null)
                {
                    tab = AddTab(diagram);
                }
                else
                {
                    DockManager.ActiveDocument = tab;
                }
                if (activateDiagramTab)
                {
                    tab.ActivateTab(false);
                }
                return tab;
            }
            else
            {
                return DockManager.ActiveDocument as DiagramTab;
            }
        }

        void IDiagramTabManager.ActivateDiagram(Diagram diagram)
        {
            this.ActivateDiagram(diagram);
        }

        private void SelectedItems_CollectionChanged()
        {
            Current.InvokeSelectionChanged();
        }        

        /// <summary>
        /// Finds among currently open PanelWindows the one which is associated with given <paramref name="diag"/>
        /// <param name="diag">Diagram whose tab has to be found</param>
        /// </summary>
        private DiagramTab FindTab(Diagram diag)
        {
            foreach (DiagramTab tab in DockManager.Documents.OfType<DiagramTab>())
            {
                if (tab.DiagramView.Diagram == diag)
                {
                    return tab;
                }
            }
            return null;
        }

        /// <summary>
        /// Adds a new tab to the tab panel, associated with given <paramref name="diagram"/>
        /// <param name="diagram">Diagram to which the new tab should be bound</param>
        /// </summary>
        internal DiagramTab AddTab(Diagram diagram)
        {
            DiagramTab newTab = null;

            if (diagram is PIMDiagram)
            {
                newTab = new PIMDiagramTab();
            }
            else if (diagram is PSMDiagram)
            {
                newTab = new PSMDiagramTab();
            }

            if (newTab != null)
            {
                newTab.BindTab(diagram);
                newTab.DiagramView.SuspendBindingInChildren = true; 
                newTab.DiagramView.LoadDiagram(diagram);                
                newTab.DiagramView.SelectionChanged += SelectedItems_CollectionChanged;
                newTab.Name = "D" + diagram.ID.ToString().Replace("-", "_");
                ActivePane.Items.Add(newTab);
                ActivePane.UpdateLayout();
                //ActivePane.BringHeaderToFront(newTab);
                if (newTab.ContainerPane != null)
                {
                    DockManager.ActiveDocument = newTab;
                } 
                newTab.DiagramView.SuspendBindingInChildren = false; 
            }

            return newTab;
        }


        /// <summary>
        /// Closes active PanelWindow
        /// </summary>
        public void CloseActiveTab()
        {
            if (DockManager.ActiveDocument != null)
            {
                int index = DockManager.MainDocumentPane.Items.IndexOf(DockManager.ActiveDocument);
                DiagramTab pw = DockManager.ActiveDocument as DiagramTab;
                if (pw != null)
                    RemoveTab(pw);
                else
                {
                    DockManager.ActiveDocument.Close();
                }
            }
        }

        internal void RemoveTab(DiagramTab tab)
        {
            tab.Close();
            if (DockManager.ActiveDocument == null && DockManager.Documents.Count() > 0)
            {
                DockManager.ActiveDocument = DockManager.Documents.Last();
            }
        }

        /// <summary>
        /// Activates given diagram and selects given element on it
        /// </summary>
        public void ActivateDiagramWithElement(Diagram diagram, Component selectedComponent, bool activateDiagramTab = true)
        {
            DiagramTab tab = ActivateDiagram(diagram, activateDiagramTab);
            tab.DiagramView.ClearSelection();

            if (selectedComponent != null)
            {
                tab.DiagramView.SetSelection(selectedComponent, true);
            }

            Current.ActiveDiagram = diagram;
        }

        public DiagramView GetOpenedDiagramView(Diagram diagram)
        {
            DiagramTab diagramTab = FindTab(diagram);
            if (diagramTab != null)
            {
                return diagramTab.DiagramView;
            }
            else return null;
        }

        /// <summary>
        /// Reaction on change of active document.
        /// Invokes ActiveDiagramChanged event.
        /// </summary>
        private void DockManager_ActiveTabChanged(object sender, EventArgs eventArgs)
        {
            if (!MainWindow.CommandsDisabled)
            {
                DiagramTab diagramTab = DockManager.ActiveDocument as DiagramTab;
                if (diagramTab != null)
                {
                    Current.ActiveDiagram = diagramTab.DiagramView.Diagram;
                }
                else
                {
                    if (System.Environment.MachineName.Contains("TRUPIK") && DockManager.ActiveDocument is IFilePresenterTab)
                    {
                        return;
                    }
                    
                    Current.ActiveDiagram = null;
                }
            }
            else
            {
                Current.InvokeSelectionChanged();
            }
        }

        internal void project_DiagramRemoved(object sender, DiagramEventArgs e)
        {
            DiagramTab tab = FindTab(e.Diagram);
            if (tab != null)
            {
                RemoveTab(tab);
            }
        }

        internal void project_DiagramAdded(object sender, DiagramEventArgs e)
        {
            AddTab(e.Diagram);            
        }

        public void CloseAllTabs()
        {
            foreach (DocumentContent documentContent in DockManager.Documents.ToList())
            {
                documentContent.Close();
            }
            Current.ActiveDiagram = null;
        }

        public void BindToProjectVersion(ProjectVersion projectVersion)
        {
            if (projectVersion != null)
            {
                projectVersion.PIMDiagrams.CollectionChanged += Diagrams_CollectionChanged;
                projectVersion.PSMDiagrams.CollectionChanged += Diagrams_CollectionChanged;
            }
        }

        public void UnBindFromProjectVersion(ProjectVersion projectVersion)
        {
            //CloseAllTabs();
            if (projectVersion != null)
            {
                projectVersion.PIMDiagrams.CollectionChanged -= Diagrams_CollectionChanged;
                projectVersion.PSMDiagrams.CollectionChanged -= Diagrams_CollectionChanged;
            }
        }

        private void Diagrams_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action.IsAmong(NotifyCollectionChangedAction.Replace, NotifyCollectionChangedAction.Reset))
            {
                CloseAllTabs();
                OpenTabsForProjectVersion(Current.ProjectVersion);
            }
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (Diagram newItem in e.NewItems)
                {
                    AddTab(newItem);
                }
            }
            if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (Diagram oldItem in e.OldItems)
                {
                    DiagramTab diagramTab = FindTab(oldItem);
                    if (diagramTab != null)
                        RemoveTab(diagramTab);
                }
            }
        }


        public void OpenTabsForProjectVersion(ProjectVersion projectVersion)
        {
            foreach (PIMDiagram pimDiagram in projectVersion.PIMDiagrams)
            {
                if (FindTab(pimDiagram) == null)
                    AddTab(pimDiagram);
            }

            foreach (PSMDiagram psmDiagram in projectVersion.PSMDiagrams)
            {
				if (psmDiagram.Caption == "XRX PSM")
					continue;
                if (FindTab(psmDiagram) == null)
                    AddTab(psmDiagram);
            }

	        ActivateDiagram(projectVersion.PIMDiagrams.FirstOrDefault());
        }

        public IFilePresenterTab DisplayFile(XDocument xmlDocument, EDisplayedFileType fileType, string fileName = null, ILog log = null, PSMSchema validationSchema = null, PSMSchema sourcePSMSchema = null, FilePresenterButtonInfo[] additionalActions = null, object tag = null)
        {

            StringBuilder sb = new StringBuilder();
			if (xmlDocument != null)
			{
				using (TextWriter tw = new UTF8StringWriter(sb))
				{
					xmlDocument.Save(tw);
				}
			}
	        var f = DisplayFile(sb.ToString(), fileType, fileName, log, validationSchema, sourcePSMSchema, additionalActions, tag);
            return f;
        }

		public IFilePresenterTab DisplayFile(string fileContents, EDisplayedFileType fileType, string fileName = null, ILog log = null, PSMSchema validationSchema = null, PSMSchema sourcePSMSchema = null, FilePresenterButtonInfo[] additionalActions = null, object tag = null)
		{
			FileTab f = new FileTab();
			f.DisplayFile(fileType, fileContents, fileName, log, validationSchema, sourcePSMSchema);
			f.FloatingWindowSize = new Size(800, 600);
			if (additionalActions != null)
			{
				f.CreateAdditionalActionsButtons(additionalActions);
			}
			f.Show(DockManager, true);
			f.Tag = tag;
			return f;
		}

        public IList<DiagramView> GetTopDiagramViews()
        {
            List<DiagramView> result = new List<DiagramView>();
            foreach (DocumentContent d in MainWindow.dockManager.Documents)
            {
                if (d is DiagramTab && d.IsActiveInItsGroup())
                {
                    result.Add(((DiagramTab) d).DiagramView);
                }
            }
            return result;
        }

        public IList<DiagramView> GetOpenedDiagramViews()
        {
            List<DiagramView> result = new List<DiagramView>();
            foreach (DocumentContent d in MainWindow.dockManager.Documents.OfType<DiagramTab>())
            {
                result.Add(((DiagramTab) d).DiagramView);
            }
            return result;
        }

        public IEnumerable<ExolutioVersionedObject> AnotherOpenedVersions(ExolutioVersionedObject item)
        {
            List<ExolutioVersionedObject> result = null;
            if (item is Diagram)
            {
                foreach (DocumentContent d in MainWindow.dockManager.Documents)
                {
                    if (d is DiagramTab && d.IsActiveInItsGroup())
                    {
                        Diagram diagram = ((DiagramTab)d).DiagramView.Diagram;
                        if (item != diagram && diagram.Project.VersionManager.AreItemsLinked(item, diagram))
                        {
                            if (result == null)
                            {
                                result = new List<ExolutioVersionedObject>();
                            }
                            result.Add(diagram);
                        }
                    }
                }
            }
            if (result != null)
            {
                return result;
            }
            else
            {
                return new Diagram[0];
            }
        }
    }    
}