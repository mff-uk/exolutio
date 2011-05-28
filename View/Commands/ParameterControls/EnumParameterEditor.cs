using System;
using System.Windows.Controls;
using Exolutio.SupportingClasses;

namespace Exolutio.View.Commands.ParameterControls
{
    public class EnumParameterEditor : ComboBox, IOperationParameterControl
    {
        public void InitControl()
        {
            this.Items.Clear();
#if SILVERLIGHT
            var values = EnumHelper.GetValues(EnumType);
#else
            var values = Enum.GetValues(EnumType);
#endif
            foreach (object value in values)
            {
                this.Items.Add(new ComboBoxItem() { Content = value.ToString(), Tag = value });
            }
        }

        public Type EnumType { get; set; }

        public void SetSuggestedValue(object suggestedValue)
        {
            foreach (ComboBoxItem comboBoxItem in Items)
            {
                if (comboBoxItem.Content.ToString() == suggestedValue.ToString())
                {
                    this.SelectedValue = comboBoxItem;                                                                          
                    return;
                }
            }
        }

        object IOperationParameterControl.Value
        {
            get 
            {
                #if SILVERLIGHT
                return Enum.Parse(EnumType, ((ComboBoxItem)SelectedValue).Content.ToString(), true);
                #else
                return Enum.Parse(EnumType, ((ComboBoxItem)SelectedValue).Content.ToString());
                #endif
            }
        }
    }
}