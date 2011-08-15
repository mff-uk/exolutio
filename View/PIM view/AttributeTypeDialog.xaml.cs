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
//using Exolutio.Controller.Commands.Atomic.MacroWrappers;
using Exolutio.Controller.Commands.Complex.PIM;
using Exolutio.Controller.Commands.Reflection;
using Exolutio.Dialogs;
using Exolutio.Model.PIM;
using Exolutio.Model;
using Exolutio.Controller;
using Exolutio.Model.PSM;
using Exolutio.SupportingClasses;
using cmdDeletePIMAttribute = Exolutio.Controller.Commands.Atomic.PIM.MacroWrappers.cmdDeletePIMAttribute;
using Exolutio.Controller.Commands.Atomic.MacroWrappers;

namespace Exolutio.View
{
    /// <summary>
    /// Interaction logic for AttributeTypeDialog.xaml
    /// </summary>
    public partial class AttributeTypeDialog
    {
        private bool dialogReady = false;

        private class FakeAttributeType : IEditableObject
        {
            public bool Checked { get; set; }

            public bool IsSealed { get; set; }
            
            public string Name { get; set; }

            public AttributeType BaseType { get; set; }

            public string XSDDefinition { get; set; }

            public AttributeType SourceAttributeType { get; set; }

            public FakeAttributeType()
            {
                Name = "Type";
            }

            public FakeAttributeType(AttributeType p)
                : this()
            {
                Name = p.Name;
                BaseType = p.BaseType;
                XSDDefinition = p.XSDDefinition;
                SourceAttributeType = p;
                Checked = true;
                IsSealed = p.IsSealed;
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
                if (SourceAttributeType == null)
                {
                    return true;
                }
                else
                {
                    return SourceAttributeType.Name != Name
                           || SourceAttributeType.BaseType != BaseType
                           || SourceAttributeType.XSDDefinition != XSDDefinition;
                }
            }
        }

        private class FakeAttributeTypeCollection : ListCollectionView
        {
            public FakeAttributeTypeCollection(IList attributes)
                : base(attributes)
            {

            }

            public FakeAttributeTypeCollection(ObservableCollection<FakeAttributeType> fakeAttributesList, IEnumerable<AttributeType> existingTypes) 
                : base(fakeAttributesList)
            {
                foreach (AttributeType attributeType in existingTypes)
                {
                    fakeAttributesList.Add(new FakeAttributeType(attributeType));
                }
            }
        }

        private FakeAttributeTypeCollection fakePIMAttributeTypes;
        private FakeAttributeTypeCollection fakePSMAttributeTypes;

        public AttributeTypeDialog()
        {
            InitializeComponent();
        }

        private Controller.Controller controller;
        private ProjectVersion projectVersion;

        public void Initialize(Exolutio.Controller.Controller controller, ProjectVersion projectVersion, PSMSchema psmSchema)
        {
            this.controller = controller;
            this.projectVersion = projectVersion;

            this.cbPSMSchema.ItemsSource = projectVersion.PSMSchemas;
            if (psmSchema != null)
                this.cbPSMSchema.SelectedItem = psmSchema;

            IEnumerable<AttributeType> existingPIMTypes = this.projectVersion.GetAvailablePIMTypes();
            baseTypeColumnPIM.ItemsSource = existingPIMTypes;
            ObservableCollection<FakeAttributeType> fakePIMAttributeTypesList = new ObservableCollection<FakeAttributeType>();
            fakePIMAttributeTypes = new FakeAttributeTypeCollection(fakePIMAttributeTypesList, existingPIMTypes);
            fakePIMAttributeTypesList.CollectionChanged += delegate { UpdateApplyEnabled(); };
            gridPIMAttributeTypes.ItemsSource = fakePIMAttributeTypesList;

            if (psmSchema != null)
            {
                PSMSchema = psmSchema;
                IEnumerable<AttributeType> existingPSMTypes = PSMSchema.GetAvailablePSMTypes();
                baseTypeColumnPSM.ItemsSource = existingPSMTypes;
                ObservableCollection<FakeAttributeType> fakePSMAttributeTypesList =
                    new ObservableCollection<FakeAttributeType>();
                fakePSMAttributeTypes = new FakeAttributeTypeCollection(fakePSMAttributeTypesList, existingPSMTypes);
                fakePSMAttributeTypesList.CollectionChanged += delegate { UpdateApplyEnabled(); };
                gridPSMAttributeTypes.ItemsSource = fakePSMAttributeTypesList;
            }
            else
            {
                PSMSchema = null;
            }

            dialogReady = true;
            bApply.IsEnabled = false;
        }

        private PSMSchema PSMSchema { get; set; }

        private PSMSchema SelectedPSMSchema
        {
            get { return 
                cbPSMSchema.SelectedItem != null ? (((PSMSchema)cbPSMSchema.SelectedItem)) : null; }
        }

