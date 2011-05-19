using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Xml.Linq;
using System.Windows.Controls;
using EvoX.Model;
using EvoX.Model.PIM;
using EvoX.Model.PSM;
using SilverlightClient;
using EvoX.View;

namespace SilverlightClient
{
    public class DiagramTabManager : IFilePresenter, IDiagramTabManager
    {
        public MainPage MainWindow
        {
            get;
            private set;
        }

        public DockManager DockManager
        {
            get { return MainWindow.DockManager; }
        }

        public DiagramTabManager(MainPage mainWindow)
        {
            MainWindow = mainWindow;

            DockManager.SelectionChanged += DockManager_ActiveTabChanged;
        }

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

        //public DocumentPane ActivePane
        //{
        //    get
        //    {
        //        if (dockManager.ActiveDocument != null && dockManager.ActiveDocument.Parent != null
        //            && dockManager.ActiveDocument.Parent is DocumentPane)
        //        {
        //            return (DocumentPane)dockManager.ActiveDocument.Parent;
        //        }
        //        else
        //        {
        //            return dockManager.MainDocumentPane;
        //        }
        //    }
        //}

        //public DiagramView ActiveDiagramView { get { return ((DiagramTab)dockManager.ActiveDocument).DiagramView; } }

        //public Diagram ActiveDiagram
        //{
        //    get
        //    {
        //        return ActiveDiagramView.Diagram;
        //    }
        //}

