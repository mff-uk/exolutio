using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using Exolutio.Controller.Commands;
using Exolutio.Controller.Commands.Reflection;
using Exolutio.Model;
using Exolutio.Model.PIM;
using Exolutio.Model.PSM;
using Exolutio.SupportingClasses;
using Exolutio.SupportingClasses.Reflection;
using Exolutio.View.Commands.ParameterControls;
using Exolutio.ViewToolkit;
using Label = System.Windows.Controls.Label;

namespace Exolutio.View.Commands
{
    public static class OperationParametersControlCreator
    {
        private struct InputHierarchyStruct
        {
            public Type SuperiorType { get; private set; }
            public string Label { get; private set; }
            public ParameterConsistency Consistency { get; private set; }

            public InputHierarchyStruct(Type superiorType, string label, string consistencyTypeKey) : this()
            {
                SuperiorType = superiorType;
                Label = label;
                Consistency = ConsistentWithAttribute.GetConsistencyInstance(consistencyTypeKey);
            }
        }

        private static Dictionary<Type, InputHierarchyStruct> inputHierarchy;

        private static void DefineInputHierarchy()
        {
            inputHierarchy = new Dictionary<Type, InputHierarchyStruct>
                                 {
                                     { typeof(Component), new InputHierarchyStruct(typeof(Schema), "Schema", SchemaComponentParameterConsistency.Key) },
                                     { typeof(AttributeType), new InputHierarchyStruct(typeof(Schema), "Schema", SchemaAttributeTypeParameterConsistency.Key) },
                                     //{ typeof(IHasCardinality), new InputHierarchyStruct(typeof(Schema), "Schema", SchemaComponentParameterConsistency.Key) },
                                     // PIM
                                     { typeof(PIMAttribute), new InputHierarchyStruct(typeof(PIMClass), "Class", PIMClassAttributeParameterConsistency.Key) },
                                     //{ typeof(PIMClass), new InputHierarchyStruct(typeof(PIMSchema), "Schema", PIMSchemaComponentParameterConsistency.Key) },
                                     //{ typeof(PIMAssociation), new InputHierarchyStruct(typeof(PIMSchema), "Schema", PIMSchemaComponentParameterConsistency.Key) },
                                     { typeof(PIMAssociationEnd), new InputHierarchyStruct(typeof(PIMAssociation), "Association", PIMAssociationAssociationEndParameterConsistency.Key) },
                                     { typeof(PIMComponent), new InputHierarchyStruct(typeof(PIMSchema), "Schema", PIMSchemaComponentParameterConsistency.Key) },
                                     // PSM
                                     { typeof(PSMAttribute), new InputHierarchyStruct(typeof(PSMClass) , "PSM class", PSMClassAttributeParameterConsistency.Key) },
                                     { typeof(PSMAssociation), new InputHierarchyStruct(typeof(PSMSchema), "PSM schema", PSMSchemaComponentParameterConsistency.Key) },
                                     { typeof(PSMAssociationMember), new InputHierarchyStruct(typeof(PSMSchema), "PSM schema", PSMSchemaComponentParameterConsistency.Key) },
                                     { typeof(PSMClass), new InputHierarchyStruct(typeof(PSMSchema), "PSM schema", PSMSchemaComponentParameterConsistency.Key) },
                                     { typeof(PSMComponent), new InputHierarchyStruct(typeof(PSMSchema), "PSM schema", PSMSchemaComponentParameterConsistency.Key) },
                                     { typeof(PSMContentModel), new InputHierarchyStruct(typeof(PSMSchema), "PSM schema", PSMSchemaComponentParameterConsistency.Key) },
                                     { typeof(PSMSchemaClass), new InputHierarchyStruct(typeof(PSMSchema), "PSM schema", PSMSchemaComponentParameterConsistency.Key) },
                                     { typeof(IPSMSemanticComponent), new InputHierarchyStruct(typeof(PSMSchema), "PSM schema", PSMSchemaComponentParameterConsistency.Key) }
                                 };
            //labelsForInputHierarchy = new Dictionary<Type, string>()
            //                    {
            //                         // PIM
            //                         { typeof(PIMAttribute), "Attribute"},
            //                         { typeof(PIMClass), "Class" },
            //                         { typeof(PIMAssociation), "Association" },
            //                         { typeof(PIMAssociationEnd), "Association end" },
            //                         { typeof(PIMComponent), "PIM component" },
            //                         // PSM
            //                         { typeof(PSMAttribute), "PSM attribute" },
            //                         { typeof(PSMAssociation), "PSM association" },
            //                         { typeof(PSMAssociationMember), "PSM association member" },
            //                         { typeof(PSMClass), "PSM class" },
            //                         { typeof(PSMComponent), "PSM component" },
            //                         { typeof(PSMContentModel), "PSM content model" },
            //                         { typeof(PSMSchemaClass), "PSM schema class" },
            //                     };
        }