        private void bOk_Click(object sender, RoutedEventArgs e)
        {
            bApply_Click(sender, e);
            if (!error)
            {
                Close();
            }
        }

        private bool error = false;

        private void bApply_Click(object sender, RoutedEventArgs e)
        {
            bApply.Focus();

            error = false;

            controller.BeginMacro();

            var addedPIMAttributes = Process(projectVersion.GetAvailablePIMTypes(), projectVersion, PSMSchema, controller, fakePIMAttributeTypes);
            List<FakeAttributeType> addedPSMAttributes;
            if (fakePSMAttributeTypes != null)
            {
                addedPSMAttributes = Process(PSMSchema.GetAvailablePSMTypes(), projectVersion, PSMSchema, controller, fakePSMAttributeTypes);
            }
            else
            {
                addedPSMAttributes = null;
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
                    {
                        foreach (FakeAttributeType attribute in addedPIMAttributes)
                        {
                            attribute.SourceAttributeType = projectVersion.GetAvailablePIMTypes().Where
                                (property => property.Name == attribute.Name).SingleOrDefault();
                        }
                        addedPIMAttributes.RemoveAll(attribute => attribute.SourceAttributeType == null);
                    }

                    if (addedPSMAttributes != null) {
                        foreach (FakeAttributeType attribute in addedPSMAttributes)
                        {
                            attribute.SourceAttributeType = PSMSchema.GetAvailablePSMTypes().Where
                                (property => property.Name == attribute.Name).SingleOrDefault();
                        }
                        addedPIMAttributes.RemoveAll(attribute => attribute.SourceAttributeType == null);
                    }

                    bApply.IsEnabled = false;
                    dialogReady = true;
                    error = false;
                }
                else
                {
                    error = true;
                }
            }
            gridPIMAttributeTypes.Items.Refresh();
        }

        private static List<FakeAttributeType> Process(IEnumerable<AttributeType> startingSet, 
            ProjectVersion projectVersion, PSMSchema psmSchema, Controller.Controller controller,
            FakeAttributeTypeCollection fakeSet) 
        {
            #region check for deleted attributes

            List<AttributeType> removedAttributeTypes = new List<AttributeType>();
            List<FakeAttributeType> addedAttributes = new List<FakeAttributeType>();
            foreach (AttributeType pimAttributeType in startingSet)
            {
                bool found = false;
                foreach (FakeAttributeType fakeAttribute in fakeSet)
                {
                    if (fakeAttribute.SourceAttributeType == pimAttributeType && fakeAttribute.Checked)
                    {
                        found = true;
                        break;
                    }
                    else if (fakeAttribute.SourceAttributeType == pimAttributeType && !fakeAttribute.Checked)
                    {
                        fakeAttribute.SourceAttributeType = null;
                    }
                }
                if (!found)
                {
                    removedAttributeTypes.Add(pimAttributeType);
                    cmdDeleteAttributeType deleteCommand = new cmdDeleteAttributeType(controller) { AttributeTypeGuid = pimAttributeType };
                    controller.CreatedMacro.Commands.Add(deleteCommand);
                }
            }

            #endregion

            #region remove dummy entries in fake collection

            List<FakeAttributeType> toRemovePIM = new List<FakeAttributeType>();
            foreach (FakeAttributeType fakePIMAttributeType in fakeSet)
            {
                if (String.IsNullOrEmpty(fakePIMAttributeType.Name))
                {
                    if (fakePIMAttributeType.SourceAttributeType != null)
                    {
                        removedAttributeTypes.Add(fakePIMAttributeType.SourceAttributeType);
                        cmdDeleteAttributeType deleteCommand = new cmdDeleteAttributeType(controller) { AttributeTypeGuid = fakePIMAttributeType.SourceAttributeType };
                        controller.CreatedMacro.Commands.Add(deleteCommand);
                    }
                    toRemovePIM.Add(fakePIMAttributeType);
                }
            }

            foreach (FakeAttributeType attribute in toRemovePIM)
            {
                fakeSet.Remove(attribute);
            }

            #endregion

            Dictionary<AttributeType, string> namesDict = new Dictionary<AttributeType, string>();
            foreach (AttributeType a in startingSet)
            {
                if (!removedAttributeTypes.Contains(a))
                {
                    namesDict.Add(a, a.Name);
                }
            }

            // check for changes and new attributes
            var modified = from FakeAttributeType a in fakeSet
                           where
                               a.SourceAttributeType != null && !removedAttributeTypes.Contains(a.SourceAttributeType) &&
                               a.SomethingChanged()
                           select a;
            var added = from FakeAttributeType a in fakeSet where a.SourceAttributeType == null select a;

            // editing exisiting attribute
            foreach (FakeAttributeType modifiedAttribute in modified)
            {
                AttributeType sourceAttributeType = modifiedAttribute.SourceAttributeType;

                cmdUpdateAttributeType updateCommand = new cmdUpdateAttributeType(controller) 
                      { AttributeTypeGuid = sourceAttributeType, 
                        NewName = modifiedAttribute.Name, 
                        NewXSDDefinition = modifiedAttribute.XSDDefinition, 
                        NewIsSealed = false, 
                        NewBaseType = modifiedAttribute.BaseType ?? Guid.Empty };
                controller.CreatedMacro.Commands.Add(updateCommand);
                namesDict[sourceAttributeType] = modifiedAttribute.Name;
            }

            List<string> names = namesDict.Values.ToList();
            // new attribute
            foreach (FakeAttributeType addedAttribute in added)
            {
                if (!string.IsNullOrEmpty(addedAttribute.Name) && addedAttribute.Checked)
                {
                    cmdNewAttributeType createNewcommand = new cmdNewAttributeType(controller)
                          {
                              ProjectVersionGuid = projectVersion.ID,
                              PSMSchemaGuid = psmSchema ?? Guid.Empty,
                              Name = addedAttribute.Name,
                              XSDDefinition = addedAttribute.XSDDefinition,
                              IsSealed = false,
                              BaseTypeGuid = addedAttribute.BaseType ?? Guid.Empty
                          };
                            
                    controller.CreatedMacro.Commands.Add(createNewcommand);
                    addedAttributes.Add(addedAttribute);
                    names.Add(addedAttribute.Name);
                }
            }
            return addedAttributes;
        }

