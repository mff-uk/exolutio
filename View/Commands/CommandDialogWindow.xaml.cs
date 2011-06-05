using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Exolutio.Model;

namespace Exolutio.View.Commands
{
    /// <summary>
    /// Interaction logic for CommandDialogWindow.xaml
    /// </summary>
    public partial class CommandDialogWindow : Window
    {
        public CommandDialogWindow()
        {
            InitializeComponent();

            Loaded += CommandDialogWindow_Loaded;
            LostFocus += CommandDialogWindow_LostFocus;
            GotFocus += CommandDialogWindow_GotFocus;
            Current.SelectionChanged += Current_SelectionChanged;
            Current.ComponentTouched += Current_ComponentTouched;
        }

        private Control lastFocused; 

        private void CommandDialogWindow_GotFocus(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is Control)
            {
                lastFocused = (Control)e.OriginalSource;
            }
        }

        void CommandDialogWindow_LostFocus(object sender, RoutedEventArgs e)
        {
                
        }

        void Current_ComponentTouched(Component component)
        {
            if (component != null)
            {
                TrySetSelectedComponent(component);
            }
        }

        void Current_SelectionChanged()
        {
            Component component = Current.ActiveDiagramView.GetSingleSelectedComponentOrNull();
            if (component != null)
            {
                TrySetSelectedComponent(component);
            }
        }

        private void TrySetSelectedComponent(Component component)
        {
            FrameworkElement findFocusedElement = ViewToolkit.WPFHelpers.FindFocusedElement(spParameters);
            if (findFocusedElement == null || findFocusedElement is Exolutio.View.Commands.ParameterControls.ScopePropertyEditor)
            {
                findFocusedElement = lastFocused;
            }
            if (findFocusedElement is Selector)
            {
                Selector selector = (Selector) findFocusedElement;
                foreach (var item in selector.Items)
                {
                    if (item is ComboBoxItem)
                    {
                        if (((ComboBoxItem)item).Tag.ToString() == component.ID.ToString())
                        {
                            selector.SelectedItem = item;
                        }
                    }
                }
            }
        }

#if SILVERLIGHT
#else
        public new bool? DialogResult { get; set; }
#endif

        void CommandDialogWindow_Loaded(object sender, RoutedEventArgs e)
        {
            
            foreach (Control child in spParameters.Children)
            {
                if (!(child is Label) && child.Visibility == Visibility.Visible)
                {    
                    if (child is Selector && ((Selector)child).Items.Count <= 1)
                    {
                        continue;
                    }
                    if (child.Tag != null && child.Tag.Equals("valueSuggested"))
                    {
                        continue;
                    }
                    child.Focus();
                    break;
                }
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            Current.SelectionChanged -= Current_SelectionChanged;
            Current.ComponentTouched -= Current_ComponentTouched;
            Current.MainWindow.EnableCommands();
            base.OnClosed(e);
        }

        private void bOK_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void bCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false; 
            Close();
        }
    }
}
