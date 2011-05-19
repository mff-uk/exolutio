using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using EvoX.Model.OCL.Types;
using EvoX.Model.PIM;

namespace EvoX.View
{
    /// <summary>
    /// Interaction logic for PIMModelTreeView.xaml
    /// </summary>
    public partial class PIMModelTreeView : UserControl
    {
        /// <summary>
        /// Actual project
        /// </summary>
        private ProjectVersion projectVersion = null;
        
        /// <summary>
        /// Constructor.
        /// </summary>
        public PIMModelTreeView()
        {
            InitializeComponent();

            //this.Icon = (ImageSource)FindResource("view_tree");
        }

        public void BindToProjectVersion(ProjectVersion projectVersion)
        {
            this.projectVersion = projectVersion;

            if (projectVersion != null)
            {
                modelClasses.ItemsSource = projectVersion.PIMSchema.PIMClasses;
                //nestedPackages.ItemsSource = model.NestedPackages;
                modelClasses.DataContext = projectVersion.PIMSchema;
            }
        }

        /// <summary>
        /// Destroys binding between this window and actual project
        /// </summary>
        public void UnBindFromProjectVersion(ProjectVersion proj)
        {
            projectVersion = null;

            modelClasses.ItemsSource = null;
            //nestedPackages.ItemsSource = null;
            modelClasses.DataContext = null;
            modelClasses.ContextMenu.DataContext = null;
        }
        
        /// <summary>
        /// Invokes selected class event with given <paramref name="aClass"/> as an argument
        /// <param name="aClass">Selected class</param>
        /// </summary>
        public void InvokeSelectedClass(Class aClass)
        {
            EventHandler<ClassEventArgs> temp = NavigatorSelectedClass;
            if (temp != null)
                temp(this, new ClassEventArgs(aClass));
        }   

        /// <summary>
        /// Finds the first visual ancestor of <paramref name="child"/> which is of type TreeViewItem.
        /// </summary>
        /// <param name="child">A child whose parent is to be found</param>
        /// <returns>Parent TreeViewItem</returns>
        private TreeViewItem FindParent(DependencyObject child)
        {
            if (child == null) return null;
            DependencyObject parent = VisualTreeHelper.GetParent(child);
            if (parent == null) return null;
            if (parent is TreeViewItem) return parent as TreeViewItem;
            else return FindParent(parent);
        }

        #region Context menu event handlers

        #region Package context menu

        private void OnPackageRemoveClick(object sender, RoutedEventArgs e)
        {
            //MenuItem m = sender as MenuItem;
            //if (m.DataContext is Package)
            //{
            //    RemovePackageMacroCommand removePackageCommand = (RemovePackageMacroCommand)RemovePackageMacroCommandFactory.Factory().Create(project.GetModelController());
            //    removePackageCommand.Set(m.DataContext as Package, ActiveDiagram.Diagram);
            //    removePackageCommand.Execute();
            //}
        }

        private void OnPackageEditClick(object sender, RoutedEventArgs e)
        {
            //if ((sender as MenuItem).DataContext is Package)
            //{
            //    Package package = (sender as MenuItem).DataContext as Package;
            //    var packageDialog = new Dialogs.PackageDialog(
            //        package, project.GetModelController());
            //    packageDialog.ShowDialog();
            //}
        }

        private void OnAddPackageClick(object sender, RoutedEventArgs e)
        {
            //MenuItem m = sender as MenuItem;
            //if (m.DataContext == null || m.DataContext is Package)
            //{
            //    NewModelPackageCommand newPackageCommand = (NewModelPackageCommand)NewModelPackageCommandFactory.Factory().Create(project.GetModelController());
            //    if (m.DataContext == null)
            //    {
            //        newPackageCommand.Package = model;
            //    }
            //    else
            //    {
            //        newPackageCommand.Package = m.DataContext as Package;
            //    }
            //    newPackageCommand.PackageName = NameSuggestor<Package>.SuggestUniqueName(newPackageCommand.Package.NestedPackages, "Package", package => package.Name);
            //    newPackageCommand.Execute();
            //    nestedPackages.DataContext = null;
            //}
        }

