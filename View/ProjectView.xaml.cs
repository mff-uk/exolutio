//#define bindAllVersions

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using Exolutio.Controller;
using Exolutio.Model;
using Exolutio.Controller.Commands;
using System.Globalization;
using Exolutio.Model.Versioning;
using Exolutio.ViewToolkit;
using Version=Exolutio.Model.Versioning.Version;
using Exolutio.View.Commands;

namespace Exolutio.View
{
    /// <summary>
    /// Interaction logic for ProjectsWindow.xaml
    /// </summary>
    public partial class ProjectView
    {
        public IMainWindow MainWindow { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public ProjectView()
        {
            InitializeComponent();

            //this.Icon = (ImageSource)FindResource("view_remove");
        }
        
        private static void DeselectAll(DependencyObject parent)
        {
            int childCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childCount; i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);
                if (child is TreeViewItem) (child as TreeViewItem).IsSelected = false;
                DeselectAll(child);
            }
        }

        public void BindToProject(Project project)
        {
            project.PropertyChanged += Project_PropertyChanged;
            if (!project.UsesVersioning)
            {
                projectView.ItemTemplate = (DataTemplate)projectView.FindResource("singleVersionProjectTemplate");
                projectView.ItemsSource = new[] {project};
            }
            else
            {
                projectView.ItemsSource = new[] { project };
                projectView.ItemTemplate = (DataTemplate)projectView.FindResource("versionedProjectTemplate");
            }
        }

        public void UnbindFromProject()
        {
            if (projectView.ItemsSource is Project)
            {
                (projectView.ItemsSource as Project).PropertyChanged -= Project_PropertyChanged;
            }
            projectView.ItemsSource = null;
            projectView.ItemTemplate = null;
        }

        private void Project_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "UsesVersioning")
            {
                UnbindFromProject();
                BindToProject((Project) sender);
            }
        }

        #region Event handlers

        // MenuItem m = sender as MenuItem;
        //     if (m.DataContext is Diagram)

        private void OnMemberDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is TreeViewItem)
            {
                object dataContext = ((TreeViewItem) sender).DataContext;
                if (dataContext is Diagram)
                {
                    Current.MainWindow.DiagramTabManager.ActivateDiagram((Diagram)dataContext);
                }
            }
        }

        private void OnMemberClick(object sender, MouseButtonEventArgs e)
        {
            TreeViewItem item = null;
            DependencyObject x = (DependencyObject) e.OriginalSource;
            while (x != null)
            {
                if (x is Visual)
                {
                    x = VisualTreeHelper.GetParent(x);
                }
                else if (x is FrameworkContentElement)
                {
                    x = ((FrameworkContentElement) x).Parent;
                }
                if (x is TreeViewItem)
                {
                    item = (TreeViewItem) x;
                    break;
                }
            }

            if (item == sender)
            {
                DeselectAll(projectView);
                (sender as TreeViewItem).IsSelected = true;
            }
        }

        private void OnItemSelected(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is Project)
            {
                (projectView.ItemContainerGenerator.ContainerFromItem(e.NewValue) as TreeViewItem).IsSelected = false;
            }
        }

        private void OnProjectChangeNamespaceClick(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException("Member ProjectView.OnProjectChangeNamespaceClick not implemented.");
        }

        private void OnChangeDiagramTargetNamespaceClick(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException("Member ProjectView.OnChangeDiagramTargetNamespaceClick not implemented.");
        }

        #endregion

        private void ExolutioContextMenu_ContextMenuOpening(object sender, RoutedEventArgs routedEventArgs)
        {
            foreach (ContextMenuItem contextMenuItem in ((ExolutioContextMenu)sender).Items)
            {
                contextMenuItem.ScopeObject = ((ExolutioContextMenu) sender).DataContext;
                if (contextMenuItem.Command is guiControllerCommand)
                {
                    ((guiControllerCommand) contextMenuItem.Command).ScopeObject = contextMenuItem.ScopeObject;
                }
            }
        }
    }

    /// <summary>
    /// Convertor for displaying project namespace only when a namespace is defined
    /// </summary>
    [ValueConversion(typeof(string), typeof(Visibility))]
    public class NamespaceVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Visibility v = value == null || (value as string).Length == 0 ? Visibility.Collapsed : Visibility.Visible;
            return v;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