        private static Dictionary<Type, InputHierarchyStruct> InputHierarchy
        {
            get
            {
                if (inputHierarchy == null)
                {
                    DefineInputHierarchy();
                }
                return inputHierarchy;
            }

        }

        public static List<Control> CreateControls(CommandDescriptor commandDescriptior, ProjectVersion projectVersion)
        {
            Dictionary<PropertyInfo, List<Control>> createdControls = new Dictionary<PropertyInfo, List<Control>>();

            Dictionary<string, Control> propertyEditors = new Dictionary<string, Control>();

            for (int index = 0; index < commandDescriptior.Parameters.Count; index++)
            {
                ParameterDescriptor parameter = commandDescriptior.Parameters[index];
                PropertyInfo parameterProperty = parameter.ParameterPropertyInfo;
                List<Control> controlsForProperty = createdControls.CreateSubCollectionIfNeeded(parameterProperty);

                if (!parameter.CreateControlInEdtors)
                    continue;

                Control _editor = null;

                if (parameterProperty != commandDescriptior.ScopeProperty)
                {
                    Label label = new Label();
                    label.Name = string.Format("lParam{0}op{2}guid_{1}", index, parameterProperty.Name,
                                               commandDescriptior.ShortCommandName);
                    label.Content = string.Format("{0}: ", parameter.ParameterName);
                    controlsForProperty.Add(label);
                }

                if (parameterProperty == commandDescriptior.ScopeProperty)
                {
                    ScopePropertyEditor scopePropertyEditor = new ScopePropertyEditor();
                    scopePropertyEditor.Name = string.Format("glParam{0}_op{2}_guid_{1}", index,
                                                         parameter.ParameterPropertyName,
                                                         commandDescriptior.ShortCommandName);
                    scopePropertyEditor.ProjectVersion = projectVersion;
                    scopePropertyEditor.InitControl();
                    scopePropertyEditor.Visibility = Visibility.Collapsed;
                    controlsForProperty.Add(scopePropertyEditor);
                    propertyEditors[parameterProperty.Name] = scopePropertyEditor;
                    _editor = scopePropertyEditor;
                }
                else if (parameterProperty.PropertyType == typeof (PIMSchema))
                {
                    PIMSchemaLookup pimSchemaLookup = new PIMSchemaLookup();
                    pimSchemaLookup.Name = string.Format("glParam{0}_op{2}_guid_{1}", index,
                                                         parameter.ParameterPropertyName,
                                                         commandDescriptior.ShortCommandName);
                    pimSchemaLookup.ProjectVersion = projectVersion;
                    pimSchemaLookup.InitControl();
                    controlsForProperty.Add(pimSchemaLookup);
                    propertyEditors[parameterProperty.Name] = pimSchemaLookup;
                    _editor = pimSchemaLookup;
                }
                else if (parameterProperty.PropertyType == typeof (PSMSchema))
                {
                    PSMSchemaLookup psmSchemaLookup = new PSMSchemaLookup();
                    psmSchemaLookup.Name = string.Format("glParam{0}_op{2}_guid_{1}", index,
                                                         parameter.ParameterPropertyName,
                                                         commandDescriptior.ShortCommandName);
                    psmSchemaLookup.ProjectVersion = projectVersion;
                    psmSchemaLookup.InitControl();
                    controlsForProperty.Add(psmSchemaLookup);
                    propertyEditors[parameter.ParameterPropertyName] = psmSchemaLookup;
                    _editor = psmSchemaLookup;
                }
                else if (parameterProperty.PropertyType.IsSubclassOf(typeof (Component)))
                {
                    GuidLookup guidLookup = new GuidLookup();
                    guidLookup.Name = string.Format("glParam{0}_op{2}_guid_{1}", index, parameter.ParameterPropertyName,
                                                    commandDescriptior.ShortCommandName);
                    guidLookup.LookedUpType = parameterProperty.PropertyType;
                    guidLookup.AllowNullInput = parameter.AllowNullInput;
                    guidLookup.ProjectVersion = projectVersion;
                    guidLookup.InitControl();
                    controlsForProperty.Add(guidLookup);
                    propertyEditors[parameter.ParameterPropertyName] = guidLookup;
                    _editor = guidLookup;
                }
                else if (parameterProperty.PropertyType == typeof (Guid))
                {
                    GuidLookup guidLookup = new GuidLookup();
                    guidLookup.Name = string.Format("glParam{0}_op{2}_guid_{1}", index, parameterProperty.Name,
                                                    commandDescriptior.ShortCommandName);
                    guidLookup.LookedUpType = parameter.ComponentType;
                    guidLookup.AllowNullInput = parameter.AllowNullInput;
                    guidLookup.ProjectVersion = projectVersion;
                    guidLookup.InitControl();
                    controlsForProperty.Add(guidLookup);
                    propertyEditors[parameterProperty.Name] = guidLookup;
                    _editor = guidLookup;
                }
                else if (parameterProperty.PropertyType == typeof (List<Guid>))
                {
                    GuidListBox guidListBox = new GuidListBox();
                    guidListBox.SelectionMode = SelectionMode.Multiple;
                    guidListBox.Width = 400;
                    guidListBox.Height = 100;
                    guidListBox.Name = string.Format("glParam{0}_op{2}_guid_{1}", index, parameterProperty.Name,
                                                     commandDescriptior.ShortCommandName);
                    guidListBox.LookedUpType = parameter.ComponentType;
                    guidListBox.AllowNullInput = parameter.AllowNullInput;
                    guidListBox.ProjectVersion = projectVersion;
                    guidListBox.InitControl();
                    controlsForProperty.Add(guidListBox);
                    propertyEditors[parameterProperty.Name] = guidListBox;
                    _editor = guidListBox;
                }
                else
                {
                    //Control _editor;
                    if (parameterProperty.PropertyType == typeof (string))
                    {
                        StringParameterEditor editor = new StringParameterEditor();
                        _editor = editor;
                    }
                    else if (parameterProperty.PropertyType == typeof (int) ||
                             parameterProperty.PropertyType == typeof (uint))
                    {
                        IntParameterEditor editor = new IntParameterEditor();
                        _editor = editor;
                    }
                    else if (parameterProperty.PropertyType == typeof (UnlimitedInt))
                    {
                        UnlimitedintParameterEditor editor = new UnlimitedintParameterEditor();
                        _editor = editor;
                    }
                    else if (parameterProperty.PropertyType == typeof (bool))
                    {
                        BoolParameterEditor editor = new BoolParameterEditor();
                        _editor = editor;
                    }
                    else if (parameterProperty.PropertyType.IsEnum)
                    {
                        EnumParameterEditor editor = new EnumParameterEditor();
                        editor.EnumType = parameterProperty.PropertyType;
                        _editor = editor;
                    }
                    else
                    {
                        throw new NotImplementedException(String.Format("Not implemented for type {0}.",
                                                                        parameterProperty.PropertyType.Name));
                    }

                    if (_editor is IOperationParameterControl)
                    {
                        ((IOperationParameterControl) _editor).InitControl();
                        if (parameter.SuggestedValue != null)
                        {
                            ((IOperationParameterControl) _editor).SetSuggestedValue(parameter.SuggestedValue);
                        }
                    }
                    _editor.Name = string.Format("glParam{0}_op{2}_guid_{1}", index, parameterProperty.Name,
                                                 commandDescriptior.ShortCommandName);
                    controlsForProperty.Add(_editor);
                    propertyEditors[parameterProperty.Name] = _editor;
                }

                if (parameter.ModifiedComponentProperty != null && commandDescriptior.ScopeObject != null)
                {
                    object currentValue = parameter.ModifiedComponentProperty.GetValue(commandDescriptior.ScopeObject, null);
                    if (currentValue != null)
                    {
                        ((IOperationParameterControl)_editor).SetSuggestedValue(currentValue);
                    }
                }

                if (propertyEditors[parameterProperty.Name] != null)
                {
                    if (!parameter.AllowNullInput && parameterProperty.PropertyType != typeof (bool))
                    {
                        Control controlForProperty = propertyEditors[parameterProperty.Name];
                        controlForProperty.BorderBrush = ViewToolkitResources.RedBrush;
                        controlForProperty.BorderThickness = new Thickness(2);
                        controlForProperty.Tag = "required";
                    }
                }
            }

            List<PropertyInfo> dependentProperties = new List<PropertyInfo>();

            foreach (ParameterDescriptor parameter in commandDescriptior.Parameters)
            {
                ConsistentWithAttribute consistentWithAttribute;
                if (parameter.ParameterPropertyInfo.TryGetAttribute(out consistentWithAttribute))
                {
                    IOperationParameterControlWithConsistencyCheck dependentPropertyEditor =
                        (IOperationParameterControlWithConsistencyCheck)
                        propertyEditors[parameter.ParameterPropertyName];

                    if (propertyEditors[consistentWithAttribute.PropertyName] is ScopePropertyEditor)
                    {
                        ScopePropertyEditor superiorPropertyEditor =
                            (ScopePropertyEditor) propertyEditors[consistentWithAttribute.PropertyName];

                        dependentPropertyEditor.SuperiorPropertyEditor = superiorPropertyEditor;
                        dependentPropertyEditor.ConsistencyChecker = consistentWithAttribute.GetConsistencyInstance();
                        superiorPropertyEditor.SelectionChanged += dependentPropertyEditor.OnSuperiorPropertyChanged;
                        
                        if (superiorPropertyEditor.Value != null)
                        {
                            dependentPropertyEditor.OnSuperiorPropertyChanged(null, null);
                        }

                        dependentProperties.Add(parameter.ParameterPropertyInfo);
                    }
                    else
                    {
                        ComboBox superiorPropertyEditor = (ComboBox) propertyEditors[consistentWithAttribute.PropertyName];

                        dependentPropertyEditor.SuperiorPropertyEditor =
                            (IOperationParameterControl) superiorPropertyEditor;
                        dependentPropertyEditor.ConsistencyChecker = consistentWithAttribute.GetConsistencyInstance();
                        superiorPropertyEditor.SelectionChanged += dependentPropertyEditor.OnSuperiorPropertyChanged;

                        if (superiorPropertyEditor.SelectedValue != null)
                        {
                            dependentPropertyEditor.OnSuperiorPropertyChanged(null, null);
                        }

                        dependentProperties.Add(parameter.ParameterPropertyInfo);
                    }
                }
            }

            for (int index = 0; index < commandDescriptior.Parameters.Count; index++)
            {
                ParameterDescriptor parameter = commandDescriptior.Parameters[index];
                if (parameter.ParameterPropertyInfo == commandDescriptior.ScopeProperty)
                {
                    continue;
                }
                PropertyInfo parameterProperty = parameter.ParameterPropertyInfo;
                if (dependentProperties.Contains(parameterProperty))
                {
                    continue;
                }
                if (parameterProperty.PropertyType.IsAmong(typeof (Guid), typeof (List<Guid>)))
                {
                    List<Control> controlsForProperty = createdControls[parameterProperty];
                    Type componentType = parameter.ComponentType;

                    int supCount = 0;
                    IOperationParameterControlWithConsistencyCheck dependentPropertyEditor =
                        (IOperationParameterControlWithConsistencyCheck) propertyEditors[parameterProperty.Name];
                    GuidLookup superiorPropertyEditor = null;
                    IOperationParameterControlWithConsistencyCheck oneBelowSuperiorPropertyEditor = null;
                    Type topLevelType = null;
                    while (InputHierarchy.ContainsKey(componentType) && parameter.CreateEditorHierarchy)
                    {
                        InputHierarchyStruct inputHierarchyStruct = InputHierarchy[componentType];
                        Type superiorType = inputHierarchyStruct.SuperiorType;
                        if (superiorType == typeof(Schema) && (commandDescriptior.Scope | ScopeAttribute.EScope.PIM) != ScopeAttribute.EScope.None)
                        {
                            superiorType = typeof (PSMSchema);
                        }
                        if (superiorType == typeof(Schema) && (commandDescriptior.Scope | ScopeAttribute.EScope.PSM) != ScopeAttribute.EScope.None)
                        {
                            superiorType = typeof (PSMSchema);
                        }
                        
                        topLevelType = superiorType;

                        // label
                        Label label = new Label();
                        label.Name = string.Format("lParam{0}_op{2}_guid_{1}sup{3}", index, parameterProperty.Name,
                                                   commandDescriptior.ShortCommandName, supCount);
                        label.Content = string.Format("{0} (for {1}): ", inputHierarchyStruct.Label,
                                                      parameter.ParameterName);
                        controlsForProperty.Insert(0, label);

                        oneBelowSuperiorPropertyEditor = superiorPropertyEditor ?? dependentPropertyEditor;
                        // drop down list
                        superiorPropertyEditor = new GuidLookup();
                        superiorPropertyEditor.Name = string.Format("glParam{0}_op{2}_guid_{1}sup{3}", index,
                                                                    parameterProperty.Name,
                                                                    commandDescriptior.ShortCommandName, supCount);
                        superiorPropertyEditor.LookedUpType = superiorType;
                        superiorPropertyEditor.ProjectVersion = projectVersion;
                        superiorPropertyEditor.InitControl();
                        controlsForProperty.Insert(1, superiorPropertyEditor);

                        dependentPropertyEditor.SuperiorPropertyEditor = superiorPropertyEditor;
                        dependentPropertyEditor.ConsistencyChecker = inputHierarchyStruct.Consistency;
                        superiorPropertyEditor.SelectionChanged += dependentPropertyEditor.OnSuperiorPropertyChanged;

                        if (superiorPropertyEditor.SelectedValue != null)
                        {
                            dependentPropertyEditor.OnSuperiorPropertyChanged(null, null);
                        }

                        // prepare for next cycle
                        supCount++;
                        dependentPropertyEditor = superiorPropertyEditor;
                        componentType = superiorType;
                    }

                    if (superiorPropertyEditor != null)
                    {
                        superiorPropertyEditor.LoadAllPossibleValues(topLevelType);
                        if (superiorPropertyEditor.SelectedValue != null && oneBelowSuperiorPropertyEditor != null)
                        {
                            oneBelowSuperiorPropertyEditor.OnSuperiorPropertyChanged(null, null);
                        }
                    }
                    else
                    {
                        dependentPropertyEditor.LoadAllPossibleValues(componentType);
                    }
                }
            }


            //Control[] controlsNodNeeded = createdControls[commandDescriptior.ScopeProperty].ToArray();
            //createdControls.Remove(commandDescriptior.ScopeProperty);
            return createdControls.Flatten();
        }

