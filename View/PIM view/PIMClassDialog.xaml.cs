using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Linq;
using Exolutio.Controller.Commands;
using Exolutio.Controller.Commands.Atomic;
using Exolutio.Controller.Commands.Complex.PIM;
using Exolutio.Dialogs;
using Exolutio.Model.PIM;
using Exolutio.Model;
using Exolutio.Controller;
using Exolutio.SupportingClasses;
using cmdDeletePIMAttribute = Exolutio.Controller.Commands.Atomic.PIM.MacroWrappers.cmdDeletePIMAttribute;

namespace Exolutio.View
{
    /// <summary>
    /// Interaction logic for PIMClassDialog.xaml
    /// </summary>
    public partial class PIMClassDialog
    {
        private bool dialogReady = false;

        public PIMClass PIMClass { get; set; }

        private class FakePIMAttribute : IEditableObject
        {
            public string Name { get; set; }
            public AttributeType Type { get; set; }
            private string multiplicity;
            public string Multiplicity
            {
                get { return multiplicity; }
                set
                {
                    bool isValid;
                    try
                    {
                        isValid = IHasCardinalityExt.IsMultiplicityStringValid(value);
                    }
                    catch
                    {
                        isValid = false;
                    }
                    if (!isValid)
                        throw new ArgumentException("Multiplicity string is invalid. ");
                    multiplicity = value;
                }
            }
            public PIMClass ComesFrom { get; set; }

            public string DefaultValue { get; set; }

            public PIMAttribute SourceAttribute { get; set; }
            
            public bool Checked { get; set; }

            public FakePIMAttribute()
            {
                Multiplicity = "1";
                Name = "Attribute";
            }

            public FakePIMAttribute(PIMAttribute p)
                : this()
            {
                Name = p.Name;
                Type = p.AttributeType;
                Multiplicity = p.GetCardinalityString();
                DefaultValue = p.DefaultValue;
                SourceAttribute = p;
                Checked = true;
            }


            public void BeginEdit()
            {
                //Checked = true;
            }

            public void EndEdit()
            {

            }

            public void CancelEdit()
            {

            }

            public bool SomethingChanged()
            {
                if (SourceAttribute == null)
                {
                    return true;
                }
                else
                {
                    uint lower;
                    UnlimitedInt upper;
                    IHasCardinalityExt.ParseMultiplicityString(Multiplicity, out lower, out upper);
                    return SourceAttribute.Name != Name || SourceAttribute.AttributeType != Type
                           || SourceAttribute.DefaultValue != DefaultValue 
                           || SourceAttribute.Lower != lower || SourceAttribute.Upper != upper;
                }
            }
        }

        private class FakeAttributeCollection : ListCollectionView
        {

            public FakeAttributeCollection(IList attributes)
                : base(attributes)
            {

            }

            public FakeAttributeCollection(ObservableCollection<FakePIMAttribute> attributesList, PIMClass PIMClass)
                : base(attributesList)
            {
                foreach (PIMAttribute PIMAttribute in PIMClass.PIMAttributes)
                {
                    attributesList.Add(new FakePIMAttribute(PIMAttribute));
                }
            }
        }

        private FakeAttributeCollection fakeAttributes;

        public PIMClassDialog()
        {
            InitializeComponent();
        }

        private Exolutio.Controller.Controller controller;

        public void Initialize(Exolutio.Controller.Controller controller, PIMClass PIMClass)
        {
            this.controller = controller;
            this.PIMClass = PIMClass;

            this.Title = string.Format(this.Title, PIMClass);

            tbName.Text = PIMClass.Name;
            //tbElementLabel.Text = PIMClass.ElementName;
            //cbAbstract.IsChecked = PIMClass.IsAbstract;
            //cbAnyAttribute.IsChecked = PIMClass.AllowAnyAttribute;

            typeColumn.ItemsSource = PIMClass.ProjectVersion.AttributeTypes;
            
            ObservableCollection<FakePIMAttribute> fakeAttributesList = new ObservableCollection<FakePIMAttribute>();

            fakeAttributes = new FakeAttributeCollection(fakeAttributesList, PIMClass);
            fakeAttributesList.CollectionChanged += delegate { UpdateApplyEnabled(); };
            gridAttributes.ItemsSource = fakeAttributesList;

            dialogReady = true;
        }

        private void bOk_Click(object sender, RoutedEventArgs e)
        {
            bApply_Click(sender, e);
            if (!error)
            {
                DialogResult = true;
                Close();
            }
        }

        private bool error = false;

