﻿using System;
using System.Linq;
using System.Windows.Controls;
using EvoX.Model;
using EvoX.Model.PSM;
using EvoX.SupportingClasses;

namespace EvoX.View.Commands.ParameterControls
{
    public class PSMSchemaLookup : ComboBox,
                                   IOperationParameterControl, IOperationParameterControl<Guid>, IOperationParameterControl<PSMSchema>
    {
        public ProjectVersion ProjectVersion { get; set; }

        object IOperationParameterControl.Value
        {
            get { return Value; }
        }

        public void InitControl()
        {
            Items.Clear();
            foreach (PSMSchema psmSchema in ProjectVersion.PSMSchemas)
            {
                ComboBoxItem listItem = new ComboBoxItem();
                listItem.Content = psmSchema.ToString();
                listItem.Tag = psmSchema.ID.ToString();
                Items.Add(listItem);
                SelectedIndex = 0;    
            }
            if (SuggestedValue != null && SuggestedValue is EvoXObject)
            {
                this.SelectedItem = this.Items.FirstOrDefault(i => ((ListBoxItem)i).Tag.ToString() == ((EvoXObject)SuggestedValue).ID.ToString());
            }
        }

        protected object SuggestedValue { get; set; }

        public void SetSuggestedValue(object suggestedValue)
        {
            this.SuggestedValue = suggestedValue;
        }

        public PSMSchema Value
        {
            get
            {
                if (ProjectVersion != null)
                {
                    return ProjectVersion.Project.TranslateComponent<PSMSchema>((this as IOperationParameterControl<Guid>).Value);
                }
                else
                {
                    return null;
                }
            }
        }

        Guid IOperationParameterControl<Guid>.Value
        {
            get { return Guid.Parse(this.Tag.ToString()); }
        }
    }
}