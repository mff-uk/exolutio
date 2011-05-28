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
using Exolutio.Controller.Commands.Complex.PSM;
using Exolutio.Dialogs;
using Exolutio.Model.PIM;
using Exolutio.Model.PSM;
using Exolutio.Model;
using Exolutio.Controller;
using Exolutio.SupportingClasses;
using cmdDeletePSMAttribute = Exolutio.Controller.Commands.Atomic.PSM.MacroWrappers.cmdDeletePSMAttribute;

namespace Exolutio.View
{
    /// <summary>
    /// Interaction logic for PSMClassDialog.xaml
    /// </summary>
    public partial class PSMClassDialog
    {
        private bool dialogReady = false;

        public PSMClass psmClass { get; set; }

        private class FakePSMAttribute : IEditableObject
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

            public PSMAttribute SourceAttribute { get; set; }
            public PIMAttribute RepresentedAttribute { get; set; }

            public bool Checked { get; set; }

            public bool XFormElement { get; set; }

            public bool IsReadonlyType
            {
                get
                {
                    return RepresentedAttribute == null;
                }
            }

            public FakePSMAttribute()
            {
                Multiplicity = "1";
                XFormElement = true;
                Name = "Attribute";
            }

            public FakePSMAttribute(PSMAttribute p)
                : this()
            {
                Name = p.Name;
                Type = p.AttributeType;
                Multiplicity = p.GetCardinalityString();
                DefaultValue = p.DefaultValue;
                SourceAttribute = p;
                RepresentedAttribute = (PIMAttribute)p.Interpretation;
                if (RepresentedAttribute != null)
                    ComesFrom = RepresentedAttribute.PIMClass;
                Checked = true;
                XFormElement = p.Element;
            }