        private void bApply_Click(object sender, RoutedEventArgs e)
        {
            bApply.Focus();

            error = false;

            controller.BeginMacro();
            //controller.CreatedMacro.Description = string.Format("PIM Classs '{0}' was updated. ", PIMClass);
            if (tbName.ValueChanged)
            {
                acmdRenameComponent renameCommand = new acmdRenameComponent(controller, PIMClass, tbName.Text);
                controller.CreatedMacro.Commands.Add(renameCommand);
                tbName.ForgetOldValue();
            }

            //if (PIMClass.IsAbstract != cbAbstract.IsChecked)
            //{
            //    PIMClassController.ChangeAbstract(cbAbstract.IsChecked == true);
            //}

            //if (PIMClass.AllowAnyAttribute != cbAnyAttribute.IsChecked)
            //{
            //    PIMClassController.ChangeAllowAnyAttributeDefinition(cbAnyAttribute.IsChecked == true);
            //}

            #region check for deleted attributes

            List<PIMAttribute> removedAttributes = new List<PIMAttribute>();
            List<FakePIMAttribute> addedAttributes = new List<FakePIMAttribute>();
            foreach (PIMAttribute PIMAttribute in PIMClass.PIMAttributes)
            {
                bool found = false;
                foreach (FakePIMAttribute fakeAttribute in fakeAttributes)
                {
                    if (fakeAttribute.SourceAttribute == PIMAttribute && fakeAttribute.Checked)
                    {
                        found = true;
                        break;
                    }
                    else if (fakeAttribute.SourceAttribute == PIMAttribute && !fakeAttribute.Checked)
                    {
                        fakeAttribute.SourceAttribute = null;
                    }
                }
                if (!found)
                {
                    removedAttributes.Add(PIMAttribute);
                    cmdDeletePIMAttribute deleteCommand = new cmdDeletePIMAttribute(controller);
                    deleteCommand.Set(PIMAttribute);
                    controller.CreatedMacro.Commands.Add(deleteCommand);
                }
            }

            #endregion

            #region remove dummy entries in fake collection

            List<FakePIMAttribute> toRemove = new List<FakePIMAttribute>();
            foreach (FakePIMAttribute fakeAttribute in fakeAttributes)
            {
                if (String.IsNullOrEmpty(fakeAttribute.Name))
                {
                    if (fakeAttribute.SourceAttribute != null)
                    {
                        removedAttributes.Add(fakeAttribute.SourceAttribute);
                        cmdDeletePIMAttribute deleteCommand = new cmdDeletePIMAttribute(controller);
                        deleteCommand.Set(fakeAttribute.SourceAttribute);
                        controller.CreatedMacro.Commands.Add(deleteCommand);
                    }
                    toRemove.Add(fakeAttribute);
                }
            }

            foreach (FakePIMAttribute attribute in toRemove)
            {
                fakeAttributes.Remove(attribute);
            }

            #endregion

            Dictionary<PIMAttribute, string> namesDict = new Dictionary<PIMAttribute, string>();
            foreach (PIMAttribute a in PIMClass.PIMAttributes)
            {
                if (!removedAttributes.Contains(a))
                {
                    namesDict.Add(a, a.Name);
                }
            }

            // check for changes and new attributes
            var modified = from FakePIMAttribute a in fakeAttributes
                           where a.SourceAttribute != null && !removedAttributes.Contains(a.SourceAttribute) && a.SomethingChanged()
                           select a;
            var added = from FakePIMAttribute a in fakeAttributes where a.SourceAttribute == null select a;

            // editing exisiting attribute
            foreach (FakePIMAttribute modifiedAttribute in modified)
            {
                PIMAttribute sourceAttribute = modifiedAttribute.SourceAttribute;
                uint lower;
                UnlimitedInt upper;
                if (
                    !IHasCardinalityExt.ParseMultiplicityString(modifiedAttribute.Multiplicity, out lower,
                                                                           out upper))
                {
                    error = true;
                }
                cmdUpdatePIMAttribute updateCommand = new cmdUpdatePIMAttribute(controller);
                updateCommand.Set(sourceAttribute, modifiedAttribute.Type, modifiedAttribute.Name, lower, upper, modifiedAttribute.DefaultValue);
                controller.CreatedMacro.Commands.Add(updateCommand);
                namesDict[sourceAttribute] = modifiedAttribute.Name;
            }

            List<string> names = namesDict.Values.ToList();
            // new attribute
            foreach (FakePIMAttribute addedAttribute in added)
            {
                if (!string.IsNullOrEmpty(addedAttribute.Name) && addedAttribute.Checked)
                {
                    uint lower = 1;
                    UnlimitedInt upper = 1;
                    if (!String.IsNullOrEmpty(addedAttribute.Multiplicity))
                    {
                        if (!IHasCardinalityExt.ParseMultiplicityString(addedAttribute.Multiplicity, out lower, out upper))
                        {
                            error = true;
                        }
                    }
                    cmdCreateNewPIMAttribute createNewPIMAttribute = new cmdCreateNewPIMAttribute(controller);
                    createNewPIMAttribute.Set(PIMClass, addedAttribute.Type, addedAttribute.Name, lower, upper, addedAttribute.DefaultValue);
                    controller.CreatedMacro.Commands.Add(createNewPIMAttribute);
                    addedAttributes.Add(addedAttribute);
                    names.Add(addedAttribute.Name);
                }
            }

            if (error)
            {
                controller.CancelMacro();
            }
            else
            {
                CommandBase tmp = (CommandBase)controller.CreatedMacro;
                controller.CommitMacro();
                if (string.IsNullOrEmpty(tmp.ErrorDescription))
                {
                    foreach (FakePIMAttribute attribute in addedAttributes)
                    {
                        attribute.SourceAttribute = PIMClass.PIMAttributes.Where
                            (property => property.Name == attribute.Name).SingleOrDefault();
                    }
                    addedAttributes.RemoveAll(attribute => attribute.SourceAttribute == null);
                    bApply.IsEnabled = false;
                    dialogReady = true;
                    error = false;
                }
                else
                {
                    error = true;
                }
            }
            gridAttributes.Items.Refresh();
        }

