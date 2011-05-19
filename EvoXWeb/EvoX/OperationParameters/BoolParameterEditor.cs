using System;
using System.Web.UI.WebControls;

namespace EvoX.Web.OperationParameters
{
    public class BoolParameterEditor: CheckBox, IOperationParameterControl<bool>, IOperationParameterControl
    {
        public bool Value
        {
            get { return Checked; }
        }

        public void InitControl()
        {
            
        }

        public void SetSuggestedValue(object suggestedValue)
        {
            this.Checked = (bool) suggestedValue;
        }

        object IOperationParameterControl.Value
        {
            get { return Value; }
        }
    }
}