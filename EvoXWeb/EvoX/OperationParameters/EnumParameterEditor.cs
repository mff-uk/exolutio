using System;
using System.Collections;
using System.Web.UI.WebControls;

namespace EvoX.Web.OperationParameters
{
    public class EnumParameterEditor : DropDownList, IOperationParameterControl
    {
        public void InitControl()
        {
            this.Items.Clear();
            foreach (object value in Enum.GetValues(EnumType))
            {
                this.Items.Add(new ListItem(value.ToString(), value.ToString()));
            }
        }

        public Type EnumType { get; set; }

        public void SetSuggestedValue(object suggestedValue)
        {
            this.SelectedValue = suggestedValue.ToString();
        }

        object IOperationParameterControl.Value
        {
            get { return Enum.Parse(EnumType, SelectedValue); }
        }
    }
}