        private void OnAddClassClick(object sender, RoutedEventArgs e)
        {
            //MenuItem m = sender as MenuItem;
            //if (m.DataContext == null || m.DataContext is Package)
            //{
            //    NewModelClassCommand newClassCommand = (NewModelClassCommand)NewModelClassCommandFactory.Factory().Create(project.GetModelController());
            //    if (m.DataContext == null)
            //    {
            //        newClassCommand.Package = model;
            //    }
            //    else
            //    {
            //        newClassCommand.Package = m.DataContext as Package;
            //    }
            //    newClassCommand.Execute();
            //}
        }

        #endregion

        #region Class context menu

        private void OnClassEditClick(object sender, RoutedEventArgs e)
        {
            //if (((MenuItem)sender).DataContext is PIMClass)
            //{
            //    PIMClass aClass = ((MenuItem)sender).DataContext as PIMClass;
            //    var classDialog = new ClassDialog(
            //        new ClassController(aClass, new DiagramController(null, project.GetModelController())), project.GetModelController());
            //    classDialog.ShowDialog();
            //}
        }

        private void OnClassRemoveClick(object sender, RoutedEventArgs e)
        {
            //DeleteFromModelMacroCommand c = (DeleteFromModelMacroCommand)DeleteFromModelMacroCommandFactory.Factory().Create(project.GetModelController());
            //c.Set(((MenuItem)sender).DataContext as PIMClass, ActiveDiagram != null ? ActiveDiagram.Controller : null);
            //if (c.Commands.Count > 0) c.Execute();
        }

        private void OnAddAttributeClick(object sender, RoutedEventArgs e)
        {
            //MenuItem m = sender as MenuItem;
            //if (m.DataContext is Class)
            //{
            //    NewAttributeCommand newAttributeCommand = (NewAttributeCommand)NewAttributeCommandFactory.Factory().Create(project.GetModelController());
            //    newAttributeCommand.Owner = m.DataContext as Class;
            //    newAttributeCommand.Name = NameSuggestor<Property>.SuggestUniqueName((newAttributeCommand.Owner as IHasAttributes).Attributes, "Attribute", property => property.Name);
            //    newAttributeCommand.Execute();
            //}
        }

        #endregion

        #region Attribute context menu

        private void OnAttributeEditClick(object sender, RoutedEventArgs e)
        {
            //if ((sender as MenuItem).DataContext is Property)
            //{
            //    Property property = (sender as MenuItem).DataContext as Property;
            //    var propertyDialog = new AttributeDialog(
            //        property, new ClassController(property.Class as PIMClass, null), project.GetModelController());
            //    propertyDialog.ShowDialog();
            //}
        }

        private void OnAttributeRemoveClick(object sender, RoutedEventArgs e)
        {
            //MenuItem m = sender as MenuItem;
            //if (m.DataContext is Property)
            //{
            //    RemoveAttributeMacroCommand removeAttributeCommand = (RemoveAttributeMacroCommand)RemoveAttributeMacroCommandFactory.Factory().Create(project.GetModelController());
            //    removeAttributeCommand.Set(m.DataContext as Property);
            //    if (removeAttributeCommand.Commands.Count > 0) removeAttributeCommand.Execute();
            //}
        }
        #endregion

        #endregion

        #region Other internal event handlers

