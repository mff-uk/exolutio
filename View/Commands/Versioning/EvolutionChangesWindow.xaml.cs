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
using Exolutio.ViewToolkit;

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

        public static bool? Show(DetectedChangesSet changes, IMainWindow MainWindow, PSMDiagram diagramOldVersion, PSMDiagram diagramNewVersion)
        {
            EvolutionChangesWindow evolutionChangesWindow = new EvolutionChangesWindow();
            evolutionChangesWindow.Changes = changes;
            evolutionChangesWindow.MainWindow = MainWindow;

            List<ChangeInstance> changesList = new List<ChangeInstance>();
            foreach (KeyValuePair<Type, List<ChangeInstance>> keyValuePair in changes)
            {
                changesList.AddRange(keyValuePair.Value);
            }

            evolutionChangesWindow.gridChanges.ItemsSource = changesList;
            evolutionChangesWindow.DiagramOldVersion = diagramOldVersion;
            evolutionChangesWindow.DiagramNewVersion = diagramNewVersion;
            evolutionChangesWindow.DiagramView = MainWindow.DiagramTabManager.GetOpenedDiagramView(diagramNewVersion);
            evolutionChangesWindow.DiagramViewOldVersion = MainWindow.DiagramTabManager.GetOpenedDiagramView(diagramOldVersion);
            evolutionChangesWindow.Topmost = true;
            evolutionChangesWindow.Show();
            return true;
        }

        protected DetectedChangesSet Changes { get; set; }

        protected PSMDiagram DiagramOldVersion { get; set; }

        protected PSMDiagram DiagramNewVersion { get; set; }

        private void gridChanges_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void GridChanges_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //XsltTestWindow.ShowDialog(Changes, DiagramOldVersion, DiagramNewVersion);
            DependencyObject source = (DependencyObject)e.OriginalSource;
            DataGridRow row = source.TryFindParent<DataGridRow>();

            //the user did not click on a row
            if (row == null) return;

            ChangeInstance change = (ChangeInstance)row.Item;

            if (DiagramView != null)
            {
                if (change.Component.ExistsInVersion(DiagramNewVersion.Version))
                {
                    MainWindow.FocusComponent(change.Component);
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
                    MainWindow.FocusComponent(change.Component.GetInVersion(DiagramOldVersion.Version));
                }
                else
                {
                    DiagramView.ClearSelection();
                }
            }

            e.Handled = true;
            this.Focus();
        }

        public DiagramView DiagramView { get; set; }
        public DiagramView DiagramViewOldVersion { get; set; }
    }
}
