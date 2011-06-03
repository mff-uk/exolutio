using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Xml.Linq;
using AvalonDock;
using Exolutio.Model;
using Exolutio.Model.PIM;
using Exolutio.Model.PSM;
using Exolutio.SupportingClasses;
using Exolutio.View;

namespace Exolutio.WPFClient
{
    public partial class DiagramTabManager : IFilePresenter, IDiagramTabManager
    {
        public MainWindow MainWindow { get; private set; }

        public DockingManager DockManager { get { return MainWindow.dockManager; } }
        
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
            Current.ActiveDiagramChanged += Current_ActiveDiagramChanged;
        }

        void Current_ActiveDiagramChanged()
        {
            if (ActiveDiagram is PIMDiagram)
            {
                MainWindow.ExolutioRibbon.pimGroup.Visibility = System.Windows.Visibility.Visible;
                MainWindow.ExolutioRibbon.psmGroup.Visibility = System.Windows.Visibility.Collapsed;
                MainWindow.ExolutioRibbon.tabPIM.Focus();
            }
            else if (ActiveDiagram is PSMDiagram)
            {
                MainWindow.ExolutioRibbon.pimGroup.Visibility = System.Windows.Visibility.Collapsed;
                MainWindow.ExolutioRibbon.psmGroup.Visibility = System.Windows.Visibility.Visible;
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
        public DiagramTab ActivateDiagram(Diagram diagram)
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
                tab.BringDocumentHeaderToView(false);
                return tab;
            }
            else
            {
                return DockManager.ActiveDocument as DiagramTab;
            }
        }

        void IDiagramTabManager.ActivateDiagram(Diagram diagram)
        {
            ActivateDiagram(diagram);
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
            foreach (DiagramTab tab in MainWindow.dockManager.Documents)
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
                newTab.DiagramView.LoadDiagram(diagram);
                newTab.DiagramView.SelectionChanged += SelectedItems_CollectionChanged;
                ActivePane.Items.Add(newTab);
                ActivePane.UpdateLayout();
                //ActivePane.BringHeaderToFront(newTab);
                if (newTab.ContainerPane != null)
                {
                    DockManager.ActiveDocument = newTab;
                }
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
                    (DockManager.ActiveDocument as DocumentContent).Close();
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
        /// <param name="diagram"></param>
        /// <param name="selectedComponent"></param>
        public void ActivateDiagramWithElement(Diagram diagram, Component selectedComponent)
        {
            DiagramTab tab = ActivateDiagram(diagram);
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
                    Current.ActiveDiagram = null;
                }
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
                if (FindTab(psmDiagram) == null)
                    AddTab(psmDiagram);
            }
        }

        public void DisplaySampleFile(XDocument xmlDocument)
        {
            FileTab f = new FileTab();
            f.DisplayFile(EDisplayedFileType.XML, xmlDocument.ToString());
            f.Title = ActiveDiagram.Caption + "_sample.xml";

            f.Show(DockManager, true);
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

    public static class AvalonDockExtension
    {
        public static bool IsActiveInItsGroup(this ManagedContent managedContent)
        {
            return managedContent.ContainerPane.SelectedItem == managedContent;
        }
    }
}