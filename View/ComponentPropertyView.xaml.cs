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
using System.Windows.Navigation;
using System.Windows.Shapes;
using EvoX.Model;
using EvoX.Model.PIM;
using EvoX.Model.PSM;
using EvoX.View.Commands;
using EvoX.ViewToolkit.FormControls;

namespace EvoX.View
{
    /// <summary>
    /// Interaction logic for ComponentPropertyView.xaml
    /// </summary>
    public partial class ComponentPropertyView : UserControl
    {


        public ComponentPropertyView()
        {
            InitializeComponent();

            Current.SelectionChanged += new Action(Current_SelectionChanged);
        }

        void Current_SelectionChanged()
        {
            if (Current.ActiveDiagramView != null)
            {
                Component component = Current.ActiveDiagramView.GetSingleSelectedComponentOrNull();
                DisplayComponent(component);
            }
            else
            {
                DisplayComponent(null);
            }
        }

        public void DisplayComponent(Component component)
        {
            DisplayNamedComponent(component);
            DisplayInterpretedComponents(component as PIMComponent);
            DisplayPSMComponent(component as PSMComponent);
            DisplayStructuralRepresentatives(component as PSMClass);
        }

        private void DisplayPSMComponent(PSMComponent psmComponent)
        {
            if (psmComponent != null)
            {
                lInterpreted.Content = psmComponent.Interpretation != null ? psmComponent.Interpretation.Name : "(none)";
                lInterpreted.DataContext = psmComponent.Interpretation;
                spPSMComponent.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                spPSMComponent.DataContext = null;
                spPSMComponent.Visibility = System.Windows.Visibility.Hidden;
            }
        }

        private void DisplayStructuralRepresentatives(PSMClass psmClass)
        {
            if (psmClass != null)
            {
                spRepresentatives.Visibility = System.Windows.Visibility.Visible;
                lvRepresentatives.ItemsSource = psmClass.Representants;
                lRepresentedPSMClass.DataContext = psmClass.RepresentedClass;
            }
            else
            {
                spRepresentatives.Visibility = System.Windows.Visibility.Collapsed;
                lvRepresentatives.ItemsSource = null;
                lRepresentedPSMClass.DataContext = null;
            }
        }

        private void DisplayInterpretedComponents(PIMComponent pimComponent)
        {
            if (pimComponent != null)
            {
                lvDerivedComponents.ItemsSource = pimComponent.GetInterpretedComponents();
                spDerivedComponents.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                lvDerivedComponents.ItemsSource = null;
                spDerivedComponents.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        private void DisplayNamedComponent(Component component)
        {
            if (component != null)
            {
                tbName.Text = component.Name;
            }
            else
            {
                tbName.Text = null;
            }
        }

        private void FocusPSMComponent(object sender, MouseButtonEventArgs e)
        {
            FrameworkElement linkLabel = (FrameworkElement)sender;
            PSMComponent component = (PSMComponent)linkLabel.DataContext;
            Current.MainWindow.FocusComponent(component.PSMSchema.PSMDiagram, component);
        }

        private void FocusPIMComponent(object sender, MouseButtonEventArgs e)
        {
            FrameworkElement linkLabel = (FrameworkElement)sender;
            PIMComponent component = (PIMComponent)linkLabel.DataContext;
            IEnumerable<PIMDiagram> pimDiagrams = Current.ProjectVersion.PIMDiagrams.Where(d => d.PIMComponents.Contains(component));
            Current.MainWindow.FocusComponent(pimDiagrams, component);
        }
    }
}
