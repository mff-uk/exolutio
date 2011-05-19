using System;
using System.Web.UI.WebControls;
using EvoX.Model;

namespace EvoX.Web.OperationParameters
{
    public class UnlimitedintParameterEditor : TextBox, IOperationParameterControl<UnlimitedInt>, IOperationParameterControl
    {
        public UnlimitedInt Value
        {
            get { return UnlimitedInt.Parse(Text); }
        }

        public void InitControl()
        {
            
        }

        public void SetSuggestedValue(object suggestedValue)
        {
            this.Text = suggestedValue.ToString();
        }

        object IOperationParameterControl.Value
        {
            get { return Value; }
        }
    }
}