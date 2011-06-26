using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Exolutio.Model;
using Exolutio.Model.PSM;
using Exolutio.Model.Versioning;
using Exolutio.Revalidation;
using Exolutio.Revalidation.Changes;
using Exolutio.Revalidation.XSLT;
using Exolutio.ViewToolkit;
using Exolutio.SupportingClasses;

namespace Exolutio.View
{
    /// <summary>
    /// Interaction logic for EvolutionChangesWindow.xaml
    /// </summary>
    public partial class EvolutionChangesWindow : Window
    {
        private EvolutionChangesWindow()
        {
            InitializeComponent();
        }

        public IMainWindow MainWindow { get; private set; }

        public static bool? Show(DetectedChangeInstancesSet changeInstances, IMainWindow MainWindow, PSMDiagram diagramOldVersion, PSMDiagram diagramNewVersion)
        {
            EvolutionChangesWindow evolutionChangesWindow = new EvolutionChangesWindow();
            evolutionChangesWindow.ChangeInstances = changeInstances;
            evolutionChangesWindow.MainWindow = MainWindow;

            List<ChangeInstance> changesList = new List<ChangeInstance>();
            foreach (KeyValuePair<Type, List<ChangeInstance>> keyValuePair in changeInstances)
            {
                changesList.AddRange(keyValuePair.Value);
            }

            evolutionChangesWindow.gridChanges.ItemsSource = changesList;
            evolutionChangesWindow.DiagramOldVersion = diagramOldVersion;
            evolutionChangesWindow.DiagramNewVersion = diagramNewVersion;
            evolutionChangesWindow.DiagramView = MainWindow.DiagramTabManager.GetOpenedDiagramView(diagramNewVersion);
            evolutionChangesWindow.DiagramViewOldVersion = MainWindow.DiagramTabManager.GetOpenedDiagramView(diagramOldVersion);
            evolutionChangesWindow.Topmost = true;

            evolutionChangesWindow.lRed.Content = changeInstances.RedNodes.ConcatWithSeparator(", ");
            evolutionChangesWindow.lBlue.Content = changeInstances.BlueNodes.ConcatWithSeparator(", ");
            evolutionChangesWindow.lGreen.Content = changeInstances.GreenNodes.ConcatWithSeparator(", ");
            
            evolutionChangesWindow.Show();
            return true;
        }

        protected DetectedChangeInstancesSet ChangeInstances { get; set; }

        protected PSMDiagram DiagramOldVersion { get; set; }

        protected PSMDiagram DiagramNewVersion { get; set; }

        private void gridChanges_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (gridChanges.SelectedItem != null)
            {
                MakeHighlightsForChangeInstance((ChangeInstance) gridChanges.SelectedItem);
            }
        }

        private void MakeHighlightsForChangeInstance(ChangeInstance change)
        {
            if (DiagramNewVersion != null)
            {
                if (change.Component.ExistsInVersion(DiagramNewVersion.Version))
                {
                    MainWindow.FocusComponent(change.Component, false);
                }
                else
                {
                    DiagramView.ClearSelection();
                }
            }

            if (DiagramViewOldVersion != null)
            {
                if (change.Component.ExistsInVersion(DiagramOldVersion.Version))
                {
                    MainWindow.FocusComponent(change.Component.GetInVersion(DiagramOldVersion.Version), false);
                }
                else
                {
                    DiagramViewOldVersion.ClearSelection();
                }
            }
        }

        public DiagramView DiagramView { get; set; }
        public DiagramView DiagramViewOldVersion { get; set; }
    }
}