        public static void ReadParameterValues(CommandDescriptor parametersDescriptors, IEnumerable<Control> controls)
        {
            foreach (ParameterDescriptor t in parametersDescriptors.Parameters)
            {
                t.ParameterValue = null;
            }
            foreach (Control control in controls)
            {
                if (control is IOperationParameterControl)
                {
                    string propertyName = control.Name.Substring(control.Name.LastIndexOf("_") + 1);
                    ParameterDescriptor parameter = parametersDescriptors.GetParameterByPropertyName(propertyName);
                    if (parameter != null)
                    {
                        PropertyInfo propertyInfo = parameter.ParameterPropertyInfo;
                        if (propertyInfo != null)
                        {
                            if (propertyInfo.PropertyType.IsAmong(typeof (Guid), typeof(List<Guid>)) &&
                                control is IOperationParameterControl<Guid>)
                            {
                                if (control is GuidListBox)
                                {
                                    parameter.ParameterValue = ((GuidListBox)control).Values;
                                }
                                else
                                {
                                    parameter.ParameterValue = ((IOperationParameterControl<Guid>)control).Value;
                                }
                            }
                            else
                            {
                                parameter.ParameterValue = ((IOperationParameterControl) control).Value;
                            }
                        }
                    }
                }
            }
        }

        public static bool VerifyAttributeClassPair(params object[] verifiedObjects)
        {
            PIMAttribute attribute = (PIMAttribute) verifiedObjects[1];
            PIMClass @class = (PIMClass) verifiedObjects[0]; 

            return @class.PIMAttributes.Contains(attribute);
        }
    }

    
}