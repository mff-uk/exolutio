using System;
using System.Windows.Controls;

namespace Exolutio.View.Commands.ParameterControls
{
    public class BoolParameterEditor: CheckBox, IOperationParameterControl<bool>, IOperationParameterControl
    {
        public bool Value
        {
            get { return IsChecked == true; }
        }

        public void InitControl()
        {
            
        }

        public void SetSuggestedValue(object suggestedValue)
        {
            this.IsChecked = (bool)suggestedValue;
        }

        object IOperationParameterControl.Value
        {
            get { return Value; }
        }
    }
}