        /// <summary>
        /// Activates a diagram
        /// </summary>
        /// <param name="diagram">Diagram to be activated</param>
        public DiagramTab ActivateDiagram(Diagram diagram)
        {
            DiagramTab Tab = FindTab(diagram);
            if (Tab == null)
            {
                Tab = AddTab(diagram);
                //MainWindow.propertiesWindow.BindDiagram(ref MainWindow.dockManager);
            }
            else
            {
                DockManager.ActiveDocument = Tab;
            }

            Tab.BringDocumentHeaderToView(false);
            return Tab;
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

        private void SelectedItems_CollectionChanged()
        {
            Current.InvokeSelectionChanged();
        }


        //public void GoToPimClass(PIMClass pimClass)
        //{
        //    //ElementDiagramDependencies dependentDiagrams = ElementDiagramDependencies.FindElementDiagramDependencies(MainWindow.CurrentProject, new[] { pimClass }, null);

        //    //if (dependentDiagrams.Count == 1)
        //    //{
        //    //    if (dependentDiagrams[pimClass].Count == 1)
        //    //    {
        //    //        ActivateDiagramWithElement(dependentDiagrams[pimClass][0], pimClass);
        //    //    }
        //    //    else
        //    //    {
        //    //        SelectItemsDialog d = new SelectItemsDialog();
        //    //        d.ToStringAction = diagram => ((Diagram)diagram).Caption;
        //    //        d.SetItems(dependentDiagrams[pimClass]);
        //    //        if (d.ShowDialog() == true)
        //    //        {
        //    //            foreach (Diagram diagram in d.selectedObjects.Cast<Diagram>())
        //    //            {
        //    //                ActivateDiagramWithElement(diagram, pimClass);
        //    //            }
        //    //        }
        //    //    }
        //    //}
        //    //else if (dependentDiagrams.Count == 0)
        //    //{
        //    //    XCaseYesNoBox.ShowOK("PIM class not used in diagrams", "PIM class is not used in any diagram. You can edit it via Navigator window. ");
        //    //}
        //}

        /// <summary>
        /// Activates given diagram and selects given element on it
        /// </summary>
        /// <param name="diagram"></param>
        /// <param name="selectedComponent"></param>
        public void ActivateDiagramWithElement(Diagram diagram, Component selectedComponent)
        {
            DiagramTab tab = ActivateDiagram(diagram);
            
            if (selectedComponent != null)
            {
                tab.DiagramView.SetSelection(selectedComponent);
            }
            else
            {
                tab.DiagramView.ClearSelection(true);
            }

            MainWindow.OnActiveDiagramChanged(tab.DiagramView);
        }

        ///// <summary>
        ///// Finds among currently open PanelWindows the one which is associated with given <paramref name="diag"/>
        ///// <param name="diag">Diagram whose tab has to be found</param>
        ///// </summary>
        //private DiagramTab FindTab(Diagram diag)
        //{
        //    foreach (DiagramTab tab in MainWindow.dockManager.Documents)
        //    {
        //        if (tab.DiagramView.Diagram == diag)
        //        {
        //            return tab;
        //        }
        //    }
        //    return null;
        //}

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

        ///// <summary>
        ///// Closes active PanelWindow
        ///// </summary>
        //internal void RemoveActiveTab()
        //{
        //    if (dockManager.ActiveDocument != null)
        //    {
        //        int index = dockManager.MainDocumentPane.Items.IndexOf(dockManager.ActiveDocument);
        //        DiagramTab pw = dockManager.ActiveDocument as DiagramTab;
        //        if (pw != null)
        //            RemoveTab(pw);
        //        else
        //        {
        //            (dockManager.ActiveDocument as DocumentContent).Close();
        //        }
        //    }
        //}

        ///// <summary>
        ///// Closes given PanelWindow
        ///// </summary>
        ///// <param name="tab">PanelWindow to be closed</param>
        internal void RemoveTab(DiagramTab tab)
        {
            DockManager.Items.Remove(tab);
            //if (dockManager.ActiveDocument == null && dockManager.Documents.Count() > 0)
            //{
            //    dockManager.ActiveDocument = dockManager.Documents.Last();
            //}
            //InvokeActiveDiagramChanged(dockManager.ActiveDocument as PanelWindow);
        }

        ///// <summary>
        ///// Handles double click on a diagram in Projects window - activates/reopens the tab with selected diagram
        ///// </summary>
        ////internal void DiagramDoubleClick(object sender, DiagramDClickArgs arg)
        ////{
        ////    ActivateDiagram(arg.Diagram);
        ////}

        ///// <summary>
        ///// Handles diagram removing event invoked by Projects window
        ///// </summary>
        ////internal void DiagramRemoveHandler(object sender, DiagramDClickArgs arg)
        ////{
        ////    if (arg.Diagram is PIMDiagram)
        ////    {
        ////        RemoveDiagramCommand removeDiagramCommand = (RemoveDiagramCommand)RemoveDiagramCommandFactory.Factory().Create(arg.Diagram.Project.GetModelController());
        ////        removeDiagramCommand.Set(arg.Diagram.Project, arg.Diagram);
        ////        removeDiagramCommand.Execute();
        ////    }
        ////    else if (arg.Diagram is PSMDiagram)
        ////    {
        ////        DiagramTab Tab = FindTab(arg.Diagram);
        ////        if (Tab != null)
        ////        {
        ////            RemovePSMDiagramMacroCommand c = (RemovePSMDiagramMacroCommand)RemovePSMDiagramMacroCommandFactory.Factory().Create(arg.Diagram.Project.GetModelController());
        ////            c.Set(arg.Diagram.Project, arg.Diagram as PSMDiagram, Tab.xCaseDrawComponent.Canvas.Controller);
        ////            if (c.Commands.Count > 0) c.Execute();
        ////        }
        ////        else
        ////        {
        ////            RemovePSMDiagramMacroCommand c = (RemovePSMDiagramMacroCommand)RemovePSMDiagramMacroCommandFactory.Factory().Create(MainWindow.CurrentProject.GetModelController());
        ////            c.Set(arg.Diagram.Project, arg.Diagram as PSMDiagram, new DiagramController(arg.Diagram as PSMDiagram, MainWindow.CurrentProject.GetModelController()));
        ////            if (c.Commands.Count > 0) c.Execute();
        ////        }

        ////    }
        ////    else throw new NotImplementedException("Unknown diagram type");
        ////}

        ///// <summary>
        ///// Handles diagram renaming event invoked by Projects window
        ///// </summary>
        ////internal void DiagramRenameHandler(object sender, DiagramRenameArgs arg)
        ////{
        ////    DiagramTab tab = FindTab(arg.Diagram);
        ////    tab.RenameDiagram(arg.NewCaption);
        ////}

        /// <summary>
        /// Reaction on change of active document.
        /// Invokes ActiveDiagramChanged event.
        /// </summary>
        private void DockManager_ActiveTabChanged(object sender, EventArgs eventArgs)
        {
            if (DockManager.ActiveDocument != null)
            {
                MainWindow.OnActiveDiagramChanged(DockManager.ActiveDocument.DiagramView);
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
            //MainWindow.propertiesWindow.BindDiagram(ref MainWindow.dockManager);
        }

        void IDiagramTabManager.ActivateDiagram(Diagram diagram)
        {
            this.ActivateDiagram(diagram);
        }

        public void CloseActiveTab()
        {
            DockManager.Items.Remove(DockManager.ActiveDocument);
        }

        public void CloseAllTabs()
        {
            foreach (TabItem tab in DockManager.Items.ToList())
            {
                DockManager.Items.Remove(tab);
            }
        }

        public void BindToCurrentProject()
        {
            Current.Project.LatestVersion.PIMDiagrams.CollectionChanged += Diagrams_CollectionChanged;
            Current.Project.LatestVersion.PSMDiagrams.CollectionChanged += Diagrams_CollectionChanged;
        }

        private void Diagrams_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action.IsAmong(NotifyCollectionChangedAction.Replace, NotifyCollectionChangedAction.Reset))
            {
                CloseAllTabs();
                OpenTabsForCurrentProject();
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


        public void OpenTabsForCurrentProject()
        {
            foreach (PIMDiagram pimDiagram in Current.Project.LatestVersion.PIMDiagrams)
            {
                AddTab(pimDiagram);
            }

            foreach (PSMDiagram psmDiagram in Current.Project.LatestVersion.PSMDiagrams)
            {
                AddTab(psmDiagram);
            }
        }

        
        public void DisplaySampleFile(XDocument xmlDocument)
        {
            throw new NotImplementedException();
        }
    }


}