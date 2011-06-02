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
using Exolutio.Controller.Commands.Atomic.PSM;
using Exolutio.Controller.Commands.Complex.PSM;
using Exolutio.Dialogs;
using Exolutio.Model.PIM;
using Exolutio.Model.PSM;
using Exolutio.Model;
using Exolutio.Controller;
using Exolutio.SupportingClasses;
using Exolutio.ViewToolkit;
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

            public Guid AddedAttributeID { get; set; }

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

            public override string ToString()
            {
                return !String.IsNullOrEmpty(Name) ? Name : "(unnamed attribute)";
            }
        }

        private class FakePSMAssociation : IEditableObject
        {
            public string Name { get; set; }
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
            
            public PSMAssociation SourceAssociation { get; set; }
            public PIMAssociation RepresentedAssociation { get; set; }

            public bool Checked { get; set; }

            public bool IsReadonlyType
            {
                get
                {
                    return RepresentedAssociation == null;
                }
            }

            public Guid AddedAssociationID { get; set; }

            public FakePSMAssociation()
            {
                Multiplicity = "1";
                Name = "Association";
            }

            public FakePSMAssociation(PSMAssociation p)
                : this()
            {
                Name = p.Name;
                Multiplicity = p.GetCardinalityString();
                SourceAssociation = p;
                RepresentedAssociation = (PIMAssociation)p.Interpretation;
                Checked = true;
            }

            public FakePSMAssociation(PIMAssociation p)
                : this()
            {
                Name = p.Name;                
                Multiplicity = "1";
                SourceAssociation = null;
                RepresentedAssociation = p;
                Checked = false;
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
                if (SourceAssociation == null)
                {
                    return true;
                }
                else
                {
                    uint lower;
                    UnlimitedInt upper;
                    IHasCardinalityExt.ParseMultiplicityString(Multiplicity, out lower, out upper);
                    return SourceAssociation.Name != Name 
                           || SourceAssociation.Interpretation != RepresentedAssociation
                           || SourceAssociation.Lower != lower || SourceAssociation.Upper != upper;
                }
            }

            public override string ToString()
            {
                return !string.IsNullOrEmpty(this.Name) ? this.Name : "(unnamed association)";
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

        private class FakeAssociationCollection : ListCollectionView
        {

            public FakeAssociationCollection(IList attributes)
                : base(attributes)
            {

            }

            public FakeAssociationCollection(ObservableCollection<FakePSMAssociation> associationList, PSMClass psmClass)
                : base(associationList)
            {
                foreach (PSMAssociation psmAssociation in psmClass.ChildPSMAssociations)
                {
                    associationList.Add(new FakePSMAssociation(psmAssociation));
                }

                bool classEmpty = psmClass.ChildPSMAssociations.Count == 0;

                if (psmClass.Interpretation != null)
                {
                    foreach (PIMAssociationEnd associationEnd in ((PIMClass)psmClass.Interpretation).PIMAssociationEnds)
                    {
                        PIMAssociation pimAssociation = associationEnd.PIMAssociation;
                        if (!associationList.Any(p => p.SourceAssociation != null &&
                            p.SourceAssociation.Interpretation == pimAssociation))
                        {
                            associationList.Add(new FakePSMAssociation(pimAssociation) { Checked = false });
                        }
                    }
                }
            }
        }

        private FakeAttributeCollection fakeAttributes;

        private FakeAssociationCollection fakeAssociations;

        public PSMClassDialog()
        {
            InitializeComponent();

            if (!Current.Project.UsesVersioning)
            {
                tabVersionLinks.IsEnabled = false;
                tabVersionLinks.ToolTip = "Disabled in projects not using versioning";
            }

            tabControl1.Items.CurrentChanging += TabControl_CurrentChanging;
        }

        private Controller.Controller controller;

        public void Initialize(Controller.Controller controller, PSMClass psmClass, PSMAttribute initialSelectedAttribute = null)
        {
            this.controller = controller;
            this.psmClass = psmClass;

            this.Title = string.Format("PSM class: {0}", psmClass);

            tbName.Text = psmClass.Name;

            #region attributes 

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

            #endregion
            
            #region associatiosn

            if (psmClass.Interpretation != null)
            {
                CompositeCollection coll = new CompositeCollection();
                //coll.Add("(None)");
                coll.Add(new CollectionContainer { Collection = ((PIMClass)psmClass.Interpretation).PIMAssociationEnds.Select(e => e.PIMAssociation) });
                interpretationAssociation.ItemsSource = coll;
            }

            ObservableCollection<FakePSMAssociation> fakeAssociationsList = new ObservableCollection<FakePSMAssociation>();

            fakeAssociations = new FakeAssociationCollection(fakeAssociationsList, psmClass);
            fakeAssociationsList.CollectionChanged += delegate { UpdateApplyEnabled(); };
            gridAssociations.ItemsSource = fakeAssociationsList;

            #endregion 

            if (initialSelectedAttribute != null)
            {
                gridAttributes.SelectedItem = fakeAttributesList.SingleOrDefault(fa => fa.SourceAttribute == initialSelectedAttribute);
            }

            dialogReady = true;
        }

        private void TabControl_CurrentChanging(object sender, CurrentChangingEventArgs e)
        {
            TabItem tab = (TabItem) ((ICollectionView)sender).CurrentItem;
            if (e.IsCancelable && Current.Project.UsesVersioning)
            {
                if (ExolutioYesNoBox.Show("Apply changes", "Click 'Yes' to apply changes. \nClick 'No' to discard changes made in this page.") == MessageBoxResult.Yes)
                {
                    if (tab.IsAmong(tabAttributes, tabGeneral))
                    {
                        ApplyChanges();
                    }    
                }
                else
                {
                    e.Cancel = true;
                    tabControl1.SelectedItem = tab;
                }
            }
        }

        private void bApply_Click(object sender, RoutedEventArgs e)
        {
            bApply.Focus();

            error = false;

            ApplyChanges();
            
        }

        private void bOk_Click(object sender, RoutedEventArgs e)
        {
            ApplyChanges();
            if (!error)
            {
                DialogResult = true;
                Close();
            }
        }

        private bool error = false;

        private void ApplyChanges()
        {
            controller.BeginMacro();

            bool canContinue = ApplyClassChanges();
            if (canContinue)
            {
                canContinue = ApplyAttributeChanges();
            }
            if (canContinue)
            {
                canContinue = ApplyAssociationChanges();
            }

            if (!canContinue)
            {
                controller.CancelMacro();
            }
            else
            {
                error = false;
                CommandBase tmp = (CommandBase)controller.CreatedMacro;
                controller.CommitMacro();
            }

            Initialize(controller, psmClass);
            gridAttributes.Items.Refresh();
            gridAssociations.Items.Refresh();
        }

        private bool ApplyClassChanges()
        {
            if (tbName.ValueChanged)
            {
                acmdRenameComponent renameCommand = new acmdRenameComponent(controller, psmClass, tbName.Text);
                controller.CreatedMacro.Commands.Add(renameCommand);
                tbName.ForgetOldValue();
            }
            return true; 
        }

        private bool ApplyAssociationChanges()
        {
            #region check for deleted associations

            foreach (PSMAssociation psmAssociation in psmClass.ChildPSMAssociations)
            {
                bool found = false;
                foreach (FakePSMAssociation fakeAssociation in fakeAssociations)
                {
                    if (fakeAssociation.SourceAssociation == psmAssociation && fakeAssociation.Checked)
                    {
                        found = true;
                        break;
                    }
                    else if (fakeAssociation.SourceAssociation == psmAssociation && !fakeAssociation.Checked)
                    {
                        fakeAssociation.SourceAssociation = null;
                    }
                }
                if (!found)
                {
                    MessageBoxResult result = ExolutioYesNoBox.Show("Cut or delete", string.Format("Click 'Yes' if you want to delete the association '{0}' with the whole subtree.\nClick 'No' if you want to delete the association and make the subtree a new tree. ", psmAssociation));
                    if (result == MessageBoxResult.No)
                    {
                        cmdDeletePSMAssociation deleteCommand = new cmdDeletePSMAssociation(controller);
                        deleteCommand.Set(psmAssociation);
                        controller.CreatedMacro.Commands.Add(deleteCommand);
                    }
                    else if (result == MessageBoxResult.Yes)
                    {
                        cmdDeletePSMAssociationRecursive deleteCommand = new cmdDeletePSMAssociationRecursive(controller);
                        deleteCommand.Set(psmAssociation);
                        controller.CreatedMacro.Commands.Add(deleteCommand);
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            #endregion

            // check for changes and new associations
            var modified = from FakePSMAssociation a in fakeAssociations
                           where a.SourceAssociation != null && a.SomethingChanged()
                           select a;
            var added = from FakePSMAssociation a in fakeAssociations where a.SourceAssociation == null select a;

            #region editing exisiting association
            foreach (FakePSMAssociation modifiedAssociation in modified)
            {
                PSMAssociation sourceAssociation = modifiedAssociation.SourceAssociation;
                uint lower;
                UnlimitedInt upper;
                if (
                    !IHasCardinalityExt.ParseMultiplicityString(modifiedAssociation.Multiplicity, out lower,
                                                                           out upper))
                {
                    error = true;
                }

                cmdUpdatePSMAssociation updateCommand = new cmdUpdatePSMAssociation(controller);
                updateCommand.Set(sourceAssociation, modifiedAssociation.Name, lower, upper);
                updateCommand.InterpretedAssociation = modifiedAssociation.RepresentedAssociation;
                controller.CreatedMacro.Commands.Add(updateCommand);
            }
            #endregion

            #region new association
            foreach (FakePSMAssociation addedAssociation in added)
            {
                if (!string.IsNullOrEmpty(addedAssociation.Name) && addedAssociation.Checked)
                {
                    uint lower = 1;
                    UnlimitedInt upper = 1;
                    if (!String.IsNullOrEmpty(addedAssociation.Multiplicity))
                    {
                        if (!IHasCardinalityExt.ParseMultiplicityString(addedAssociation.Multiplicity, out lower, out upper))
                        {
                            error = true;
                        }
                    }
                     
                    ExolutioMessageBox.Show("Creating new association", addedAssociation.Name, "");
                    //cmdCreateNewPSMAssociation createNewPsmAssociation = new cmdCreateNewPSMAssociation(controller);
                    //createNewPsmAssociation.Set(psmClass, addedAssociation.Name, lower, upper);
                    //if (addedAssociation.RepresentedAssociation != null)
                    //{
                    //    createNewPsmAssociation.InterpretedAssociation = addedAssociation.RepresentedAssociation;
                    //}
                    //controller.CreatedMacro.Commands.Add(createNewPsmAssociation);
                    
                }
            }
            #endregion

            #region ordering

            {
                List<Guid> ordering = new List<Guid>();
                foreach (FakePSMAssociation association in fakeAssociations)
                {
                    if (association.SourceAssociation != null)
                    {
                        ordering.Add(association.SourceAssociation.ID);
                    }
                    else if (association.AddedAssociationID != Guid.Empty)
                    {
                        ordering.Add(association.AddedAssociationID);
                    }
                }

                acmdReorderComponents<PSMAssociation> reorderCommand = new acmdReorderComponents<PSMAssociation>(controller);
                reorderCommand.ComponentGuids = ordering;
                reorderCommand.OwnerCollection = psmClass.ChildPSMAssociations;
                controller.CreatedMacro.Commands.Add(reorderCommand);
            }

            #endregion 

            return !error;
        }
        
        private bool ApplyAttributeChanges()
        {
            #region check for deleted attributes

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
                    cmdDeletePSMAttribute deleteCommand = new cmdDeletePSMAttribute(controller);
                    deleteCommand.Set(psmAttribute);
                    controller.CreatedMacro.Commands.Add(deleteCommand);
                }
            }

            #endregion

            // check for changes and new attributes
            var modified = from FakePSMAttribute a in fakeAttributes
                           where a.SourceAttribute != null && a.SomethingChanged()
                           select a;
            var added = from FakePSMAttribute a in fakeAttributes where a.SourceAttribute == null select a;

            #region editing exisiting attribute
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
            }
            #endregion 

            #region new attribute
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
                    createNewPsmAttribute.AttributeGuid = Guid.NewGuid();
                    addedAttribute.AddedAttributeID = createNewPsmAttribute.AttributeGuid;
                    if (addedAttribute.RepresentedAttribute != null)
                    {
                        createNewPsmAttribute.InterpretedAttribute = addedAttribute.RepresentedAttribute;
                    }
                    controller.CreatedMacro.Commands.Add(createNewPsmAttribute);
                }
            }
            #endregion

            #region ordering

            {
                List<Guid> ordering = new List<Guid>();
                foreach (FakePSMAttribute attribute in fakeAttributes)
                {
                    if (attribute.SourceAttribute != null)
                    {
                        ordering.Add(attribute.SourceAttribute.ID);
                    } 
                    else if (attribute.AddedAttributeID != Guid.Empty)
                    {
                        ordering.Add(attribute.AddedAttributeID);
                    }
                }

                acmdReorderComponents<PSMAttribute> reorderCommand = new acmdReorderComponents<PSMAttribute>(controller);
                reorderCommand.ComponentGuids = ordering;
                reorderCommand.OwnerCollection = psmClass.PSMAttributes;
                controller.CreatedMacro.Commands.Add(reorderCommand);
            }

            #endregion 

            return !error;
        }

        #region update apply enabled

        private void UpdateApplyEnabled()
        {
            int errors = Validation.GetErrors(gridAttributes).Count + Validation.GetErrors(gridAssociations).Count + Validation.GetErrors(gridVersionLinks).Count;

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

        private void tbName_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateApplyEnabled();
        }

        private void grid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            UpdateApplyEnabled();
        }

        private void grid_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            UpdateApplyEnabled();
        }

        #endregion
        
        private void gridAttributes_InitializingNewItem(object sender, InitializingNewItemEventArgs e)
        {
            ((FakePSMAttribute)e.NewItem).Checked = true;
        }

        private void gridAssociations_InitializingNewItem(object sender, InitializingNewItemEventArgs e)
        {
            ((FakePSMAssociation)e.NewItem).Checked = true;
        }

        #region SINGLE CLICK EDITING

        private void DataGridCell_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DataGridCell cell = (DataGridCell)sender;
            CheckBox cb = cell.Content as CheckBox;
            if (cb != null)
            {
                if (!cell.IsEditing)
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

                    DataGrid dataGrid = UIExtensions.FindVisualParent<DataGrid>(cell);
                    if (dataGrid != null)
                    {
                        if (dataGrid.SelectionUnit != DataGridSelectionUnit.FullRow)
                        {
                            if (!cell.IsSelected)
                                cell.IsSelected = true;
                        }
                        else
                        {
                            DataGridRow row = UIExtensions.FindVisualParent<DataGridRow>(cell);

                            if (row != null && !row.IsSelected)
                            {
                                row.IsSelected = true;
                            }

                            if (row != null)
                            {
                                cb.IsChecked = cb.IsChecked != false ? false : true;
                                
                                if (row.Item is FakePSMAttribute)
                                {
                                    if (cell.Column.Header.ToString().Contains("Element"))
                                    {
                                        ((FakePSMAttribute) row.Item).XFormElement = cb.IsChecked.Value;
                                    }
                                    else
                                    {
                                        ((FakePSMAttribute) row.Item).Checked = cb.IsChecked.Value;
                                    }
                                }
                                else if (row.Item is FakePSMAssociation)
                                {
                                    ((FakePSMAssociation)row.Item).Checked = cb.IsChecked.Value;
                                }


                                UpdateApplyEnabled();
                            }

                        }
                        dataGrid.SelectedItem = null;
                    }

                }

            }
        }

        #endregion
    }
}