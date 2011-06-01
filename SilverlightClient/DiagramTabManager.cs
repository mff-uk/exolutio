using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Xml.Linq;
using System.Windows.Controls;
using Exolutio.Model;
using Exolutio.Model.PIM;
using Exolutio.Model.PSM;
using SilverlightClient;
using Exolutio.View;

namespace SilverlightClient
{
    public class DiagramTabManager : IFilePresenter, IDiagramTabManager
    {
        public MainPage MainWindow { get; private set; }

        public DockManager DockManager { get { return MainWindow.DockManager; } }

        public DiagramView ActiveDiagramView
        {
            get
            {
                return DockManager.ActiveDocument != null ? ((DiagramTab)DockManager.ActiveDocument).DiagramView : null;
            }
        }

        public Diagram ActiveDiagram
        {
            get
            {
                return ActiveDiagramView != null ? ActiveDiagramView.Diagram : null;
            }
        }

        public DiagramTabManager(MainPage mainWindow)
        {
            MainWindow = mainWindow;

            DockManager.SelectionChanged += DockManager_ActiveTabChanged;
            Current.ActiveDiagramChanged += Current_ActiveDiagramChanged;
        }

        void Current_ActiveDiagramChanged()
        {
            ActivateDiagram(Current.ActiveDiagram);
            if (MainWindow.ExolutioRibbon.PIMMode != Current.ActiveDiagram is PIMDiagram)
            {
                MainWindow.ExolutioRibbon.PIMMode = Current.ActiveDiagram is PIMDiagram;
            }
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
            foreach (DiagramTab tab in DockManager.Documents)
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
                newTab.Title = diagram.Caption;
                newTab.DiagramView.LoadDiagram(diagram);
                newTab.DiagramView.SelectionChanged += SelectedItems_CollectionChanged;
                DockManager.Items.Add(newTab);
                DockManager.UpdateLayout();
                //ActivePane.BringHeaderToFront(newTab);
                DockManager.ActiveDocument = newTab;
            }

            return newTab;
        }

        public void CloseActiveTab()
        {
            DockManager.Items.Remove(DockManager.ActiveDocument);
        }


        internal void RemoveTab(DiagramTab tab)
        {
            DockManager.Items.Remove(tab);
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
                tab.DiagramView.SetSelection(selectedComponent);
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
            foreach (TabItem tab in DockManager.Items.ToList())
            {
                DockManager.Items.Remove(tab);
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
            foreach (PIMDiagram pimDiagram in Current.Project.LatestVersion.PIMDiagrams)
            {
                if (FindTab(pimDiagram) == null)
                    AddTab(pimDiagram);
            }

            foreach (PSMDiagram psmDiagram in Current.Project.LatestVersion.PSMDiagrams)
            {
                if (FindTab(psmDiagram) == null)
                    AddTab(psmDiagram);
            }
        }

        
        public void DisplaySampleFile(XDocument xmlDocument)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ExolutioVersionedObject> AnotherOpenedVersions(ExolutioVersionedObject item)
        {
            List<ExolutioVersionedObject> result = null;
            if (item is Diagram)
            {
                foreach (TabItem d in this.DockManager.Items)
                {
                    if (d is DiagramTab)
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