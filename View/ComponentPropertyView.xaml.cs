using System;
using System.Collections.Generic;
using System.Globalization;
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
using Exolutio.Model;
using Exolutio.Model.PIM;
using Exolutio.Model.PSM;
using Exolutio.View.Commands;
using Exolutio.ViewToolkit.FormControls;

namespace Exolutio.View
{
    /// <summary>
    /// Interaction logic for ComponentPropertyView.xaml
    /// </summary>
    public partial class ComponentPropertyView : UserControl
    {
        public ComponentPropertyView()
        {
            InitializeComponent();

            Current.SelectionChanged += Current_SelectionChanged;
            Current.ExecutedCommand += Current_ExecutedCommand;
        }

        void Current_ExecutedCommand(Controller.Commands.CommandBase command, bool isPartOfMacro, Controller.Commands.CommandBase macroCommand, bool isUndo, bool isRedo)
        {
            UpdateViewForCurrentSelection();
        }

        void Current_SelectionChanged()
        {
            UpdateViewForCurrentSelection();
        }

        private void UpdateViewForCurrentSelection()
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
            DisplayAttributes(component as PIMClass);
            DisplayAttributes(component as PSMClass);
            DisplayPSMAssociation(component as PSMAssociation);
            DisplayInterpretedComponents(component as PIMComponent);
            DisplayPSMComponent(component as PSMComponent);
            DisplayStructuralRepresentatives(component as PSMClass);
        }

        private void DisplayPSMAssociation(PSMAssociation psmAssociation)
        {
            if (psmAssociation != null)
            {
                if (psmAssociation.InterpretedAssociationEnd != null)
                {
                    lInterpretedAssocEnd.Text = psmAssociation.InterpretedAssociationEnd.ToString();
                    lInterpretedAssocEnd.ToolTip = psmAssociation.InterpretedAssociationEnd.ToString();
                }
                else
                {
                    lInterpretedAssocEnd.ToolTip = string.Empty;
                    lInterpretedAssocEnd.Text = "(none)";
                }
                lInterpretedAssocEnd.DataContext = psmAssociation.Interpretation;
                spPSMAssociation.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                spPSMAssociation.DataContext = null;
                spPSMAssociation.Visibility = System.Windows.Visibility.Hidden;
            }
        }

        private void DisplayPSMComponent(PSMComponent psmComponent)
        {
            if (psmComponent != null)
            {
                if (psmComponent.Interpretation != null)
                {
                    if (!string.IsNullOrEmpty(psmComponent.Interpretation.Name))
                    {
                        lInterpreted.ToolTip = psmComponent.ToString();
                        lInterpreted.Text = psmComponent.Interpretation.Name;
                    }
                    else
                    {
                        lInterpreted.ToolTip = psmComponent.ToString();
                        lInterpreted.Text = psmComponent.ToString();
                    }
                }
                else
                {
                    lInterpreted.ToolTip = string.Empty;
                    lInterpreted.Text = "(none)";

                }
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

        private void DisplayAttributes(PSMClass psmClass)
        {
            if (psmClass != null)
            {
                lvPSMAttributes.ItemsSource = psmClass.PSMAttributes;
                spPSMAttributes.Visibility = System.Windows.Visibility.Visible;

                if (psmClass.IsStructuralRepresentative)
                {
                    lPSMAttributesSRInherited.Visibility = System.Windows.Visibility.Visible;
                    lvPSMAttributesSRInhereted.Visibility = System.Windows.Visibility.Visible;
                    lvPSMAttributesSRInhereted.ItemsSource = psmClass.GetPSMAttributesOfRepresentedClasses();
                }
                else
                {
                    lPSMAttributesSRInherited.Visibility = System.Windows.Visibility.Collapsed;
                    lvPSMAttributesSRInhereted.Visibility = System.Windows.Visibility.Collapsed;
                }

            }
            else
            {
                lvPSMAttributes.ItemsSource = null;
                spPSMAttributes.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        private void DisplayAttributes(PIMClass pimClass)
        {
            if (pimClass != null)
            {
                lvPIMAttributes.ItemsSource = pimClass.PIMAttributes;
                lvPIMAttributesInherited.ItemsSource = pimClass.GetInheritedAttributes();
                spPIMAttributes.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                lvPIMAttributes.ItemsSource = null;
                lvPIMAttributesInherited.ItemsSource = null;
                spPIMAttributes.Visibility = System.Windows.Visibility.Collapsed;
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
                lName.Visibility = System.Windows.Visibility.Visible;
                tbName.Text = component.Name;
            }
            else
            {
                tbName.Text = null;
                lName.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        private void FocusPSMComponent(object sender, MouseButtonEventArgs e)
        {
            FrameworkElement linkLabel = (FrameworkElement)sender;
            PSMComponent component = (PSMComponent)linkLabel.DataContext;
            if (component == null)
            {
                return;
            }
            Current.MainWindow.FocusComponent(component);
        }

        private void FocusPIMComponent(object sender, MouseButtonEventArgs e)
        {
            FrameworkElement linkLabel = (FrameworkElement)sender;
            PIMComponent component = (PIMComponent)linkLabel.DataContext;
            if (component == null)
            {
                return;
            }
            Current.MainWindow.FocusComponent(component);
        }
    }

    public class DerivedAttributeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && value is PIMAttribute)
                return ((PIMAttribute)value).GetInterpretedComponents();
            else
                return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class DerivedAttributesVisibleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && value is PIMAttribute)
                return ((PIMAttribute)value).GetInterpretedComponents().Count > 0 ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
            else
                return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
