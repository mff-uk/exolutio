using System;
using System.Windows.Controls;

namespace EvoX.View.Commands.ParameterControls
{
    public class StringParameterEditor:TextBox, IOperationParameterControl<string>, IOperationParameterControl
    {
        public void InitControl()
        {
            
        }

        public void SetSuggestedValue(object suggestedValue)
        {
            this.Text = suggestedValue.ToString();
        }

        public string Value
        {
            get { return Text; }
        }

        object IOperationParameterControl.Value
        {
            get { return Value; }
        }
    }
}