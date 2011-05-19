using System;
using System.Linq;
using EvoX.SupportingClasses;
using System.Windows.Controls;
using EvoX.Model;
using EvoX.Model.PIM;

namespace EvoX.View.Commands.ParameterControls
{
    public class PIMSchemaLookup : ComboBox,
                                   IOperationParameterControl, IOperationParameterControl<Guid>, IOperationParameterControl<PIMSchema>
    {
        public ProjectVersion ProjectVersion { get; set; }

        object IOperationParameterControl.Value
        {
            get { return Value; }
        }

        public void InitControl()
        {
            Items.Clear();

            ComboBoxItem listItem = new ComboBoxItem();
            listItem.Content = ProjectVersion.PIMSchema.ToString();
            listItem.Tag = ProjectVersion.PIMSchema.ID.ToString();
            Items.Add(listItem);
            SelectedIndex = 0;
            if (SuggestedValue != null && SuggestedValue is EvoXObject)
            {
                this.SelectedItem = this.Items.FirstOrDefault(i => ((ListBoxItem)i).Tag.ToString() == ((EvoXObject)SuggestedValue).ID.ToString());
            }
        }

        protected object SuggestedValue { get; set; }

        public void SetSuggestedValue(object suggestedValue)
        {
            this.SuggestedValue = suggestedValue;
            if (SuggestedValue != null && SuggestedValue is EvoXObject)
            {
                this.SelectedItem = this.Items.FirstOrDefault(i => ((ListBoxItem)i).Tag.ToString() == ((EvoXObject)SuggestedValue).ID.ToString());
            }
        }

        public PIMSchema Value
        {
            get
            {
                if (ProjectVersion != null)
                {
                    return ProjectVersion.Project.TranslateComponent<PIMSchema>((this as IOperationParameterControl<Guid>).Value);
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