            public FakePSMAttribute(PIMAttribute p)
                : this()
            {
                Name = p.Name;
                Type = p.AttributeType;
                Multiplicity = p.GetCardinalityString();
                DefaultValue = p.DefaultValue;
                SourceAttribute = null;
                RepresentedAttribute = p;
                ComesFrom = (PIMClass)p.PIMClass;
                Checked = false;
                XFormElement = true;
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
                           || SourceAttribute.DefaultValue != DefaultValue || SourceAttribute.Interpretation != RepresentedAttribute
                           || SourceAttribute.Lower != lower || SourceAttribute.Upper != upper || XFormElement != SourceAttribute.Element;
                }
            }
        }

        private class FakeAttributeCollection : ListCollectionView
        {

            public FakeAttributeCollection(IList attributes)
                : base(attributes)
            {

            }

            public FakeAttributeCollection(ObservableCollection<FakePSMAttribute> attributesList, PSMClass psmClass)
                : base(attributesList)
            {
                foreach (PSMAttribute psmAttribute in psmClass.PSMAttributes)
                {
                    attributesList.Add(new FakePSMAttribute(psmAttribute));
                }

                bool classEmpty = psmClass.PSMAttributes.Count == 0;

                if (psmClass.Interpretation != null)
                {
                    foreach (PIMAttribute attribute in ((PIMClass)psmClass.Interpretation).PIMAttributes)
                    {
                        if (!attributesList.Any(p => p.SourceAttribute != null && p.SourceAttribute.Interpretation == attribute))
                        {
                            attributesList.Add(new FakePSMAttribute(attribute) { Checked = classEmpty });
                        }
                    }
                }
            }
        }

        private FakeAttributeCollection fakeAttributes;

        public PSMClassDialog()
        {
            InitializeComponent();
        }

        private Exolutio.Controller.Controller controller;

        public void Initialize(Exolutio.Controller.Controller controller, PSMClass psmClass)
        {
            this.controller = controller;
            this.psmClass = psmClass;

            this.Title = string.Format(this.Title, psmClass);

            tbName.Text = psmClass.Name;
            //tbElementLabel.Text = psmClass.ElementName;
            //cbAbstract.IsChecked = psmClass.IsAbstract;
            //cbAnyAttribute.IsChecked = psmClass.AllowAnyAttribute;

            typeColumn.ItemsSource = psmClass.ProjectVersion.AttributeTypes;
            if (psmClass.Interpretation != null)
            {
                CompositeCollection coll = new CompositeCollection();
                //coll.Add("(None)");
                coll.Add(new CollectionContainer { Collection = ((PIMClass)psmClass.Interpretation).PIMAttributes });
                interpretationColumn.ItemsSource = coll;
            }

            ObservableCollection<FakePSMAttribute> fakeAttributesList = new ObservableCollection<FakePSMAttribute>();

            fakeAttributes = new FakeAttributeCollection(fakeAttributesList, psmClass);
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
            //controller.CreatedMacro.Description = string.Format("PSM Classs '{0}' was updated. ", psmClass);
            if (tbName.ValueChanged)
            {
                acmdRenameComponent renameCommand = new acmdRenameComponent(controller, psmClass, tbName.Text);
                controller.CreatedMacro.Commands.Add(renameCommand);
                tbName.ForgetOldValue();
            }

            //if (psmClass.IsAbstract != cbAbstract.IsChecked)
            //{
            //    psmClassController.ChangeAbstract(cbAbstract.IsChecked == true);
            //}

            //if (psmClass.AllowAnyAttribute != cbAnyAttribute.IsChecked)
            //{
            //    psmClassController.ChangeAllowAnyAttributeDefinition(cbAnyAttribute.IsChecked == true);
            //}

            #region check for deleted attributes

            List<PSMAttribute> removedAttributes = new List<PSMAttribute>();
            List<FakePSMAttribute> addedAttributes = new List<FakePSMAttribute>();
            foreach (PSMAttribute psmAttribute in psmClass.PSMAttributes)
            {
                bool found = false;
                foreach (FakePSMAttribute fakeAttribute in fakeAttributes)
                {
                    if (fakeAttribute.SourceAttribute == psmAttribute && fakeAttribute.Checked)
                    {
                        found = true;
                        break;
                    }
                    else if (fakeAttribute.SourceAttribute == psmAttribute && !fakeAttribute.Checked)
                    {
                        fakeAttribute.SourceAttribute = null;
                    }
                }
                if (!found)
                {
                    removedAttributes.Add(psmAttribute);
                    cmdDeletePSMAttribute deleteCommand = new cmdDeletePSMAttribute(controller);
                    deleteCommand.Set(psmAttribute);
                    controller.CreatedMacro.Commands.Add(deleteCommand);
                }
            }

            #endregion

            #region remove dummy entries in fake collection

            List<FakePSMAttribute> toRemove = new List<FakePSMAttribute>();
            foreach (FakePSMAttribute fakeAttribute in fakeAttributes)
            {
                if (String.IsNullOrEmpty(fakeAttribute.Name))
                {
                    if (fakeAttribute.SourceAttribute != null)
                    {
                        removedAttributes.Add(fakeAttribute.SourceAttribute);
                        cmdDeletePSMAttribute deleteCommand = new cmdDeletePSMAttribute(controller);
                        deleteCommand.Set(fakeAttribute.SourceAttribute);
                        controller.CreatedMacro.Commands.Add(deleteCommand);
                    }
                    toRemove.Add(fakeAttribute);
                }
            }

            foreach (FakePSMAttribute attribute in toRemove)
            {
                fakeAttributes.Remove(attribute);
            }

            #endregion

            Dictionary<PSMAttribute, string> namesDict = new Dictionary<PSMAttribute, string>();
            foreach (PSMAttribute a in psmClass.PSMAttributes)
            {
                if (!removedAttributes.Contains(a))
                {
                    namesDict.Add(a, a.Name);
                }
            }

            // check for changes and new attributes
            var modified = from FakePSMAttribute a in fakeAttributes
                           where a.SourceAttribute != null && !removedAttributes.Contains(a.SourceAttribute) && a.SomethingChanged()
                           select a;
            var added = from FakePSMAttribute a in fakeAttributes where a.SourceAttribute == null select a;

            // editing exisiting attribute
            foreach (FakePSMAttribute modifiedAttribute in modified)
            {
                PSMAttribute sourceAttribute = modifiedAttribute.SourceAttribute;
                uint lower;
                UnlimitedInt upper;
                if (
                    !IHasCardinalityExt.ParseMultiplicityString(modifiedAttribute.Multiplicity, out lower,
                                                                           out upper))
                {
                    error = true;
                }
                cmdUpdatePSMAttribute updateCommand = new cmdUpdatePSMAttribute(controller);
                updateCommand.Set(sourceAttribute, modifiedAttribute.Type, modifiedAttribute.Name, lower, upper, modifiedAttribute.XFormElement, modifiedAttribute.DefaultValue);
                updateCommand.InterpretedAttribute = modifiedAttribute.RepresentedAttribute;
                controller.CreatedMacro.Commands.Add(updateCommand);
                namesDict[sourceAttribute] = modifiedAttribute.Name;
            }

            List<string> names = namesDict.Values.ToList();
            // new attribute
            foreach (FakePSMAttribute addedAttribute in added)
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
                    cmdCreateNewPSMAttribute createNewPsmAttribute = new cmdCreateNewPSMAttribute(controller);
                    createNewPsmAttribute.Set(psmClass, addedAttribute.Type, addedAttribute.Name, lower, upper, addedAttribute.XFormElement);
                    if (addedAttribute.RepresentedAttribute != null)
                    {
                        createNewPsmAttribute.InterpretedAttribute = addedAttribute.RepresentedAttribute;
                    }
                    controller.CreatedMacro.Commands.Add(createNewPsmAttribute);
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
                    foreach (FakePSMAttribute attribute in addedAttributes)
                    {
                        if (attribute.RepresentedAttribute != null)
                        {
                            attribute.SourceAttribute = (PSMAttribute)psmClass.PSMAttributes.Where
                                                                           (property =>
                                                                            ((PSMAttribute)property).
                                                                                Interpretation ==
                                                                            attribute.RepresentedAttribute).
                                                                           SingleOrDefault();
                        }
                        else
                        {
                            attribute.SourceAttribute = psmClass.PSMAttributes.Where
                                (property => property.Name == attribute.Name).SingleOrDefault();
                        }
                        //else
                        //{
                        //    attribute.SourceAttribute = (PSMAttribute)psmClassController.Class.AllAttributes.Where
                        //        (property => (property.RepresentedAttribute == attribute.RepresentedAttribute).SingleOrDefault();
                        //}
                        //if (attribute.SourceAttribute.RepresentedAttribute != null)
                        //    attribute.RepresentedAttribute = attribute.SourceAttribute.RepresentedAttribute;
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

            if (e.Row != null && e.Row.Item is FakePSMAttribute)
            {
                FakePSMAttribute editedAttribute = ((FakePSMAttribute)e.Row.Item);
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
            foreach (FakePSMAttribute fakeAttribute in fakeAttributes)
            {
                fakeAttribute.Checked = true;
            }
        }

        private void DeselectAllClick(object sender, RoutedEventArgs e)
        {
            foreach (FakePSMAttribute fakeAttribute in fakeAttributes)
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

                            if (row.Item is FakePSMAttribute)
                            {
                                cb.IsChecked = !cb.IsChecked.Value;
                                if (cell.Column.Header.ToString().Contains("Element"))
                                {
                                    ((FakePSMAttribute)row.Item).XFormElement = cb.IsChecked.Value;
                                }
                                else
                                {
                                    ((FakePSMAttribute)row.Item).Checked = cb.IsChecked.Value;
                                }
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

    }
}