        private void tbName_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            UpdateApplyEnabled();
        }

        private void gridAttributes_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            UpdateApplyEnabled();
        }

        private void gridAttributes_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            UpdateApplyEnabled();
        }

        private void UpdateApplyEnabled()
        {
            int errors = System.Windows.Controls.Validation.GetErrors(gridAttributes).Count;

            if (dialogReady && errors == 0)
            {
                bApply.IsEnabled = true;
                bOk.IsEnabled = true;
            }

            if (errors > 0)
            {
                bApply.IsEnabled = false;
                bOk.IsEnabled = false;
            }
        }

        private void tbElementLabel_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            UpdateApplyEnabled();
        }

        private void cbAbstract_Checked(object sender, RoutedEventArgs e)
        {
            UpdateApplyEnabled();
        }

        private void gridAttributes_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            //if (e.Column == checkedColumn)
            //    return;

            if (e.Row != null && e.Row.Item is FakePIMAttribute)
            {
                FakePIMAttribute editedAttribute = ((FakePIMAttribute)e.Row.Item);
                if (!editedAttribute.Checked)
                {
                }

                //if (e.Column == typeColumn && editedAttribute.RepresentedAttribute != null)
                //{
                //    ErrorMsgBox.Show("Type can be changed only for PIM-less attributes. ", "You can change the represented attribute's type instead. ");
                //}
            }

        }

        private void SelectAllClick(object sender, RoutedEventArgs e)
        {
            foreach (FakePIMAttribute fakeAttribute in fakeAttributes)
            {
                fakeAttribute.Checked = true;
            }
        }

        private void DeselectAllClick(object sender, RoutedEventArgs e)
        {
            foreach (FakePIMAttribute fakeAttribute in fakeAttributes)
            {
                fakeAttribute.Checked = false;
            }
        }

        #region SINGLE CLICK EDITING

        private void DataGridCell_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DataGridCell cell = sender as DataGridCell;
            System.Windows.Controls.CheckBox cb = cell.Content as System.Windows.Controls.CheckBox;
            if (cb != null)
            {

                if (cell != null &&
                    !cell.IsEditing)
                {
                    try
                    {
                        if (!cell.IsFocused)
                        {
                            cell.Focus();
                        }
                    }
                    catch (Exception)
                    {

                    }

                    DataGrid dataGrid = FindVisualParent<DataGrid>(cell);
                    if (dataGrid != null)
                    {
                        if (dataGrid.SelectionUnit != DataGridSelectionUnit.FullRow)
                        {
                            if (!cell.IsSelected)
                                cell.IsSelected = true;
                        }
                        else
                        {
                            DataGridRow row = FindVisualParent<DataGridRow>(cell);

                            if (row != null && !row.IsSelected)
                            {
                                row.IsSelected = true;
                            }

                            if (row.Item is FakePIMAttribute)
                            {
                                cb.IsChecked = !cb.IsChecked.Value;
                                ((FakePIMAttribute) row.Item).Checked = cb.IsChecked.Value;
                                UpdateApplyEnabled();
                            }
                        }
                        dataGrid.SelectedItem = null;
                    }

                }

            }
        }


        static T FindVisualParent<T>(UIElement element) where T : UIElement
        {
            UIElement parent = element;
            while (parent != null)
            {
                T correctlyTyped = parent as T;
                if (correctlyTyped != null)
                {
                    return correctlyTyped;
                }

                parent = VisualTreeHelper.GetParent(parent) as UIElement;
            }
            return null;
        }

        #endregion

        private void gridAttributes_InitializingNewItem(object sender, InitializingNewItemEventArgs e)
        {
            ((FakePIMAttribute) e.NewItem).Checked = true;
        }

    }
}