        private void gridAttributeTypes_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            UpdateApplyEnabled();
        }

        private void gridAttributeTypes_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            UpdateApplyEnabled();
        }

        private void UpdateApplyEnabled()
        {
            int errors = System.Windows.Controls.Validation.GetErrors(gridPIMAttributeTypes).Count;

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

        private void gridAttributeTypes_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            //if (e.Column == checkedColumn)
            //    return;

            if (e.Row != null && e.Row.Item is FakeAttributeType)
            {
                FakeAttributeType editedAttributeType = ((FakeAttributeType)e.Row.Item);
                if (!editedAttributeType.Checked)
                {
                }

                if (editedAttributeType.IsSealed)
                {
                    ExolutioErrorMsgBox.Show("This is a built-in type and it can not be changed.", "Built-in types can not be created, deleted or modified by the user. ");
                    e.Cancel = true;
                }

                if (e.Column == isSealedPIM || e.Column == isSealedPSM)
                {
                    ExolutioErrorMsgBox.Show("This property can not be modified.", "It is not possible to add more built-in types into the system. ");
                    return;
                }

                //if (e.Column == baseTypeColumn && editedAttribute.RepresentedAttribute != null)
                //{
                //    ErrorMsgBox.Show("Type can be changed only for PIM-less attributes. ", "You can change the represented attribute's type instead. ");
                //}
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

                            if (row.Item is FakeAttributeType)
                            {
                                if (cell.Column == isSealedPIM || cell.Column == isSealedPSM)
                                {
                                    if (cb.IsChecked == true)
                                    {
                                        ExolutioErrorMsgBox.Show("This is a built-in type and it can not be changed.", "Built-in types can not be created, deleted or modified by the user. ");
                                        return;
                                    }
                                    else
                                    {
                                        ExolutioErrorMsgBox.Show("This property can not be modified.", "It is not possible to add more built-in types into the system. ");
                                        return;
                                    }
                                }
                                if (cell.Column == checkedColumnPIM || cell.Column == checkedColumnPSM)
                                {
                                    if (((FakeAttributeType)row.Item).IsSealed)
                                    {
                                        ExolutioErrorMsgBox.Show("This is a built-in type and it can not be changed.", "Built-in types can not be created, deleted or modified by the user. ");
                                        return;
                                    }
                                }
                                cb.IsChecked = !cb.IsChecked.Value;
                                ((FakeAttributeType) row.Item).Checked = cb.IsChecked.Value;
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

        private void gridPIMAttributeTypes_InitializingNewItem(object sender, InitializingNewItemEventArgs e)
        {
            ((FakeAttributeType)e.NewItem).Checked = true;
        }

        private void gridPSMAttributeTypes_InitializingNewItem(object sender, InitializingNewItemEventArgs e)
        {
            ((FakeAttributeType) e.NewItem).Checked = true;
        }

        private void bCancel_OnClick(object sender, RoutedEventArgs e)
        {
            this.Close(false);
        }

        private void cbPSMSchema_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (bApply.IsEnabled)
            {
                if (ExolutioYesNoBox.Show("Changes not applied", "Apply performed changes?") == MessageBoxResult.Yes)
                {
                    bApply_Click(null, null);
                }
            }
            bApply.IsEnabled = false;
            Initialize(controller, projectVersion, SelectedPSMSchema);
        }
    }
}