        /// <summary>
        /// Handles left button click on a class.
        /// Invokes event NavigatorSelectedClass.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnClassLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                InvokeSelectedClass((sender as TextBlock).DataContext as Class);
            }
        }

        /// <summary>
        /// Handles displaying of ClassDialog after double click on a class.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnClassDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //if (((Button)sender).DataContext is PIMClass)
            //{
            //    PIMClass aClass = ((Button)sender).DataContext as PIMClass;
            //    var classDialog = new ClassDialog(
            //        new ClassController(aClass, new DiagramController(null, project.GetModelController())), project.GetModelController());
            //    classDialog.ShowDialog();
            //    e.Handled = true;
            //}
        }

        /// <summary>
        /// Handles displaying of PackageDialog after double click on a package.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnPackageDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //if ((sender as TreeViewItem).DataContext is Package && e.OriginalSource is TextBlock && FindParent(e.OriginalSource as TextBlock).DataContext == (sender as TreeViewItem).DataContext)
            //{
            //    Package package = (sender as TreeViewItem).DataContext as Package;
            //    var packageDialog = new Dialogs.PackageDialog(
            //        package, project.GetModelController());
            //    packageDialog.ShowDialog();
            //}
            //e.Handled = true;
        }

        /// <summary>
        /// Handles click on an attribute.
        /// Selects this attribute in Navigator and invokes event NavigatorSelectedClass.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnAttributeClick(object sender, MouseButtonEventArgs e)
        {
            //InvokeSelectedClass(((sender as TreeViewItem).DataContext as Property).Class);
            //DeselectAll(navigatorTreeView);
            //(sender as TreeViewItem).IsSelected = true;
        }

        /// <summary>
        /// Handles displaying of AttributeDialog after double click on an attribute.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnAttributeDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //if ((sender as TreeViewItem).DataContext is Property)
            //{
            //    Property property = (sender as TreeViewItem).DataContext as Property;
            //    var propertyDialog = new AttributeDialog(
            //        property, new ClassController(property.Class as PIMClass, null), project.GetModelController());
            //    propertyDialog.ShowDialog();
            //}
        }

        /// <summary>
        /// Suppresses selecting of whole package in Navigator.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnPackageSelected(object sender, RoutedEventArgs e)
        {
            //if ((e.OriginalSource as TreeViewItem).DataContext is Package || ((e.OriginalSource as TreeViewItem).Header as string) == "Nested Packages")
            //    (e.OriginalSource as TreeViewItem).IsSelected = false;
        }

        #endregion

        #region Reaction to class selection on canvas - selecting that class in Navigator

        private void ExpandAndSelectClass(TreeViewItem parentContainer, Class selectedClass)
        {
            //parentContainer.IsExpanded = true;
            //if (parentContainer.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
            //{
            //    TreeViewItem currentContainer = parentContainer.ItemContainerGenerator.ContainerFromItem(selectedClass) as TreeViewItem;
            //    currentContainer.IsSelected = true;
            //}
            //else
            //{
            //    EventHandler eh = null;
            //    eh = new EventHandler(delegate
            //    {
            //        if (parentContainer.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
            //        {
            //            ExpandAndSelectClass(parentContainer, selectedClass);
            //            //remove the StatusChanged event handler since we just handled it (we only needed it once)
            //            parentContainer.ItemContainerGenerator.StatusChanged -= eh;
            //        }
            //    });
            //    parentContainer.ItemContainerGenerator.StatusChanged += eh;
            //}
        }

        //private void ExpandNestedPackages(TreeViewItem nestedPackagesItem, Class selectedClass, Collection<Package> packages, int index)
        //{
        //    nestedPackagesItem.IsExpanded = true;
        //    if (nestedPackagesItem.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
        //    {
        //        TreeViewItem currentContainer = nestedPackagesItem.ItemContainerGenerator.ContainerFromItem(packages[index - 1]) as TreeViewItem;
        //        ExpandPackage(currentContainer, selectedClass, packages, index - 1);
        //    }
        //    else
        //    {
        //        EventHandler eh = null;
        //        eh = new EventHandler(delegate
        //        {
        //            if (nestedPackagesItem.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
        //            {
        //                TreeViewItem currentContainer = nestedPackagesItem.ItemContainerGenerator.ContainerFromItem(packages[index - 1]) as TreeViewItem;
        //                //remove the StatusChanged event handler since we just handled it (we only needed it once)
        //                nestedPackagesItem.ItemContainerGenerator.StatusChanged -= eh;
        //                ExpandPackage(currentContainer, selectedClass, packages, index - 1);
        //            }
        //        });
        //        nestedPackagesItem.ItemContainerGenerator.StatusChanged += eh;
        //    }
        //}

        //private void ExpandPackage(TreeViewItem package, Class selectedClass, Collection<Package> packages, int index)
        //{
        //    package.IsExpanded = true;
        //    if (package.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
        //    {
        //        TreeViewItem currentContainer;
        //        if (index > 0)
        //        {
        //            currentContainer = GetNestedPackagesItem(package)[0];
        //            ExpandNestedPackages(currentContainer, selectedClass, packages, index);
        //        }
        //        else
        //        {
        //            currentContainer = GetPackageClassesItem(package)[0];
        //            ExpandAndSelectClass(currentContainer, selectedClass);
        //        }
        //    }
        //    else
        //    {
        //        EventHandler eh = null;
        //        eh = new EventHandler(delegate
        //        {
        //            if (package.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
        //            {
        //                TreeViewItem currentContainer;
        //                if (index > 0)
        //                {
        //                    currentContainer = GetNestedPackagesItem(package)[0];
        //                    package.ItemContainerGenerator.StatusChanged -= eh;
        //                    ExpandNestedPackages(currentContainer, selectedClass, packages, index);
        //                }
        //                else
        //                {
        //                    currentContainer = GetPackageClassesItem(package)[0];
        //                    package.ItemContainerGenerator.StatusChanged -= eh;
        //                    ExpandAndSelectClass(currentContainer, selectedClass);
        //                }
        //            }
        //        });
        //        package.ItemContainerGenerator.StatusChanged += eh;
        //    }
        //}

        private List<TreeViewItem> GetNestedPackagesItem(DependencyObject parent)
        {
            List<TreeViewItem> list = new List<TreeViewItem>();
            int childCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childCount; i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);
                if (child is TreeViewItem && ((child as TreeViewItem).Header as string) == "Nested Packages") list.Add(child as TreeViewItem);
                list.AddRange(GetNestedPackagesItem(child));
            }
            return list;
        }

        private List<TreeViewItem> GetPackageClassesItem(DependencyObject parent)
        {
            List<TreeViewItem> list = new List<TreeViewItem>();
            int childCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childCount; i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);
                if (child is TreeViewItem && ((child as TreeViewItem).Header as string) == "Package Classes") list.Add(child as TreeViewItem);
                list.AddRange(GetPackageClassesItem(child));
            }
            return list;
        }

        private void DeselectAll(DependencyObject parent)
        {
            int childCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childCount; i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);
                if (child is TreeViewItem) (child as TreeViewItem).IsSelected = false;
                DeselectAll(child);
            }
        }

        internal void SelectedItems_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            //SelectedItemsCollection selection = (SelectedItemsCollection)sender;

            //if (selection.Count == 1 && selection[0] is XCaseViewBase)
            //{
            //    DeselectAll(modelClasses);
            //    DeselectAll(nestedPackages);

            //    XCaseViewBase selectedXCaseClass = (XCaseViewBase)selection[0];

            //    if (selectedXCaseClass.ModelElement.VersionManager != null &&
            //        selectedXCaseClass.ModelElement.Version != project.Version)
            //    {
            //        BindToProject(selectedXCaseClass.ModelElement.VersionManager.VersionedProjects[selectedXCaseClass.ModelElement.Version]);
            //    }

            //    if (selectedXCaseClass.Controller != null && selectedXCaseClass.Controller.NamedElement is Class && !(selectedXCaseClass.Controller.NamedElement is AssociationClass))
            //    {
            //        Class selectedClass;
            //        if (selectedXCaseClass.Controller.NamedElement is PIMClass)
            //        {
            //            selectedClass = selectedXCaseClass.Controller.NamedElement as Class;
            //        }
            //        else
            //        {
            //            selectedClass = (selectedXCaseClass.Controller.NamedElement as PSMClass).RepresentedClass;
            //            if (selectedClass is AssociationClass) selectedClass = null;
            //        }
            //        if (selectedClass != null)
            //        {
            //            Package nestingPackage = selectedClass.Package;
            //            if (nestingPackage == model)
            //            {
            //                ExpandAndSelectClass(modelClasses, selectedClass);
            //            }
            //            else
            //            {
            //                Collection<Package> packagePath = new Collection<Package>();
            //                Package package = nestingPackage;
            //                while (package != null)
            //                {
            //                    packagePath.Add(package);
            //                    package = package.NestingPackage;
            //                }
                            
            //                ExpandNestedPackages(nestedPackages, selectedClass, packagePath, packagePath.Count - 1);
            //            }
            //        }
            //    }
            //}
        }

        #endregion

        /// <summary>
        /// Event invoked when a class is selected in Navigator.
        /// Used for selecting the same class on canvas and in Properties window.
        /// </summary>
        public event EventHandler<ClassEventArgs> NavigatorSelectedClass;
    }

    /// <summary>
    /// Event arguments containing a Class.
    /// Used by the event NavigatorSelectedClass.
    /// </summary>
    public class ClassEventArgs : EventArgs
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="aClass">Class contained in event arguments</param>
        public ClassEventArgs(Class aClass)
        {
            SelectedClass = aClass;
        }

        /// <summary>
        /// Class contained in event arguments
        /// </summary>
        public Class SelectedClass { get; private set; }
    }

}
